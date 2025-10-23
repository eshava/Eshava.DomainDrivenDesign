using System.Threading.Tasks;
using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.Core.Validation.Interfaces;
using Eshava.DomainDrivenDesign.Infrastructure.Interfaces;
using Eshava.DomainDrivenDesign.Infrastructure.Providers;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands;
using Eshava.Example.Infrastructure.Organizations.Offices;
using Microsoft.Extensions.Logging;

namespace Eshava.Example.Infrastructure.Organizations.Customers
{
	internal class CustomerInfrastructureProviderService : AbstractAggregateInfrastructureProvider<Domain.Organizations.SomeFeature.Customer, int>, ICustomerInfrastructureProviderService
	{
		private readonly IOfficeRepository _officeRepository;

		public CustomerInfrastructureProviderService(
			ICustomerRepository customerRepository,
			IOfficeRepository officeRepository,
			IDatabaseSettings databaseSettings,
			IValidationEngine validationEngine,
			ILogger<CustomerInfrastructureProviderService> logger
		) : base(databaseSettings, customerRepository)
		{
			_officeRepository = officeRepository;
		}

		protected override async Task<ResponseData<bool>> ExcecuteCompletionActionsForCreateAsync(Domain.Organizations.SomeFeature.Customer customer)
		{
			var creationBag = new CustomerCreationBag(customer.Id.Value);
			var officesResult = await SaveChildsAsync(customer.Offices, creationBag, _officeRepository, _ => true.ToResponseDataAsync());
			if (officesResult.IsFaulty)
			{
				return officesResult;
			}

			return true.ToResponseData();
		}

		protected override async Task<ResponseData<bool>> ExcecuteCompletionActionsForUpdateAsync(Domain.Organizations.SomeFeature.Customer customer)
		{
			var creationBag = new CustomerCreationBag(customer.Id.Value);
			var officesResult = await SaveChildsAsync(customer.Offices, creationBag, _officeRepository, _ => true.ToResponseDataAsync());
			if (officesResult.IsFaulty)
			{
				return officesResult;
			}

			return true.ToResponseData();
		}

		protected override async Task<ResponseData<bool>> ExcecutePrerequisitesActionsForDeleteAsync(Domain.Organizations.SomeFeature.Customer customer)
		{
			var officesResult = await DeleteChildsAsync(customer.Offices, _officeRepository, _ => true.ToResponseDataAsync());
			if (officesResult.IsFaulty)
			{
				return officesResult;
			}

			return true.ToResponseData();
		}
	}
}