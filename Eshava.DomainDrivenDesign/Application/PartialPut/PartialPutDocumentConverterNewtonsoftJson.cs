using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Eshava.Core.Extensions;
using Eshava.DomainDrivenDesign.Domain.Attributes;
using Eshava.DomainDrivenDesign.Domain.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Eshava.DomainDrivenDesign.Application.PartialPut
{
	public class PartialPutDocumentConverterNewtonsoftJson : JsonConverter
	{
		private const string PATCHPROPERTY = "patch";
		private const string IDPROPERTY = "id";
		private const string ADDPROPERTY = "add";
		private const string REMOVEPROPERTY = "remove";
		private static readonly Type _typeGuid = typeof(Guid);
		private static readonly Type _typeDateTime = typeof(DateTime);
		private static readonly Type _partialPutDocumentType = typeof(PartialPutDocument<>);

		internal static DefaultContractResolver DefaultContractResolver { get; } = new();

		public override bool CanConvert(Type objectType)
		{
			if (!objectType.IsGenericType)
			{
				return false;
			}

			return objectType.GetGenericTypeDefinition() == _partialPutDocumentType;
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			Type genericType = null;
			JObject jObject = null;

			try
			{
				if (reader.TokenType == JsonToken.Null)
				{
					return null;
				}

				genericType = objectType.GenericTypeArguments[0];

				// load jObject
				jObject = JObject.Load(reader);
				var jTokenPatch = jObject.Children().FirstOrDefault(t => IsToken(t, PATCHPROPERTY));
				if (jTokenPatch == null)
				{
					jTokenPatch = jObject;
				}
				else
				{
					jTokenPatch = jTokenPatch.First();
				}

				var patchDocumentLayer = DeserializeObject(null, null, genericType, jTokenPatch);

				var targetOperations = new object[] {
					patchDocumentLayer.Operations,
					patchDocumentLayer.Layers
				};

				return Activator.CreateInstance(objectType, targetOperations);
			}
			catch (Exception ex)
			{
				throw new PartialPutDocumentConverterNewtonsoftJsonException("InvalidPartialPutDocument", ex, genericType, jObject);
			}
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, SerializeObject(value));
		}

		private static JObject SerializeObject(object value)
		{
			var type = value?.GetType();
			if (type == null || type.IsNoClass())
			{
				return null;
			}

			var jObject = new JObject();

			foreach (var propertyInfo in type.GetProperties())
			{
				if (!propertyInfo.CanRead)
				{
					continue;
				}

				var propertyValue = propertyInfo.GetValue(value);
				if (propertyValue == null)
				{
					continue;
				}

				var newtonsoftJsonIgnore = propertyInfo.GetCustomAttribute<Newtonsoft.Json.JsonIgnoreAttribute>();
				if (newtonsoftJsonIgnore != null)
				{
					continue;
				}

				var jsonIgnore = propertyInfo.GetCustomAttribute<System.Text.Json.Serialization.JsonIgnoreAttribute>();
				if (jsonIgnore != null)
				{
					continue;
				}

				var propertyValueType = propertyValue.GetType();
				if (propertyValueType.ImplementsIEnumerable() && !propertyValueType.ImplementsInterface(typeof(IDictionary)))
				{
					var child = new JArray();
					var dataRecordEnumerable = propertyValue as System.Collections.IEnumerable;
					var enumerationType = propertyValueType.GetDataTypeFromIEnumerable();

					if (!enumerationType.IsNoClass())
					{
						foreach (var subItem in dataRecordEnumerable)
						{
							var childItem = SerializeObject(subItem);
							if (childItem != null)
							{
								child.Add(childItem);
							}
						}
					}
					else
					{
						foreach (var subItem in dataRecordEnumerable)
						{
							child.Add(new JValue(subItem));
						}
					}

					jObject.Add(new JProperty(GetJsonPropertyName(propertyInfo), child));
				}
				else if (!propertyValueType.IsNoClass())
				{
					var child = SerializeObject(propertyValue);
					if (child != null)
					{
						jObject.Add(new JProperty(GetJsonPropertyName(propertyInfo), child));
					}
				}
				else
				{
					jObject.Add(new JProperty(GetJsonPropertyName(propertyInfo), propertyValue));
				}
			}

			return jObject;
		}

		private static PartialPutDocumentLayer DeserializeObject(object id, string propertyName, Type type, JToken jObject)
		{
			var operations = new List<PartialPutOperation>();
			var subLayers = new List<PartialPutDocumentLayer>();

			foreach (var propertyInfo in type.GetProperties())
			{
				if (!propertyInfo.CanWrite)
				{
					continue;
				}

				var jsonIgnore = propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>();
				if (jsonIgnore != null)
				{
					continue;
				}

				var isReadOnly = propertyInfo.GetCustomAttribute<ReadOnlyAttribute>();
				if (isReadOnly != null && isReadOnly.IsReadOnly)
				{
					continue;
				}

				var autoPatchBlocked = propertyInfo.GetCustomAttribute<AutoPatchBlockedAttribute>();
				if (autoPatchBlocked is not null)
				{
					continue;
				}

				var propertyNameToLower = propertyInfo.Name.ToLower();
				if (propertyNameToLower.Equals(IDPROPERTY))
				{
					continue;
				}

				var jToken = jObject.Children().FirstOrDefault(t => IsToken(t, propertyNameToLower));
				if (jToken == null)
				{
					continue;
				}

				if (propertyInfo.PropertyType.ImplementsIEnumerable() && !propertyInfo.PropertyType.ImplementsInterface(typeof(IDictionary)))
				{
					var jArray = jToken.First() as JArray;
					if (jArray == null)
					{
						continue;
					}

					var enumerationType = propertyInfo.PropertyType.GetDataTypeFromIEnumerable();
					if (!enumerationType.IsNoClass())
					{
						foreach (var jArrayItem in jArray)
						{
							var jTokenId = jArrayItem.Children().FirstOrDefault(t => IsToken(t, IDPROPERTY));
							var jTokenAdd = jArrayItem.Children().FirstOrDefault(t => IsToken(t, ADDPROPERTY));
							var jTokenRemove = jArrayItem.Children().FirstOrDefault(t => IsToken(t, REMOVEPROPERTY));

							if (jTokenId == null || (jTokenId as JProperty).Value == null || jTokenAdd != null)
							{
								operations.Add(new PartialPutOperation
								(
									propertyName: propertyInfo.Name,
									value: jArrayItem.ToObject(propertyInfo.PropertyType.GetDataTypeFromIEnumerable()),
									type: PartialPutOperationType.Add
								));

								continue;
							}

							var propertyInfoId = enumerationType.GetProperty("Id");

							if (jTokenId != null && (jTokenId as JProperty)?.Value != null && jTokenRemove != null)
							{
								operations.Add(new PartialPutOperation
								(
									propertyName: propertyInfo.Name,
									value: ToType(jTokenId as JProperty, propertyInfoId.PropertyType),
									type: PartialPutOperationType.Remove
								));

								continue;
							}

							subLayers.Add(DeserializeObject(ToType(jTokenId as JProperty, propertyInfoId.PropertyType), propertyInfo.Name, enumerationType, jArrayItem));
						}
					}
					else
					{
						var enumerableType = propertyInfo.PropertyType.IsArray
							? enumerationType.MakeArrayType()
							: typeof(List<>).MakeGenericType(new[] { enumerationType })
							;

						operations.Add(new PartialPutOperation
						(
							propertyName: propertyInfo.Name,
							value: jToken.First().ToObject(enumerableType),
							type: PartialPutOperationType.Replace
						));
					}
				}
				else if (!propertyInfo.PropertyType.IsNoClass())
				{
					if (!(jToken.First() is JObject))
					{
						continue;
					}

					subLayers.Add(DeserializeObject(null, propertyInfo.Name, propertyInfo.PropertyType, jToken.First()));
				}
				else
				{
					operations.Add(new PartialPutOperation
					(
						propertyName: propertyInfo.Name,
						value: ToType(jToken as JProperty, propertyInfo.PropertyType),
						type: PartialPutOperationType.Replace
					));
				}
			}

			return new PartialPutDocumentLayer(id, propertyName, operations, subLayers);
		}

		private static object ToType(JProperty jProperty, Type type)
		{
			var jValue = jProperty.Value as JValue;
			if (jValue?.Value == null)
			{
				return null;
			}

			if (type.GetDataType().IsEnum)
			{
				return Enum.Parse(type.GetDataType(), jValue.Value.ToString());
			}

			if (type.GetDataType() == _typeGuid)
			{
				return Guid.Parse(jValue.Value.ToString());
			}

			if (type.GetDataType() == _typeDateTime)
			{
				return (DateTime?)jValue.Value;
			}

			return Convert.ChangeType(jValue.Value, type.GetDataType(), CultureInfo.InvariantCulture);
		}

		private static bool IsToken(JToken jToken, string propertyNameToLower)
		{
			if (jToken is JObject jObject)
			{
				return jObject.Path.ToLower().Equals(propertyNameToLower);
			}

			if (jToken is JArray jArray)
			{
				return jArray.Path.ToLower().Equals(propertyNameToLower);
			}

			if (jToken is JProperty jProperty)
			{
				return jProperty.Name.ToLower().Equals(propertyNameToLower);
			}

			return false;
		}

		private static string GetJsonPropertyName(PropertyInfo propertyInfo)
		{
			var jsonProperty = propertyInfo.GetCustomAttribute<JsonPropertyAttribute>();
			if (jsonProperty != null)
			{
				return jsonProperty.PropertyName;
			}

			return propertyInfo.Name.Substring(0, 1).ToLower() + propertyInfo.Name.Substring(1);
		}
	}
}