using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PolytopiaA10.Carrier.Hubs.ModifiedProtocol
{
    public class NewtonsoftJsonAotHubProtocolOptions
    {
        public JsonSerializerSettings PayloadSerializerSettings { get; init; } =  new() { ContractResolver = new CamelCasePropertyNamesContractResolver() };
}
}
