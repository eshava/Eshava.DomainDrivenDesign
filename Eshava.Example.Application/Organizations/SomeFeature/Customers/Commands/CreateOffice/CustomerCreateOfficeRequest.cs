namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.CreateOffice
{
	public class CustomerCreateOfficeRequest
	{
		[Newtonsoft.Json.JsonIgnore]
		[System.Text.Json.Serialization.JsonIgnore]
		public int CustomerId { get; set; }
		public CustomerCreateOfficeDto Office { get; set; }
	}
}