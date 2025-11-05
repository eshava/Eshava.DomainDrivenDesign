using Eshava.Core.Validation.Interfaces;
using Eshava.DomainDrivenDesign.Domain.Interfaces;

namespace Eshava.DomainDrivenDesign.Domain.Models
{
	public abstract class AbstractDomainModel<TDomain, TIdentifier> : AbstractEntity<TDomain, TIdentifier>
		  where TDomain : class, IEntity<TDomain, TIdentifier>
		  where TIdentifier : struct
	{

		protected AbstractDomainModel(
			IValidationEngine validation
		)
		: base(validation)
		{

		}

		public sealed override void ClearChanges() => base.ClearChanges();
		public sealed override void ClearEvents() => base.ClearEvents();
	}
}