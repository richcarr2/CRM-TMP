using MCSShared;
using MCSUtilities2011;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;


namespace VA.TMP.CRM
{
    public class SPEmailNotification
    {

        internal static void ResourcePackage(IOrganizationService OrganizationService, Entity resourcePackage, Entity participatingSite)
        {
            if (resourcePackage == null)
            {
                return;
            }
            //Hub Value is null-- Email goes to Provider Facility Team
            if (resourcePackage.Attributes["cvt_usagetype"] != null && resourcePackage.GetAttributeValue<bool>("cvt_usagetype") == false)
            {

                if (resourcePackage.GetAttributeValue<EntityReference>("cvt_providerfacility") != null && resourcePackage.GetAttributeValue<EntityReference>("cvt_hub") == null)
                {
                    EntityReference providerFacility = resourcePackage.GetAttributeValue<EntityReference>("cvt_providerfacility");

                    if (providerFacility == null)
                    {
                        return;
                    }

                    SendEmail(OrganizationService, (int)Teamcvt_Type.FTC, resourcePackage, providerFacility, "ProviderFacility", participatingSite);
                }

                //Hub Value is Equal to Provider Value -- Email goes to Provider Facility Team
                if (resourcePackage.GetAttributeValue<EntityReference>("cvt_hub") != null && resourcePackage.GetAttributeValue<EntityReference>("cvt_providerfacility") != null && resourcePackage.GetAttributeValue<EntityReference>("cvt_hub").Id == resourcePackage.GetAttributeValue<EntityReference>("cvt_providerfacility").Id)
                {
                    EntityReference providerFacility = resourcePackage.GetAttributeValue<EntityReference>("cvt_providerfacility");

                    if (providerFacility == null)
                    {
                        return;
                    }

                    SendEmail(OrganizationService, (int)Teamcvt_Type.FTC, resourcePackage, providerFacility, "ProviderFacility", participatingSite);
                }

                //Hub Value is not Equal to Provider Value -- Email goes to Provider Facility Team and Hub Facility Team
                if (resourcePackage.GetAttributeValue<EntityReference>("cvt_hub") != null && resourcePackage.GetAttributeValue<EntityReference>("cvt_providerfacility") != null && resourcePackage.GetAttributeValue<EntityReference>("cvt_hub").Id != resourcePackage.GetAttributeValue<EntityReference>("cvt_providerfacility").Id)
                {
                    EntityReference providerFacility = resourcePackage.GetAttributeValue<EntityReference>("cvt_providerfacility");

                    EntityReference hub = resourcePackage.GetAttributeValue<EntityReference>("cvt_hub");

                    if (providerFacility != null)
                    {

                        SendEmail(OrganizationService, (int)Teamcvt_Type.FTC, resourcePackage, providerFacility, "ProviderFacility", participatingSite);
                    }

                    if (hub != null)
                    {

                        SendEmail(OrganizationService, (int)Teamcvt_Type.FTC, resourcePackage, hub, "HUB", participatingSite);
                    }

                }
            }


            //Patient Facility -- Email goes to Patient Facility Team
            if (resourcePackage.Attributes["cvt_groupappointment"] != null && resourcePackage.GetAttributeValue<bool>("cvt_groupappointment") && resourcePackage.GetAttributeValue<OptionSetValue>("cvt_intraorinterfacility") != null && resourcePackage.GetAttributeValue<OptionSetValue>("cvt_intraorinterfacility").Value == (int)cvt_resourcepackagecvt_intraorinterfacility.Interfacility && resourcePackage.GetAttributeValue<EntityReference>("cvt_patientfacility") != null)
            {

                EntityReference patientFacility = resourcePackage.GetAttributeValue<EntityReference>("cvt_patientfacility");

                if (patientFacility == null)
                {
                    return;
                }
                SendEmail(OrganizationService, (int)Teamcvt_Type.FTC, resourcePackage, patientFacility, "PatientFacility", participatingSite);

            }
        }

