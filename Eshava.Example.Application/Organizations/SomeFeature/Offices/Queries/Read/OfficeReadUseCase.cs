using System;
using System.Threading.Tasks;
using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Application.UseCases;
using Eshava.DomainDrivenDesign.Domain.Constants;
using Eshava.DomainDrivenDesign.Domain.Extensions;
using Eshava.Example.Application.Settings;
using Microsoft.Extensions.Logging;

namespace Eshava.Example.Application.Organizations.SomeFeature.Offices.Queries.Read
{
	internal class OfficeReadUseCase : AbstractReadUseCase<OfficeReadRequest, OfficeReadDto>, IOfficeReadUseCase
	{
		private readonly ExampleScopedSettings _scopedSettings;
		private readonly IOfficeQueryInfrastructureProviderService _officeQueryInfrastructureProviderService;
		private readonly ILogger<OfficeReadUseCase> _logger;

		public OfficeReadUseCase(
			ExampleScopedSettings scopedSettings,
			IOfficeQueryInfrastructureProviderService officeQueryInfrastructureProviderService,
			ILogger<OfficeReadUseCase> logger
			)
		{
			_scopedSettings = scopedSettings;
			_officeQueryInfrastructureProviderService = officeQueryInfrastructureProviderService;
			_logger = logger;
		}

		public async Task<ResponseData<OfficeReadResponse>> ReadAsync(OfficeReadRequest request)
		{
			try
			{
				var officeResult = await _officeQueryInfrastructureProviderService.ReadAsync(request.OfficeId);
				if (officeResult.IsFaulty)
				{
					return officeResult.ConvertTo<OfficeReadResponse>();
				}

				if (officeResult.Data is null)
				{
					return MessageConstants.NOTEXISTING.ToFaultyResponse<OfficeReadResponse>();
				}

				officeResult = await AdjustDtoAsync(request, officeResult.Data);
				if (officeResult.IsFaulty)
				{
					return officeResult.ConvertTo<OfficeReadResponse>();
				}

				return new OfficeReadResponse
				{
					Office = officeResult.Data
				}.ToResponseData();
			}
			catch (Exception ex)
			{
				var messageGuid = _logger.LogError(this, _scopedSettings, "Entity Office could not be read", ex, additional: new
				{
					request.OfficeId
				});

				return ResponseData<OfficeReadResponse>.CreateInternalServerError(MessageConstants.READDATAERROR, ex, messageGuid);
			}
		}
	}
}