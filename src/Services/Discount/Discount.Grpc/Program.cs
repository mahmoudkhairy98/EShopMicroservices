using Discount.Grpc.Data;
using Discount.Grpc.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddDbContext<DiscountContext>(options =>
{
    //Since SQLite is an embedded DB, the connection string will be just the datasource of the file path so it will be Datasource=db_name which is discountdb
    //That means SQLite DB located inside our application server so when we give datasource as discount DB, this discountdb SQLlite database will create new database
    //into the root directory of our application and we will see discountdb SQLlite database under the discount.Grpc project file
    options.UseSqlite(builder.Configuration.GetConnectionString("Database"));  
});

var app = builder.Build();

app.UseMigration();

// Configure the HTTP request pipeline.
//By this way, we can make ready this application to accomodate incoming gRPC calls and we will specifically invoke discount proto methods, RPC methods with gRPC calls
//and this will be trigger our DiscountService because we have configured our services in the ASP.Net HTTP request pipeline as mentioned below
app.MapGrpcService<DiscountService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
