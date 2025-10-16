﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.Core.Validation.Interfaces;
using Eshava.DomainDrivenDesign.Domain.Attributes;
using Eshava.DomainDrivenDesign.Domain.Constants;
using Eshava.DomainDrivenDesign.Domain.Enums;
using Eshava.DomainDrivenDesign.Domain.Extensions;
using Eshava.DomainDrivenDesign.Domain.Interfaces;

namespace Eshava.DomainDrivenDesign.Domain.Models
{
	public abstract class AbstractEntity<TDomain, TIdentifier> : IEntity<TDomain, TIdentifier>
		where TDomain : class, IEntity<TDomain, TIdentifier>
		where TIdentifier : struct
	{
		private bool _isValid = false;
		private Dictionary<string, Patch<TDomain>> _changes = [];
		private static ConcurrentDictionary<Type, ImmutableHashSet<string>> _autoPatchBlocked = new();

		protected AbstractEntity(IValidationEngine validationEngine)
		{
			SetValidationStatus(true);
			Init(typeof(TDomain));
			ValidationEngine = validationEngine;
		}

		[AutoPatchBlocked]
		public TIdentifier? Id { get; protected set; }

		[AutoPatchBlocked]
		public Status Status { get; protected set; }

		[AutoPatchBlocked]
		public virtual bool IsValid => _isValid;

		[AutoPatchBlocked]
		public virtual bool IsChanged => _changes.Count > 0;

		public static ImmutableHashSet<string> AutoPatchBlocked
		{
			get
			{
				var type = typeof(TDomain);

				Init(type);

				_autoPatchBlocked.TryGetValue(type, out var autoPatchBlocked);

				return autoPatchBlocked;
			}
		}

		protected IValidationEngine ValidationEngine { get; private set; }

		public ResponseData<bool> SetIdentifier(TIdentifier identifier)
		{
			if (Id.HasValue && !Equals(Id.Value, default))
			{
				return MessageConstants.IMMUTABLEERROR.ToFaultyResponse<bool>()
					.AddValidationError(nameof(Id), MessageConstants.ALREADYEXISTING);
			}

			Id = identifier;

			return true.ToResponseData();
		}

		public void SetUnchanged()
		{
			_changes.Clear();
		}

		public IReadOnlyList<Patch<TDomain>> GetChanges()
		{
			return _changes.Values.ToList().AsReadOnly();
		}

		public virtual ResponseData<bool> Deactivate()
		{
			var patches = new List<Patch<TDomain>>
			{
				 Patch<TDomain>.Create(p => p.Status, Status.Inactive)
			};

			var applyResponse = ApplyPatches(patches);
			if (applyResponse.IsFaulty)
			{
				return applyResponse;
			}

			return Validate();
		}

		public bool IsPropertyChanged(string propertyName)
		{
			return _changes.ContainsKey(propertyName);
		}

		protected virtual Task<ResponseData<bool>> IsStorableAsync()
		{
			if (!IsValid)
			{
				return MessageConstants.INVALIDDATAERROR.ToFaultyResponse<bool>().ToTask();
			}

			if (!IsChanged)
			{
				return MessageConstants.NOCHANGESERROR.ToFaultyResponse<bool>().ToTask();
			}

			return true.ToResponseDataAsync();
		}

		protected virtual ResponseData<bool> Update(IList<Patch<TDomain>> patches)
		{
			var applyResponse = ApplyPatches(patches);
			if (applyResponse.IsFaulty)
			{
				return applyResponse;
			}

			ExecuteAfterAction();

			return Validate();
		}

		protected virtual ResponseData<bool> Create(IList<Patch<TDomain>> patches)
		{
			patches.Add(Patch<TDomain>.Create(p => p.Status, Status.Active));

			var applyResponse = ApplyPatches(patches);
			if (applyResponse.IsFaulty)
			{
				return applyResponse;
			}

			ExecuteAfterAction();

			return Validate();
		}

		protected ResponseData<bool> AreAllPatchesAllowed(IList<Patch<TDomain>> patches)
		{
			_autoPatchBlocked.TryGetValue(typeof(TDomain), out var autoPatchBlocked);

			var forbiddenPatches = patches
				.Where(p => autoPatchBlocked.Contains(p.PropertyName))
				.Select(p => new ValidationError { PropertyName = p.PropertyName, ErrorType = MessageConstants.AUTOPATCHBLOCKED })
				.ToList();

			if (forbiddenPatches.Count > 0)
			{
				return ResponseData<bool>.CreateInvalidDataResponse()
					.AddValidationErrors(forbiddenPatches);
			}

			return true.ToResponseData();
		}

		protected ResponseData<bool> ApplyPatches(IList<Patch<TDomain>> patches)
		{
			var allSet = true;
			var validationErrors = new List<ValidationError>();
			var appliesPatches = new List<Patch<TDomain>>();

			foreach (var patch in patches)
			{
				var result = patch.Property.SetPropertyValue(this, patch.Value);
				allSet &= !result.IsFaulty;

				if (result.IsFaulty)
				{
					validationErrors.Add(new ValidationError
					{
						PropertyName = patch.PropertyName,
						ErrorType = result.GetRawMessage(),
						MethodType = nameof(ApplyPatches)
					});
				}

				if (result.Data)
				{
					appliesPatches.Add(patch);
				}
			}

			if (!allSet)
			{
				SetValidationStatus(false);

				return ResponseData<bool>.CreateInvalidDataResponse()
					.AddValidationErrors(validationErrors);
			}

			appliesPatches.ForEach(SetChange);

			return true.ToResponseData();
		}

		protected virtual ResponseData<bool> Validate()
		{
			var validationResult = ValidationEngine.Validate(this);
			SetValidationStatus(validationResult.IsValid);

			if (!validationResult.IsValid)
			{
				return ResponseData<bool>.CreateInvalidDataResponse()
					.AddValidationErrors(validationResult.ValidationErrors);
			}

			return true.ToResponseData();
		}

		protected virtual void ExecuteAfterAction()
		{

		}

		protected void SetValidationStatus(bool isValid)
		{
			_isValid = isValid;
		}

		protected void SetChange<TPropertyType>(Expression<Func<TDomain, TPropertyType>> property, TPropertyType value)
		{
			SetChange(Patch<TDomain>.Create(property, value));
		}

		protected void SetChange(Patch<TDomain> patch)
		{
			if (!_changes.TryAdd(patch.PropertyName, patch))
			{
				_changes[patch.PropertyName] = patch;
			}
		}

		private static void Init(Type type)
		{
			if (_autoPatchBlocked.ContainsKey(type))
			{
				return;
			}

			var autoPatchBlocked = new HashSet<string>();
			var propertyInfos = type.GetProperties();
			foreach (var propertyInfo in propertyInfos)
			{
				var attribute = propertyInfo.GetCustomAttribute<AutoPatchBlockedAttribute>();
				if (attribute is null)
				{
					continue;
				}

				autoPatchBlocked.Add(propertyInfo.Name);
			}

			_autoPatchBlocked.TryAdd(type, autoPatchBlocked.ToImmutableHashSet());
		}
	}
}