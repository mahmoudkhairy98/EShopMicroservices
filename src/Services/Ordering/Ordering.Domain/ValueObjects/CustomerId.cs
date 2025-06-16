namespace Ordering.Domain.ValueObjects
{
    public record CustomerId
    {
        public Guid Value { get; }

        /// <summary>
        //this protected constructor added in order to align with EF
        //Also to avoid the below error while adding the migration 
        //Unable to create a 'DbContext' of type ''. The exception 'No suitable constructor was found for entity type 'CustomerId'. The following constructors had parameters that could not be bound to properties of the entity type: 
        //Cannot bind 'value' in 'CustomerId(Guid value)'
        //Cannot bind 'original' in 'CustomerId(CustomerId original)'
        //Note that only mapped properties can be bound to constructor parameters. Navigations to related entities, including references to owned types, cannot be bound.' was thrown while attempting to create an instance. For the different patterns supported at design time
        /// </summary>
        protected CustomerId() { }
        private CustomerId(Guid value) => Value = value;
        public static CustomerId Of(Guid value)
        {
            ArgumentNullException.ThrowIfNull(value);
            if (value == Guid.Empty)
            {
                throw new DomainException("CustomerId cannot be empty.");
            }

            return new CustomerId(value);
        }
    }
}
