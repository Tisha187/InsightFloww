using InsightFlow.Core.Authentication;
using InsightFlow.Core.Models;
using InsightFlow.Core.Repositories;
using InsightFlow.Core.Services;
using InsightFlow.Core.Services.Forecasting;
using InsightFlow.Core.Storage;
using InsightFlow.Core.Validation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var geminiApiKey = builder.Configuration["Gemini:ApiKey"]
    ?? throw new InvalidOperationException(
        "Gemini API key is missing. Configure Gemini:ApiKey using User Secrets.");

#pragma warning disable SKEXP0070
builder.Services.AddGoogleAIGeminiChatCompletion(
    modelId: "gemini-3.8-flash",
    apiKey: geminiApiKey,
    apiVersion: GoogleAIVersion.V1_Beta);

builder.Services.AddScoped<Kernel>(serviceProvider =>
    new Kernel(serviceProvider));
#pragma warning restore SKEXP0070

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Enter your JWT access token."
    });

    options.AddSecurityRequirement(document => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });
});


builder.Services.AddDbContext<InsightFlowAiContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IDatasetRepository, DatasetRepository>();


builder.Services.AddScoped<DatasetAnalyticsService>();

builder.Services.AddScoped<DatasetReader>();

builder.Services.AddSingleton<
    IFileStorageService,
    LocalFileStorageService>();

builder.Services.AddSingleton<DatasetFileValidator>();

builder.Services
    .AddIdentityCore<AspNetUser>(options =>
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
    })
    .AddUserStore<ApplicationUserStore>()
    .AddSignInManager();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularClient", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddScoped<IDatasetService, DatasetService>();

builder.Services.AddHttpClient<
    IForecastingClient,
    ForecastingClient>(client =>
    {
        var baseUrl = builder.Configuration["ForecastingService:BaseUrl"]
            ?? throw new InvalidOperationException(
                "Forecasting service URL is missing.");

        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = TimeSpan.FromSeconds(120);
    });



builder.Services.AddScoped<
    IForecastRepository,
    ForecastRepository>();

builder.Services.AddScoped<
    InsightFlow.Core.Services.Forecasting.ForecastingService>();


var app = builder.Build();

// Enable Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseCors("AngularClient");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
