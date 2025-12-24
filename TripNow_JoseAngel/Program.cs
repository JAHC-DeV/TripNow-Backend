using Microsoft.EntityFrameworkCore;
using TripNow_JoseAngel.Application.Interfaces;
using TripNow_JoseAngel.Infrastructure.Background;
using TripNow_JoseAngel.Infrastructure.ExternalServices;
using TripNow_JoseAngel.Infrastructure.Persistence;
using TripNow_JoseAngel.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Agrega CORS para permitir todas las entradas
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

//Agrega el DbContext al contenedor de inyeccion de dependencias
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Agrega al contenedor de inyeccion de dependencias el Repositorio de Reservas
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();


// Agrega al contenedor de inyeccion de dependencias el Servicio de Evaluacion de Riesgos
builder.Services.AddHttpClient();
builder.Services.AddScoped<IRiskEvaluation, RiskEvaluationService>();

// Agrega el Background Service para procesar reservas pendientes
builder.Services.AddHostedService<RiskEvaluationBackgroundService>();


var app = builder.Build();

// Aplica las migraciones pendientes al iniciar la aplicacion
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{    
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Habilita CORS
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
