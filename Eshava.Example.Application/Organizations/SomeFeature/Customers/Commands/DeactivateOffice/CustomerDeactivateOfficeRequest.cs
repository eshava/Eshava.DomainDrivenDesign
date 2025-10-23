namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.DeactivateOffice
{
	public class CustomerDeactivateOfficeRequest
	{
		[Newtonsoft.Json.JsonIgnore]
		[System.Text.Json.Serialization.JsonIgnore]
		public int CustomerId { get; set; }

		[Newtonsoft.Json.JsonIgnore]
		[System.Text.Json.Serialization.JsonIgnore]
		public int OfficeId { get; set; }
	}
}