namespace Eshava.Example.Infrastructure.Organizations.Customers
{
	internal class Customer : AbstractExampleDatabaseModel<int>
	{
		public string CompanyName { get; set; }
		public Domain.Organizations.SomeFeature.Classification Classification { get; set; }
		public string AddressStreet { get; set; }
		public string AddressStreetNumber { get; set; }
		public string AddressCity { get; set; }
		public string AddressZipCode { get; set; }
		public string AddressCountry { get; set; }

		/// <summary>
		/// Navigation Property
		/// </summary>
		public Offices.Office Office { get; set; }
	}
}
