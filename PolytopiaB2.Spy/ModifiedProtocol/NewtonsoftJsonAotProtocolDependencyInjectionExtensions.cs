using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PolytopiaB2.Spy.ModifiedProtocol
{
    public static class NewtonsoftJsonProtocolDependencyInjectionExtensions
    {
        public static IHubConnectionBuilder AddNewtonsoftAotProtocolX(
            this IHubConnectionBuilder builder)
        {
            return AddNewtonsoftAotProtocolX(builder);
        }
        
        public static TBuilder AddNewtonsoftJsonAotProtocolX<TBuilder>(this TBuilder builder) where TBuilder : ISignalRBuilder
            => AddNewtonsoftJsonAotProtocol(builder, _ => { });
        
        public static TBuilder AddNewtonsoftJsonAotProtocol<TBuilder>(this TBuilder builder, Action<NewtonsoftJsonAotHubProtocolOptions> configure) where TBuilder : ISignalRBuilder
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IHubProtocol, NewtonsoftJsonAotHubProtocol>());
            builder.Services.Configure(configure);

            //var x = builder.Services[295];
            
            return builder;
        }
    }
}