using Discount.Grpc;
using JasperFx.Events.Daemon;

namespace Basket.API.Basket.StoreBasket
{
    public record StoreBasketCommand(ShoppingCart Cart) : ICommand<StoreBasketResult>;
    public record StoreBasketResult(string UserName);
    public class StoreBasketCommandValidator : AbstractValidator<StoreBasketCommand>
    {
        public StoreBasketCommandValidator()
        {
            RuleFor(x => x.Cart).NotNull().WithMessage("Cart cannot be null");
            RuleFor(x => x.Cart.UserName).NotEmpty().WithMessage("UserName is required");
        }
    }
    public class StoreBasketCommandHandler(IBasketRepository repository,DiscountProtoService.DiscountProtoServiceClient discountProto) 
        : ICommandHandler<StoreBasketCommand, StoreBasketResult>
    {
        public async Task<StoreBasketResult> Handle(StoreBasketCommand command, CancellationToken cancellationToken)
        {
            await DeductDiscount(command.Cart, cancellationToken);

            await repository.StoreBasket(command.Cart, cancellationToken);

            return new StoreBasketResult(command.Cart.UserName);
        }

        public async Task DeductDiscount(ShoppingCart Cart,CancellationToken cancellationToken)
        {
            //TODO: communicate with Discount.Grpc and calculate latest prices of products into basket
            //To communicate with Discount.Grpc, follow the below steps
            //1-Right click on Connected Services under the project
            //2-Choose Manage connected services
            //3-Add New service reference then chosse gRPC
            //4-Choose the proto which is discount.proto in Discount.Grpc project
            //5-Choose Client in the type of class to be generated dropdown
            //The result will be Protos folder will be created under the project and references will be added in the project file

            foreach (var item in Cart.Items)
            {
                var coupon = await discountProto.GetDiscountAsync(new GetDiscountRequest { ProductName = item.ProductName }, cancellationToken: cancellationToken);
                item.Price -= coupon.Amount;
            }
        }
    }
}
