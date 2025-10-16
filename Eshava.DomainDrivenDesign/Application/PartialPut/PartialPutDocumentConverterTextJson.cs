using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Eshava.Core.Extensions;
using Eshava.DomainDrivenDesign.Domain.Attributes;
using Eshava.DomainDrivenDesign.Domain.Extensions;

namespace Eshava.DomainDrivenDesign.Application.PartialPut
{
  public class PartialPutDocumentConverterTextJson<TDto> : JsonConverter<PartialPutDocument<TDto>> where TDto : class
	{
		private const string PATCHPROPERTY = "patch";
		private const string IDPROPERTY = "id";
		private const string ADDPROPERTY = "add";
		private const string REMOVEPROPERTY = "remove";

		private static readonly Type _intType = typeof(int);
		private static readonly Dictionary<Type, Func<JsonElement, object>> _typeConverter = new Dictionary<Type, Func<JsonElement, object>>
		{
			{ typeof(byte), element => element.GetByte() },
			{ typeof(short), element => element.GetInt16() },
			{ typeof(ushort), element => element.GetUInt16() },
			{ _intType, element => element.GetInt32() },
			{ typeof(uint), element => element.GetUInt32() },
			{ typeof(long), element => element.GetInt64() },
			{ typeof(ulong), element => element.GetUInt64() },
			{ typeof(decimal), element => element.GetDecimal() },
			{ typeof(double), element => element.GetDouble() },
			{ typeof(bool), element => element.GetBoolean() },
			{ typeof(DateTime), element => element.GetDateTime() },
			{ typeof(string), element => element.GetString() },
			{ typeof(Guid), element => element.GetGuid() },
			{ typeof(float), element => element.GetSingle() },
		};
		private readonly JsonSerializerOptions _options;

		public PartialPutDocumentConverterTextJson(JsonSerializerOptions options)
		{
			_options = options;
		}

		public override PartialPutDocument<TDto> Read(ref Utf8JsonReader reader, Type objectType, JsonSerializerOptions options)
		{
			Type genericType = null;
			JsonDocument jsonDocument = null;

			try
			{
				if (reader.TokenType != JsonTokenType.StartObject)
				{
					return null;
				}

				// load jObject
				if (!JsonDocument.TryParseValue(ref reader, out jsonDocument))
				{
					return null;
				}

				genericType = objectType.GenericTypeArguments[0];


				var patchElement = GetProperty(jsonDocument.RootElement, PATCHPROPERTY);
				if (patchElement is null)
				{
					patchElement = jsonDocument.RootElement;
				}

				var patchDocumentLayer = DeserializeObject(null, null, genericType, patchElement.Value, options ?? _options);

				return new PartialPutDocument<TDto>(patchDocumentLayer.Operations, patchDocumentLayer.Layers);
			}
			catch (Exception ex)
			{
				throw new PartialPutDocumentConverterTextJsonException("InvalidPartialPutDocument", ex, genericType, jsonDocument);
			}
		}

		public override void Write(Utf8JsonWriter writer, PartialPutDocument<TDto> partialPutDocument, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			writer.WriteEndObject();
		}

		private static PartialPutDocumentLayer DeserializeObject(object id, string propertyName, Type type, JsonElement jObject, JsonSerializerOptions options)
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

				var propertyNameToLower = GetJsonPropertyName(propertyInfo);
				if (propertyNameToLower.Equals(IDPROPERTY))
				{
					continue;
				}

				var jToken = GetProperty(jObject, propertyNameToLower);
				if (jToken == null)
				{
					continue;
				}

