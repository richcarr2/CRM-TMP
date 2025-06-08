using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.VirtualMeetingRoom.StateObject;

namespace VA.TMP.Integration.VirtualMeetingRoom.PipelineSteps.Create
{
    /// <summary>
    /// Compare URLs step.
    /// </summary>
    public class CompareUrlsStep : IFilter<VirtualMeetingRoomCreateStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public CompareUrlsStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(VirtualMeetingRoomCreateStateObject state)
        {
            if (state.ExceptionOccured) return;

            // The Misc Data sent back from Vyopta is semicolon delimited. Vyopta is currently adding a trailing semicolon but that could change so I'm only checking for minimum of 2 strings.
            var urls = state.MiscDataForResponse.Split(';');
            if (urls.Length < 2) throw new VmrMismatchException(string.Format("Misc Data returned from Vyopta is not in the correct format: Misc Data is {0}", state.MiscDataForResponse));

            // Parse out the Patient/Host and Provider/Guest URLs. We use a utility function to trim the beginning.
            var providerUrl = urls[0].TrimStart(state.VyoptaHostUrlPrefix);
            var patientUrl = urls[1].TrimStart(state.VyoptaGuestUrlPrefix);

            // Compare the Patient/Host and Provider/Guest URLs for TMP and Vyopta.
            if (state.PatientUrl != patientUrl) throw new VmrMismatchException(string.Format("TMP and Vyopta Patient URLs DO NOT match! TMP - {0}, Vyopta - {1}", state.PatientUrl, patientUrl));
            if (state.ProviderUrl != providerUrl) throw new VmrMismatchException(string.Format("TMP and Vyopta Provider URLs DO NOT match! TMP - {0}, Vyopta - {1}", state.ProviderUrl, providerUrl));
        }
    }
}
