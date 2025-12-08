using System.Collections.Generic;
using Eshava.Core.Models;
using Eshava.Core.Validation.Attributes;

namespace Eshava.DomainDrivenDesign.Domain.Models
{
	public abstract class AbstractValueObject
	{
		[ValidationExecution]
		public virtual IEnumerable<ValidationError> Validate()
		{
			// NULL and empty lists are considered as valid

			return null;
		}
	}
}