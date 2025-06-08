namespace VA.TMP.Integration.Common
{
    /// <summary>
    /// Enumeration to specify Patient/Provider.
    /// </summary>
    public enum Side
    {
        Patient,
        Provider
    }

    /// <summary>
    /// Enumeration to specify appointment type.
    /// </summary>
    public enum AppointmentType
    {
        HOME_MOBILE,

        STORE_FORWARD,

        GROUP,

        CLINIC_BASED
    }

    /// <summary>
    /// Enumeration to specify the status of the Vista appointment.
    /// </summary>
    public enum VistaStatus
    {
        SCHEDULED,

        FAILED_TO_SCHEDULE,

        CANCELED,

        FAILED_TO_CANCEL
    }
}