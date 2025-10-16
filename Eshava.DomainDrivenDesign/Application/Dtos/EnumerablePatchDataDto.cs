using Eshava.DomainDrivenDesign.Domain.Models;
using System.Collections.Generic;

namespace Eshava.DomainDrivenDesign.Application.Dtos
{
	public class EnumerablePatchDataDto<TKey, TDto, TDomain> where TDto : class where TDomain : class
	{
		public EnumerablePatchDataDto()
		{
			ItemsToAdd = new List<TDto>();
			ItemsToPatch = new Dictionary<TKey, IList<Patch<TDomain>>>();
			ItemsToRemove = new List<TKey>();
		}

		public IList<TDto> ItemsToAdd { get; set; }
		public Dictionary<TKey, IList<Patch<TDomain>>> ItemsToPatch { get; set; }
		public IList<TKey> ItemsToRemove { get; set; }
	}
}