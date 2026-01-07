using System.Collections.Generic;
using Eshava.Core.Linq.Interfaces;
using Eshava.DomainDrivenDesign.Domain.Models;
using Eshava.DomainDrivenDesign.Infrastructure.Interfaces;
using Eshava.Example.Application.Settings;
using Microsoft.Extensions.Logging;

namespace Eshava.Example.Infrastructure.Organizations.Offices
{
	internal class OfficeRepository : AbstractExampleChildDomainModelRepository<Domain.Organizations.SomeFeature.Office, Customers.CustomerCreationBag, Office, int, ExampleScopedSettings>, IOfficeRepository
	{
		public OfficeRepository(
			IDatabaseSettings databaseSettings,
			ExampleScopedSettings scopedSettings,
			ITransformQueryEngine transformQueryEngine,
			ILogger<OfficeRepository> logger
			) : base(databaseSettings, scopedSettings, transformQueryEngine, logger)
		{
		}

		protected override Office FromDomainModel(Domain.Organizations.SomeFeature.Office model, Customers.CustomerCreationBag creationBag)
		{
			if (model is null)
			{
				return null;
			}

			var instance = new Office
			{
				Name = model.Name,
				CustomerId = creationBag.CustomerId,
			};

			if (model.Address is not null)
			{
				instance.AddressStreet = model.Address.Street;
				instance.AddressStreetNumber = model.Address.StreetNumber;
				instance.AddressCity = model.Address.City;
				instance.AddressZipCode = model.Address.ZipCode;
				instance.AddressCountry = model.Address.Country;
			}

			return FromDomainModel(instance, model, creationBag);
		}


		protected override void MapValueObjects(IEnumerable<Patch<Domain.Organizations.SomeFeature.Office>> patches, IDictionary<string, object> dataModelChanges)
		{
			foreach (var patch in patches)
			{
				if (patch.PropertyName != nameof(Domain.Organizations.SomeFeature.Office.Address))
				{
					continue;
				}

				var address = patch.Value as Domain.Organizations.SomeFeature.Address;
				if (address is null)
				{
					dataModelChanges.Add(nameof(Office.AddressStreet), null);
					dataModelChanges.Add(nameof(Office.AddressStreetNumber), null);
					dataModelChanges.Add(nameof(Office.AddressCity), null);
					dataModelChanges.Add(nameof(Office.AddressZipCode), null);
					dataModelChanges.Add(nameof(Office.AddressCountry), null);
				}
				else
				{
					dataModelChanges.Add(nameof(Office.AddressStreet), address.Street);
					dataModelChanges.Add(nameof(Office.AddressStreetNumber), address.StreetNumber);
					dataModelChanges.Add(nameof(Office.AddressCity), address.City);
					dataModelChanges.Add(nameof(Office.AddressZipCode), address.ZipCode);
					dataModelChanges.Add(nameof(Office.AddressCountry), address.Country);
				}
			}
		}
	}
}