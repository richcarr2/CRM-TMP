using System.Diagnostics;
using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.Mvi;
using VA.TMP.Integration.Mvi.Processor;

namespace VA.TMP.Integration.Mvi.MessageHandler
{
    /// <summary>
    /// Get Person Identifiers handler.
    /// </summary>
    public class GetPersonIdentifiersHandler
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private string _serializedRequest;
        private Stopwatch _timer;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GetPersonIdentifiersHandler(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        /// Handle the Get Person Identifiers Request and send back a response.
        /// </summary>
        /// <param name="message">GetPersonIdentifiersRequestMessage.</param>
        /// <returns>GetPersonIdentifiersResponseMessage.</returns>
        public GetPersonIdentifiersResponseMessage HandleRequestResponse(GetPersonIdentifiersRequestMessage message)
        {
            _timer = new Stopwatch();

            _timer.Start();
            _serializedRequest = Serialization.DataContractSerialize(message);

            var requestProcessor = new GetPersonIdentifiersProcessor(_logger, _settings);
            if(string.IsNullOrEmpty(message.ServerName)) message.ServerName = _settings.Items.Find(s => s.Key.Equals("CrmBaseOrgUri")).Value.ToString();
            var response = requestProcessor.Execute(message);

            if (response.ExceptionOccured) _logger.Error($"ERROR: MVI Get Person Identifiers: {response.ExceptionMessage}");

            response.VimtRequest = _serializedRequest;
            response.VimtResponse = Serialization.DataContractSerialize(response);
            _timer.Stop();
            response.VimtProcessingMs = (int)_timer.ElapsedMilliseconds;

            return response;
        }
    }
}
