namespace Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Read
{
	public class OfficeReadRequest
	{
		[Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public int OfficeId { get; set; }
	}
}