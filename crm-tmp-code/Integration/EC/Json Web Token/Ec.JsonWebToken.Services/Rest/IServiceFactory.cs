using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Ec.JsonWebToken.Services.Rest
{
    public interface IServiceFactory
    {
        Task<string> EncryptToken(JwtPayload payload);
        Task<string> EncryptToken(string notBefore, string expiration, string secId);
    }
}