using System.Collections.Generic;
using Eshava.Core.Validation.Models;

namespace Eshava.DomainDrivenDesign.Application.Dtos
{
	public class ValidationConfigurationResponse
	{
		public IEnumerable<ValidationPropertyInfo> Configurations { get; set; }
	}
}