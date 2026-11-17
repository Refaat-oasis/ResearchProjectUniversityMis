using System.Threading.Tasks;

namespace researchProject.Services.Interfaces
{
    public interface IUnconfirmedEmailCleanupService
    {
        Task<int> CleanupUnconfirmedAccountsAsync();
    }
}

