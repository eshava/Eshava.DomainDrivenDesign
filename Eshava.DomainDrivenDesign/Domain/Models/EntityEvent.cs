using System.Collections.Generic;
using System.Linq;

namespace Eshava.DomainDrivenDesign.Domain.Models
{
	internal class EntityEvent
	{
		public EntityEvent(string eventName, IEnumerable<string> changedProperties, object data)
		{
			EventName = eventName;
			Data = data;
			ChangedProperties = (changedProperties?.ToList() ?? []).AsReadOnly();
		}

		public string EventName { get; }
		public object Data { get; }
		public IReadOnlyList<string> ChangedProperties { get; }
	}
}
