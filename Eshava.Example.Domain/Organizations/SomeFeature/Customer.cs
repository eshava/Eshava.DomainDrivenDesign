using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.Core.Validation.Attributes;
using Eshava.Core.Validation.Interfaces;
using Eshava.DomainDrivenDesign.Domain.Extensions;
using Eshava.DomainDrivenDesign.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace Eshava.Example.Domain.Organizations.SomeFeature
{
	public class Customer : AbstractAggregate<Customer, int>
	{
		private Func<Office, ResponseData<bool>> _officeChanged = p => true.ToResponseData();
		private IList<Office> _offices = default;

		private Customer(IValidationEngine validationEngine)
			: base(validationEngine)
		{

		}

		[Required]
		[MaxLength(250)]
		public string Name { get; private set; }

		[Enumeration(invalidateZero: true)]
		public Classification Classification { get; private set; }

		public IReadOnlyList<Office> Offices => _offices.AsReadOnly();

		public static Customer DataToInstance(IEnumerable<Patch<Customer>> patches, IEnumerable<Office> officeList, IValidationEngine validationEngine)
		{
			var instance = new Customer(validationEngine);
			foreach (var office in officeList)
			{
				office.SetActionCallback(instance._officeChanged);
			}

			instance.SetChilds(officeList);
			instance.ApplyPatches(patches.ToList());
			instance.SetUnchanged();

			return instance;
		}

		public static ResponseData<Customer> CreateEntity<TDto>(TDto dto, IValidationEngine validationEngine, IEnumerable<(Expression<Func<TDto, object>> Dto, Expression<Func<Customer, object>> Domain)> mappings = null)
			where TDto : class
		{
			var patches = dto.ToPatches(mappings);
			var instance = new Customer(validationEngine);
			instance.SetChilds(null);

			var createResult = instance.Create(patches);
			if (createResult.IsFaulty)
			{
				return createResult.ConvertTo<Customer>();
			}

			return instance.ToResponseData();
		}

		public static ResponseData<Customer> CreateEntity(string name, Classification classification, IValidationEngine validationEngine)
		{
			var patches = new List<Patch<Customer>>()
			{
				Patch<Customer>.Create(p => p.Name, name),
				Patch<Customer>.Create(p => p.Classification, classification)
			};

			var instance = new Customer(validationEngine);
			instance.SetChilds(null);

			var createResult = instance.Create(patches);
			if (createResult.IsFaulty)
			{
				return createResult.ConvertTo<Customer>();
			}

			return instance.ToResponseData();
		}

		public ResponseData<bool> Patch(IList<Patch<Customer>> patches)
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

		public override ResponseData<bool> Deactivate()
		{
			var officeResult = DeactivateChilds<Office, int>(_offices);
			if (officeResult.IsFaulty)
			{
				return officeResult;
			}

			return base.Deactivate();
		}

		public ResponseData<Office> GetOffice(int officeId)
		{
			return GetChild(_offices, officeId);
		}

		public ResponseData<Office> AddOffice<TDto>(TDto dto, IEnumerable<(Expression<Func<TDto, object>> Dto, Expression<Func<Office, object>> Domain)> mappings = null)
			where TDto : class
		{
			var createResult = Office.CreateEntity(dto, ValidationEngine, mappings);
			if (createResult.IsFaulty)
			{
				return createResult;
			}

			_offices.Add(createResult.Data);
			if (_officeChanged is null)
			{
				return createResult;
			}

			createResult.Data.SetActionCallback(_officeChanged);
			var actionCallbackResult = _officeChanged(createResult.Data);
			if (actionCallbackResult.IsFaulty)
			{
				return actionCallbackResult.ConvertTo<Office>();
			}

			return createResult;
		}

		protected override bool AreAllChildsValid()
		{
			return _offices.All(p => p.IsValid);
		}

		protected override bool HasChangesInChilds()
		{
			return _offices.Any(p => p.IsChanged);
		}

		private void SetChilds(IEnumerable<Office> offices)
		{
			_offices = offices?.ToList() ?? new List<Office>();
		}

		protected virtual ResponseData<bool> CreatedOrChangedOffice(Office office)
		{
			return Validate();
		}

		protected override void Init()
		{
			_officeChanged = CreatedOrChangedOffice;
		}
	}
}