﻿using Juice.AspNetCore.Mvc.Formatters;
using Juice.EventBus.RabbitMQ;
using Juice.Extensions.Swagger;
using Juice.MultiTenant;
using Juice.MultiTenant.Api;
using Juice.MultiTenant.Domain.AggregatesModel.TenantAggregate;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


ConfigureMultiTenant(builder);

ConfigureGRPC(builder.Services);

ConfigureEvents(builder);

ConfigureDistributedCache(builder.Services, builder.Configuration);

ConfigureSecurity(builder);

ConfigureApiVersioning(builder);

if (builder.Environment.IsDevelopment())
{
    ConfigureSwagger(builder);
}

ConfigureOrigins(builder);

builder.Services.AddControllers(options =>
{
    options.InputFormatters.Add(new TextSingleValueFormatter());
}).AddNewtonsoftJson(options =>
{
    options.SerializerSettings.Converters.Add(new StringEnumConverter());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCors("AllowKnownOrigins");

app.MapGet("/app", async context => { context.Response.Redirect("/app/index.html"); });

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapTenantGrpcServices();
app.RegisterTenantIntegrationEventSelfHandlers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    UseTenantSwagger(app);
}

app.MapControllers();


app.Run();



static void ConfigureMultiTenant(WebApplicationBuilder builder)
{
    builder.Services
    .AddMultiTenant()
    .ConfigureTenantHost(builder.Configuration, options =>
    {
        options.DatabaseProvider = "PostgreSQL";
        options.ConnectionName = "PostgreConnection";
        options.Schema = "App";
    }).WithBasePathStrategy(options => options.RebaseAspNetCorePathBase = true)
    .WithRouteStrategy()
    ;

    builder.Services.AddTenantIntegrationEventSelfHandlers<Tenant>();

    builder.Services.AddTenantOwnerResolverDefault();
}

static void ConfigureGRPC(IServiceCollection services)
{
    // Add services to the container.
    services.AddGrpc(o => o.EnableDetailedErrors = true);
}

static void ConfigureEvents(WebApplicationBuilder builder)
{

    builder.Services.RegisterRabbitMQEventBus(builder.Configuration.GetSection("RabbitMQ"),
       options =>
       {
           options.BrokerName = "topic.cfio_bus";
           options.SubscriptionClientName = "cfio_tenants_app_events";
           options.ExchangeType = "topic";
       });

}

static void ConfigureDistributedCache(IServiceCollection services, IConfiguration configuration)
{
    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = configuration.GetConnectionString("Redis");
        options.InstanceName = "Tenants";
    });
}

static void ConfigureSecurity(WebApplicationBuilder builder)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration.GetSection("OpenIdConnect:Authority").Get<string>();
        options.Audience = builder.Configuration.GetSection("OpenIdConnect:Audience").Get<string?>();
        options.RequireHttpsMetadata = false;
    });

    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddTenantAuthorizationTest();
    }
    else
    {
        builder.Services.AddTenantAuthorizationDefault();
    }
}

static void ConfigureOrigins(WebApplicationBuilder builder)
{
    var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>();
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowKnownOrigins",
                       builder =>
                       {
                           builder.WithOrigins(origins)
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                       });
    });
}

static void ConfigureApiVersioning(WebApplicationBuilder builder)
{
    builder.Services.AddApiVersioning(setup =>
    {
        setup.DefaultApiVersion = new ApiVersion(2, 0);
        setup.AssumeDefaultVersionWhenUnspecified = true;
        setup.ReportApiVersions = true;
    });

    builder.Services.AddVersionedApiExplorer(setup =>
    {
        setup.GroupNameFormat = "'v'VVV";
        setup.SubstituteApiVersionInUrl = true;
    });
}

static void ConfigureSwagger(WebApplicationBuilder builder)
{
    builder.Services.ConfigureSwaggerApiOptions(builder.Configuration.GetSection("Api"));
    builder.Services.AddSwaggerGen(c =>
    {
        c.IgnoreObsoleteActions();

        c.IgnoreObsoleteProperties();

        c.SchemaFilter<SwaggerIgnoreFilter>();

        c.UseInlineDefinitionsForEnums();

        c.DescribeAllParametersInCamelCase();

        c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri(builder.Configuration.GetSection("OpenIdConnect:Authority").Get<string>() + "/connect/authorize"),
                    TokenUrl = new Uri(builder.Configuration.GetSection("OpenIdConnect:Authority").Get<string>() + "/connect/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        { "openid", "OpenId" },
                        { "profile", "Profile" },
                        { "tenants-api", "Tenants API" }
                    }
                }
            },
            Scheme = "Bearer"
        });

        c.OperationFilter<AuthorizeCheckOperationFilter>();
        c.OperationFilter<ReApplyOptionalRouteParameterOperationFilter>();
        c.DocumentFilter<TenantDocsFilter>();
    });

    builder.Services.AddSwaggerGenNewtonsoftSupport();

    builder.Services.ConfigureSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Tenants API V1",
            Description = "Provide Tenants Management Web API"
        });

        c.SwaggerDoc("v2", new OpenApiInfo
        {
            Version = "v2",
            Title = "Tenants API V2",
            Description = "Provide Tenants Management Web API"
        });

        c.IncludeReferencedXmlComments();
    });

}

static void UseTenantSwagger(WebApplication app)
{
    app.UseSwagger(options => options.RouteTemplate = "tenants/swagger/{documentName}/swagger.json");
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("v1/swagger.json", "Tenants API V1");
        c.SwaggerEndpoint("v2/swagger.json", "Tenants API V2");
        c.RoutePrefix = "tenants/swagger";

        c.OAuthClientId("tenants_api_swaggerui");
        c.OAuthAppName("Tenants API Swagger UI");
        c.OAuthUsePkce();
    });
}
