using Microsoft.Xrm.Sdk;
using System;

namespace MCSHelperClass
{
    public class MCSHelper
    {
        #region Private Fields
        private Entity _thisEntity;

        private Entity _preEntity;
        #endregion

        #region Constructor
        public MCSHelper(Entity thisEntity, Entity preEntity)
        {
            _thisEntity = thisEntity;

            _preEntity = preEntity;
        }

        public MCSHelper()
        {
        }
        #endregion

        #region Public Methods
        public Entity setThisEntity
        {
            set => _thisEntity = value;
        }

        public Entity setPreEntity
        {
            set => _preEntity = value;
        }

        public bool getBoolValue(string attributeLogicalName)
        {
            if (_thisEntity.Attributes.Contains(attributeLogicalName.ToLower()))
            {
                return (bool)_thisEntity[attributeLogicalName.ToLower()];
            }

            if (_preEntity.Attributes.Contains(attributeLogicalName.ToLower()))
            {
                return (bool)_preEntity[attributeLogicalName.ToLower()];
            }

            return new bool();
        }

        public int getIntValue(string attributeLogicalName)
        {
            if (_thisEntity.Attributes.Contains(attributeLogicalName.ToLower()))
            {
                return (int)_thisEntity[attributeLogicalName.ToLower()];
            }

            if (_preEntity.Attributes.Contains(attributeLogicalName.ToLower()))
            {
                return (int)_preEntity[attributeLogicalName.ToLower()];
            }

            return int.MinValue;
        }
        public string getStringValue(string attributeLogicalName)
        {
            if (_thisEntity.Attributes.Contains(attributeLogicalName.ToLower()))
            {
                return _thisEntity[attributeLogicalName.ToLower()].ToString();
            }

            if (_preEntity.Attributes.Contains(attributeLogicalName.ToLower()))
            {
                return _preEntity[attributeLogicalName.ToLower()].ToString();
            }

            return null;
        }
        public DateTime getDateTimeValue(string attributeLogicalName)
        {
            if (_thisEntity.Attributes.Contains(attributeLogicalName.ToLower()))
            {
                return (DateTime)_thisEntity[attributeLogicalName.ToLower()];
            }

            if (_preEntity.Attributes.Contains(attributeLogicalName.ToLower()))
            {
                return (DateTime)_preEntity[attributeLogicalName.ToLower()];
            }

            return DateTime.MinValue;
        }

        public object getObject(string attributeLogicalName)
        {
            if (_thisEntity.Attributes.Contains(attributeLogicalName.ToLower()))
            {
                return _thisEntity[attributeLogicalName.ToLower()];
            }

            if (_preEntity.Attributes.Contains(attributeLogicalName.ToLower()))
            {
                return _preEntity[attributeLogicalName.ToLower()];
            }

            return null;
        }

        public string getStringOptionSetValue(string attributeLogicalName)
        {

            if (_thisEntity.FormattedValues.Contains(attributeLogicalName.ToLower()))
            {
                return _thisEntity.FormattedValues[attributeLogicalName.ToLower()].ToString();
            }

            if (_preEntity.FormattedValues.Contains(attributeLogicalName.ToLower()))
            {
                return _preEntity.FormattedValues[attributeLogicalName.ToLower()].ToString();
            }

            return null;
        }

        public int getOptionSetValue(string attributeLogicalName)
        {
            OptionSetValue myOpt;

            if (_thisEntity.Attributes.Contains(attributeLogicalName.ToLower()))
            {
                myOpt = (OptionSetValue)_thisEntity[attributeLogicalName.ToLower()];

                if (myOpt != null)
                {
                    return myOpt.Value;
                }
            }

            if (_preEntity.Attributes.Contains(attributeLogicalName.ToLower()))
            {
                myOpt = (OptionSetValue)_preEntity[attributeLogicalName.ToLower()];

                if (myOpt != null)
                {
                    return myOpt.Value;
                }
            }

            return 0;
        }

        public string getEntRefName(string attributeLogicalName)
        {
            EntityReference myRef;

            if (_thisEntity.Attributes.Contains(attributeLogicalName.ToLower()))
            {
                myRef = (EntityReference)_thisEntity[attributeLogicalName.ToLower()];

                return myRef.Name;
            }
            if (_preEntity.Attributes.Contains(attributeLogicalName.ToLower()))
            {
                myRef = (EntityReference)_preEntity[attributeLogicalName.ToLower()];

                return myRef.Name;
            }

            return null;
        }

        public Guid getEntRefID(string attributeLogicalName)
        {
            EntityReference myRef;

            if (_thisEntity.Attributes.Contains(attributeLogicalName.ToLower()))
            {
                myRef = (EntityReference)_thisEntity[attributeLogicalName.ToLower()];

                return myRef.Id;
            }
            if (_preEntity.Attributes.Contains(attributeLogicalName.ToLower()))
            {
                myRef = (EntityReference)_preEntity[attributeLogicalName.ToLower()];

                return myRef.Id;
            }

            return Guid.Empty;
        }

        public string getEntRefType(string attributeLogicalName)
        {
            EntityReference myRef;

            if (_thisEntity.Attributes.Contains(attributeLogicalName.ToLower()))
            {
                myRef = (EntityReference)_thisEntity[attributeLogicalName.ToLower()];

                return myRef.LogicalName;
            }

            if (_preEntity.Attributes.Contains(attributeLogicalName.ToLower()))
            {
                myRef = (EntityReference)_preEntity[attributeLogicalName.ToLower()];

                return myRef.LogicalName;
            }

            return null;
        }
        #endregion
    }
}