        internal static void SendEmail(IOrganizationService OrganizationService, int teamtype, Entity resourcePackage, EntityReference entityReference, string bodyFlag, Entity participatingSite)
        {
            using (var srv = new Xrm(OrganizationService))
            {
                var activeSettings = srv.mcs_settingSet.FirstOrDefault(s => s.mcs_name == "Active Settings");
                if (activeSettings != null)
                {

                    if (activeSettings.mcs_BypassEmail != null && activeSettings.mcs_BypassEmail.Value)
                    {

                    }
                    else
                    {
                        string hub = resourcePackage.GetAttributeValue<EntityReference>("cvt_hub") != null ? resourcePackage.GetAttributeValue<EntityReference>("cvt_hub").Name : string.Empty;

                        string specialtyText = resourcePackage.GetAttributeValue<EntityReference>("cvt_specialty") != null ? resourcePackage.GetAttributeValue<EntityReference>("cvt_specialty").Name : string.Empty;

                        string specialtySubType = resourcePackage.GetAttributeValue<EntityReference>("cvt_specialtysubtype") != null ? resourcePackage.GetAttributeValue<EntityReference>("cvt_specialtysubtype").Name : string.Empty;

                        string providerFacility = resourcePackage.GetAttributeValue<EntityReference>("cvt_providerfacility") != null ? resourcePackage.GetAttributeValue<EntityReference>("cvt_providerfacility").Name : string.Empty;

                        string patientFacility = resourcePackage.GetAttributeValue<EntityReference>("cvt_patientfacility") != null ? resourcePackage.GetAttributeValue<EntityReference>("cvt_patientfacility").Name : string.Empty;

                            var team = srv.TeamSet.FirstOrDefault(t => t.cvt_Type.Value == (int)teamtype && t.cvt_Facility.Id == entityReference.Id);

                            if (team == null)
                            {
                                return;
                            }

                            QueryExpression query = new QueryExpression("systemuser");
                            query.Criteria.AddCondition("internalemailaddress", ConditionOperator.Equal, "Joe.Brian@va.gov");

                            EntityCollection coll = OrganizationService.RetrieveMultiple(query);
                        if (coll.Entities.Count > 0)
                        {
                            Entity systemUser = coll.Entities[0];


                            Entity fromParty = new Entity("activityparty");
                            fromParty["partyid"] = new EntityReference(systemUser.LogicalName, systemUser.Id);

                            Entity email = new Entity("email");

                            email["from"] = new Entity[] { fromParty };
                            string body = string.Empty;
                            string subjectText = string.Empty;
                            if (participatingSite == null)
                            {

                                subjectText = specialtySubType != null && specialtySubType != string.Empty ? "A TMP Scheduling Package to your Facility has been created for " + specialtyText + " : " + specialtySubType : "A TMP Scheduling Package to your Facility has been created for " + specialtyText;

                                email["subject"] = subjectText;
                                email["regardingobjectid"] = new EntityReference(resourcePackage.LogicalName, resourcePackage.Id);

                                body += subjectText;
                                body += "<br/>";

                                if (bodyFlag == "HUB")
                                {
                                    body += "<ul><b><li>Hub Facility:  " + hub + "</li></b><li>Provider Facility:  " + providerFacility + "</li><li>Patient Facility:  " + patientFacility + "</li></ul>";
                                }
                                else if (bodyFlag == "ProviderFacility")
                                {
                                    body += "<ul><li>Hub Facility:  " + hub + "</li><b><li>Provider Facility:  " + providerFacility + "</li></b><li>Patient Facility:  " + patientFacility + "</li></ul>";
                                }
                                else
                                {
                                    body += "<ul><li>Hub Facility:  " + hub + "</li><li>Provider Facility:  " + providerFacility + "</li><b><li>Patient Facility:  " + patientFacility + "</li></b></ul>";
                                }

                                body += "Please coordinate with the Provider Facility FTC or TMP Admin to set up the following Scheduling Package. <br/>";
                                body += "<br/>";
                                body += GetRecordLink(resourcePackage, OrganizationService, "Click Here to view this TMP Scheduling Package");
                                body += "<br/><br/>";
                                body += "Once all TMP Users and Resources are finalized, the Scheduling Package will be put into a ‘Can Be Scheduled’ Status for each side of the encounter and scheduling can start. <br/>";
                                body += "<br/><br/>";
                                body += "<i>NOTE:  Please Do Not Reply to this message.  It comes from an unmonitored mailbox.  For any questions or concerns, please contact your VA Facility or VA Clinical Team.</i>";

                            }
                            else
                            {

                                string tmpSiteString = participatingSite.GetAttributeValue<EntityReference>("cvt_site") != null ? participatingSite.GetAttributeValue<EntityReference>("cvt_site").Name : string.Empty;

                                subjectText = specialtySubType != null && specialtySubType != string.Empty ? " A TMP Participating Site to your Facility has been created for " + specialtyText + " : " + specialtySubType + " at " + tmpSiteString : " A TMP Participating Site to your Facility has been created for " + specialtyText + " at " + tmpSiteString;

                                email["subject"] = subjectText;
                                email["regardingobjectid"] = new EntityReference(resourcePackage.LogicalName, resourcePackage.Id);

                                body += subjectText;
                                body += "<br/>";

                                if (bodyFlag == "HUB")
                                {
                                    body += "<ul><b><li>Hub Facility:  " + hub + "</li></b><li>Provider Facility:  " + providerFacility + "</li><li>Patient Facility:  " + patientFacility + "</li></ul>";
                                }
                                else if (bodyFlag == "ProviderFacility")
                                {
                                    body += "<ul><li>Hub Facility:  " + hub + "</li><b><li>Provider Facility:  " + providerFacility + "</li></b><li>Patient Facility:  " + patientFacility + "</li></ul>";
                                }
                                else
                                {
                                    body += "<ul><li>Hub Facility:  " + hub + "</li><li>Provider Facility:  " + providerFacility + "</li><b><li>Patient Facility:  " + patientFacility + "</li></b></ul>";
                                }

                                body += "Please coordinate with the Provider Facility FTC or TMP Admin to set up the following Scheduling Package. <br/>";
                                body += "<br/>";
                                body += GetRecordLink(resourcePackage, OrganizationService, "Click Here to view this TMP Scheduling Package");
                                body += "<br/><br/>";
                                body += GetRecordLink(participatingSite, OrganizationService, "Click Here to view this Participating Site");
                                body += "<br/><br/>";
                                body += "Once all TMP Users and Resources are finalized, the Scheduling Package will be put into a ‘Can Be Scheduled’ Status for each side of the encounter and scheduling can start. <br/>";
                                body += "<br/><br/>";
                                body += "<i>NOTE:  Please Do Not Reply to this message.  It comes from an unmonitored mailbox.  For any questions or concerns, please contact your VA Facility or VA Clinical Team.</i>";
                            }


                            email["description"] = body;
                            Guid newEmailID = OrganizationService.Create(email);

                            Email newEmail = new Email()
                            {
                                Id = newEmailID,
                                To = null
                            };
                            var toListCount = RetrieveTeamMembers(OrganizationService, newEmail, team.Id);
                            if (toListCount.Count > 0)
                            {
                                newEmail.To = RetrieveTeamMembers(OrganizationService, newEmail, team.Id);
                                OrganizationService.Update(newEmail);
                                SendEmailRequest request = new SendEmailRequest
                                {
                                    EmailId = newEmailID,
                                    IssueSend = true,
                                    TrackingToken = ""
                                };
                                OrganizationService.Execute(request);
                            }
                            else
                            {
                                throw new InvalidPluginExecutionException("TO List is Empty. Please check the team.");
                            }

                        }
                    }
                }
            }
        }

