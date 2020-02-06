using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Internal;
using System.Reflection;
using System.Xml;

namespace ClassLibrary1
{
    public class Class1
    {
        public void ConfigurationManager()
        {
            var config_proxy = new ConfigurationProxy("ClassLibrary1.dll");
            config_proxy.InjectToConfigurationManager();
        }
    }

    public sealed class ConfigurationProxy: IInternalConfigSystem
    {
        Configuration config;
        Dictionary<string, IConfigurationSectionHandler> customSections;

        public ConfigurationProxy(string fileName)
        {
            customSections = new Dictionary<string, IConfigurationSectionHandler>();

            if (!Load(fileName))
                throw new ConfigurationErrorsException(string.Format(
                    "File: {0} could not be found or was not a valid cofiguration file.",
                    config.FilePath));
        }

        private bool Load(string file)
        {
            config = ConfigurationManager.OpenExeConfiguration(file);
            return config.HasFile;
        }

        public Configuration Configuration
        {
            get { return config; }
        }

        #region IInternalConfigSystem Members

        public object GetSection(string configKey)
        {
            if (configKey == "appSettings")
                return BuildAppSettings();

            object sect = config.GetSection(configKey);

            if (customSections.ContainsKey(configKey) && sect != null)
            {
                var xml = new XmlDocument();

                xml.LoadXml(((ConfigurationSection)sect).SectionInformation.GetRawXml());
                sect = customSections[configKey].Create(config,
                    config.EvaluationContext,
                    xml.FirstChild);
            }

            return sect;
        }

        public void RefreshConfig(string sectionName)
        {
            Load(config.FilePath);
        }

        public bool SupportsUserConfig
        {
            get { return false; }
        }

        #endregion

        private NameValueCollection BuildAppSettings()
        {
            var coll = new NameValueCollection();

            foreach (var key in config.AppSettings.Settings.AllKeys)
                coll.Add(key, config.AppSettings.Settings[key].Value);

            return coll;
        }

        public bool InjectToConfigurationManager()
        {
            var configSystem = typeof(ConfigurationManager).GetField("s_configSystem",
                BindingFlags.Static | BindingFlags.NonPublic);

            configSystem.SetValue(null, this);

            if (ConfigurationManager.AppSettings.Count == config.AppSettings.Settings.Count)
                return true;

            return false;
        }
    }
}
