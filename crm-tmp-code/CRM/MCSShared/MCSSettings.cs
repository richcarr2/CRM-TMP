using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.ServiceModel;

namespace MCSUtilities2011
{
    public class MCSSettings
    {
        private IOrganizationService _Service;
        public IOrganizationService setService
        {
            set => _Service = value;
        }
        private MCSLogger _Logger;
        public MCSLogger setLogger
        {
            set => _Logger = value;
        }
        private static bool _Debug;
        public bool getDebug => _Debug;

        private static bool _GranularTxnTiming;
        public bool getGranular => _GranularTxnTiming;
        private static bool _Transactionaltimings;
        public bool getTxnTiming => _Transactionaltimings;
        private string _DebugField;
        public string setDebugField
        {
            set => _DebugField = value;
        }
        private string _SystemSetting;
        public string systemSetting
        {
            get => _SystemSetting;
            set => _SystemSetting = value;
        }
        private string _UnexpectedMessage;
        public string getUnexpectedErrorMessage => _UnexpectedMessage;
        private Guid _SettingId;
        public Guid getSettingsId => _SettingId;

        public void GetStartupSettings()
        {
            try
            {
                _UnexpectedMessage = string.Empty;

                QueryByAttribute query = new QueryByAttribute
                {
                    ColumnSet = new ColumnSet(true),
                    EntityName = "mcs_setting"
                };
                query.AddAttributeValue("mcs_name", _SystemSetting);

                EntityCollection results = _Service.RetrieveMultiple(query);

                if (results.Entities.Count > 0)
                {
                    _SettingId = (Guid)results.Entities[0]["mcs_settingid"];

                    _GranularTxnTiming = (bool)results.Entities[0]["mcs_granulartimings"];
                    _Transactionaltimings = (bool)results.Entities[0]["mcs_transactionaltiming"];
                    if (results.Entities[0].Contains(_DebugField))
                    {
                        bool myOpt = (bool)results.Entities[0][_DebugField];
                        _Debug = myOpt;
                    }
                    else
                    {
                        _Logger.WriteToFile("No debug field found:" + _DebugField);
                        _Debug = true;
                    }
                }
                else
                {
                    _Logger.WriteToFile("Active Settings not Found");
                    _GranularTxnTiming = false;
                    _Transactionaltimings = false;
                    _Debug = false;

                }

            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                _Logger.setMethod = "GetStartupSettings";
                _Logger.WriteToFile(ex.Message);

            }
            catch (Exception ex)
            {
                _Logger.setMethod = "GetStartupSettings";
                _Logger.WriteToFile(ex.Message);
            }
        }
        public string GetSingleSetting(string field, string fieldType)
        {
            try
            {
                string returnvalue = null;
                QueryByAttribute query = new QueryByAttribute
                {
                    ColumnSet = new ColumnSet(field),
                    EntityName = "mcs_setting"
                };
                //_Logger.WriteToFile("_SystemSetting:" + _SystemSetting);
                query.AddAttributeValue("mcs_name", _SystemSetting);

                EntityCollection results = _Service.RetrieveMultiple(query);
                if (results.Entities[0].Attributes.Contains(field))
                {
                    switch (fieldType.ToLower())
                    {
                        case "entityreference":
                            EntityReference myRef = (EntityReference)results.Entities[0][field];
                            returnvalue = myRef.Id.ToString();
                            break;
                        case "optionsetvalue":
                            OptionSetValue myOpt = (OptionSetValue)results.Entities[0][field];
                            returnvalue = myOpt.Value.ToString();
                            break;
                        case "string":
                            returnvalue = results.Entities[0][field].ToString();
                            break;
                        default:
                            break;
                    }
                }

                return returnvalue;

            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                _Logger.setMethod = "GetSingleSettings";
                _Logger.WriteToFile(ex.Message);
                return null;

            }
            catch (Exception ex)
            {
                _Logger.setMethod = "GetSingleSettings";
                _Logger.WriteToFile(ex.Message);
                return null;
            }
        }

    }
}
