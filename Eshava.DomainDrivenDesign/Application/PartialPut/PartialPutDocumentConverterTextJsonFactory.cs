using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Eshava.DomainDrivenDesign.Application.PartialPut
{
	public class PartialPutDocumentConverterTextJsonFactory : JsonConverterFactory
	{
		private static readonly Type _partialPutDocumentType = typeof(PartialPutDocument<>);

		public override bool CanConvert(Type typeToConvert)
		{
			if (!typeToConvert.IsGenericType)
			{
				return false;
			}

			return typeToConvert.GetGenericTypeDefinition() == _partialPutDocumentType;
		}

		public override JsonConverter CreateConverter(Type type, JsonSerializerOptions options)
		{
			var typeArguments = type.GetGenericArguments();
			var dtoType = typeArguments[0];

			var converter = (JsonConverter)Activator
				.CreateInstance(typeof(PartialPutDocumentConverterTextJson<>)
				.MakeGenericType([dtoType]), BindingFlags.Instance | BindingFlags.Public, binder: null, args: [options], culture: null)!;

			return converter;
		}
	}
}