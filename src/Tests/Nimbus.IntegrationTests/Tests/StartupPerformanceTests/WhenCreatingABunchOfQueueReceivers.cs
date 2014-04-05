using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Nimbus.Configuration.Settings;
using Nimbus.Extensions;
using Nimbus.Infrastructure;
using Nimbus.Logger;
using NUnit.Framework;
using Shouldly;

namespace Nimbus.IntegrationTests.Tests.StartupPerformanceTests
{
    [TestFixture]
    public class WhenCreatingABunchOfQueueReceivers
    {
        private int _numMessagesReceived;

        [Test]
        [Timeout(60*1000)]
        public async Task ItShouldntTakeTooLong()
        {
            const int numQueues = 100;
            const int numMessages = numQueues;

            var logger = new ConsoleLogger();

            Func<NamespaceManager> namespaceManagerFunc = delegate
                                                          {
                                                              logger.Debug("Constructing NamespaceManager...");
                                                              return NamespaceManager.CreateFromConnectionString(CommonResources.ConnectionString);
                                                          };
            Func<MessagingFactory> messagingFactoryFunc = delegate
                                                          {
                                                              logger.Debug("Constructing MessagingFactory...");
                                                              return MessagingFactory.CreateFromConnectionString(CommonResources.ConnectionString);
                                                          };

            var namespaceManager = namespaceManagerFunc();
            var messagingFactory = messagingFactoryFunc();

            var queueManager = new AzureQueueManager(
                () => namespaceManager,
                () => messagingFactory,
                new MaxDeliveryAttemptSetting(),
                logger,
                new DefaultMessageLockDurationSetting());

            logger.Debug("Ensuring that all queues exist before we start the clock...");
            Enumerable.Range(0, numQueues)
                      .AsParallel()
                      .WithDegreeOfParallelism(numQueues)
                      .Select(CreateQueuePath)
                      .Do(queueManager.EnsureQueueExists)
                      .Done();

            logger.Debug("Pre-populating each queue with a command so that they can get picked up by the message pumps...");
            Enumerable.Range(0, numQueues)
                      .AsParallel()
                      .Do(i => SendMessage(i, queueManager))
                      .Done();

            using (var factory = new NimbusMessagingFactory(queueManager, new ConcurrentHandlerLimitSetting()))
            {
                _numMessagesReceived = 0;

                logger.Info("Starting the clock!");
                var sw = Stopwatch.StartNew();
                Enumerable.Range(0, numQueues)
                          .AsParallel()
                          .WithDegreeOfParallelism(numQueues)
                          .Select(CreateQueuePath)
                          .Do(queuePath => StartReceiver(queuePath, factory, logger))
                          .Done();

                logger.Info("All QueueReceiver instances are running. Waiting for messages to be received...");

                while (_numMessagesReceived < numMessages)
                {
                    Thread.Sleep(10);
                    if (sw.Elapsed > TimeSpan.FromSeconds(30)) throw new TimeoutException();
                }

                sw.Stop();
                logger.Info("Total elapsed time to spin up queue receivers and retrieve all waiting messages: {0}", sw.Elapsed.ToString());

                sw.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(30));
            }
        }

        private string CreateQueuePath(int queueNumber)
        {
            var queuePath = string.Format("DummyQueue{0:000}", queueNumber).ToLowerInvariant();
            return queuePath;
        }

        private void StartReceiver(string queuePath, NimbusMessagingFactory factory, ILogger logger)
        {
            var receiver = factory.GetQueueReceiver(queuePath);

            logger.Debug("Starting receiver for queue {0}", queuePath);
            receiver.Start(msg => Task.Run(delegate
                                           {
                                               Interlocked.Increment(ref _numMessagesReceived);
                                               logger.Debug("Received message on queue {0}", queuePath);
                                           }));
            logger.Debug("Started receiver for queue {0}", queuePath);
        }

        private void SendMessage(int i, AzureQueueManager queueManager)
        {
            var queuePath = string.Format("DummyQueue{0:000}", i);
            var sender = queueManager.CreateMessageSender(queuePath);
            sender.Send(new BrokeredMessage());
        }
    }
}