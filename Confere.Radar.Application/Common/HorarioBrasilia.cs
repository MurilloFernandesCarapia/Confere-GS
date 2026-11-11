namespace Confere.Radar.Application.Common
{
    public static class HorarioBrasilia
    {
        private static readonly TimeSpan Fuso = TimeSpan.FromHours(-3);

        public static DateTimeOffset Converter(DateTime dataUtc)
        {
            var utc = DateTime.SpecifyKind(dataUtc, DateTimeKind.Utc);
            return new DateTimeOffset(utc).ToOffset(Fuso);
        }

        public static DateTimeOffset? Converter(DateTime? dataUtc)
        {
            return dataUtc.HasValue ? Converter(dataUtc.Value) : null;
        }
    }
}
