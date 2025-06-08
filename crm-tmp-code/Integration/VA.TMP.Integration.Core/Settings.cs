using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;

namespace VA.TMP.Integration.Core
{
    /// <summary>
    /// Class to manage application specific settings.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public Settings()
        {
            Items = new List<KeyValuePair<string, string>>();
        }

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public Settings(NameValueCollection appSettings)
        {
            Items = appSettings.AllKeys.ToDictionary(k => k, v => ConfigurationManager.AppSettings[v]).ToList();
        }

        /// <summary>
        /// Gets the Settings;
        /// </summary>
        public List<KeyValuePair<string, string>> Items { get; private set; }

        /// <summary>
        /// Add a single setting.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public void Add(string key, string value)
        {
            Items.Add(new KeyValuePair<string, string>(key, value));
        }

        /// <summary>
        /// Add multiple settings.
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(List<KeyValuePair<string, string>> items)
        {
            Items.AddRange(items);
        }
    }
}