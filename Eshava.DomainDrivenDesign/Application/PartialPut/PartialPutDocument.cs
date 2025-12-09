using System.Collections.Generic;

namespace Eshava.DomainDrivenDesign.Application.PartialPut
{
	[Newtonsoft.Json.JsonConverter(typeof(PartialPutDocumentConverterNewtonsoftJson))]
	public class PartialPutDocument<TDto> : PartialPutDocumentLayer where TDto : class
	{
		public PartialPutDocument(IEnumerable<PartialPutOperation> operations, IEnumerable<PartialPutDocumentLayer> layers)
			: base(null, null, operations, layers)
		{

		}
	}
}