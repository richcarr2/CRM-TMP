namespace VA.TMP.Integration.Messages.TelehealthSpecialtyLocation.Interfaces
{
    public interface IResponseMessage
    {
        bool ErrorOccurred { get; set; }
        string ErrorMessage { get; set; }
        string Status { get; set; }
        string DebugInfo { get; set; }
    }
}
