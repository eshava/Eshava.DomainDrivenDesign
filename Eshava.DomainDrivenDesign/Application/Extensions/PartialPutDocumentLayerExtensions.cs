using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.DomainDrivenDesign.Application.Dtos;
using Eshava.DomainDrivenDesign.Application.PartialPut;
using Eshava.DomainDrivenDesign.Domain.Attributes;
using Eshava.DomainDrivenDesign.Domain.Extensions;
using Eshava.DomainDrivenDesign.Domain.Models;

namespace Eshava.DomainDrivenDesign.Application.Extensions
{
	public static class PartialPutDocumentLayerExtensions
	{
		public static ResponseData<IList<Patch<Target>>> GetPatchInformation<Source, Target>(this PartialPutDocumentLayer documentLayer, IEnumerable<(Expression<Func<Source, object>> Source, Expression<Func<Target, object>> Target)> mappings = null)
			where Source : class
			where Target : class
		{
			var patches = new List<Patch<Target>>();
			var targetType = typeof(Target);

			CollectReplaceOperations(targetType, documentLayer, patches, mappings);
			
			return patches.ToIListResponseData();
		}

		public static ResponseData<EnumerablePatchDataDto<TIdentifier, TargetDto, TargetDomain>> GetPatchInformation<Source, TargetDto, TargetDomain, TIdentifier>(
			this PartialPutDocumentLayer documentLayer,
			Expression<Func<Source, object>> enumerableProperty,
			IEnumerable<(Expression<Func<TargetDto, object>> Source, Expression<Func<TargetDomain, object>> Target)> mappings = null
		)
			where Source : class
			where TargetDto : class
			where TargetDomain : class
			where TIdentifier : struct
		{
			var data = new EnumerablePatchDataDto<TIdentifier, TargetDto, TargetDomain>();
			var validationErrors = new List<ValidationError>();
			var enumerablePropertyName = enumerableProperty.GetMemberExpressionString();

			foreach (var operation in documentLayer.Operations)
			{
				if (operation.Type == PartialPutOperationType.Replace || operation.PropertyName != enumerablePropertyName)
				{
					continue;
				}

				if (operation.Type == PartialPutOperationType.Add)
				{
					data.ItemsToAdd.Add((TargetDto)operation.Value);

					continue;
				}

				if (operation.Type == PartialPutOperationType.Remove)
				{
					data.ItemsToRemove.Add((TIdentifier)operation.Value);

					continue;
				}
			}

			CollectReplaceOperations(null, documentLayer.Layers.Where(l => l.PropertyName == enumerablePropertyName), data, mappings, validationErrors);

			if (validationErrors.Count > 0)
			{
				return ResponseData<EnumerablePatchDataDto<TIdentifier, TargetDto, TargetDomain>>.CreateInvalidDataResponse()
					.AddValidationErrors(validationErrors);
			}

			return data.ToResponseData();
		}

		public static PartialPutDocumentLayer GetLayerForIdentifier<Source, TIdentifier>(
			this PartialPutDocumentLayer documentLayer,
			Expression<Func<Source, object>> enumerableProperty,
			TIdentifier identifier
		)
			where Source : class
			where TIdentifier : struct
		{
			var enumerablePropertyName = enumerableProperty.GetMemberExpressionString();

			return documentLayer.Layers.FirstOrDefault(layer => layer.PropertyName == enumerablePropertyName && Equals(((TIdentifier)layer.Id), identifier));
		}

		private static void CollectReplaceOperations<Source, Target>(
			Type targetType,
			PartialPutDocumentLayer documentLayer,
			List<Patch<Target>> patches,
			IEnumerable<(Expression<Func<Source, object>> Source, Expression<Func<Target, object>> Target)> mappings
		)
			where Source : class
			where Target : class
		{
			foreach (var operation in documentLayer.Operations.Where(p => p.Type == PartialPutOperationType.Replace))
			{
				if (mappings is not null)
				{
					var mapping = mappings.FirstOrDefault(m => m.Source?.GetMemberExpressionString() == operation.PropertyName);
					if (mapping.Target is null)
					{
						mapping = mappings.FirstOrDefault(m => m.Source?.GetMemberExpressionString() == $"{documentLayer.PropertyName}.{operation.PropertyName}");
					}

					if (mapping.Target is not null)
					{
						patches.Add(Patch<Target>.Create(mapping.Target, operation.Value));

						continue;
					}
				}

				var propertyInfo = targetType.GetProperty(operation.PropertyName);
				if (propertyInfo is null)
				{
					continue;
				}

				var autoPatchBlocked = propertyInfo.GetCustomAttribute<AutoPatchBlockedAttribute>();
				if (autoPatchBlocked is not null)
				{
					continue;
				}

				patches.Add(propertyInfo.ToPatch<Target, object>(operation.Value));
			}

			// Check for value objects
			foreach (var layer in documentLayer.Layers.Where(l => l.Id is null))
			{
				CollectReplaceOperations(targetType, layer , patches, mappings);
			}
		}

		private static void CollectReplaceOperations<TIdentifier, TargetDto, TargetDomain>(
			TIdentifier? parentLayerId,
			IEnumerable<PartialPutDocumentLayer> layers,
			EnumerablePatchDataDto<TIdentifier, TargetDto, TargetDomain> data,
			IEnumerable<(Expression<Func<TargetDto, object>> Source, Expression<Func<TargetDomain, object>> Target)> mappings,
			List<ValidationError> validationErrors
		)
			where TargetDto : class
			where TargetDomain : class
			where TIdentifier : struct
		{
			foreach (var layer in layers)
			{
				var layerPatches = new List<Patch<TargetDomain>>();
				var layerResult = layer.GetPatchInformation(mappings);
				if (layerResult.IsFaulty)
				{
					validationErrors.AddRange(layerResult.ValidationErrors);

					continue;
				}

				layerPatches.AddRange(layerResult.Data);
				var layerIdentifier = parentLayerId ?? (TIdentifier)layer.Id;

				if (!data.ItemsToPatch.TryGetValue(layerIdentifier, out var existingPatches))
				{
					existingPatches = [];
					data.ItemsToPatch.Add(layerIdentifier, existingPatches);
				}

				foreach (var layerPatch in layerPatches)
				{
					existingPatches.Add(layerPatch);
				}

				// Check for value object changes
				if (layer.Layers.Count > 0)
				{
					CollectReplaceOperations((TIdentifier)layer.Id, layer.Layers, data, mappings, validationErrors);
				}
			}
		}
	}
}