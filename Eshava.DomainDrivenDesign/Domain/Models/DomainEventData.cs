using System.Collections.Generic;
using System.Linq;

namespace Eshava.DomainDrivenDesign.Domain.Models
{
	public class DomainEventData
	{
		public DomainEventData(IEnumerable<string> changedProperties, IEnumerable<object> data)
		{
			Data =  (data?.ToList() ?? []).AsReadOnly();;
			ChangedProperties = (changedProperties?.ToList() ?? []).AsReadOnly();
		}

		public IReadOnlyList<object> Data { get; }
		public IReadOnlyList<string> ChangedProperties { get; }
	}
}