using MCSShared;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VA.TMP.DataModel;
using VA.TMP.OptionSets;

namespace VA.TMP.CRM.CVTStagingComponent
{
    public class StagingComponentUpdatePostStageRunner : PluginRunner
    {
        #region Constructor
        public StagingComponentUpdatePostStageRunner(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
        #endregion
        public override string McsSettingsDebugField
        {
            get { return "mcs_stagingcomponentplugin"; }
        }

        public override void Execute()
        {
            if (PrimaryEntity.LogicalName != cvt_stagingcomponent.EntityLogicalName)
                return;
            try
            {
                cvt_stagingcomponent stgComp = PrimaryEntity.ToEntity<cvt_stagingcomponent>();
                cvt_stagingcomponent stgCompPre = PluginExecutionContext.PreEntityImages["PreImage"]?.ToEntity<cvt_stagingcomponent>();

                if (stgComp != null && stgCompPre != null)
                {
                    if (stgComp.cvt_connectedcomponentid == null || !stgComp.cvt_connectedcomponentid.Equals(stgCompPre.cvt_connectedcomponentid))
                    {
                        using (var srv = new Xrm(OrganizationService))
                        {
                            if (stgCompPre != null)
                            {
                                if (stgCompPre.cvt_connectedcomponentid != null)
                                {
                                    var comp = srv.cvt_stagingcomponentSet.First(x => x.Id == stgComp.Id);

                                    DeleteMismatchCompRecords(comp, srv);

                                    srv.SaveChanges();
                                }
                                else if (stgComp.cvt_connectedcomponentid != null)
                                {
                                    var comp = srv.cvt_stagingcomponentSet.First(x => x.Id == stgComp.Id);

                                    CreateComponentsMismatchRecords(comp, srv);

                                    srv.SaveChanges();
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.WriteToFile(ex.ToString());
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

        private void AddMismatchRecordsToContext(List<Entity> mismatches, Xrm srv)
        {
            foreach (cvt_fieldmismatch mismatch in mismatches)
            {
                srv.AddObject(mismatch);
            }
        }
        private void DeleteMismatchCompRecords(cvt_stagingcomponent stagingComponent, Xrm srv)
        {
            Relationship mismatchRelationship = new Relationship("cvt_stagingcomponent_fieldmismatch");

            stagingComponent.cvt_connectedcomponentid = null;
            stagingComponent.cvt_action = new OptionSetValue((int)cvt_stagingcomponentcvt_action.CreateNewComponent);
            srv.UpdateObject(stagingComponent);
            var mismatches = srv.cvt_fieldmismatchSet.Where(x => x.cvt_stagingcomponentId.Id == stagingComponent.Id && x.statecode == cvt_fieldmismatchState.Active).ToList<Entity>();

            foreach (cvt_fieldmismatch mismatch in mismatches)
            {
                OrganizationService.Delete(cvt_fieldmismatch.EntityLogicalName, mismatch.Id);
            }
        }
        private void CreateComponentsMismatchRecords(cvt_stagingcomponent stgComp, Xrm srv)
        {
            var stgResource = srv.cvt_stagingresourceSet.First(x => x.Id == stgComp.cvt_relatedresourceid.Id);

            var compToFind = srv.cvt_componentSet.FirstOrDefault(
                x => x.Id == stgComp.cvt_connectedcomponentid.Id &&
                x.statecode.Value == cvt_componentState.Active);

            if (compToFind != null)
            {
                List<Entity> mismatch = CvtHelper.FindMismatch(compToFind, stgComp, stgResource);

                AddMismatchRecordsToContext(mismatch, srv);
            }
        }
    }
}