				if (propertyInfo.PropertyType.ImplementsIEnumerable() && !propertyInfo.PropertyType.ImplementsInterface(typeof(IDictionary)))
				{
					var propertyElement = GetProperty(jObject, GetJsonPropertyName(propertyInfo));
					if (!propertyElement.HasValue)
					{
						continue;
					}

					var enumerationType = propertyInfo.PropertyType.GetDataTypeFromIEnumerable();
					if (!enumerationType.IsNoClass())
					{
						foreach (var jArrayItem in propertyElement.Value.EnumerateArray())
						{
							var jTokenId = GetProperty(jArrayItem, IDPROPERTY);
							var jTokenAdd = GetProperty(jArrayItem, ADDPROPERTY);
							var jTokenRemove = GetProperty(jArrayItem, REMOVEPROPERTY);

							if (jTokenId == null || jTokenId.Value.ValueKind == JsonValueKind.Null || jTokenAdd != null)
							{
								operations.Add(new PartialPutOperation
								(
									propertyName: propertyInfo.Name,
									value: jArrayItem.Deserialize(enumerationType, options),
									type: PartialPutOperationType.Add
								));

								continue;
							}

							var propertyInfoId = enumerationType.GetProperty("Id");
							var itemId = jTokenId is null ? null : ToType(jTokenId.Value, propertyInfoId.PropertyType);

							if (itemId is not null && jTokenRemove != null)
							{
								operations.Add(new PartialPutOperation
								(
									propertyName: propertyInfo.Name,
									value: itemId,
									type: PartialPutOperationType.Remove
								));

								continue;
							}

							subLayers.Add(DeserializeObject(itemId, propertyInfo.Name, enumerationType, jArrayItem, options));
						}
					}
					else
					{
						var enumerableType = propertyInfo.PropertyType.IsArray
							? enumerationType.MakeArrayType()
							: typeof(List<>).MakeGenericType(new[] { enumerationType })
							;

						var enumerable = propertyElement.Value.Deserialize(enumerableType, options);

						operations.Add(new PartialPutOperation
						(
							propertyName: propertyInfo.Name,
							value: enumerable,
							type: PartialPutOperationType.Replace
						));
					}
				}
				else if (!propertyInfo.PropertyType.IsNoClass())
				{
					var propertyElement = GetProperty(jObject, GetJsonPropertyName(propertyInfo));
					if (!propertyElement.HasValue)
					{
						continue;
					}

					subLayers.Add(DeserializeObject(null, propertyInfo.Name, propertyInfo.PropertyType, propertyElement.Value, options));
				}
				else
				{
					operations.Add(new PartialPutOperation
					(
						propertyName: propertyInfo.Name,
						value: ToType(jToken.Value, propertyInfo.PropertyType),
						type: PartialPutOperationType.Replace
					));
				}
			}

			return new PartialPutDocumentLayer(id, propertyName, operations, subLayers);
		}

		private static object ToType(JsonElement jProperty, Type type)
		{
			if (jProperty.ValueKind == JsonValueKind.Null
				|| jProperty.ValueKind == JsonValueKind.Undefined)
			{
				return null;
			}

			type = type.GetDataType();

			if (type.IsEnum)
			{
				var intValue = (int)_typeConverter[_intType](jProperty);

				return Enum.Parse(type.GetDataType(), intValue.ToString());
			}
			else if (_typeConverter.TryGetValue(type, out var converter))
			{
				return converter(jProperty);
			}

			return null;
		}

		private static string GetJsonPropertyName(PropertyInfo propertyInfo)
		{
			var jsonProperty = propertyInfo.GetCustomAttribute<JsonPropertyNameAttribute>();
			if (jsonProperty != null)
			{
				return jsonProperty.Name;
			}

			return ToJsonName(propertyInfo.Name);
		}

		private static JsonElement? GetProperty(JsonElement element, string name)
		{
			return element.ValueKind != JsonValueKind.Null
				&& element.ValueKind != JsonValueKind.Undefined
				&& element.TryGetProperty(name, out var value)
				? value
				: null;
		}

		public static string ToJsonName(string typeName)
		{
			var variableName = typeName[0].ToString().ToLower() + typeName.Substring(1);

			return variableName;
		}
	}
}
