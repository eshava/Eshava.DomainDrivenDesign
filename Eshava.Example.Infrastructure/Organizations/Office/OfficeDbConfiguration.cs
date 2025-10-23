using Eshava.Storm.MetaData.Builders;
using Eshava.Storm.MetaData.Interfaces;

namespace Eshava.Example.Infrastructure.Organizations.Offices
{
	internal class OfficeDbConfiguration : IEntityTypeConfiguration<Office>
	{
		public void Configure(EntityTypeBuilder<Office> builder)
		{
			builder.HasKey(p => p.Id);
			builder.Property(p => p.Id).ValueGeneratedOnAdd();
			builder.ToTable("Offices", "organizations");
		}
	}
}