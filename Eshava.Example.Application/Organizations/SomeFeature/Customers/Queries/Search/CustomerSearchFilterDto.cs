using Eshava.DomainDrivenDesign.Application.Dtos;

namespace Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Search
{
	public class CustomerSearchFilterDto: AbstractFilterDto
    {
        public CustomerSearchFilterFieldsDto FilterFields { get; set; }
        public CustomerSearchSortFieldsDto SortFields { get; set; }

        public override object GetFilterFields() => FilterFields;
        public override object GetSortFields() => SortFields;
    }
}