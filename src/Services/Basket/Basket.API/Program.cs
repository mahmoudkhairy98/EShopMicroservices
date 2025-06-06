
using BuildingBlocks.Exceptions.Handler;
using Discount.Grpc;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

//Add services to the DI container.

//Application Services
var assembly = typeof(Program).Assembly;

//By this way we can add Carter related classes into our container
builder.Services.AddCarter();

builder.Services.AddMediatR(config =>
{
    //this to activate the mediatr in this assemlby as it is installed in buildingblocks project
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});


//Data Services
//UseLightweightSessions is used to optimize performance
builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("Database")!);
    opts.Schema.For<ShoppingCart>().Identity(x => x.UserName);
}).UseLightweightSessions();

builder.Services.AddScoped<IBasketRepository, BasketRepository>();
//Here if we need to register 2 repository to IBasketRepository, it is not feasible to directly register multiple implementation to IBasketRepository and it makes
//problem with direct DI and if we added this line builder.Services.AddScoped<IBasketRepository, CachedBasketRepository>(); , it will take the last registration which is 
//CachedBasketRepository in this case and ignore the first registration BasketRepository so what is the solution?
//The first solution that we can manullay decorate CachedBasketRepository as shown below
//builder.Services.AddScoped<IBasketRepository>(provider =>
//{
//    var basketRepository = provider.GetRequiredService<BasketRepository>();
//    return new CachedBasketRepository(basketRepository, provider.GetRequiredService<IDistributedCache>());
//}); but as suugested this solution is not scalable and not managable and insted of that we should use scrutor library
//Scrutor library is a library that simplifies the registration of decorators in DI container
//Now, when the request sent, the handle method will call CachedBasketRepository first to check if there is data stored in the cache, if there is no data, it will call the DB
//through BasketRepository and the purpose of this is to reduce calls to the DB to increase the performance otherwise we can use BasketRepository only without using CachedBasketRepository
builder.Services.Decorate<IBasketRepository, CachedBasketRepository>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

//Grpc Services
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]!);
})
//this extension method should be added if we receive the below exception
//IMP***this extension can be used in development and not in production due to security risks
//{
//    "title": "RpcException",
//    "status": 500,
//    "detail": "Status(StatusCode=\"Internal\", Detail=\"Error starting gRPC call. HttpRequestException: The SSL connection could not be established, see inner exception. AuthenticationException: The remote certificate is invalid according to the validation procedure: RemoteCertificateNameMismatch, RemoteCertificateChainErrors\", DebugException=\"System.Net.Http.HttpRequestException: The SSL connection could not be established, see inner exception.\n ---> System.Security.Authentication.AuthenticationException: The remote certificate is invalid according to the validation procedure: RemoteCertificateNameMismatch, RemoteCertificateChainErrors\n   at System.Net.Security.SslStream.SendAuthResetSignal(ReadOnlySpan`1 alert, ExceptionDispatchInfo exception)\n   at System.Net.Security.SslStream.CompleteHandshake(SslAuthenticationOptions sslAuthenticationOptions)\n   at System.Net.Security.SslStream.ForceAuthenticationAsync[TIOAdapter](Boolean receiveFirst, Byte[] reAuthenticationData, CancellationToken cancellationToken)\n   at System.Net.Http.ConnectHelper.EstablishSslConnectionAsync(SslClientAuthenticationOptions sslOptions, HttpRequestMessage request, Boolean async, Stream stream, CancellationToken cancellationToken)\n   --- End of inner exception stack trace ---\n   at System.Net.Http.ConnectHelper.EstablishSslConnectionAsync(SslClientAuthenticationOptions sslOptions, HttpRequestMessage request, Boolean async, Stream stream, CancellationToken cancellationToken)\n   at System.Net.Http.HttpConnectionPool.ConnectAsync(HttpRequestMessage request, Boolean async, CancellationToken cancellationToken)\n   at System.Net.Http.HttpConnectionPool.AddHttp2ConnectionAsync(QueueItem queueItem)\n   at System.Threading.Tasks.TaskCompletionSourceWithCancellation`1.WaitWithCancellationAsync(CancellationToken cancellationToken)\n   at System.Net.Http.HttpConnectionPool.SendWithVersionDetectionAndRetryAsync(HttpRequestMessage request, Boolean async, Boolean doRequestAuth, CancellationToken cancellationToken)\n   at System.Net.Http.DiagnosticsHandler.SendAsyncCore(HttpRequestMessage request, Boolean async, CancellationToken cancellationToken)\n   at System.Net.Http.RedirectHandler.SendAsync(HttpRequestMessage request, Boolean async, CancellationToken cancellationToken)\n   at Microsoft.Extensions.Http.Logging.LoggingHttpMessageHandler.<SendCoreAsync>g__Core|5_0(HttpRequestMessage request, Boolean useAsync, CancellationToken cancellationToken)\n   at Microsoft.Extensions.Http.Logging.LoggingScopeHttpMessageHandler.<SendCoreAsync>g__Core|5_0(HttpRequestMessage request, Boolean useAsync, CancellationToken cancellationToken)\n   at Grpc.Net.Client.Balancer.Internal.BalancerHttpHandler.SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)\n   at Grpc.Net.Client.Internal.GrpcCall`2.RunCall(HttpRequestMessage request, Nullable`1 timeout)\")",
//    "instance": "/basket"
//}
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
    return handler;
});

//Cross-Cutting Services 
//we add this service in order to get formative exception if any exception happens sfter sending the request
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

//Add helath checks with its extension methods to check the health as this API is relying on postgres and redis
builder.Services.AddHealthChecks()
                .AddNpgSql(builder.Configuration.GetConnectionString("Database")!)
                .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

var app = builder.Build();

//Configure the HTTP request pipeline.

//Carter is a library that simplifies the creation of Minimal API endpoints
//this will scan all ICarterModule interface implementation in this assembly and expose all minimal APIs
app.MapCarter();

app.UseExceptionHandler(opts => { });

//this is the basic configuration of health check and the response will be "Healthy" if the API and iys backing services like DB is up & running
//app.UseHealthChecks("/health");
//If we need to make our health endpoint response as a json response including all entries, we have to install this package AspNetCore.HealthChecks.UI.Client and add the below
app.UseHealthChecks("/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.Run();
