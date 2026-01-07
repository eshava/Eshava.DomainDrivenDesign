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

		public Address Address { get; private set; }
		public MetaData MetaData { get; private set; }

		public IReadOnlyList<Office> Offices => _offices.AsReadOnly();
		public override string EventDomain => "organizations";

		public static Customer DataToInstance(IEnumerable<Patch<Customer>> patches, IEnumerable<Office> officeList, IValidationEngine validationEngine)
		{
			var instance = new Customer(validationEngine);
			foreach (var office in officeList)
			{
				office.SetActionCallback(instance._officeChanged);
			}

			instance.SetChilds(officeList);
			instance.ApplyPatches(patches.ToList());
			instance.ClearChanges();

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

		public static ResponseData<Customer> CreateEntity(string name, string street, string streetNumber, string city, string zipCode, string country, Classification classification, IValidationEngine validationEngine)
		{
			var patches = new List<Patch<Customer>>()
			{
				Patch<Customer>.Create(p => p.Name, name),
				Patch<Customer>.Create(p => p.Classification, classification)
			};

			var address = CreateAddressInstance(street, streetNumber, city, zipCode, country);
			if (address is not null)
			{
				patches.Add(Patch<Customer>.Create(p => p.Address, address));
			}

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

			var updateResult = Update(patches);
			if (updateResult.IsFaulty)
			{
				return updateResult;
			}

			IncreaseVersion();

			return updateResult;
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

			IncreaseVersion();

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

		protected override IEnumerable<DomainEvent> GetChildDomainEvents()
		{
			return GetChildDomainEvents<Office, int>(_offices);
		}

		protected override void ClearChildChanges()
		{
			ClearChildChanges<Office, int>(_offices);
		}

		protected override void ClearChildEvents()
		{
			ClearChildEvents<Office, int>(_offices);
		}

		protected virtual ResponseData<bool> CreatedOrChangedOffice(Office office)
		{
			var validationResult = Validate();
			if (validationResult.IsFaulty)
			{
				return validationResult;
			}

			IncreaseVersion();

			return validationResult;
		}

		protected override void Init()
		{
			_officeChanged = CreatedOrChangedOffice;
		}

		protected override ResponseData<bool> Create(IList<Patch<Customer>> patches)
		{
			var metaData = new MetaData(1, [DateTime.UtcNow]);
			patches.Add(Patch<Customer>.Create(p => p.MetaData, metaData));

			return base.Create(patches);
		}

		private static Address CreateAddressInstance(string street, string streetNumber, string city, string zipCode, string country)
		{
			if (street.IsNullOrEmpty()
				&& streetNumber.IsNullOrEmpty()
				&& city.IsNullOrEmpty()
				&& zipCode.IsNullOrEmpty()
				&& country.IsNullOrEmpty())
			{
				// All address parts are not set

				return null;
			}

			return new Address(street, streetNumber, city, zipCode, country);
		}

		private void IncreaseVersion()
		{
			if (IsPropertyChanged(nameof(MetaData)))
			{
				return;
			}

			Domain.Organizations.SomeFeature.MetaData metaData;
			if (MetaData is null)
			{
				metaData = new MetaData(1, [DateTime.UtcNow]);
			}
			else
			{
				var version = MetaData.Version + 1;
				var timestampes = new List<DateTime>(MetaData.Timestamps)
				{
					DateTime.UtcNow
				};

				metaData = new MetaData(version, timestampes);
			}

			ApplyPatches([Patch<Customer>.Create(p => p.MetaData, metaData)]);
		}
	}
}