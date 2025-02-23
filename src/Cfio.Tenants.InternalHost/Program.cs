using Cfio.Tenants.InternalHost;
using Finbuckle.MultiTenant;
using Juice.EventBus.RabbitMQ;
using Juice.MultiTenant;
using Juice.MultiTenant.Api;
using Juice.MultiTenant.Domain.AggregatesModel.TenantAggregate;
using Juice.MultiTenant.Shared.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();

ConfigureMultiTenant(builder);

ConfigureGRPC(builder.Services);

ConfigureEvents(builder);

ConfigureDistributedCache(builder.Services, builder.Configuration);

ConfigureSecurity(builder);

ConfigureOrigins(builder);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCors("AllowKnownOrigins");

app.MapGet("/", () => "Support gRPC only!");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapTenantGrpcServices();
app.RegisterTenantIntegrationEventSelfHandlers();

app.Run();

static void ConfigureMultiTenant(WebApplicationBuilder builder)
{
    builder.Services
    .AddMultiTenant()
    .ConfigureTenantGrpcHost(builder.Configuration, options =>
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
           options.SubscriptionClientName = "cfio_tenants_internal_host_events";
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
    builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["IdentityServer:Authority"];
        options.RequireHttpsMetadata = false;
        options.Audience = "cfio_tenants_internal_host";
    });
    // This service is intended read-only access to tenant settings
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(Policies.TenantAdminPolicy, policy =>
        {
            policy.RequireAssertion(context =>
            {
                return false;
            });
        });

        options.AddPolicy(Policies.TenantDeletePolicy, policy =>
        {
            policy.RequireAssertion(context =>
            {
                return false;
            });
        });

        options.AddPolicy(Policies.TenantSettingsPolicy, policy =>
        {
            policy.RequireAssertion(context =>
            {
                return false;
            });
        });

        options.AddPolicy(Policies.TenantCreatePolicy, policy =>
        {
            policy.RequireAssertion(context =>
            {
                return false;
            });
        });

        options.AddPolicy(Policies.TenantOwnerPolicy, policy =>
        {
            policy.RequireAssertion(context =>
            {
                return false;
            });
        });

        options.AddPolicy(Policies.TenantOperationPolicy, policy =>
        {
            policy.RequireAssertion(context =>
            {
                return false;
            });
        });
    });
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

