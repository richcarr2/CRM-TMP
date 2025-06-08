using MCSShared;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;
using System.Text.RegularExpressions;
using Microsoft.Xrm.Sdk.Query;
using MCS.ApplicationInsights;

namespace VA.TMP.CRM
{
    public class ServiceAppointmentUpdatePreStageRunner : AILogicBase
    {
        #region Constructor
        public ServiceAppointmentUpdatePreStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        { }

        #endregion

        #region Execute
        public override void ExecuteLogic()
        {
            if (PrimaryEntity.LogicalName != ServiceAppointment.EntityLogicalName)
                throw new Exception("Target entity is not of type ServiceAppointment");
            SetSearchText(PrimaryEntity);
        }

        public void SetSearchText(Entity PrimaryEntity)
        {
            using (var srv = new Xrm(OrganizationService))
            {
                if (PrimaryEntity.Attributes != null && (PrimaryEntity.Attributes.Contains("customers") || PrimaryEntity.Attributes.Contains("subject")))
                {
                    string searchText = "";
                    var customers = (EntityCollection)PrimaryEntity.Attributes["customers"];
                    
                    string filter = "";
                    foreach (Entity party in customers.Entities)
                    {
                        if (filter.Length.Equals(0)) filter += "<filter type='or'>";
                        filter += "<condition attribute='contactid' operator='eq' value='{" + ((EntityReference)party["partyid"]).Id  + "}' />";                       
                    }
                    if (!filter.Length.Equals(0)) filter += "</filter>";

                    string fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                  "<entity name='contact'>" +
                                    "<attribute name='tmp_searchtext' />" +
                                     filter +
                                  "</entity>" +
                                "</fetch>";

                    //Logger.WriteDebugMessage($"Patient Search fetchXml: {fetchXml}");
                    Trace($"Patient Search fetchXml: {fetchXml}", LogLevel.Debug);

                    EntityCollection entCol = OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));
                    foreach (Entity ent in entCol.Entities)
                    {
                        searchText += ent.Attributes.Contains("tmp_searchtext") ? ent.Attributes["tmp_searchtext"] + ";" : "";
                    }
                    if (PrimaryEntity.Attributes.Contains("customers"))
                    {
                        PrimaryEntity.Attributes["tmp_searchtext"] = searchText;
                    }
                    else
                    {
                        PrimaryEntity.Attributes.Add("tmp_searchtext", searchText);
                    }
                }

            }
        }
        #endregion

        #region Additional Interface Methods/Properties
        public override string McsSettingsDebugField
        {
            get { return "cvt_serviceactivityplugin"; }
        }
        #endregion
    }
}
