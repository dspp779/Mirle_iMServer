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
        private GMapProvider mapProvider;
        private AccentColorMenuData accentColor;

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
                xmlDoc.SelectSingleNode("Settings/Map/MapProvider").InnerText = mapProvider.Name;
                xmlDoc.Save(fileName);
            }
        }
        public AccentColorMenuData AccentColor
        {
            get { return accentColor; }
            set
            {
                accentColor = value;
                xmlDoc.SelectSingleNode("Settings/Theme/Accent").InnerText = accentColor.Name;
                xmlDoc.Save(fileName);
            }
        }

        public GplcSettings(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;

            mapProvider = _viewModel.GMapProviderList[0];
            accentColor = _viewModel.AccentColors[0];

            fileName = Path.Combine(AppDataPath, SettingFileName);
            // read xml file
            xmlDoc = new XmlDocument();

            if (!Directory.Exists(AppDataPath))
            {
                Directory.CreateDirectory(AppDataPath);
            }
            if (!File.Exists(fileName))
                {
                try
                {
                    newSetting();
                }
                catch (Exception)
                {
                }
            }
            xmlDoc.Load(fileName);

            loadMapSettings();
            loadThemeSettings();
        }

        private void newSetting()
        {
            using (XmlWriter writer = XmlWriter.Create(fileName))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Settings");

                writer.WriteStartElement("Map");
                writer.WriteStartElement("MapProvider");
                writer.WriteString(mapProvider.Name);
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteStartElement("Theme");
                writer.WriteStartElement("Accent");
                writer.WriteString(accentColor.Name);
                writer.WriteEndElement();
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();


                xmlDoc.Save(writer);
            }
        }

        private void loadMapSettings()
        {
            // load map setting
            try
            {
                string providerName = xmlDoc.SelectSingleNode("Settings/Map/MapProvider").InnerText;
                foreach (GMapProvider provider in _viewModel.GMapProviderList)
                {
                    if (provider.Name.Equals(providerName))
                    {
                        mapProvider = provider;
                        break;
                    }
                }
            }
            catch (Exception)
            {
                mapProvider = GMapProviders.GoogleMap;
            }
        }

        private void loadThemeSettings()
        {
            // load theme setting
            try
            {
                string accent = xmlDoc.SelectSingleNode("Settings/Theme/Accent").InnerText;
                foreach (AccentColorMenuData a in _viewModel.AccentColors)
                {
                    if (a.Name.Equals(accent))
                    {
                        accentColor = a;
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
