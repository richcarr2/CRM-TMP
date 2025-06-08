using System;
using System.Linq;
using log4net;
using Microsoft.Xrm.Sdk.Query;
using VA.TMP.DataModel;
using VA.TMP.Integration.Core;
using VA.TMP.Integration.Core.Exceptions.LobExceptions;
using VA.TMP.Integration.HealthShare.StateObject;

namespace VA.TMP.Integration.HealthShare.PipelineSteps.MakeCancelInbound
{
    /// <summary>
    /// Get Integration Result Step.
    /// </summary>
    public class GetIntegrationResultStep : IFilter<MakeCancelInboundStateObject>
    {
        // private readonly ILog _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger.</param>
        //public GetIntegrationResultStep(ILog logger)
        //{
        //    _logger = logger;
        //}


        private readonly Settings _settings;
        private readonly ILog _logger;

        private string maxRetriesCount => _settings.Items.First(x => x.Key == "maxRetries").Value;

        private string sleepThreadPeriod => _settings.Items.First(x => x.Key == "SleepThreadPeriod").Value;

        public GetIntegrationResultStep(ILog logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }



        /// <summary>
        /// Execute the step.
        /// </summary>
        /// <param name="state">State object.</param>
        public void Execute(MakeCancelInboundStateObject state)
        {
            using (var srv = new Xrm(state.OrganizationServiceProxy))
            {
                if (state.IntegrationResult == null)
                {
                    
                    int count = 0;
                    var maxRetries = 0;
                    int.TryParse(maxRetriesCount, out maxRetries);
                    var threadSleep = 0;
                    int.TryParse(sleepThreadPeriod, out threadSleep);


                    if (maxRetries == 0)
                    {
                     
                        state.IntegrationResult = srv.mcs_integrationresultSet.FirstOrDefault(x => x.cvt_controlid == state.RequestMessage.ControlId);

                     

                    }
                    else
                    {
                        while (count < maxRetries)
                        {
                            
                            state.IntegrationResult = srv.mcs_integrationresultSet.FirstOrDefault(x => x.cvt_controlid == state.RequestMessage.ControlId);
                            if (state.IntegrationResult != null)
                            {
                                _logger.Info("Integration Result is found after " + count + " retries");
                                count = maxRetries;
                            }
                            else
                            {
                                System.Threading.Thread.Sleep(threadSleep);
                            }

                            count++;

                        }

                    }

                    if (state.IntegrationResult != null && state.IntegrationResult.Contains("cvt_controlid"))
                    {
                        _logger.Info($"Max retries: {maxRetries}, Thread sleep: {threadSleep}, Controlid: {state.IntegrationResult.cvt_controlid}");
                    }
                    else
                    {
                        state.IntegrationResult = new mcs_integrationresult();                        
                    }
                }
            }
        }
    }
}
