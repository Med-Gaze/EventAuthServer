using System.Threading.Tasks;

namespace med.common.library.configuration.service
{
    public interface IConfigurationService
    {
        Task<bool> SeedModules();
    }
}
