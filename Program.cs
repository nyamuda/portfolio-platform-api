using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PortfolioPlatform.Api.Data;
using PortfolioPlatform.Api.Models;
using PortfolioPlatform.Api.Models.Auth;
using PortfolioPlatform.Api.Models.OAuth;
using PortfolioPlatform.Api.Services.Abstractions.Auth;
using PortfolioPlatform.Api.Services.Abstractions.Auth.OAuth;
using PortfolioPlatform.Api.Services.Abstractions.BlogPosts;
using PortfolioPlatform.Api.Services.Abstractions.Contact;
using PortfolioPlatform.Api.Services.Abstractions.Dashboard;
using PortfolioPlatform.Api.Services.Abstractions.Email;
using PortfolioPlatform.Api.Services.Abstractions.Profiles;
using PortfolioPlatform.Api.Services.Abstractions.Projects;
using PortfolioPlatform.Api.Services.Abstractions.Users;
using PortfolioPlatform.Api.Services.Implementations.Auth;
using PortfolioPlatform.Api.Services.Implementations.Auth.OAuth;
using PortfolioPlatform.Api.Services.Implementations.BlogPosts;
using PortfolioPlatform.Api.Services.Implementations.Contact;
using PortfolioPlatform.Api.Services.Implementations.Dashboard;
using PortfolioPlatform.Api.Services.Implementations.Email;
using PortfolioPlatform.Api.Services.Implementations.Profiles;
using PortfolioPlatform.Api.Services.Implementations.Projects;
using PortfolioPlatform.Api.Services.Implementations.Users;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEmailTemplateBuilder, EmailTemplateBuilder>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IBlogPostService, BlogPostService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

string connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Missing configuration: 'DefaultConnection' is not set."
    );

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.Configure<Company>(builder.Configuration.GetSection("Company"));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.Configure<GoogleOAuthOptions>(builder.Configuration.GetSection("Authentication:OAuth:Google"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Authentication:JwtSettings"));

JwtSettings jwtSettings =
    builder.Configuration.GetSection("Authentication:JwtSettings").Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "Missing configuration: 'Authentication:JwtSettings' is not set."
    );

builder
    .Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
    });

builder.Services.AddAuthorization();

string localFrontendUrl =
    builder.Configuration.GetValue<string>("Frontend:LocalUrl")
    ?? throw new InvalidOperationException("Frontend local URL is missing in application settings.");

string? productionFrontendUrl = builder.Configuration.GetValue<string>("Frontend:ProductionUrl");

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "Frontend",
        policy =>
        {
            List<string> allowedOrigins = [localFrontendUrl];
            if (!string.IsNullOrWhiteSpace(productionFrontendUrl))
            {
                allowedOrigins.Add(productionFrontendUrl);
            }

            policy
                .WithOrigins(allowedOrigins.ToArray())
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    );
});

builder
    .Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        );
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();


