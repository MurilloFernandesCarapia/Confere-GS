namespace Confere.Radar.Domain.Models
{
    public record InicioIdempotencia(bool Repetida, int? StatusHttp, string? CorpoResposta)
    {
        public static InicioIdempotencia Nova() => new(false, null, null);

        public static InicioIdempotencia Replay(int statusHttp, string? corpoResposta) => new(true, statusHttp, corpoResposta);
    }
}
