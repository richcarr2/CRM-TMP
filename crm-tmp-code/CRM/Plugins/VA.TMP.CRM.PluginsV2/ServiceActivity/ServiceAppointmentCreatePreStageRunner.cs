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
    public class ServiceAppointmentCreatePreStageRunner : AILogicBase
    {
        #region Constructor
        public ServiceAppointmentCreatePreStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        { }

        #endregion

        #region Execute
        public override void ExecuteLogic()
        {
            if (PrimaryEntity.LogicalName != ServiceAppointment.EntityLogicalName)
                throw new Exception("Target entity is not of type ServiceAppointment");
            CheckVistaIENs(PrimaryEntity);
            //SetSearchText(PrimaryEntity);
            SetCernerCommentsField(PrimaryEntity);
        }

        public void CheckVistaIENs(Entity PrimaryEntity)
        {
            //Logger.setMethod = "CheckVistaIENs";
            //Logger.WriteDebugMessage("Starting 'CheckVistaIENs' method.");
            Trace("Starting 'CheckVistaIENs' method.", LogLevel.Debug);
            if (PrimaryEntity.LogicalName.ToLower() != ServiceAppointment.EntityLogicalName.ToLower())
            {
                //Logger.WriteToFile($"ERROR: Plugin is currently registered with entity {PrimaryEntity.LogicalName} i.e., different from Service Appointment");
                Trace($"ERROR: Plugin is currently registered with entity {PrimaryEntity.LogicalName} i.e., different from Service Appointment", LogLevel.Debug);
                return;
            }

            //Review all the associated clinic resources for the appointment. Verify if any clinic do not have an IEN or have a alphanumeric IEN and throw the error message.
            if (PrimaryEntity.Attributes != null && PrimaryEntity.Attributes.Contains("resources"))
            {
                var resources = (EntityCollection)PrimaryEntity.Attributes["resources"];
                using (var srv = new Xrm(OrganizationService))
                {
                    foreach (Entity party in resources.Entities)
                    {
                        foreach (var resource in party.Attributes)
                        {
                            if (resource.Value.GetType() == typeof(EntityReference))
                            {
                                EntityReference reference = (EntityReference)resource.Value;
                                if (reference.LogicalName == Equipment.EntityLogicalName)
                                {
                                    var clinic = (from e in srv.EquipmentSet
                                                  join r in srv.mcs_resourceSet on e.mcs_relatedresource.Id equals r.mcs_resourceId.Value
                                                  where e.cvt_type.Value == 251920000 && e.EquipmentId.Value == reference.Id
                                                  select r).FirstOrDefault();
                                    if (clinic != null)
                                    {
                                        if (string.IsNullOrEmpty(clinic.cvt_ien) || !int.TryParse(clinic.cvt_ien, out var n))
                                        {
                                            throw new InvalidPluginExecutionException($"customThe appointment could not be saved as the Clinic associated to the appointment {clinic.mcs_name} does not have a valid IEN");
                                        }
                                    }
                                    else
                                    {
                                        //Logger.WriteDebugMessage($"Skipping as the equipment {reference.Name}, Id: {reference.Id} is either not a clinic or the resource was not found.");
                                        Trace($"Skipping as the equipment {reference.Name}, Id: {reference.Id} is either not a clinic or the resource was not found.", LogLevel.Debug);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //Logger.WriteDebugMessage("Finished 'CheckVistaIENs' method.");
            Trace("Finished 'CheckVistaIENs' method.", LogLevel.Debug);
        }

        //public void SetSearchText(Entity PrimaryEntity)
        //{
        //    using (var srv = new Xrm(OrganizationService))
        //    {
        //        if (PrimaryEntity.Attributes != null && (PrimaryEntity.Attributes.Contains("customers") || PrimaryEntity.Attributes.Contains("subject")))
        //        {
        //            string searchText = "";
        //            var customers = (EntityCollection)PrimaryEntity.Attributes["customers"];

        //            string filter = "";
        //            foreach (Entity party in customers.Entities)
        //            {
        //                filter += "<condition attribute='contactid' operator='eq' value='{" + ((EntityReference)party["partyid"]).Id + "}' />";

        //            }
        //            string fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
        //                          "<entity name='contact'>" +
        //                            "<attribute name='tmp_searchtext' />" +
        //                            "<filter type='or'>" +
        //                             filter +
        //                            "</filter>" +
        //                          "</entity>" +
        //                        "</fetch>";

        //            EntityCollection entCol = OrganizationService.RetrieveMultiple(new FetchExpression(fetchXml));
        //            foreach (Entity ent in entCol.Entities)
        //            {
        //                searchText += ent.Attributes.Contains("tmp_searchtext") ? ent.Attributes["tmp_searchtext"] + ";" : "";
        //            }
        //            if (PrimaryEntity.Attributes.Contains("customers"))
        //            {
        //                PrimaryEntity.Attributes["tmp_searchtext"] = searchText;
        //            }
        //            else
        //            {
        //                PrimaryEntity.Attributes.Add("tmp_searchtext", searchText);
        //            }
        //        }

        //    }
        //}

        public void SetCernerCommentsField(Entity serviceAppointment)
        {
            Logger.WriteDebugMessage("Started 'SetCernerCommentsField' method.");

            using (var srv = new Xrm(OrganizationService))
            {
                // cvt_cernercomments
                var cvtComments = string.Empty;
                var cernerComments = string.Empty;
                var verifiedPhone = string.Empty;
                var verifiedEmail = string.Empty;
                var teamMembers = string.Empty;

                if (serviceAppointment.Attributes != null)
                {
                    Logger.WriteDebugMessage("serviceAppointment.Attributes != null");
                    if (serviceAppointment.Attributes.Contains("cvt_comments"))
                    {
                        Logger.WriteDebugMessage("serviceAppointment.Attributes.Contains(\"cvt_comments\")");
                        cvtComments = $"{serviceAppointment.Attributes["cvt_comments"]}";
                        cvtComments = RemoveProblemChars(cvtComments);
                    }

                    // check modality
                    if (serviceAppointment.Attributes.Contains("tmp_appointmentmodality"))
                    {
                        Logger.WriteDebugMessage($"serviceAppointment.Attributes.Contains(\"tmp_appointmentmodality\") : {((OptionSetValue)serviceAppointment.Attributes["tmp_appointmentmodality"]).Value}");
                        switch (((OptionSetValue)serviceAppointment.Attributes["tmp_appointmentmodality"]).Value)
                        {
                            case 178970002://VVC
                            case 178970003://VVC w/PSRR
                            case 178970004://PHONE
                            case 178970005://PHONE w/PSRR
                                Logger.WriteDebugMessage("Started adding primary verified phone and email to Cerner Comments in 'SetCernerCommentsField' method.");
                                if (serviceAppointment.Attributes.Contains("cvt_patientverifiedphone"))
                                {
                                    Logger.WriteDebugMessage("serviceAppointment.Attributes.Contains(\"cvt_patientverifiedphone\")");
                                    verifiedPhone = $"PVP:{serviceAppointment.Attributes["cvt_patientverifiedphone"]};";
                                    if (serviceAppointment.Attributes.Contains("cvt_patientverifiedemail"))
                                    {
                                        verifiedEmail = $"PVE:{serviceAppointment.Attributes["cvt_patientverifiedemail"]};";
                                    }
                                }
                                else if (serviceAppointment.Attributes.Contains("cvt_patientverifiedemail"))
                                {
                                    Logger.WriteDebugMessage("serviceAppointment.Attributes.Contains(\"cvt_patientverifiedemail\")");
                                    verifiedEmail = $"PVE:{serviceAppointment.Attributes["cvt_patientverifiedemail"]};";
                                }
                                break;
                            case 178970000: //CVT
                            case 178970001: //SFT
                            case 178970006: //CVT GRP
                                Logger.WriteDebugMessage("Started adding each patient site TMP Site Team member's fullname, phone, and email to Cerner Comments in 'SetCernerCommentsField' method.");
                                // iterate the patient site's tmp site team and extract for each systemuser record:
                                // fullname, officephone, primaryemail
                                if (serviceAppointment.Attributes.Contains("mcs_relatedsite"))
                                {
                                    EntityReference patientSiteId = (EntityReference)serviceAppointment.Attributes["mcs_relatedsite"];
                                    // retrieve patient site
                                    mcs_site patientSite = srv.mcs_siteSet.FirstOrDefault(s => s.Id == patientSiteId.Id);
                                    if (patientSite.cvt_TSSSiteTeam != null)
                                    {
                                        Logger.WriteDebugMessage("Patient Site for Cerner Comments exists in 'SetCernerCommentsField' method.");
                                        var teamId = patientSite.cvt_TSSSiteTeam;
                                        Team siteTeam = srv.TeamSet.FirstOrDefault(t => t.Id == teamId.Id);
                                        if (siteTeam != null)
                                        {
                                            Logger.WriteDebugMessage("TMP Site Team for Cerner Comments exists in 'SetCernerCommentsField' method.");
                                            // retrieve site's TMP Site Team members and iterate
                                            List<TeamMembership> siteTeamMembers = (from tm in srv.TeamMembershipSet where tm.TeamId == siteTeam.Id select tm).ToList();
                                            if (siteTeamMembers.Count > 0)
                                            {
                                                Logger.WriteDebugMessage("TMP Site Team has " + siteTeamMembers.Count.ToString() + " members for Cerner Comments exists in 'SetCernerCommentsField' method.");
                                            }
                                            foreach (TeamMembership teamUser in siteTeamMembers)
                                            {
                                                SystemUser user = srv.SystemUserSet.FirstOrDefault(u => u.Id == teamUser.SystemUserId);
                                                if (user != null)
                                                {
                                                    teamMembers = $"{teamMembers}{user.FullName},{user.cvt_officephone},{user.InternalEMailAddress};";
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                    Logger.WriteDebugMessage("No Patient Site for which to add team members to Cerner Comments in 'SetCernerCommentsField' method.");
                                break;
                            default:
                                break;
                        }
                        if (!string.IsNullOrWhiteSpace(verifiedPhone) || !string.IsNullOrWhiteSpace(verifiedEmail) || !string.IsNullOrWhiteSpace(teamMembers))
                        {
                            var cvtCommentsSegment = string.Empty;
                            if (!string.IsNullOrWhiteSpace(cvtComments))
                            {
                                cvtCommentsSegment = $"{cvtComments};";
                            }

                            var cernerCommentsSegment = $"{cvtCommentsSegment}{verifiedPhone}{verifiedEmail}{teamMembers}";

                            // remove final ';' character
                            cernerCommentsSegment = cernerCommentsSegment.Substring(0, cernerCommentsSegment.Length - 1);
                            // replace/remove problem characters
                            cernerCommentsSegment = RemoveProblemChars(cernerCommentsSegment);

                            if (cernerCommentsSegment.Length > 250)
                            {
                                Logger.WriteDebugMessage("cernerCommentsSegment.Length > 250");
                                cernerCommentsSegment = cernerCommentsSegment.Substring(0, 250);
                            }

                            // Commenting out until VDIF approves Health Connect modifications to parse this string.
                            // Segment before '|' will go to VistA. Segment after will go to Cerner.
                            // Uncomment the line below when Health Connect has logic in place.
                            cernerComments = $"{cvtComments}|{cernerCommentsSegment}";

                            // For now send newly built comments string without pipe.
                            // Logic Apps will send cvt_cernercomments field to Health Connect
                            // It will get passed to both systems as is, so we'll cap it at 150 characters to meet VistA's restriction
                            // Remove the line below when Health Connect has logic in place. Also remove the truncation at 150 chars.
                            //cernerComments = cernerCommentsSegment;
                            //if (cernerComments.Length > 150)
                            //{
                            //    Logger.WriteDebugMessage("cernerComments.Length > 150");
                            //    cernerComments = cernerComments.Substring(0, 150);
                            //}
                        }
                        else
                        {
                            cernerComments = cvtComments;
                        }

                        Logger.WriteDebugMessage($"cvtComments : {cvtComments}");
                        Logger.WriteDebugMessage($"cernerComments : {cernerComments}");

                        serviceAppointment.Attributes["cvt_cernercomments"] = cernerComments;
                        serviceAppointment.Attributes["cvt_comments"] = cvtComments;

                        if (!srv.IsAttached(serviceAppointment))
                        {
                            srv.Attach(serviceAppointment);
                        }
                        srv.UpdateObject(serviceAppointment);
                        Logger.WriteDebugMessage("Finished 'SetCernerCommentsField' method.");
                    }
                }
                else
                    Logger.WriteDebugMessage("No SA attributes passed into 'SetCernerCommentsField' method.");
            }
        }

        private string RemoveProblemChars(string inputString)
        {
            return inputString.Replace("\"", "'").Replace("/", "-");
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