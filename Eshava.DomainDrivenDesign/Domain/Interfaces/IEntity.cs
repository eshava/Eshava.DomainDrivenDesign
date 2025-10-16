using System.Collections.Generic;
using Eshava.DomainDrivenDesign.Domain.Enums;
using Eshava.DomainDrivenDesign.Domain.Models;

namespace Eshava.DomainDrivenDesign.Domain.Interfaces
{
	public interface IEntity<TDomain, TIdentifier>
		where TIdentifier : struct
		where TDomain : class
	{
		TIdentifier? Id { get; }
		Status Status { get; }
		IReadOnlyList<Patch<TDomain>> GetChanges();
	}
}