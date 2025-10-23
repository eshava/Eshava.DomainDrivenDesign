using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.Core.Validation.Interfaces;
using Eshava.DomainDrivenDesign.Domain.Extensions;
using Eshava.DomainDrivenDesign.Domain.Models;

namespace Eshava.Example.Domain.Organizations.SomeOtherFeature
{
	public class InterestedParty : AbstractDomainModel<InterestedParty, int>
	{
		private InterestedParty(IValidationEngine validationEngine)
			: base(validationEngine)
		{

		}

		[Required]
		[MaxLength(250)]
		public string Name { get; private set; }
		public string Note { get; private set; }

		public static InterestedParty DataToInstance(IEnumerable<Patch<InterestedParty>> patches, IValidationEngine validationEngine)
		{
			var instance = new InterestedParty(validationEngine);
			instance.ApplyPatches(patches.ToList());
			instance.SetUnchanged();

			return instance;
		}

		public static ResponseData<InterestedParty> CreateEntity<TDto>(TDto dto, IValidationEngine validationEngine, IEnumerable<(Expression<Func<TDto, object>> Dto, Expression<Func<InterestedParty, object>> Domain)> mappings = null)
			where TDto : class
		{
			var patches = dto.ToPatches(mappings);
			var instance = new InterestedParty(validationEngine);

			var createResult = instance.Create(patches);
			if (createResult.IsFaulty)
			{
				return createResult.ConvertTo<InterestedParty>();
			}

			return instance.ToResponseData();
		}

		public static ResponseData<InterestedParty> CreateEntity(string name, string note, IValidationEngine validationEngine)
		{
			var patches = new List<Patch<InterestedParty>>()
			{
				Patch<InterestedParty>.Create(p => p.Name, name),
				Patch<InterestedParty>.Create(p => p.Note, note)
			};

			var instance = new InterestedParty(validationEngine);

			var createResult = instance.Create(patches);
			if (createResult.IsFaulty)
			{
				return createResult.ConvertTo<InterestedParty>();
			}

			return instance.ToResponseData();
		}

		public ResponseData<bool> Patch(IList<Patch<InterestedParty>> patches)
		{
			if ((patches?.Count ?? 0) <= 0)
			{
				return true.ToResponseData();
			}

			var areAllPatchesAllowedResult = AreAllPatchesAllowed(patches);
			if (areAllPatchesAllowedResult.IsFaulty)
			{
				return areAllPatchesAllowedResult;
			}

			return Update(patches);
		}
	}
}