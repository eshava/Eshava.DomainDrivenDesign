using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Eshava.DomainDrivenDesign.Domain.Models;
using Microsoft.Extensions.Hosting;

namespace Eshava.Example.Application.HostedServices
{
	internal class DomainEventChannelProcessor : BackgroundService
	{
		private readonly Channel<DomainEvent> _domainEventChannel;

		public DomainEventChannelProcessor(
			Channel<DomainEvent> domainEventChannel
		)
		{
			_domainEventChannel = domainEventChannel;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (await _domainEventChannel.Reader.WaitToReadAsync(stoppingToken))
			{
				var domainEvent = await _domainEventChannel.Reader.ReadAsync(stoppingToken);

				if (domainEvent.ProcessNotBeforeUtc.HasValue)
				{
					Console.WriteLine($"{domainEvent.Event}: {domainEvent.EntityId?.ToString()} -> {domainEvent.ProcessNotBeforeUtc.Value:yyyy.MM.dd HH:mm:ss}");
				}
				else
				{
					Console.WriteLine($"{domainEvent.Event}: {domainEvent.EntityId?.ToString()}");
				}
			}
		}
	}
}