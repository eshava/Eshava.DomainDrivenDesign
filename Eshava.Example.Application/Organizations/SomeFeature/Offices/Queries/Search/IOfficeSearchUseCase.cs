using System.Threading.Tasks;
using Eshava.Core.Models;

namespace Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Search
{
	public interface IOfficeSearchUseCase
	{
		Task<ResponseData<OfficeSearchResponse>> SearchAsync(OfficeSearchRequest request);
	}
}