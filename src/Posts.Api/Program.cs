using Posts.Api.Extensions;
using Posts.Infrastructure;
using Posts.Application;
using Posts.Infrastructure.Core.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

var connectionString = builder.Configuration.GetConnectionString("PostsDatabase")
    ?? throw new ArgumentNullException("ConnectionString is not provided");

var jwtOptions = builder.Configuration.GetSection("JwtOptions").Get<JwtOptions>()
    ?? throw new ArgumentNullException("JwtOptions are not provided");

builder.Services
    .AddAuth()
    .AddCore()
    .AddInfrastructure(connectionString, jwtOptions)
    .AddApplication();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
