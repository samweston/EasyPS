using EasyPS.Properties;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;

namespace EasyPS
{
    public interface IHistory
    {
        IList<string> GetHistory(string scriptPath, string property);
        void AddHistory(string scriptPath, string property, string value);
    }

    public class HistorySetting : IHistory
    {
        // With help from http://stackoverflow.com/questions/175726/c-create-new-settings-at-run-time
        // this will use runtime created settings.
        // Some of this could possibly be moved into Settings.cs.

        protected string GetSettingName(string scriptPath, string property)
        {
            return "history_" + scriptPath + "_" + property;
        }

        protected void EnsureSettingCreated(string settingName)
        {
            if (Settings.Default.Properties[settingName] == null)
            {
                SettingsProperty settingProperty = new SettingsProperty(Settings.Default.Properties["BaseHistorySetting"]);
                settingProperty.Name = settingName;
                Settings.Default.Properties.Add(settingProperty);
                Settings.Default.Reload();
            }
            
            if (Settings.Default[settingName] == null)
            {
                Settings.Default[settingName] = new StringCollection();
            }
        }

        public void AddHistory(string scriptPath, string property, string value)
        {
            string settingName = GetSettingName(scriptPath, property);
            
            EnsureSettingCreated(settingName);

            StringCollection values = (StringCollection)Settings.Default[settingName];
            values.Remove(value);
            values.Insert(0, value);

            Settings.Default.Save();
        }

        public IList<string> GetHistory(string scriptPath, string property)
        {
            string settingName = GetSettingName(scriptPath, property);
            
            EnsureSettingCreated(settingName);

            var result = new List<string>();
            StringCollection values = (StringCollection)Settings.Default[settingName];
            
            foreach (string value in values)
            {
                result.Add(value);
            }
            return result;
        }
    }
}
