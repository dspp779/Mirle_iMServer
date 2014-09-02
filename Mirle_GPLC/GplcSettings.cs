using GMap.NET;
using GMap.NET.MapProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Mirle_GPLC
{
    public class GplcSettings
    {
        private readonly MainWindowViewModel _viewModel;

        private XmlDocument xmlDoc;

        // settings
        private GMapProvider mapProvider;
        private AccessMode mapAccessMode;
        private AccentColorMenuData accentColor;
        private int pollingRate;

        // setting file path
        private static string AppDataPath =
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Mirle_GPLC";
        private static string SettingFileName = "settings.xml";
        private string fileName;


        public GMapProvider MapProvider
        {
            get { return mapProvider; }
            set
            {
                mapProvider = value;
                writeSetting("Settings/Map/MapProvider", mapProvider.Name);
            }
        }
        public AccessMode MapAccessMode
        {
            get { return mapAccessMode; }
            set
            {
                mapAccessMode = value;
                writeSetting("Settings/Map/AccessMode", mapAccessMode.ToString());
            }
        }
        public AccentColorMenuData AccentColor
        {
            get { return accentColor; }
            set
            {
                accentColor = value;
                writeSetting("Settings/Theme/Accent", accentColor.Name);
            }
        }
        public int PollingRate
        {
            get { return pollingRate; }
            set
            {
                pollingRate = value;
                writeSetting("Settings/PollingRate", pollingRate.ToString());
            }
        }

        public GplcSettings(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;

            mapProvider = _viewModel.GMapProviderList[0];
            accentColor = _viewModel.AccentColors[0];
            mapAccessMode = AccessMode.ServerOnly;
            pollingRate = 1000;

            fileName = Path.Combine(AppDataPath, SettingFileName);

            if (!Directory.Exists(AppDataPath))
            {
                Directory.CreateDirectory(AppDataPath);
            }
            if (!File.Exists(fileName))
            {
                newSetting();
            }
            loadSettings();
        }

        private void newSetting()
        {
            xmlDoc = new XmlDocument();

            XmlElement setting = xmlDoc.CreateElement("Settings");
            xmlDoc.AppendChild(setting);

            XmlElement map = xmlDoc.CreateElement("Map");
            setting.AppendChild(map);
            XmlElement provider = xmlDoc.CreateElement("MapProvider");
            map.AppendChild(provider);
            XmlElement accessMode = xmlDoc.CreateElement("AccessMode");
            map.AppendChild(accessMode);

            XmlElement theme = xmlDoc.CreateElement("Theme");
            setting.AppendChild(theme);
            XmlElement accent = xmlDoc.CreateElement("Accent");
            theme.AppendChild(accent);

            XmlElement polling = xmlDoc.CreateElement("PollingRate");
            setting.AppendChild(polling);

            provider.InnerText = mapProvider.Name;
            accessMode.InnerText = mapAccessMode.ToString();
            accent.InnerText = accentColor.Name;
            polling.InnerText = pollingRate.ToString();

            xmlDoc.Save(fileName);
        }

        private void loadSettings()
        {
            try
            {
                // read xml file
                xmlDoc = new XmlDocument();
                xmlDoc.Load(fileName);

                loadMapSettings();
                loadThemeSettings();
                loadPollingSetting();
            }
            catch (Exception)
            {
                newSetting();
                mapProvider = GMapProviders.GoogleMap;
                mapAccessMode = AccessMode.ServerAndCache;
                accentColor = _viewModel.AccentColors[0];
                pollingRate = 1000;
            }
        }

        private void loadMapSettings()
        {
            // load map setting
            string providerName = xmlDoc.SelectSingleNode("Settings/Map/MapProvider").InnerText;
            foreach (GMapProvider provider in _viewModel.GMapProviderList)
            {
                if (provider.Name.Equals(providerName))
                {
                    mapProvider = provider;
                    break;
                }
            }
            string accessModeName = xmlDoc.SelectSingleNode("Settings/Map/AccessMode").InnerText;
            mapAccessMode = (AccessMode)Enum.Parse(typeof(AccessMode), accessModeName);
        }

        private void loadThemeSettings()
        {
            // load theme setting
            string accent = xmlDoc.SelectSingleNode("Settings/Theme/Accent").InnerText;
            foreach (AccentColorMenuData a in _viewModel.AccentColors)
            {
                if (a.Name.Equals(accent))
                {
                    accentColor = a;
                }
            }
        }

        private void loadPollingSetting()
        {
            string pRate = xmlDoc.SelectSingleNode("Settings/PollingRate").InnerText;
            pollingRate = int.Parse(pRate);
        }

        private void writeSetting(string node, string innerText)
        {
            try
            {
                xmlDoc.SelectSingleNode(node).InnerText = innerText;
                xmlDoc.Save(fileName);
            }
            catch (Exception)
            {
                newSetting();
                xmlDoc.SelectSingleNode(node).InnerText = innerText;
                xmlDoc.Save(fileName);
            }
        }
    }
}
