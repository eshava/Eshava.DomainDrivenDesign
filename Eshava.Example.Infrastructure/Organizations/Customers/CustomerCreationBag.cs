namespace Eshava.Example.Infrastructure.Organizations.Customers
{
	internal class CustomerCreationBag
	{
		internal CustomerCreationBag(int customerId)
		{
			CustomerId = customerId;
		}

		public int CustomerId { get; }
	}
}