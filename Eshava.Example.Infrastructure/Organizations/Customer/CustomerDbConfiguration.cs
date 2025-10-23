using Eshava.Storm.MetaData.Builders;
using Eshava.Storm.MetaData.Interfaces;

namespace Eshava.Example.Infrastructure.Organizations.Customers
{
	internal class CustomerDbConfiguration : IEntityTypeConfiguration<Customer>
	{
		public void Configure(EntityTypeBuilder<Customer> builder)
		{
			builder.HasKey(p => p.Id);
			builder.Property(p => p.Id).ValueGeneratedOnAdd();
			builder.ToTable("Customers", "organizations");
		}
	}
}