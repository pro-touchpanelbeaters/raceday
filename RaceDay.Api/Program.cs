using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RaceDay.Api.Data;
using RaceDay.Api.Middleware;
using RaceDay.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "RaceDay API", Version = "v1", Description = "RESTful API for the RaceDay event management system." });
    options.AddSecurityDefinition("SessionCookie", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey, Name = ".AspNetCore.Session", In = ParameterLocation.Cookie,
        Description = "Login through POST /api/auth/login. The server-side session cookie is then sent automatically by the browser."
    });
});

var connectionString = builder.Configuration.GetConnectionString("RaceDay")
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=RaceDayDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
builder.Services.AddDbContext<RaceDayDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".RaceDay.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});
builder.Services.AddAuthentication(SessionAuthenticationHandler.SchemeName)
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, SessionAuthenticationHandler>(SessionAuthenticationHandler.SchemeName, null);
builder.Services.AddAuthorization();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IEnrolmentService, EnrolmentService>();
builder.Services.AddScoped<IResultService, ResultService>();

builder.Services.AddCors(options => options.AddPolicy("MvcClient", policy => policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true)));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("MvcClient");
app.UseSession();
app.UseMiddleware<SessionAuthenticationMiddleware>();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
    await DbSeeder.SeedAsync(db);
}

app.Run();

public partial class Program { }
