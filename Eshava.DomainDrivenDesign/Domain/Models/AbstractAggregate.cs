using System.Collections.Generic;
using System.Linq;
using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.Core.Validation.Interfaces;
using Eshava.DomainDrivenDesign.Domain.Constants;
using Eshava.DomainDrivenDesign.Domain.Interfaces;

namespace Eshava.DomainDrivenDesign.Domain.Models
{
	public abstract class AbstractAggregate<TDomain, TIdentifier> : AbstractEntity<TDomain, TIdentifier>
		 where TDomain : class, IEntity<TDomain, TIdentifier>
		 where TIdentifier : struct
	{
		protected AbstractAggregate(
			IValidationEngine validation
		)
		: base(validation)
		{
			Init();
		}

		public sealed override bool IsValid => base.IsValid && AreAllChildsValid();
		public sealed override bool IsChanged => base.IsChanged || HasChangesInChilds();

		protected ResponseData<T> GetChild<T, Identifier>(IEnumerable<T> childs, Identifier childId)
			where T : AbstractEntity<T, TIdentifier>
			where Identifier : struct
		{
			var child = childs.FirstOrDefault(c => c.Id.HasValue && c.Id.Value.Equals(childId) && c.Status == Enums.Status.Active);
			if (child is null)
			{
				return ResponseData<T>.CreateInvalidDataResponse()
					.AddValidationError(typeof(T).Name, MessageConstants.NOTEXISTING, childId);
			}

			return child.ToResponseData();
		}

		protected ResponseData<bool> DeactivateChild<T, Identifier>(IEnumerable<T> childs, Identifier childId)
			where T : AbstractEntity<T, TIdentifier>
			where Identifier : struct
		{
			var childResult = GetChild<T, Identifier>(childs, childId);
			if (childResult.IsFaulty)
			{
				return childResult.ConvertTo<bool>();
			}

			return childResult.Data.Deactivate();
		}

		protected ResponseData<bool> DeactivateChilds<T, Identifier>(IEnumerable<T> childs)
			where T : AbstractEntity<T, Identifier>
			where Identifier : struct
		{
			foreach (var child in childs)
			{
				if (child.Status != Enums.Status.Active)
				{
					continue;
				}

				var deactivateResult = child.Deactivate();
				if (deactivateResult.IsFaulty)
				{
					SetValidationStatus(false);

					return deactivateResult;
				}
			}

			return true.ToResponseData();
		}

		protected abstract bool AreAllChildsValid();
		protected abstract bool HasChangesInChilds();
		protected virtual void Init() { }
	}
}