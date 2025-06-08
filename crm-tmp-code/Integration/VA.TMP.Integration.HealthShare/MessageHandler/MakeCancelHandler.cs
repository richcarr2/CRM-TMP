using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.Processor;
using VA.TMP.Integration.Messages.HealthShare;

namespace VA.TMP.Integration.HealthShare.MessageHandler
{
    /// <summary>
    /// HealthShare Make Cancel handler.
    /// </summary>
    public class MakeCancelHandler
    {
        private readonly ILog _logger;
        private readonly Settings _settings;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public MakeCancelHandler(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        /// Handle the HealthShare Make and Cancel Appointment Request and send back a response.
        /// </summary>
        /// <param name="message">TmpHealthShareMakeAndCancelAppointmentRequestMessage.</param>
        /// <returns>TmpHealthShareMakeAndCancelAppointmentResponseMessage.</returns>
        public TmpHealthShareMakeAndCancelAppointmentResponseMessage HandleRequestResponse(TmpHealthShareMakeAndCancelAppointmentRequestMessage message)
        {
            var requestProcessor = new MakeCancelProcessor(_logger, _settings);
            var response = requestProcessor.Execute(message);

            return response;
        }
    }
}