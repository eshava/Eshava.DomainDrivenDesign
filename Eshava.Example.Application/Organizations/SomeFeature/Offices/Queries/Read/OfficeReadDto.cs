namespace Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Read
{
	public class OfficeReadDto
	{
		public int Id { get; set; }
		public int CustomerId { get; set; }
		public string Name { get; set; }
		public OfficeReadAddressDto Address { get; set; }
	}
}