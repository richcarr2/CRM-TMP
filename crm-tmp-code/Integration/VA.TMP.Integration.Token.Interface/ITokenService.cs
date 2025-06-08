using System.Threading.Tasks;

namespace VA.TMP.Integration.Token.Interface
{
    public interface ITokenService
    {
        Task<string> GetToken(string cacheKey);

        Task<string> GetCrmToken(string cacheKey);
    }
}