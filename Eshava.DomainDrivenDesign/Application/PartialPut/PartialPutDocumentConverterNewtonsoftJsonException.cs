using System;
using Newtonsoft.Json.Linq;

namespace Eshava.DomainDrivenDesign.Application.PartialPut
{
    public class PartialPutDocumentConverterNewtonsoftJsonException: Exception
	{
		public PartialPutDocumentConverterNewtonsoftJsonException(string message, Exception innerException, Type objectType, JObject value) : base(message, innerException)
		{
			ObjectType = objectType;
			Value = value;
		}

		public Type ObjectType { get; set; }
		public JObject Value { get; set; }
	}
}