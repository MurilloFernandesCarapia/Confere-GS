using Confere.Radar.Api.Filters;

namespace Confere.Radar.Api.Extensions
{
    public static class EndpointExtensions
    {
        public static RouteHandlerBuilder ComValidacao<T>(this RouteHandlerBuilder builder) where T : class
        {
            return builder
                .AddEndpointFilter<ValidacaoFilter<T>>()
                .ProducesProblem(StatusCodes.Status400BadRequest);
        }

        public static RouteHandlerBuilder ComIdempotencia(this RouteHandlerBuilder builder)
        {
            return builder
                .AddEndpointFilter<IdempotenciaFilter>()
                .WithMetadata(new ExigeIdempotencia())
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status409Conflict)
                .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
        }
    }

    public sealed class ExigeIdempotencia
    {
    }
}
