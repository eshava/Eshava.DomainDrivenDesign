namespace Eshava.DomainDrivenDesign.Application.Dtos
{
	public abstract class AbstractFilterDto
	{
		public int Skip { get; set; }

		public int Take { get; set; }

		public virtual string GetSearchTerm()
		{
			return null;
		}

		public abstract object GetFilterFields();

		public abstract object GetSortFields();
	}
}