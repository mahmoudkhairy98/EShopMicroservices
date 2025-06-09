namespace Ordering.Domain.Models
{
    public class Order : Aggregate<Guid>
    {
        private readonly List<OrderItem> _orderItems = new();
        //public IReadOnlyList<OrderItem> OrderItems { get { return _orderItems.AsReadOnly(); } }
        public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();//same syntax like above but in different way by using Expression Bodied Property
        public Guid CustomerId { get; private set; } = default!;
        public Guid OrderId { get; private set; } = default!;
        public Address ShippingAddress { get; private set; } = default!;
        public Address BillingAddress { get; private set; } = default!;
        public Payment Payment { get; private set; } = default!;
        public OrderStatus Status { get; private set; } = OrderStatus.Pending;
        public decimal TotalPrice
        {
            get
            {
                return OrderItems.Sum(x => x.Price * x.Quantity);
            }
            set { }
        }
    }
}
