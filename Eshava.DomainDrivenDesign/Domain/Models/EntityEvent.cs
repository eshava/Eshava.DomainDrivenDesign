using System;
using System.Collections.Generic;
using System.Linq;

namespace Eshava.DomainDrivenDesign.Domain.Models
{
	internal class EntityEvent
	{
		public EntityEvent(string eventName, IEnumerable<string> changedProperties, object data, DateTime? processNotBeforeUtc)
		{
			EventName = eventName;
			Data = data;
			ChangedProperties = (changedProperties?.ToList() ?? []).AsReadOnly();
			ProcessNotBeforeUtc = processNotBeforeUtc;
		}

		public string EventName { get; }
		public object Data { get; }
		public IReadOnlyList<string> ChangedProperties { get; }
		public DateTime? ProcessNotBeforeUtc { get; }
	}
}
