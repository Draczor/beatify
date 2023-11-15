using MassTransit;
using Microsoft.EntityFrameworkCore;
using UserService.DbContexts;
using UserService.Repositories;
using UserService.Repositories.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using UserService.Consumers;
using UserService.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<UserContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("UserServiceConnection")));

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddMassTransit(options =>
{
    options.UsingRabbitMq((registrationContext, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ://localhost"] ?? "rabbitmq-1", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(registrationContext, new KebabCaseEndpointNameFormatter(true));

        cfg.ReceiveEndpoint("users", e =>
        {
            e.Consumer<CommandMessageConsumer>();
            //e.Consumer<UserCreatedEventConsumer>();
        });
    });
    
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
