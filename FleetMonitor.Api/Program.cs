using FleetMonitor.Api.Middleware;
using FleetMonitor.Api.Hubs;
using FleetMonitor.Core.Models;
using Microsoft.EntityFrameworkCore;
using FleetMonitor.Api.Services;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton(new JwtService(builder.Configuration["Jwt:Secret"] ?? "EstaEsUnaClaveSuperSecretaDeAlMenos32Caracteres"));

builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddAuthentication("ManualJwt")
    .AddScheme<AuthenticationSchemeOptions, ManualJwtHandler>("ManualJwt", null);

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<ManualJwtMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<TelemetryHub>("/hubs/telemetry");


app.UseCors();

app.Run();
