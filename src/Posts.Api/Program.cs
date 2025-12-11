using Posts.Api.Extensions;
using Posts.Api.Middlewares;
using Posts.Application;
using Posts.Infrastructure;
using Posts.Infrastructure.Core.Models;
using Posts.Migrations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("PostsDatabase")
    ?? throw new ArgumentNullException("ConnectionString is not provided");

var jwtOptions = builder.Configuration.GetSection("JwtOptions").Get<JwtOptions>()
    ?? throw new ArgumentNullException("JwtOptions are not provided");

var encryptionKey = builder.Configuration.GetValue<string>("EncryptionKey")
    ?? throw new ArgumentNullException("EncryptionKey are not provided");

var runMigrations = builder.Configuration.GetValue<bool?>("RunMigrations") ?? false;

builder.Services
    .AddAuth(jwtOptions)
    .AddCore()
    .AddInfrastructure(connectionString, encryptionKey, jwtOptions)
    .AddApplication();

builder.Services.AddTransient<ExceptionMiddleware>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (runMigrations)
{
    await Migrations.RunAsync(connectionString);
}

app.UseCors("AllowFrontend");
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
