using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;

var builder = WebApplication.CreateBuilder(args);

//Add services to the container.
builder.Services.AddCarter();

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    //This configuration provides that our validation and logging behavior are applied to every mediator request
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("Database")!);
}).UseLightweightSessions();

if (builder.Environment.IsDevelopment())
    builder.Services.InitializeMartenWith<CatalogInitialData>();

builder.Services.AddExceptionHandler<CustomExceptionHandler>();

builder.Services.AddHealthChecks();

var app = builder.Build();

//Configure the HTTP request pipeline.
app.MapCarter();

app.UseExceptionHandler(options => { });

app.UseHealthChecks("/health");

app.Run();
