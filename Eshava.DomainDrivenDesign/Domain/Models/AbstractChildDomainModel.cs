using Eshava.Core.Validation.Interfaces;
using Eshava.DomainDrivenDesign.Domain.Interfaces;

namespace Eshava.DomainDrivenDesign.Domain.Models
{
	public abstract class AbstractChildDomainModel<TDomain, TIdentifier> : AbstractEntity<TDomain, TIdentifier>
		where TDomain : class, IEntity<TDomain, TIdentifier>
		where TIdentifier : struct
	{
		protected AbstractChildDomainModel(
			IValidationEngine validation
		)
		: base(validation)
		{
			
		}
	}
}