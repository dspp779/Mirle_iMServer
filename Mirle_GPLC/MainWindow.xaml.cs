using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GMap.NET;
using MahApps.Metro.Controls;
using GMap.NET.WindowsPresentation;
using GMap.NET.MapProviders;
using Mirle.iMServer.Model;
using Mirle_GPLC.CustomeMarkers;

namespace Mirle_GPLC
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        // marker
        GMapMarker currentMarker;
        List<GMapMarker> deviceMarkers;

        // project data
        List<ProjectData> pList;

        public MainWindow()
        {
            InitializeComponent();

            // add your custom map db provider
            //MySQLPureImageCache ch = new MySQLPureImageCache();
            //ch.ConnectionString = @"server=sql2008;User Id=trolis;Persist Security Info=True;database=gmapnetcache;password=trolis;";
            //MainMap.Manager.SecondaryCache = ch;
        }

        private void loadProjectData()
        {
            // 載入所有專案
            this.pList = ModelUtil.getProjectList();
            // 更新專案列表
            // 清空專案列表
            projectListView.Items.Clear();
            // 加入專案列表
            foreach (ProjectData p in pList)
            {
                projectListView.Items.Add(p);
                addPMarker(p);
            }
        }

        // add marker to overlay delegate
        delegate void addMarkerHandler(GMapMarker marker);
        private void addMarker(GMapMarker marker)
        {
            // add to overlay
            gMap.Markers.Add(marker);
        }
        private void addPMarker(ProjectData p)
        {
            addPMarker(new PointLatLng(p.lat, p.lng), p);
        }
        private void addPMarker(PointLatLng latlng, ProjectData p)
        {
            GMapMarker marker = new GMapMarker(latlng);
            marker.Shape = new ProjectMarker(gMap, marker, p);
            addMarker(marker);
        }

        #region -- Event Handlers --
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 初始化地圖元件
            // 設定地圖來源
            gMap.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            // 設定語言
            GMap.NET.MapProviders.GMapProvider.Language = GMap.NET.LanguageType.ChineseTraditional;
            // 設定圖塊取得機制: ServerOnly, ServerAndCache, CacheOnly.
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            // 設定初始檢視大小
            gMap.Zoom = 8;
            // 關閉顯示中心紅十字
            gMap.ShowCenter = false;
            // 設定滑鼠滑動地圖按鈕為左鍵 (預設為右鍵)
            gMap.DragButton = MouseButton.Left;
            // 設定初始位置
            gMap.Position = new PointLatLng(23.8, 121);
            // 設定滑鼠滾輪忽略標記，以縮放地圖
            //gMap.IgnoreMarkerOnMouseWheel = true;

            // 載入專案資料
            loadProjectData();
        }

        private void comboBox_maptype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch(comboBox_maptype.SelectedIndex)
            {
                case 0:
                    gMap.MapProvider = GoogleMapProvider.Instance;
                    break;
                case 1:
                    gMap.MapProvider = GoogleTerrainMapProvider.Instance;
                    break;
                case 2:
                    gMap.MapProvider = OpenStreetMapProvider.Instance;
                    break;
            }
        }
        #endregion

    }
}
