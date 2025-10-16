using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Application.Dtos;
using Eshava.DomainDrivenDesign.Domain.Constants;
using Eshava.DomainDrivenDesign.Domain.Extensions;

namespace Eshava.DomainDrivenDesign.Application.UseCases
{
	public abstract class AbstractDomainModelUseCase
	{
		protected static Task<ResponseData<bool>> IsValidForeignKeyAsync<Dto>(string propertyName, object propertyValue, Func<FilterRequestDto<Dto>, Task<ResponseData<int>>> countAsync, params Expression<Func<Dto, bool>>[] filter) where Dto : class
		{
			return ExecuteAsync(CheckType.Existence, propertyName, propertyValue, countAsync, filter);
		}

		protected static Task<ResponseData<bool>> IsUniquAsync<Dto>(string propertyName, object propertyValue, Func<FilterRequestDto<Dto>, Task<ResponseData<int>>> countAsync, params Expression<Func<Dto, bool>>[] filter) where Dto : class
		{
			return ExecuteAsync(CheckType.Unique, propertyName, propertyValue, countAsync, filter);
		}

		protected static Task<ResponseData<bool>> CheckAssignmentAsync<Dto>(string modelName, Func<FilterRequestDto<Dto>, Task<ResponseData<int>>> countAsync, params Expression<Func<Dto, bool>>[] filter) where Dto : class
		{
			return ExecuteAsync(CheckType.Assignment, modelName, null, countAsync, filter);
		}

		private static async Task<ResponseData<bool>> ExecuteAsync<Dto>(CheckType checkType, string propertyName, object propertyValue, Func<FilterRequestDto<Dto>, Task<ResponseData<int>>> countAsync, params Expression<Func<Dto, bool>>[] filter) where Dto : class
		{
			var countRequest = FilterRequestDto<Dto>.Create(filter);
			var countResult = await countAsync(countRequest);
			if (countResult.IsFaulty)
			{
				return countResult.ConvertTo<bool>();
			}

			switch (checkType)
			{
				case CheckType.Existence when countResult.Data == 0:

					return MessageConstants.INVALIDDATAERROR.ToFaultyResponse<bool>()
						.AddValidationError(propertyName, MessageConstants.NOTEXISTING, propertyValue)
						;

				case CheckType.Unique when countResult.Data > 0:

					return MessageConstants.INVALIDDATAERROR.ToFaultyResponse<bool>()
						.AddValidationError(propertyName, MessageConstants.ALREADYEXISTING, propertyValue)
						;

				case CheckType.Assignment when countResult.Data > 0:

					return MessageConstants.INVALIDDATAERROR.ToFaultyResponse<bool>()
						.AddValidationError(propertyName, MessageConstants.STILLASSIGNED)
							;
			}

			return true.ToResponseData();
		}

		private enum CheckType
		{
			Existence = 1,
			Unique = 2,
			Assignment = 3
		}
	}
}