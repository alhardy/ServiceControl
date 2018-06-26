﻿using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.AcceptanceTesting.Support;
using TestConventions = NServiceBus.AcceptanceTesting.Customization.Conventions;
using NServiceBus.Configuration.AdvancedExtensibility;
using ServiceBus.Management.AcceptanceTests;

public class ConfigureEndpointAzureServiceBusTransport : ITransportIntegration
{
    public string Name => "AzureServiceBus";

    public string TypeName => "NServiceBus.AzureServiceBusTransport, NServiceBus.Azure.Transports.WindowsAzureServiceBus";

    public string ConnectionString { get; set; }
    
    public Task Configure(string endpointName, EndpointConfiguration configuration, RunSettings settings, PublisherMetadata publisherMetadata)
    {
        configuration.UseSerialization<NewtonsoftSerializer>();

        var topology = Environment.GetEnvironmentVariable("AzureServiceBusTransport.Topology");

        if (!string.IsNullOrEmpty(topology))
        {
            configuration.GetSettings().Set("AzureServiceBus.AcceptanceTests.UsedTopology", topology);
        }

        var transportConfig = configuration.UseTransport<AzureServiceBusTransport>();

        transportConfig.ConnectionString(ConnectionString);

        if (topology == "ForwardingTopology")
        {
            transportConfig.UseForwardingTopology();
        }
        else
        {
            var endpointOrientedTopology = transportConfig.UseEndpointOrientedTopology();
            foreach (var publisher in publisherMetadata.Publishers)
            {
                foreach (var eventType in publisher.Events)
                {
                    endpointOrientedTopology.RegisterPublisher(eventType, publisher.PublisherName);
                }
            }
        }

        transportConfig.Sanitization().UseStrategy<ValidateAndHashIfNeeded>();

        // w/o retries ASB will move attempted messages to the error queue right away, which will cause false failure.
        // ScenarioRunner.PerformScenarios() verifies by default no messages are moved into error queue. If it finds any, it fails the test.
        configuration.Recoverability().Immediate(retriesSettings => retriesSettings.NumberOfRetries(3));

        return Task.FromResult(0);
    }

    public Task Cleanup()
    {
        return Task.FromResult(0);
    }
}