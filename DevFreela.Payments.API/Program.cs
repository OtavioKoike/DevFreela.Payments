using DevFreela.Payments.API.Consumers;
using DevFreela.Payments.Application.Services.Implementations;
using DevFreela.Payments.Application.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// É possivel criar as injeções de Dependencias em uma classe especifica de extensions
builder.Services.AddScoped<IPaymentService, PaymentService>();
// Hosted Service -> Rodar debaixo dos panos
builder.Services.AddHostedService<ProcessPaymentConsumer>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
