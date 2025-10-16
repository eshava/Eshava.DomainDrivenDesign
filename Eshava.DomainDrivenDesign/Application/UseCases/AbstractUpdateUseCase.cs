using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.Core.Validation.Interfaces;
using Eshava.DomainDrivenDesign.Application.Dtos;
using Eshava.DomainDrivenDesign.Application.PartialPut;
using Eshava.DomainDrivenDesign.Domain.Interfaces;
using Eshava.DomainDrivenDesign.Domain.Models;

namespace Eshava.DomainDrivenDesign.Application.UseCases
{
	public class AbstractUpdateUseCase<TDomain, TDto, TIdentifier> : AbstractDomainModelUseCase
		where TDomain : class, IEntity<TDomain, TIdentifier>
		where TDto : class
		where TIdentifier : struct
	{
		private readonly IValidationRuleEngine _validationConfiguration;

		public AbstractUpdateUseCase(
			IValidationRuleEngine validationConfiguration
		)
		{
			_validationConfiguration = validationConfiguration;
		}

		protected virtual Task<ResponseData<IList<Patch<TDomain>>>> ExecuteBeforeAsync(IList<Patch<TDomain>> patches, PartialPutDocument<TDto> document)
		{
			return patches.ToResponseDataAsync();
		}

		protected virtual Task<ResponseData<bool>> ExecuteAfterAsync(TDomain entity, PartialPutDocument<TDto> document)
		{
			return true.ToResponseDataAsync();
		}

		protected ResponseData<ValidationConfigurationResponse> GetValidationConfiguration<T>(bool produceTreeStructure = false) where T : class
		{
			return new ResponseData<ValidationConfigurationResponse>
			{
				Data = new ValidationConfigurationResponse
				{
					Configurations = _validationConfiguration.CalculateValidationRules<T>(produceTreeStructure).ToList()
				}
			};
		}
	}
}