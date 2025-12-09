using System.Threading.Tasks;
using Eshava.Example.Api.Extensions;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.CreateOffice;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.DeactivateOffice;
using Eshava.Example.Application.Organizations.SomeFeature.Customers.Commands.UpdateOffice;
using Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Read;
using Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Search;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Eshava.Example.Api.Endpoints
{
	public class OfficeEndpoints
	{
		public void Map(WebApplication app)
		{
			app.MapPost("/offices/search", SearchAsync).Produces(200, typeof(OfficeSearchResponse));
			app.MapGet("/offices/{id}", GetAsync).Produces(200, typeof(OfficeReadResponse));
			app.MapPost("/customers/{customerId}/offices", CreateAsync).Produces(200, typeof(CustomerCreateOfficeResponse));
			app.MapPut("/customers/{customerId}/offices/{id}", UpdateAsync).Produces(204);
			app.MapDelete("/customers/{customerId}/offices/{id}", DeleteAsync).Produces(204);
		}

		private async Task<IResult> SearchAsync(IOfficeSearchUseCase officeSearchUseCase, OfficeSearchRequest request)
		{
			return await officeSearchUseCase.SearchAsync(request).ToResultAsync();
		}

		private async Task<IResult> GetAsync(IOfficeReadUseCase officeReadUseCase, int id)
		{
			var request = new OfficeReadRequest
			{
				OfficeId = id
			};

			return await officeReadUseCase.ReadAsync(request).ToResultAsync();
		}

		private async Task<IResult> CreateAsync(ICustomerCreateOfficeUseCase customerCreateOfficeUseCase, int customerId, CustomerCreateOfficeRequest request)
		{
			request?.CustomerId = customerId;

			return await customerCreateOfficeUseCase.CreateOfficeAsync(request).ToResultAsync();
		}

		private async Task<IResult> UpdateAsync(ICustomerUpdateOfficeUseCase customerUpdateOfficeUseCase, int customerId, int id, CustomerUpdateOfficeRequest request)
		{
			if (request != null)
			{
				request.CustomerId = customerId;
				request.OfficeId = id;
			}

			return await customerUpdateOfficeUseCase.UpdateOfficeAsync(request).ToResultAsync();
		}

		private async Task<IResult> DeleteAsync(ICustomerDeactivateOfficeUseCase customerDeactivateOfficeUseCase, int customerId, int id)
		{
			var request = new CustomerDeactivateOfficeRequest
			{
				CustomerId = customerId,
				OfficeId = id
			};

			return await customerDeactivateOfficeUseCase.DeactivateOfficeAsync(request).ToResultAsync();
		}
	}
}