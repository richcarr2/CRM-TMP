using System.Diagnostics;
using log4net;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Messages.Vista;
using VA.TMP.Integration.Vista.Processor;

namespace VA.TMP.Integration.Vista.MessageHandler
{
    public class ViaLoginHandler
    {
        private readonly ILog _logger;
        private readonly Settings _settings;
        private string _serializedRequest;
        private Stopwatch _timer;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public ViaLoginHandler(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public ViaLoginResponseMessage HandleRequestResponse(ViaLoginRequestMessage message)
        {
            _timer = new Stopwatch();
            _timer.Start();
            _serializedRequest = Serialization.DataContractSerialize(message);

            var processor = new ViaLoginProcessor(_logger, _settings);
            var response = processor.Execute(message);
            if (response != null && response.ExceptionOccured) _logger.Error($"Error in ViaLogin Pipeline: {response.ExceptionMessage}");

            response.VimtRequest = _serializedRequest;
            response.VimtResponse = Serialization.DataContractSerialize(response);
            response.SerializedInstance = response.SerializedInstance;
            _timer.Stop();
            response.VIMTProcessingTime = (int)_timer.ElapsedMilliseconds;

            return response;
        }
    }
}
