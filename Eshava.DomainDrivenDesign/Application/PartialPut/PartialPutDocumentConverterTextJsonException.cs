using System;
using System.Text.Json;

namespace Eshava.DomainDrivenDesign.Application.PartialPut
{
	public class PartialPutDocumentConverterTextJsonException : Exception
	{
		public PartialPutDocumentConverterTextJsonException(string message, Exception innerException, Type objectType, JsonDocument value) : base(message, innerException)
		{
			ObjectType = objectType;
			Value = value;
		}

		public Type ObjectType { get; set; }
		public JsonDocument Value { get; set; }
	}
}