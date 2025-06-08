using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using VA.TMP.DataModel;

namespace VA.TMP.CRM
{
    public static class UrlBuilder
    {
        /// <summary>
        /// Creates a url from a template stored in the URL Definition entity.
        /// </summary>
        /// <param name="orgService">The CRM organization service.</param>
        /// <param name="pluginName">The name of the plugin calling this method.</param>
        /// <param name="primaryEntity">The entity that contains the majority of the data to use in the url.</param>
        /// <param name="secondaryEntities">Any additional entities that contain data needed in the url.</param>
        /// <param name="url">The formatted url.</param>
        /// <returns>True if the url is created; False otherwise.</returns>
        public static bool TryGetUrl(IOrganizationService orgService, string pluginName, Entity primaryEntity, List<Entity> secondaryEntities, out string url)
        {
            url = string.Empty;
            var returnFalse = false;

            using (var crm = new Xrm(orgService))
            {
                //a manufacturer and model is required to determine the correct url format to use.
                if (!primaryEntity.Attributes.ContainsKey("cvt_manufacturerid") || !primaryEntity.Attributes.ContainsKey("cvt_modelnumber"))
                    return returnFalse;

                var manufacturerId = ((EntityReference)primaryEntity.Attributes["cvt_manufacturerid"]).Id;
                var modelNumberId = ((EntityReference)primaryEntity.Attributes["cvt_modelnumber"]).Id;

                //get the definition
                var urlRecord = (from u in crm.CreateQuery<cvt_urldefinition>()
                                 where u.cvt_pluginname == pluginName &&
                                       u.cvt_sourceentity == primaryEntity.LogicalName &&
                                       u.cvt_Manufacturer.Id == manufacturerId &&
                                       u.cvt_model.Id == modelNumberId
                                 select new
                                 {
                                     Id = u.cvt_urldefinitionId,
                                     UrlFormat = u.cvt_urlformat
                                 }).FirstOrDefault();

                if (urlRecord != null)
                {
                    //get the variables associated with the definition
                    var urlVariables = (from v in crm.CreateQuery<cvt_urlvariable>()
                                        where v.cvt_urldefinition.Id == urlRecord.Id
                                        orderby v.cvt_index
                                        select new
                                        {
                                            Index = v.cvt_index,
                                            SchemaName = v.cvt_schemaname
                                        }).ToList();

                    if (urlVariables != null && urlVariables.Count > 0)
                    {
                        var variableValues = new List<string>();

                        //convert the variable names into values from the entities received from the plugin
                        foreach (var variable in urlVariables)
                        {
                            var parts = variable.SchemaName.Split('.');

                            //if the variables are not in the dotted format, return false;
                            if (parts.Length != 2)
                                return returnFalse;

                            Entity source = null;
                            if (parts[0] == primaryEntity.LogicalName)
                                source = primaryEntity;
                            else
                            {
                                //if no secondary entity array, return false
                                if (secondaryEntities == null || secondaryEntities.Count == 0)
                                    return returnFalse;

                                //find the entity in the secondaryEntity list. if not there, return false
                                source = secondaryEntities.FirstOrDefault(s => s.LogicalName.ToLower() == parts[0].ToLower());

                                if (source == null)
                                    return returnFalse;
                            }

                            if (!source.Attributes.ContainsKey(parts[1]))
                                return returnFalse;

                            var attribute = source.Attributes[parts[1]];

                            if (attribute.GetType() == typeof(System.DateTime))
                            {
                                var encodedTime = ((DateTime)attribute).ToString("yyyy-MM-ddTHH:mm:ssZ");
                                variableValues.Add(encodedTime);
                            }
                            else
                                variableValues.Add(attribute.ToString());
                        }

                        try
                        {
                            //create the url string
                            url = string.Format(urlRecord.UrlFormat, variableValues.ToArray());
                            return true;
                        }
                        catch
                        {
                            return returnFalse;
                        }
                    }
                    else
                        return returnFalse;
                }
                else
                    return returnFalse;
            }
        }
    }
}
