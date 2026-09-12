using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SecureCheck.Core.Interfaces;
using SecureCheck.Infrastructure.Data;
using SecureCheck.Infrastructure.Repositories;
using SecureCheck.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var defaultConnection =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? $"Data Source={Path.Combine(builder.Environment.ContentRootPath, "securecheck.db")}";

builder.Services.AddDbContext<SecureCheckDbContext>(options =>
    options.UseSqlite(defaultConnection));

builder.Services.AddScoped<IRegistrationRepository, RegistrationRepository>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();

var admitCardFolder = Path.Combine(
    builder.Environment.ContentRootPath,
    "wwwroot",
    "admitcards");

builder.Services.AddSingleton<IAdmitCardService>(_ => new AdmitCardPdfService(admitCardFolder));

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("InvigilatorClient", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SecureCheckDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("InvigilatorClient");
app.MapControllers();

app.Run();

public partial class Program { }
