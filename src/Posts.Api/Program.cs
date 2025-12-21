using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Posts.Api.Extensions;
using Posts.Api.Filters;
using Posts.Api.Middlewares;
using Posts.Application;
using Posts.Infrastructure;
using Posts.Infrastructure.Core.Models;
using Posts.Migrations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(opts =>
{
    opts.Filters.Add<ValidationFilter>();
});

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("PostsDatabase")
    ?? throw new ArgumentNullException("ConnectionString is not provided");

var jwtOptions = builder.Configuration.GetSection("JwtOptions").Get<JwtOptions>()
    ?? throw new ArgumentNullException("JwtOptions are not provided");

var encryptionOptions = builder.Configuration.GetSection("EncryptionOptions").Get<EncryptionOptions>()
    ?? throw new ArgumentNullException("EncryptionOptions are not provided");

var s3Options = builder.Configuration.GetSection("S3Options").Get<S3Options>()
    ?? throw new ArgumentNullException("S3Options are not provided");

var runMigrations = builder.Configuration.GetValue<bool?>("RunMigrations") ?? false;

var dbOpts = new DbConnectionOptions { ConnectionString = connectionString };

builder.Services
    .AddAuth(jwtOptions)
    .AddCore()
    .AddInfrastructure(dbOpts, encryptionOptions, jwtOptions, s3Options)
    .AddApplication();

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
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

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
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
