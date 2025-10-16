using System.Linq;
using System.Threading.Tasks;
using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.Core.Validation.Interfaces;
using Eshava.DomainDrivenDesign.Application.Dtos;
using Eshava.DomainDrivenDesign.Domain.Interfaces;

namespace Eshava.DomainDrivenDesign.Application.UseCases
{
	public class AbstractCreateUseCase<TDto, TDomain, TIdentifier> : AbstractDomainModelUseCase
		 where TDto : class
		 where TDomain : class, IEntity<TDomain, TIdentifier>
		 where TIdentifier : struct
	{
		private readonly IValidationRuleEngine _validationConfiguration;

		public AbstractCreateUseCase(
			IValidationRuleEngine validationConfiguration
		)
		{
			_validationConfiguration = validationConfiguration;
		}

		protected virtual Task<ResponseData<bool>> ExecuteBeforeAsync(TDto dto)
		{
			return true.ToResponseDataAsync();
		}

		protected virtual Task<ResponseData<bool>> ExecuteAfterAsync(TDto dto, TDomain entity)
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