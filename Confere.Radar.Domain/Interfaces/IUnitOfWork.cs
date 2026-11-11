namespace Confere.Radar.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        Task SalvarAsync();
    }
}
