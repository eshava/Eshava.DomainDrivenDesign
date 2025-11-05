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

namespace Eshava.Example.Domain.Organizations.SomeFeature
{
	public class Office : AbstractChildDomainModel<Office, int>
	{
		private Func<Office, ResponseData<bool>> _actionCallback = null;

		private Office(IValidationEngine validationEngine)
			: base(validationEngine)
		{

		}

		[Required]
		[MaxLength(250)]
		public string Name { get; private set; }
		public override string EventDomain => "organizations";

		public static Office DataToInstance(IEnumerable<Patch<Office>> patches, IValidationEngine validationEngine)
		{
			var instance = new Office(validationEngine);
			instance.ApplyPatches(patches.ToList());
			instance.ClearChanges();

			return instance;
		}

		internal static ResponseData<Office> CreateEntity<TDto>(TDto dto, IValidationEngine validationEngine, IEnumerable<(Expression<Func<TDto, object>> Dto, Expression<Func<Office, object>> Domain)> mappings = null)
			where TDto : class
		{
			var patches = dto.ToPatches(mappings);
			var instance = new Office(validationEngine);

			var createResult = instance.Create(patches);
			if (createResult.IsFaulty)
			{
				return createResult.ConvertTo<Office>();
			}

			return instance.ToResponseData();
		}

		internal static ResponseData<Office> CreateEntity(bool @public, string name, IValidationEngine validationEngine)
		{
			var patches = new List<Patch<Office>>()
			{
				Patch<Office>.Create(p => p.Name, name)
			};

			var instance = new Office(validationEngine);
			var createResult = instance.Create(patches);
			if (createResult.IsFaulty)
			{
				return createResult.ConvertTo<Office>();
			}

			return instance.ToResponseData();
		}

		public ResponseData<bool> Patch(IList<Patch<Office>> patches)
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

			var updateResult = Update(patches);
			if (updateResult.IsFaulty)
			{
				return updateResult;
			}

			if (_actionCallback is null)
			{
				return updateResult;
			}

			return _actionCallback(this);
		}

		public override ResponseData<bool> Deactivate()
		{
			return base.Deactivate();
		}

		internal void SetActionCallback(Func<Office, ResponseData<bool>> actionCallback)
		{
			_actionCallback = actionCallback;
		}
	}
}