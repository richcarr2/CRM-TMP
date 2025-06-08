using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace VA.TMP.Integration.Certificate.Interface
{
    public interface IKeyVaultCert
    {
        Task<X509Certificate2> GetKeyVaultCertificate(string cacheKey);
    }
}