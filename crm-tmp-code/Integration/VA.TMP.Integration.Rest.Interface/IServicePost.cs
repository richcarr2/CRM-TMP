using System.Threading.Tasks;
using VA.TMP.Integration.Core;

namespace VA.TMP.Integration.Rest.Interface
{
    public interface IServicePost
    {
        Task<R> PostToEc<T, R>(string ecName, string uri, Settings settings, T payload);
    }
}