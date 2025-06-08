using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.Processor;
using VA.TMP.Integration.Messages.HealthShare;

namespace VA.TMP.Integration.HealthShare.MessageHandler
{
    /// <summary>
    /// HealthShare Update Outbound Handler.
    /// </summary>
    public class UpdateClinicHandler
    {
        private readonly ILog _logger;
        private readonly Settings _settings;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public UpdateClinicHandler(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        /// Handle the HealthShare Update Clinic Request and send back a response.
        /// </summary>
        /// <param name="message">TmpHealthShareUpdateClinicRequestMessage.</param>
        /// <returns>TmpHealthShareUpdateClinicResponseMessage.</returns>
        public TmpHealthShareUpdateClinicResponseMessage HandleRequestResponse(TmpHealthShareUpdateClinicRequestMessage message)
        {
            var requestProcessor = new UpdateClinicProcessor(_logger, _settings);
            var response = requestProcessor.Execute(message);

            return response;
        }
    }
}