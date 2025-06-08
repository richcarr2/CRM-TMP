using Ec.VirtualMeetingRoom.Services.Vyopta.UcManager;

namespace Ec.VirtualMeetingRoom.Services
{
    public interface IServiceFactory
    {
        IUcManagerService GetVirtualMeetingRoomWebServiceReference();
    }
}