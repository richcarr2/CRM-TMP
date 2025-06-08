using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    public class McsServicesDeletePreStageRunner : PluginRunner
    {
        //Instantiate McsServiceDeletePreStageRunner object for thread safety purposes
        public McsServicesDeletePreStageRunner(IServiceProvider serviceProvider) : base(serviceProvider) { }

        #region PrimaryFunctionality
        public override void Execute()
        {
            //plugin registered on an entity no longer in use
            Logger.WriteDebugMessage("plugin registered on deprecated entity 'mcs_services'");
            //DeleteTSA();
        }

        //internal void DeleteTSA()
        //{
        //    Logger.setMethod = "Delete TSA";
        //    Logger.WriteDebugMessage("Starting Delete TSA");
        //    using (var srv = new Xrm(OrganizationService))
        //    {
        //        //Starting Validation Checks before deleting the TSA. 
        //        var thisTSA = srv.mcs_servicesSet.FirstOrDefault(i => i.Id == PrimaryEntity.Id);

        //        if (thisTSA.mcs_RelatedServiceId == null) { return; }

        //        var systemService = srv.ServiceSet.FirstOrDefault(i => i.Id == thisTSA.mcs_RelatedServiceId.Id);

        //        //Looks for any Service Activities for this TSA
        //        var ServiceActivity = srv.ServiceAppointmentSet.FirstOrDefault(i => i.mcs_relatedtsa.Id == PrimaryEntity.Id);                                  
                
        //        //If an associated Patient Site Resource does exist, we will throw an exception with a message and stop the delete of the MCS Resource, to prevent orphan data. 
        //        if (ServiceActivity != null)
        //        {
        //           // Will Prevent deletion of TSA's if any Service Activities exists for now. Pending input. Do we need to only block if the SA is in a specific status
        //                Logger.WriteDebugMessage("Service Activities:" + ServiceActivity.Subject);
        //                throw new InvalidPluginExecutionException("customPlease check Left Nav on TSA form for Service Activities this TSA is associated with. TSA cannot be deleted while associated open or scheduled Service Activities exist.");
        //        }

        //        //If there are no related Service Activites, we will delete the related System Service. 
        //        OrganizationService.Delete(systemService.LogicalName, systemService.Id);
        //        Logger.WriteDebugMessage("System Service Deleted");

        //    }
        //}

        public override string McsSettingsDebugField
        {
            get { return "mcs_tsaplugin"; }
        }

        public override Entity GetPrimaryEntity()
        {
            return (Entity)PluginExecutionContext.PreEntityImages["pre"];
        }
    
        #endregion
    }
}
