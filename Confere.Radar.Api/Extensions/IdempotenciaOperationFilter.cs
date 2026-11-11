using Confere.Radar.Api.Filters;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Confere.Radar.Api.Extensions
{
    public class IdempotenciaOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var exige = context.ApiDescription.ActionDescriptor.EndpointMetadata.OfType<ExigeIdempotencia>().Any();
            if (!exige)
            {
                return;
            }

            operation.Parameters ??= new List<IOpenApiParameter>();
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = IdempotenciaFilter.HeaderChave,
                In = ParameterLocation.Header,
                Required = true,
                Description = "UUID novo para cada operação. Repetir a mesma chave com o mesmo corpo devolve a resposta salva (header Idempotent-Replayed: true); com corpo diferente dá 422. Vale por 24 horas. No PowerShell: [guid]::NewGuid()",
                Schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid" }
            });
        }
    }
}
