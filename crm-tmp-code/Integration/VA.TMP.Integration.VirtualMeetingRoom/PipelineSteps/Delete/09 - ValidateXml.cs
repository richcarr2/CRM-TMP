using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.VirtualMeetingRoom.StateObject;

namespace VA.TMP.Integration.VirtualMeetingRoom.PipelineSteps.Delete
{
    /// <summary>
    /// Validate XML step.
    /// </summary>
    public class ValidateDeleteXmlStep : IFilter<VirtualMeetingRoomDeleteStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public ValidateDeleteXmlStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(VirtualMeetingRoomDeleteStateObject state)
        {
            //Common.Schema.ValidateSchema("Virtual Meeting Room",
            //    state.SchemaPath,
            //    new List<string> { "http://va.gov/vyopta/schemas/exchange/VirtualMeetingRoom/1.0" },
            //    new List<string> { "VirtualMeetingRoom.xsd" },
            //    state.SerializedVirtualMeetingRoomDelete);
        }
    }
}
