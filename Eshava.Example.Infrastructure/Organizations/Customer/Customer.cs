namespace Eshava.Example.Infrastructure.Organizations.Customers
{
	internal class Customer : AbstractExampleDatabaseModel<int>
	{
		public string CompanyName { get; set; }
		public Domain.Organizations.SomeFeature.Classification Classification { get; set; }

		/// <summary>
		/// Navigation Property
		/// </summary>
		public Offices.Office Office { get; set; }
	}
}
