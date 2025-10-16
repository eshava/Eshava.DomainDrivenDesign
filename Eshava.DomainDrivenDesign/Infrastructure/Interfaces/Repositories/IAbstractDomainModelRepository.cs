using System.Threading.Tasks;
using Eshava.Core.Models;

namespace Eshava.DomainDrivenDesign.Infrastructure.Interfaces.Repositories
{
	public interface IAbstractDomainModelRepository<TDomain, TIdentifier> where TDomain : class where TIdentifier : struct
	{
		Task<ResponseData<TDomain>> ReadAsync(TIdentifier identifier);
		Task<ResponseData<TIdentifier>> CreateAsync(TDomain model);
		Task<ResponseData<bool>> UpdateAsync(TDomain model);
		Task<ResponseData<bool>> DeleteAsync(TDomain model);
	}
}