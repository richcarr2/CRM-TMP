using System.Diagnostics;
using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.Mvi;
using VA.TMP.Integration.Mvi.Processor;

namespace VA.TMP.Integration.Mvi.MessageHandler
{
    /// <summary>
    /// Person Search handler.
    /// </summary>
    public class PersonSearchHandler
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private string _serializedRequest;
        private Stopwatch _timer;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public PersonSearchHandler(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        /// <summary>
        /// Handle the Person Search Request and send back a response.
        /// </summary>
        /// <param name="message">PersonSearchRequestMessage.</param>
        /// <returns>PersonSearchResponseMessage.</returns>
        public PersonSearchResponseMessage HandleRequestResponse(PersonSearchRequestMessage message)
        {
            _timer = new Stopwatch();
            _timer.Start();

            _serializedRequest = Serialization.DataContractSerialize(message);

            var requestProcessor = new PersonSearchProcessor(_logger, _settings);
            var response = requestProcessor.Execute(message);

            if (response.ExceptionOccured) _logger.Error($"ERROR: MVI Person Search: {response.ExceptionMessage}");

            response.VimtRequest = _serializedRequest;
            response.VimtResponse = Serialization.DataContractSerialize(response);
            _timer.Stop();
            response.VimtProcessingMs = (int)_timer.ElapsedMilliseconds;

            return response;
        }
    }
}
