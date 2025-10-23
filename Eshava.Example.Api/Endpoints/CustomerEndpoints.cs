using System.Threading.Tasks;
using Eshava.Example.Api.Extensions;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.Create;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.Deactivate;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.Update;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Read;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Queries.Search;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Eshava.Example.Api.Endpoints
{
	public class CustomerEndpoints
	{
		public void Map(WebApplication app)
		{
			app.MapPost("/customers/search", SearchAsync);
			app.MapGet("/customers/{id}", GetAsync);
			app.MapPost("/customers", CreateAsync);
			app.MapPut("/customers/{id}", UpdateAsync);
			app.MapDelete("/customers/{id}", DeleteAsync);
		}

		private async Task<IResult> SearchAsync(ICustomerSearchUseCase customerSearchUseCase, CustomerSearchRequest request)
		{
			return await customerSearchUseCase.SearchAsync(request).ToResultAsync();
		}

		private async Task<IResult> GetAsync(ICustomerReadUseCase customerReadUseCase, int id)
		{
			var request = new CustomerReadRequest
			{
				CustomerId = id
			};

			return await customerReadUseCase.ReadAsync(request).ToResultAsync();
		}

		private async Task<IResult> CreateAsync(ICustomerCreateUseCase customerCreateUseCase, CustomerCreateRequest request)
		{
			return await customerCreateUseCase.CreateAsync(request).ToResultAsync();
		}

		private async Task<IResult> UpdateAsync(ICustomerUpdateUseCase customerUpdateUseCase, int id, CustomerUpdateRequest request)
		{
			if (request != null)
			{
				request.CustomerId = id;
			}

			return await customerUpdateUseCase.UpdateAsync(request).ToResultAsync();
		}

		private async Task<IResult> DeleteAsync(ICustomerDeactivateUseCase customerDeactivateUseCase, int id)
		{
			var request = new CustomerDeactivateRequest
			{
				CustomerId = id
			};

			return await customerDeactivateUseCase.DeactivateAsync(request).ToResultAsync();
		}
	}
}