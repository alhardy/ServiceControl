namespace ServiceControlInstaller.Engine.Instances
{
    public static class TransportNames
    {
        public const string AzureServiceBus = "Azure Service Bus";

        public const string AzureServiceBusEndpointOrientedTopology = "Azure Service Bus - Endpoint-oriented topology (Legacy)";
        // for backward compatibility
        public const string AzureServiceBusEndpointOrientedTopologyOld = "Azure Service Bus - Endpoint-oriented topology (Old)";
        
        public const string AzureServiceBusForwardingTopology = "Azure Service Bus - Forwarding topology (Legacy)";
        // for backward compatibility
        public const string AzureServiceBusForwardingTopologyOld = "Azure Service Bus - Forwarding topology (Old)";

        public const string AmazonSQS = "AmazonSQS";

        public const string AzureStorageQueue = "Azure Storage Queue";

        public const string MSMQ = "MSMQ";

        public const string SQLServer = "SQL Server";

        public const string RabbitMQConventionalRoutingTopology = "RabbitMQ - Conventional routing topology";

        public const string RabbitMQDirectRoutingTopology = "RabbitMQ - Direct routing topology (Old)";
    }
}