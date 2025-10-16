using Eshava.Storm.Linq.Models;

namespace Eshava.DomainDrivenDesign.Infrastructure.Settings
{
	public class SharedWhereQuerySettings : WhereQuerySettings
	{
		public string QueryPartBetweenWhereAndOrderBy { get; set; }
	}
}