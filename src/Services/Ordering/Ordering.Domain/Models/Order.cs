namespace Ordering.Domain.Models
{
    public class Order : Aggregate<OrderId>
    {
        private readonly List<OrderItem> _orderItems = new();
        //public IReadOnlyList<OrderItem> OrderItems { get { return _orderItems.AsReadOnly(); } }
        public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();//same syntax like above but in different way by using Expression Bodied Property
        public CustomerId CustomerId { get; private set; } = default!;
        public OrderName OrderName { get; private set; } = default!;
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

        public static Order Create(OrderId orderId,CustomerId customerId,OrderName orderName,Address shippingAddress,Address billingAddress,Payment payment)
        {
            //We will not apply here any domain rules or validations because all the parameters are value objects and already we have applied all rules & validations inside
            var order = new Order
            {
                Id = orderId,
                CustomerId = customerId,
                OrderName = orderName,
                ShippingAddress = shippingAddress,
                BillingAddress = billingAddress,
                Payment = payment,
                Status = OrderStatus.Pending
            };

            order.AddDomainEvent(new OrderCreatedEvent(order));

            return order;
        }

        public void Update(OrderName orderName,Address shippingAddress,Address billingAddress,Payment payment,OrderStatus status)
        {
            OrderName = orderName;
            ShippingAddress = shippingAddress;
            BillingAddress = billingAddress;
            Payment = payment;
            Status = status;

            AddDomainEvent(new OrderUpdatedEvent(this));
        }

        //This method is responsible for adding new item into the order aggregate
        public void Add(ProductId productId,int quantity,decimal price)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);

            var orderItem = new OrderItem(Id, productId, quantity, price);
            _orderItems.Add(orderItem);
        }

        public void Remove(ProductId productId)
        {
            var orderItem = _orderItems.FirstOrDefault(x => x.ProductId == productId);
            if(orderItem is not null)
            {
                _orderItems.Remove(orderItem);
            }
        }
    }
}
