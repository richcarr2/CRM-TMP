using System.Linq;
using log4net;
using VA.TMP.DataModel;
using VA.TMP.Integration.Common;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.HealthShare.StateObject;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancelOutbound
{
    /// <summary>
    /// Get Vista Integration Result step.
    /// </summary>
    public class GetVistaIntegrationResultStep : IFilter<MakeCancelOutboundStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public GetVistaIntegrationResultStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(MakeCancelOutboundStateObject state)
        {
            if (!state.IsGroupAppointment || state.RequestMessage.VisitStatus != VistaStatus.CANCELED.ToString()) return;

            if (!state.VistaIntegrationResultId.HasValue) throw new MissingVistaIntegrationResultException("For Group Cancel a Vista Integration Result is required");

            using (var srv = new Xrm(state.OrganizationServiceProxy))
            {
                state.VistaIntegrationResult = srv.cvt_vistaintegrationresultSet.FirstOrDefault(x => x.Id == state.VistaIntegrationResultId.Value);
                if (state.VistaIntegrationResult == null) throw new MissingVistaIntegrationResultException($"Group Cancel: Cannot find Vista Integration Result: {state.VistaIntegrationResultId.Value}");
            }
        }
    }
}
