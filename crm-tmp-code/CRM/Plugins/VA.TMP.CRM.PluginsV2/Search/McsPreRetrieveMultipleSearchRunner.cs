using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VA.TMP.CRM
{
    internal class McsPreRetrieveMultipleSearchRunner : PluginRunner
    {
        private IServiceProvider serviceProvider;

        public McsPreRetrieveMultipleSearchRunner(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public override string McsSettingsDebugField
        {
            get { return "McsPreRetrieveMultipleSearch"; }
        }

        public override void Execute()
        {
            Microsoft.Xrm.Sdk.IPluginExecutionContext context = (Microsoft.Xrm.Sdk.IPluginExecutionContext)
            serviceProvider.GetService(typeof(Microsoft.Xrm.Sdk.IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            if (!context.InputParameters.Contains("Query"))
                return;

            if (context.InputParameters["Query"].GetType().Equals(typeof(FetchExpression)))
            {

                FetchExpression query = (FetchExpression)context.InputParameters["Query"];
                string fetchXml = (context.InputParameters["Query"] as FetchExpression).Query;

                if (fetchXml.Contains("isquickfindfields"))
                {
                    string tempText = fetchXml.Substring(fetchXml.IndexOf(@"like") + 13);
                    string testTobeReplaced = tempText.Split('/')[0].Replace("\"", "");
                    string tempTexts = testTobeReplaced.Replace("%","");
                    fetchXml = fetchXml.Replace(testTobeReplaced, '%' + tempTexts + '%');
                }
                query.Query = fetchXml;
                context.InputParameters["Query"] = query;

            }
            if (context.InputParameters["Query"].GetType().Equals(typeof(QueryExpression)))
            {
                QueryExpression query = (QueryExpression)context.InputParameters["Query"];
                foreach(ConditionExpression condition in query.Criteria.Conditions)
                {
                    if (condition.Operator == ConditionOperator.Like)
                    {
                        for (int i = 0; i < condition.Values.Count; i++)
                        {
                            condition.Values[i] = '%' + condition.Values[i].ToString().Replace(@"%", "") + '%';
                        }
                    }
                }

                foreach(FilterExpression conditions in  query.Criteria.Filters)
                {
                    foreach(ConditionExpression condition in conditions.Conditions)
                    {
                        if(condition.Operator == ConditionOperator.Like)
                        {
                            for( int i = 0; i < condition.Values.Count; i++)
                            {
                                condition.Values[i] = '%' + condition.Values[i].ToString().Replace(@"%", "") + '%';
                            }
                        }
                    }
                    
                }
               
                context.InputParameters["Query"] = query;
            }

        }
    }
}
