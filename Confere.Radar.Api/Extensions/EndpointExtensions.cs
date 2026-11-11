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
    }
}
