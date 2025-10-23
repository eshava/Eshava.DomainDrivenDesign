using Eshava.DomainDrivenDesign.Application.Dtos;

namespace Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Search
{
	public class OfficeSearchFilterDto : AbstractFilterDto
    {
        public OfficeSearchFilterFieldsDto FilterFields { get; set; }
        public OfficeSearchSortFieldsDto SortFields { get; set; }

        public override object GetFilterFields() => FilterFields;
        public override object GetSortFields() => SortFields;
    }
}