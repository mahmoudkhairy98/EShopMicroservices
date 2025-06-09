namespace Ordering.Domain.ValueObjects
{
    //we are using record here instead of class because it is very good in defining value objects in terms of comparing and removing ID operations
    public record Address
    {
        public string FirstName { get; } = default!;
        public string LsstName { get; } = default!;
        public string? EmailAddress { get; } = default!;
        public string AddressLine { get; } = default!;
        public string Country { get; } = default!;
        public string State { get; } = default!;
        public string ZipCode { get; } = default!;
    }
}
