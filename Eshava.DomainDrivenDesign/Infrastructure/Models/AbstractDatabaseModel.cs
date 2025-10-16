namespace Eshava.DomainDrivenDesign.Infrastructure.Models
{
	public abstract class AbstractDatabaseModel<TIdentifier> where TIdentifier : struct
    {
        public TIdentifier Id { get; set; }
    }
}