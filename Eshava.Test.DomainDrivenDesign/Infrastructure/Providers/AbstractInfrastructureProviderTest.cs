using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eshava.Core.Extensions;
using Eshava.Core.Models;
using Eshava.Core.Validation.Interfaces;
using Eshava.DomainDrivenDesign.Domain.Enums;
using Eshava.DomainDrivenDesign.Domain.Models;
using Eshava.DomainDrivenDesign.Infrastructure.Interfaces;
using Eshava.DomainDrivenDesign.Infrastructure.Interfaces.Repositories;
using Eshava.DomainDrivenDesign.Infrastructure.Providers;
using FakeItEasy;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Eshava.Test.DomainDrivenDesign.Infrastructure.Providers
{
	[TestClass, TestCategory("Infrastructure")]
	public class AbstractInfrastructureProviderTest
	{
		private IAbstractDomainModelRepository<Order, int> _repository;
		private OrderProvider _provider;

		[TestInitialize]
		public void Setup()
		{
			_repository = A.Fake<IAbstractDomainModelRepository<Order, int>>();
			A.CallTo(() => _repository.UpdateAsync(A<Order>._)).Returns(true.ToResponseData());
			_provider = new OrderProvider(A.Fake<IDatabaseSettings>(), _repository);
		}

		[TestMethod]
		public async Task RootIsNotUpdatedWhenOnlyChildsChangedTest()
		{
			// Arrange
			var order = Order.Persisted();
			order.ChangeChild();

			// Act
			var result = await _provider.SaveAsync(order);

			// Assert
			result.IsFaulty.Should().BeFalse();
			A.CallTo(() => _repository.UpdateAsync(A<Order>._)).MustNotHaveHappened();
			_provider.SavedChilds.Should().Be(1);
		}

		[TestMethod]
		public async Task RootIsUpdatedWhenItChangedItselfTest()
		{
			// Arrange
			var order = Order.Persisted();
			order.Rename("Changed");
			order.ChangeChild();

			// Act
			var result = await _provider.SaveAsync(order);

			// Assert
			result.IsFaulty.Should().BeFalse();
			A.CallTo(() => _repository.UpdateAsync(order)).MustHaveHappenedOnceExactly();
			_provider.SavedChilds.Should().Be(1);
		}

		public class Order : AbstractAggregate<Order, int>
		{
			private bool _isChildChanged;

			private Order(IValidationEngine validationEngine) : base(validationEngine)
			{
			}

			public string Name { get; private set; }
			public override string EventDomain => "test";
			protected override bool CreateDomainEvents => false;

			public static Order Persisted()
			{
				var order = new Order(A.Fake<IValidationEngine>());
				order.Status = Status.Active;
				order.SetIdentifier(7);

				return order;
			}

			public void Rename(string name)
			{
				Update([Patch<Order>.Create(order => order.Name, name)]);
			}

			public void ChangeChild()
			{
				_isChildChanged = true;
			}

			protected override IEnumerable<DomainEvent> GetChildDomainEvents() => Enumerable.Empty<DomainEvent>();
			protected override void ClearChildEvents() { }
			protected override void ClearChildChanges() => _isChildChanged = false;
			protected override bool AreAllChildsValid() => true;
			protected override bool HasChangesInChilds() => _isChildChanged;
		}

		private class OrderProvider : AbstractAggregateInfrastructureProvider<Order, int>
		{
			public OrderProvider(IDatabaseSettings databaseSettings, IAbstractDomainModelRepository<Order, int> repository) : base(databaseSettings, repository)
			{
			}

			public override bool UseTransaction => false;
			public int SavedChilds { get; private set; }

			protected override Task<ResponseData<bool>> ExcecuteCompletionActionsForUpdateAsync(Order entity)
			{
				SavedChilds++;

				return true.ToResponseDataAsync();
			}
		}
	}
}