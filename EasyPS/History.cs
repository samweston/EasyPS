using EasyPS.Properties;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace EasyPS
{
    public interface IHistory
    {
        IEnumerable<string> GetHistory(string scriptPath, string property);
        void AddHistory(string scriptPath, string property, string value);
    }

    public class HistoryXML : IHistory
    {
        /*
        <root>
            <script name="" >
                <property name=""></property>
            </script>
        </root>
        */

        protected string SavePath { get; set; }
        protected XElement Root { get; set; }

        protected readonly string ScriptElement = "script";
        protected readonly string PropertyElement = "property";
        protected readonly string NameAttribute = "name";

        protected void LoadHistory()
        {
            try
            {
                this.Root = XElement.Load(SavePath);
            }
            catch (FileNotFoundException)
            {
                var document = new XDocument(new XElement("root"));
                this.Root = document.Root;
                document.Save(SavePath);
            }
        }

        public void SetSavePath(string path)
        {
            this.SavePath = path + "\\history.xml";
            LoadHistory();
        }

        public IEnumerable<string> GetHistory(string scriptPath, string property)
        {
            return Root.Elements(ScriptElement)
                .FirstOrDefault(e => (string)e.Attribute(NameAttribute) == scriptPath)
                ?.Elements(PropertyElement)
                .Where(e => (string)e.Attribute(NameAttribute) == property)
                .Select(e => e.Value);
        }

        public void AddHistory(string scriptPath, string property, string value)
        {
            LoadHistory();

            var scriptRoot = Root.Elements(ScriptElement)
                .FirstOrDefault(e => (string)e.Attribute(NameAttribute) == scriptPath);
            if (scriptRoot == null)
            {
                scriptRoot = new XElement(ScriptElement);
                scriptRoot.SetAttributeValue(NameAttribute, scriptPath);
                Root.Add(scriptRoot);
            }

            if (scriptRoot.Elements(PropertyElement).Count(e => (string)e.Value == value) == 0)
            {
                var newProperty = new XElement(PropertyElement);
                newProperty.SetAttributeValue(NameAttribute, property);
                newProperty.Value = value;
                scriptRoot.Add(newProperty);

                Root.Save(SavePath);
            }
        }
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

        public IEnumerable<string> GetHistory(string scriptPath, string property)
        {
            string settingName = GetSettingName(scriptPath, property);
            
            EnsureSettingCreated(settingName);

            var result = new List<string>();
            StringCollection values = (StringCollection)Settings.Default[settingName];

            if (values.Count == 0)
            {
                return null;
            } 
            else
            {
                foreach (string value in values)
                {
                    result.Add(value);
                }
                return result;
            }
        }
    }
}
