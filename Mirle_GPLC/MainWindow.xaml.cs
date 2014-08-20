using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using GMap.NET.MapProviders;
using Mirle.iMServer.Model;
using Mirle_GPLC.CustomeMarkers;
using System;

namespace Mirle_GPLC
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly MainWindowViewModel _viewModel;

        // marker
        //GMapMarker currentMarker;
        //List<GMapMarker> deviceMarkers;

        // project data
        List<ProjectData> pList;

        public MainWindow()
        {
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
            InitializeComponent();

            // add your custom map db provider
            //MySQLPureImageCache ch = new MySQLPureImageCache();
            //ch.ConnectionString = @"server=sql2008;User Id=trolis;Persist Security Info=True;database=gmapnetcache;password=trolis;";
            //MainMap.Manager.SecondaryCache = ch;
        }

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

            //textBox_searchProject.button

            // 初始化當前Marker為空值
            //currentMarker = null;
            // 載入專案資料
            try
            {
                loadProjectData("");
                refreshProjectData();
            }
            catch (Exception ex)
            {
            }
        }

        private void loadProjectData(string keyword)
        {
            Regex reg = new Regex("\\s+");
            keyword = reg.Replace(keyword.Trim(), " ");
            string[] strs = keyword.Split(' ');
            // 載入所有專案
            this.pList = ModelUtil.getProjectList(strs);
        }

        private void refreshProjectData()
        {
            // 更新專案列表
            // 清空專案列表
            projectListView.Items.Clear();
            gMap.Markers.Clear();
            // 加入專案列表
            foreach (ProjectData p in pList)
            {
                GMapMarker pm = newProjectMarker(p);
                projectListView.Items.Add(p);
            }
        }

        private void searchProject(string keyword)
        {
            loadProjectData(keyword);
            refreshProjectData();
        }

        #region -- marker functions --
        private GMapMarker newProjectMarker(ProjectData p)
        {
            return newProjectMarker(new PointLatLng(p.lat, p.lng), p);
        }
        private GMapMarker newProjectMarker(PointLatLng latlng, ProjectData p)
        {
            GMapMarker marker = new GMapMarker(latlng);
            marker.Shape = new ProjectMarker(gMap, marker, p);
            addMarker(marker);
            return marker;
        }
        // add marker to overlay delegate
        delegate void addMarkerHandler(GMapMarker marker);
        private void addMarker(GMapMarker marker)
        {
            // add to overlay
            gMap.Markers.Add(marker);
        }
        #endregion

        #region -- Event Handlers --

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

        private void textBox_searchProject_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchProject(textBox_searchProject.Text);
        }

        private void textBox_searchTag_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchProject(textBox_searchTag.Text);
        }

        #endregion

        // 非同步視窗關閉確認
        private async void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool _shutdown = false;
            e.Cancel = !_shutdown;
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "確定",
                NegativeButtonText = "取消",
                AnimateShow = true,
                AnimateHide = false
            };

            var result = await this.ShowMessageAsync(
                "關閉地理資訊系統?", "確定要關閉地理資訊系統嗎?",
                MessageDialogStyle.AffirmativeAndNegative, mySettings);

            _shutdown = result == MessageDialogResult.Affirmative;
            
            if (_shutdown)
                Application.Current.Shutdown();
        }

        private void projectListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProjectData p = projectListView.SelectedItem as ProjectData;
            projectFlyout.Header = p.name;
            projectFlyout.IsOpen = true;
            initFlyout(p);
        }

        private void initFlyout(ProjectData project)
        {
            textBlock_projectAddr.Text = project.addr;
            projectTagTable.Items.Clear();

            List<Device> devices = project.devices;

            foreach (Device device in devices)
            {
                List<Tag> tags = device.tags;
                foreach (Tag tag in tags)
                {
                    projectTagTable.Items.Add(tag);
                }
            }
        }

    }
}
