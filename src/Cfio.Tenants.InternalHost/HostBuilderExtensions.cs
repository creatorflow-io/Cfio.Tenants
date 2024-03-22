using Finbuckle.MultiTenant;
using Juice.EF;
using Juice.MultiTenant.EF;
using Juice.MultiTenant;
using Juice.Services;

namespace Cfio.Tenants.InternalHost
{
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Configure Tenant microservice to provide tenant grpc service, tenant settings grpc service
        /// <para></para>JuiceIntegration
        /// <para></para>WithHeaderStrategy for grpc services
        /// <para></para>WithEFStore for Tenant EF store
        /// <para></para>TenantSettings
        /// </summary>
        /// <returns></returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> ConfigureTenantGrpcHost<TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder,
            IConfiguration configuration,
            Action<DbOptions> configureTenantDb)
            where TTenantInfo : class, ITenant, ITenantInfo, new()
        {
            builder.JuiceIntegration()
                    .WithHeaderStrategy() // for grpc incoming request
                    .WithEFStore(configuration, configureTenantDb);

            builder.Services.AddDefaultStringIdGenerator();
            
            var dbOptions = new DbOptions<TenantStoreDbContext>();
            configureTenantDb(dbOptions);

            builder.Services.AddTenantSettingsDbContext(configuration, configureTenantDb);


            return builder;
        }
    }
}
