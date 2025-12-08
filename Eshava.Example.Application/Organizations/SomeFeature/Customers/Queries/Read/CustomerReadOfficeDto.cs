namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Read
{
	public class CustomerReadOfficeDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public CustomerReadAddressDto Address { get; set; }
	}
}