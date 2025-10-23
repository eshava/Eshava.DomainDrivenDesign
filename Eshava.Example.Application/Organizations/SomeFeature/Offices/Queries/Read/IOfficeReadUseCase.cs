using System.Threading.Tasks;
using Eshava.Core.Models;

namespace Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Read
{
	public interface IOfficeReadUseCase
	{
		Task<ResponseData<OfficeReadResponse>> ReadAsync(OfficeReadRequest request);
	}
}