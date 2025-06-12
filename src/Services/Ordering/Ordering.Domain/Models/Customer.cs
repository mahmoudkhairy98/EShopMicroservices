namespace Ordering.Domain.Models
{
    public class Customer : Entity<CustomerId>
    {
        public string Name { get; private set; } = default!;
        public string Email { get; private set; } = default!;

        //this Create method is a factory methodthat encapsulates the creation of customer instance
        public static Customer Create(CustomerId customerId,string name,string email)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentException.ThrowIfNullOrWhiteSpace(email);

            Customer customer = new Customer
            {
                Id = customerId,
                Name = name,
                Email = email
            };

            return customer;
        }
    }
}
