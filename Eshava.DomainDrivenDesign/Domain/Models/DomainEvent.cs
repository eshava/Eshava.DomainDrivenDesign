using System;

namespace Eshava.DomainDrivenDesign.Domain.Models
{
	public class DomainEvent
	{
		public DomainEvent(string @event, object entityId, DomainEventData eventData, DateTime? processNotBeforeUtc)
		{
			Event = @event;
			EntityId = entityId;
			EventData = eventData;
			ProcessNotBeforeUtc = processNotBeforeUtc;
		}

		/// <summary>
		/// domain.domainmodel.eventname
		/// </summary>
		public string Event { get; }
		public object EntityId { get; }
		public DomainEventData EventData { get; }
		public DateTime? ProcessNotBeforeUtc { get; }
	}
}