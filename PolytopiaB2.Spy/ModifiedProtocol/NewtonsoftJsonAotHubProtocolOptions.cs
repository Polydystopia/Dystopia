using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PolytopiaB2.Spy.ModifiedProtocol
{
    public class NewtonsoftJsonAotHubProtocolOptions
    {
        public JsonSerializerSettings PayloadSerializerSettings { get; set; } =  new() { ContractResolver = new CamelCasePropertyNamesContractResolver() };
}
}
