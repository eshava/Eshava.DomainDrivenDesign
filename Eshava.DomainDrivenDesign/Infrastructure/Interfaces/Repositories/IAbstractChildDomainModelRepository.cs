using System.Threading.Tasks;
using Eshava.Core.Models;

namespace Eshava.DomainDrivenDesign.Infrastructure.Interfaces.Repositories
{
	public interface IAbstractChildDomainModelRepository<TDomain, TCreationBag, TIdentifier> 
		where TDomain : class 
		where TCreationBag : class 
		where TIdentifier : struct
	{
		Task<ResponseData<TIdentifier>> CreateAsync(TDomain model, TCreationBag creationBag);
		Task<ResponseData<bool>> UpdateAsync(TDomain model);
		Task<ResponseData<bool>> DeleteAsync(TDomain model);
	}
}