using PRTGInsight.Models;
using System.Threading.Tasks;

namespace PRTGInsight.Services
{
    public interface ISettingsService
    {
        Task<AppSettings> LoadSettingsAsync();
        Task SaveSettingsAsync(AppSettings settings);
    }
}