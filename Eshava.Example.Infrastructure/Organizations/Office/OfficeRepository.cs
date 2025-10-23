using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Eshava.Core.Linq.Interfaces;
using Eshava.DomainDrivenDesign.Domain.Extensions;
using Eshava.DomainDrivenDesign.Domain.Models;
using Eshava.DomainDrivenDesign.Infrastructure.Interfaces;
using Eshava.Example.Application.Settings;
using Microsoft.Extensions.Logging;

namespace Eshava.Example.Infrastructure.Organizations.Offices
{
	internal class OfficeRepository : AbstractExampleChildDomainModelRepository<Domain.Organizations.SomeFeature.Office, Customers.CustomerCreationBag, Office, int, ExampleScopedSettings>, IOfficeRepository
	{
		private static readonly List<(Expression<Func<Office, object>> Data, Expression<Func<Domain.Organizations.SomeFeature.Office, object>> Domain)> _officeDataToOfficeDomain = [];
		
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
				CustomerId = creationBag.CustomerId
			};

			return FromDomainModel(instance, model, creationBag);
		}

		protected override string GetPropertyName(Patch<Domain.Organizations.SomeFeature.Office> patch)
		{
			var mapping = _officeDataToOfficeDomain.FirstOrDefault(p => p.Domain.GetMemberExpressionString() == patch.PropertyName);
			if (mapping.Domain is not null)
			{
				return mapping.Data.GetMemberExpressionString();
			}

			return base.GetPropertyName(patch);
		}
	}
}