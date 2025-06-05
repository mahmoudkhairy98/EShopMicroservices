
using BuildingBlocks.Exceptions.Handler;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

//Add services to the DI container.

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
