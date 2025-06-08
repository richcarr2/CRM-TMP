using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.ServiceModel;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    public class McsAppointmentMasterDeletePreStageRunner : PluginRunner
    {
        //Purpose:  To Create Series of Service Activiites based on the Appointments created. 

        #region Constructor
        public McsAppointmentMasterDeletePreStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion

        #region Internal Methods
        public override void Execute()
        {
            //DeleteServiceActivities(PluginExecutionContext.PrimaryEntityId);                     
        }

        //internal void DeleteServiceActivities(Guid thisApptMasterId)
        //{
        //    Logger.setMethod = "DeleteServiceActivities";
        //    Logger.WriteDebugMessage("starting DeleteServiceActivities");
        //    try
        //    {
        //        using (var srv = new Xrm(OrganizationService))
        //        {
        //            var ServiceActivities = from sa in srv.ServiceAppointmentSet
        //                                    where (sa.cvt_recurringappointmentsmaster.Id == thisApptMasterId)
        //                                    select sa;

        //            if (ServiceActivities == null)
        //            {
        //                Logger.WriteDebugMessage("No Service Activities found");
        //                return;
        //            }

        //            foreach (var sa in ServiceActivities)
        //            {
        //                Logger.WriteDebugMessage("Got Service Activity:" + sa.Subject);

        //                //Delete the related Service Activity
        //                OrganizationService.Delete(sa.LogicalName, sa.Id);
        //                Logger.WriteDebugMessage("Service Activity Deleted");
        //            }
        //        }
        //    }
        //    catch (FaultException<OrganizationServiceFault> ex)
        //    {
        //        Logger.WriteToFile(ex.Message);
        //        throw new InvalidPluginExecutionException(McsSettings.getUnexpectedErrorMessage + ":" + ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.Message.StartsWith("custom"))
        //        {
        //            Logger.WriteDebugMessage(ex.Message.Substring(6));
        //            throw new InvalidPluginExecutionException(ex.Message.Substring(6));
        //        }
        //        else
        //        {
        //            Logger.setMethod = "Execute";
        //            Logger.WriteToFile(ex.Message);
        //            throw new InvalidPluginExecutionException(McsSettings.getUnexpectedErrorMessage + ":" + ex.Message);
        //        }
        //    }
        //}
        #endregion
        #region Additional Interface Methods
        public override string McsSettingsDebugField
        {
            get { return "mcs_appointmentplugin"; }
        }
        public override Entity GetPrimaryEntity()
        {
            return (Entity)PluginExecutionContext.PreEntityImages["pre"];
        }
        public override Entity GetSecondaryEntity()
        {
            return (Entity)PluginExecutionContext.PreEntityImages["pre"];
        }
        #endregion
    }
}