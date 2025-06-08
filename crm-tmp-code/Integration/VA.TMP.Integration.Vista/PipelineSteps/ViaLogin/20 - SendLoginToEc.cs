using System.Diagnostics;
using System.Linq;
using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.Rest.Interface;
using VA.TMP.Integration.Vista.Mappers;
using VA.TMP.Integration.Vista.StateObject;
using VEIS.Messages.VIAScheduling;

namespace VA.TMP.Integration.Vista.PipelineSteps.ViaLogin
{
    public class SendLoginToEcStep : IFilter<ViaLoginStateObject>
    {
        private readonly ILog _logger;
        private readonly IServicePost _servicePost;
        private readonly Settings _settings;

        private string EcLoginUri => _settings.Items.First(x => x.Key == "EcLoginUri").Value;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public SendLoginToEcStep(ILog logger, IServicePost servicePost, Settings settings)
        {
            _logger = logger;
            _servicePost = servicePost;
            _settings = settings;
        }

        public void Execute(ViaLoginStateObject state)
        {
            var timer = new Stopwatch();
            timer.Start();
            if (!string.IsNullOrEmpty(state.FakeResponseType))
            {
                _logger.Info("+=+=+=+=+=+= SEND FAKE VIA LOGIN +=+=+=+=+=+=");
                state.EcResponse = VistaMapperHelper.VistaLoginFakeResponse(state.FakeResponseType);
            }
            else
            {
                _logger.Info("+=+=+=+=+=+= BEGIN SEND VIA LOGIN +=+=+=+=+=+=");
                state.EcRequest.LegacyServiceHeaderInfo = new VEIS.Core.Messages.LegacyHeaderInfo();
                state.EcResponse = _servicePost.PostToEc<VEISVIAScheLIloginVIARequest, VEISVIAScheLIloginVIAResponse>(
                    "VIA Login", EcLoginUri, _settings, state.EcRequest).ConfigureAwait(false).GetAwaiter().GetResult();
                _logger.Info("+=+=+=+=+=+= END SEND VIA LOGIN +=+=+=+=+=+=");
            }
            timer.Stop();
            state.EcProcessingTimeMs += (int)timer.ElapsedMilliseconds;

            if (state.EcResponse == null) throw new MissingViaResponseException("No EC Response was returned");
        }
    }
}
