using OC.LUAC.ApiLayer.Storage;

namespace OC.LUAC.ApiLayer
{
    public static class DependencyInjectionAPI
    {

        public static IServiceCollection AddApiLayerServices(this IServiceCollection services)
        {

            services.AddScoped<IImageStorage, LocalImageStorage>();


            return services;

        }

    }
}
