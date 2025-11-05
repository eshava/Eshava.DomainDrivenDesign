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

		protected virtual bool AddAggregateChangedDomainEventOnChildDomainEvent { get; } = false;
		protected virtual bool ReduceChildDomainEventsToAggregateChangedDomainEvent { get; } = false;
		protected virtual bool RemoveChildDomainEventsOnAggregateCreateEvent { get; } = true;

		public sealed override bool IsValid => base.IsValid && AreAllChildsValid();
		public sealed override bool IsChanged => base.IsChanged || HasChangesInChilds();

		public sealed override void ClearChanges()
		{
			base.ClearChanges();
			ClearChildChanges();
		}

		public sealed override void ClearEvents()
		{
			base.ClearEvents();
			ClearChildEvents();
		}

		public sealed override IReadOnlyList<DomainEvent> GetDomainEvents()
		{
			var domainEventsReadOnly = base.GetDomainEvents();
			var domainEventCreatedName = EventModelName + DomainEventKeys.CREATED;
			var hasCreatedEvent = domainEventsReadOnly.Any(de => de.Event == domainEventCreatedName);

			if (RemoveChildDomainEventsOnAggregateCreateEvent && hasCreatedEvent)
			{
				return domainEventsReadOnly;
			}

			var domainEvents = domainEventsReadOnly.ToList();
			var childDomainEvents = GetChildDomainEvents();
			if (childDomainEvents.Any())
			{
				var addChangedEvent = ReduceChildDomainEventsToAggregateChangedDomainEvent || AddAggregateChangedDomainEventOnChildDomainEvent;
				if (!hasCreatedEvent && addChangedEvent)
				{
					var domainEventChangedName = EventModelName + DomainEventKeys.CHANGED;
					if (domainEvents.All(de => de.Event != domainEventChangedName))
					{
						domainEvents.Add(new DomainEvent(domainEventChangedName, Id ?? default, null));
					}
				}

				if (!ReduceChildDomainEventsToAggregateChangedDomainEvent)
				{
					domainEvents.AddRange(childDomainEvents);
				}
			}

			return domainEvents.AsReadOnly();
		}

		protected IEnumerable<DomainEvent> GetChildDomainEvents<T, Identifier>(IEnumerable<T> childs)
			where T : AbstractEntity<T, TIdentifier>
			where Identifier : struct
		{
			return childs.SelectMany(child => child.GetDomainEvents()).ToList();
		}

		protected void ClearChildChanges<T, Identifier>(IEnumerable<T> childs)
			where T : AbstractEntity<T, TIdentifier>
			where Identifier : struct
		{
			foreach (var child in childs)
			{
				child.ClearChanges();
			}
		}

		protected void ClearChildEvents<T, Identifier>(IEnumerable<T> childs)
			where T : AbstractEntity<T, TIdentifier>
			where Identifier : struct
		{
			foreach (var child in childs)
			{
				child.ClearEvents();
			}
		}

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

		protected abstract IEnumerable<DomainEvent> GetChildDomainEvents();
		protected abstract void ClearChildEvents();
		protected abstract void ClearChildChanges();
		protected abstract bool AreAllChildsValid();
		protected abstract bool HasChangesInChilds();
		protected virtual void Init() { }
	}
}