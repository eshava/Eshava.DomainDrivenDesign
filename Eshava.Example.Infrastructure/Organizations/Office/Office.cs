namespace Eshava.Example.Infrastructure.Organizations.Offices
{
	internal class Office : AbstractExampleDatabaseModel<int>
	{
		public string Name { get; set; }
		public int CustomerId { get; set; }
		public string AddressStreet { get; set; }
		public string AddressStreetNumber { get; set; }
		public string AddressCity { get; set; }
		public string AddressZipCode { get; set; }
		public string AddressCountry { get; set; }

		/// <summary>
		/// Navigation Property
		/// </summary>
		public Customers.Customer Customer { get; set; }
	}
}