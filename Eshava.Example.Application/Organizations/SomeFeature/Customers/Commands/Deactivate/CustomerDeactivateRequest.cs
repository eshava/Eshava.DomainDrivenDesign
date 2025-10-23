namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.Deactivate
{
	public class CustomerDeactivateRequest
	{
		[Newtonsoft.Json.JsonIgnore]
		[System.Text.Json.Serialization.JsonIgnore]
		public int CustomerId { get; set; }
	}
}