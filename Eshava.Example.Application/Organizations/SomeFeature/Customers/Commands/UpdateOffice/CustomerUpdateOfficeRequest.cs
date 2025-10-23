using Eshava.DomainDrivenDesign.Application.PartialPut;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.UpdateOffice
{
	public class CustomerUpdateOfficeRequest
	{
		[Newtonsoft.Json.JsonIgnore]
		[System.Text.Json.Serialization.JsonIgnore]
		public int CustomerId { get; set; }

		[Newtonsoft.Json.JsonIgnore]
		[System.Text.Json.Serialization.JsonIgnore]
		public int OfficeId { get; set; }

		public PartialPutDocument<CustomerUpdateOfficeDto> Office { get; set; }
	}
}