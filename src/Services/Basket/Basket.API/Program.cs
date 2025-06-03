
using BuildingBlocks.Exceptions.Handler;

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

//we add this service in order to get formative exception if any exception happens sfter sending the request
builder.Services.AddExceptionHandler<CustomExceptionHandler>();

var app = builder.Build();

//Configure the HTTP request pipeline.

//Carter is a library that simplifies the creation of Minimal API endpoints
//this will scan all ICarterModule interface implementation in this assembly and expose all minimal APIs
app.MapCarter();

app.UseExceptionHandler(opts => { });

app.Run();
