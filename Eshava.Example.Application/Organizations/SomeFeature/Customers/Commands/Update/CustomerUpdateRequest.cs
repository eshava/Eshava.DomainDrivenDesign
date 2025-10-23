using Eshava.DomainDrivenDesign.Application.PartialPut;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.Update
{
	public class CustomerUpdateRequest
	{
		[Newtonsoft.Json.JsonIgnore]
		[System.Text.Json.Serialization.JsonIgnore]
		public int CustomerId { get; set; }
		public PartialPutDocument<CustomerUpdateDto> Customer { get; set; }

	}
}