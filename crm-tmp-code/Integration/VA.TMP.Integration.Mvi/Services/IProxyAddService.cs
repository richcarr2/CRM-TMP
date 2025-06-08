using VA.TMP.Integration.Mvi.StateObject;

namespace VA.TMP.Integration.Mvi.Services
{
    public interface IProxyAddService
    {
        void SendIdentifierToProxyAddEc(ref ProxyAddStateObject state, bool isPatient);
    }
}
