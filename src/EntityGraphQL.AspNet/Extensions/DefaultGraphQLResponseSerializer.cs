using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EntityGraphQL.AspNet
{
    /// <summary>
    /// Serializes GraphQL responses into a JSON response format.
    /// </summary>
    public class DefaultGraphQLResponseSerializer : IGraphQLResponseSerializer
    {
        private readonly JsonSerializerOptions jsonOptions;

        public DefaultGraphQLResponseSerializer(JsonSerializerOptions? jsonOptions = null)
        {
            if (jsonOptions != null)
                this.jsonOptions = jsonOptions;
            else
            {
                this.jsonOptions = new JsonSerializerOptions
                {
                    IncludeFields = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };
                this.jsonOptions.Converters.Add(new JsonStringEnumConverter());
            }
        }

        public async Task SerializeAsync<T>(Stream body, T data)
        {
            await JsonSerializer.SerializeAsync(body, data, jsonOptions);
        }
    }
}