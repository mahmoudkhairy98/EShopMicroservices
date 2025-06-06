using Discount.Grpc.Data;
using Discount.Grpc.Models;
using Grpc.Core;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Discount.Grpc.Services
{
    //DiscountProtoService is generated from Visual Studio when building our application
    //If you right click and go to definition, you can see the service class and all related operations under the service class
    //you can find all the CRUD operations, you can go to the method call and examine the generated codes
    //The idea of this class that it is generated from Visual Studio and exposing create, update, delete and get operations as a gRPC methods to the outside of the world
    //so this means that this generated class you can think that this is the presentation layer and in order to hook up the gRPC call we will inherit our
    //discount service from the generated class and we will implement the actual business logic into our service class
    //so this class DiscountService will be our application business logic layer that we should implement our use cases which is the CRUD operations of the coupon model
    //And we will override these base methods from DiscountProtoService base class in order to implement our specific use case for perforing CRUD operations on discount
    public class DiscountService(DiscountContext dbContext,ILogger<DiscountService> logger) 
        : DiscountProtoService.DiscountProtoServiceBase
    {
        public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
        {
            var coupon = await dbContext.Coupons.FirstOrDefaultAsync(x => x.ProductName == request.ProductName);

            if (coupon is null)
                coupon = new Coupon { ProductName = "No Discount", Description = "No Discount Desc", Amount = 0 };

            logger.LogInformation("Discount is retrieved for ProductName : {ProductName},Amount : {Amount}", coupon.ProductName, coupon.Amount);

            var couponModel = coupon.Adapt<CouponModel>();
            return couponModel;
        }

        public override async Task<CouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext context)
        {
            var coupon = request.Coupon.Adapt<Coupon>();

            if (coupon is null)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request object!"));

            dbContext.Coupons.Add(coupon);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Discount has been created successfully for Product Name: {ProductName}", request.Coupon.ProductName);

            var couponModel = coupon.Adapt<CouponModel>();
            return couponModel;
        }

        public override async Task<CouponModel> UpdateDiscount(UpdateDiscountRequest request, ServerCallContext context)
        {
            var coupon = request.Coupon.Adapt<Coupon>();

            if (coupon is null)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid request object!"));

            dbContext.Coupons.Update(coupon);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Discount has been updated successfully for Product Name: {ProductName}", request.Coupon.ProductName);

            var couponModel = coupon.Adapt<CouponModel>();
            return couponModel;
        }

        public override async Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request, ServerCallContext context)
        {
            var coupon = await dbContext.Coupons.FirstOrDefaultAsync(x => x.ProductName == request.ProductName);

            if (coupon is null)
                throw new RpcException(new Status(StatusCode.NotFound, $"Discount for product name : {request.ProductName} is not found!"));

            dbContext.Coupons.Remove(coupon);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Discount has been deleted successfully for Product Name: {ProductName}", request.ProductName);

            return new DeleteDiscountResponse { Success = true };
        }
    }
}
