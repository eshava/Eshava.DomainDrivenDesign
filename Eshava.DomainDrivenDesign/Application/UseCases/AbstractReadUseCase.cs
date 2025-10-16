using System.Threading.Tasks;
using Eshava.Core.Extensions;
using Eshava.Core.Models;

namespace Eshava.DomainDrivenDesign.Application.UseCases
{
	public abstract class AbstractReadUseCase<TRequest, TDto>
		where TRequest : class
		where TDto : class
	{

		protected virtual Task<ResponseData<TDto>> AdjustDtoAsync(TRequest request, TDto dto)
		{
			if (dto is null)
			{
				return new ResponseData<TDto>().ToTask();
			}

			return dto.ToResponseDataAsync();
		}
	}
}