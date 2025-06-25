namespace BuildingBlocks.Messaging.Events
{
    public record IntegrationEvent
    {
        public Guid Id => new Guid();
        public DateTime OccurredOn => DateTime.Now;
        public string EventType => GetType().AssemblyQualifiedName;    
    }
}