        internal static string GetRecordLink(Entity record, IOrganizationService OrganizationService, string clickHereText = "")
        {
            var etc = CvtHelper.GetEntityTypeCode(OrganizationService, record.LogicalName);
            string servernameAndOrgname = CvtHelper.getServerURL(OrganizationService);
            string url = servernameAndOrgname + "/userDefined/edit.aspx?etc=" + etc + "&id=" + record.Id;
            return String.Format("<a href=\"{0}\">{1}</a>", url, !string.IsNullOrEmpty(clickHereText) ? clickHereText : url);
        }


        internal static List<ActivityParty> RetrieveTeamMembers(IOrganizationService OrganizationService, Email email, Guid TeamId)
        {
            //TracingService.Trace("Starting RetrieveFacilityTeamMembers function");
           

            using (var srv = new Xrm(OrganizationService))
            {
                var teamMembers = srv.TeamMembershipSet.Where(t => t.TeamId == TeamId).ToList();
                var recipientList = new List<ActivityParty>();

                foreach (var member in teamMembers)
                {
                    var user = srv.SystemUserSet.FirstOrDefault(u => u.Id == member.SystemUserId && u.IsDisabled == false);

                    if (user != null)
                    {
                        if ((!String.IsNullOrEmpty(user.InternalEMailAddress)))
                        {
                            var party = new ActivityParty()
                            {
                                ActivityId = new EntityReference(email.LogicalName, email.Id),
                                PartyId = new EntityReference(SystemUser.EntityLogicalName, user.Id)
                            };
                            recipientList.Add(party);
                        }

                    }
                }
                return recipientList;
            }
        }

    }
}
