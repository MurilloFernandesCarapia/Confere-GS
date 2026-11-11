namespace Confere.Radar.UnitTests.Helpers
{
    public class RelogioFixo : TimeProvider
    {
        private DateTimeOffset _agora;

        public RelogioFixo() : this(new DateTimeOffset(2026, 11, 18, 17, 30, 0, TimeSpan.Zero))
        {
        }

        public RelogioFixo(DateTimeOffset agora)
        {
            _agora = agora;
        }

        public DateTime AgoraUtc => _agora.UtcDateTime;

        public override DateTimeOffset GetUtcNow() => _agora;

        public void Avancar(TimeSpan tempo)
        {
            _agora = _agora.Add(tempo);
        }
    }
}
