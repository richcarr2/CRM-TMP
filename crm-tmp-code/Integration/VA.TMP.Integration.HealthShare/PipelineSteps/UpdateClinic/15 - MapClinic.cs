using log4net;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.HealthShare.Mappers;
using VA.TMP.Integration.HealthShare.StateObject;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.UpdateClinic
{
    /// <summary>
    /// Map Clinic to CRM etities step.
    /// </summary>
    public class MapClinicStep : IFilter<UpdateClinicStateObject>
    {
        private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        public MapClinicStep(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(UpdateClinicStateObject state)
        {
            state.Clinic = new UpdateClinicMapper(state.RequestMessage, state.OrganizationServiceProxy, _logger).Map();
        }
    }
}