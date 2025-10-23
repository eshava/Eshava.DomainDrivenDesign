namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Read
{
	public class CustomerReadRequest
	{
		[Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public int CustomerId { get; set; }
	}
}