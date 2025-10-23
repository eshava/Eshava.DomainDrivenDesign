namespace Eshava.Example.Infrastructure.Organizations.Offices
{
	internal class Office : AbstractExampleDatabaseModel<int>
	{
		public string Name { get; set; }
		public int CustomerId { get; set; }

		/// <summary>
		/// Navigation Property
		/// </summary>
		public Customers.Customer Customer { get; set; }
	}
}