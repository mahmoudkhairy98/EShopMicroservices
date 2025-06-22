namespace Ordering.Application.Orders.Queries.GetOrdersByCustomer
{
    public class GetOrdersByCustomerHandler(IApplicationDbContext dbContext) : IQueryHandler<GetOrdersByCustomerQuery, GetOrdersByCustomerResult>
    {
        public async Task<GetOrdersByCustomerResult> Handle(GetOrdersByCustomerQuery query, CancellationToken cancellationToken)
        {
            //get orders by customer using dbContext
            //return result

            var orders = await dbContext.Orders
                    .Include(o => o.OrderItems)
                    .AsNoTracking()//this is a good optimization when you are only reading data and don't plan to update these entiries in the same context
                    .Where(o => o.CustomerId == CustomerId.Of(query.CustomerId))
                    .OrderBy(o => o.OrderName.Value)
                    .ToListAsync(cancellationToken);

            return new GetOrdersByCustomerResult(orders.ToOrderDtoList());
        }
    }
}
