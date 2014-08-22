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
using System.Windows.Data;
using System.Collections.ObjectModel;

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
        List<ProjectData> projectList;
        List<TagData> tagList = new List<TagData>();

        ListCollectionView TagDataSource;

        public MainWindow()
        {
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
            InitializeComponent();

            // 初始化Tag Table
            tagList.Add(TagData.Empty);
            var data = new ObservableCollection<TagData>(tagList);
            TagDataSource = new ListCollectionView(data);
            _viewModel.DataSource = TagDataSource;

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


            // 初始化地圖選擇方塊
            comboBox_maptype.SelectedItem = gMap.MapProvider;
            try
            {
                // 載入專案資料
                this.projectList = ModelUtil.getProjectList();
                // 加入專案列表
                refreshProjectData(projectList);
            }
            catch (Exception ex)
            {
                messageDialog("發生錯誤", "載入專案資料時發生錯誤\n"+ex.Message);
            }
        }

        private void refreshProjectData(List<ProjectData> pList)
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

        private void searchProject(string keywordCommand)
        {
            string[] keywordList = parseKeyword(keywordCommand);

            foreach (ProjectData p in projectList)
            {
                // removal phase
                if (projectListView.Items.Contains(p))
                {
                    if(!p.containsKeyword(keywordList))
                        projectListView.Items.Remove(p);
                }
                // add back phase
                else
                {
                    if(p.containsKeyword(keywordList))
                        projectListView.Items.Add(p);
                }
            }
        }

        private void searchTag(string keywordCommand)
        {
            string[] keywordList = parseKeyword(keywordCommand);

            foreach (TagData t in tagList)
            {
                // removal phase
                if (TagDataSource.Contains(t))
                {
                    if (!t.containsKeyword(keywordList))
                        TagDataSource.Remove(t);
                }
                // add back phase
                else
                {
                    if (t.containsKeyword(keywordList))
                        TagTableAdd(t);
                }
            }
        }

        private string[] parseKeyword(string keyword)
        {
            string Bopomofo = "[ㄅㄆㄇㄈㄉㄊㄋㄌㄍㄎㄏㄐㄑㄒㄓㄔㄕㄖㄗㄘㄙㄧㄨㄩㄚㄛㄜㄝㄞㄟㄠㄡㄢㄣㄤㄥㄦˊˇˋ˙]+";
            Regex reg = new Regex(Bopomofo);
            keyword = reg.Replace(keyword, "");
            reg = new Regex("\\s+");
            keyword = reg.Replace(keyword.Trim(), " ");
            return keyword.Split(' ');
        }

        #region -- marker functions --
        private GMapMarker newProjectMarker(ProjectData p)
        {
            return newProjectMarker(new PointLatLng(p.lat, p.lng), p);
        }
        private GMapMarker newProjectMarker(PointLatLng latlng, ProjectData p)
        {
            GMapMarker marker = new GMapMarker(latlng);
            marker.Shape = new ProjectMarker(this, marker, p);
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

        // 地圖選擇改變事件處理
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

        // 搜尋Project文字方塊文字變更事件處理
        private void textBox_searchProject_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchProject(textBox_searchProject.Text);
        }

        // 搜尋Tag文字方塊文字變更事件處理
        private void textBox_searchTag_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            searchTag(textBox_searchTag.Text);
        }

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

        // 專案選項變更事件處理
        private void projectListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProjectData p = projectListView.SelectedItem as ProjectData;
            if (p != null)
            {
                selectProject(p);
            }
        }

        /* selection change does not trigger when mouse down on the selected item
         * and mouse down event does not trigger when mouse down on listbox item,
         * so use preview mouse down instead.
         * selectionChanged 在使用者對已選擇項目按下按鈕時不會觸發
         * mouseDown 則不會在項目被按下時觸發
         * 所以使用previewMouseDown
         * 專門針對使用者對已選擇項目按下按鈕，開啟專案資料瀏覽
         * */
        private void projectListView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // get newly selected listbox item
            // PreviewMouseDown 的觸發在selection改變之前
            // 所以使用者所按下項目需由下面方法取得
            var item = ItemsControl.ContainerFromElement(projectListView,
                e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
            {
                // get project data
                ProjectData p = item.Content as ProjectData;
                // 專門針對使用者對已選擇項目按下按鈕
                if (p != null && p.Equals(projectListView.SelectedItem))
                {
                    selectProject(p);
                }
            }
            else
            {
                projectListView.UnselectAll();
            }
        }

        #endregion

        // 開啟專案內容瀏覽畫面
        private void initProjectDataViewFlyout(ProjectData project)
        {
            // 初始化專案內容瀏覽畫面
            projectFlyout.Header = project.name;
            textBlock_projectAddr.Text = project.addr;
            textBox_searchTag.Text = "";
            // 清空 Tag table
            TagTableClear();
            // 清空 Tag list
            tagList.Clear();

            // 讀取專案內的裝置
            List<DeviceData> devices = project.devices;

            // 加入所有裝置的點位到 taglist
            foreach (DeviceData device in devices)
            {
                tagList.AddRange(device.tags);
            }
            // 加入點位到Tag Table
            TagTableAdd(tagList);
            TagDataSource.Refresh();
            // 分類
            //var data = new ObservableCollection<Tag>(tagList);
            //_viewModel.DataSource = new ListCollectionView(data);
            // 開啟Flyout
            projectFlyout.IsOpen = true;
        }

        // 非同步對話方塊
        private async void messageDialog(string title, string message)
        {
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "確定",
                AnimateShow = true,
                AnimateHide = false
            };

            await this.ShowMessageAsync(title, message, MessageDialogStyle.Affirmative, mySettings);
        }

        // 選擇專案
        public void selectProject(ProjectData project)
        {
            // 切換至地圖瀏覽頁面
            tabControl1.SelectedItem = tabItem_map;
            // 切換專案列表的已選取專案
            projectListView.SelectedItem = project;

            // 將專案位置設為中心並調整zoom大小
            //gMap.Position = new PointLatLng(project.lat, project.lng);
            //gMap.Zoom = 8;

            /* 更換地圖類別時
             * 有時會造成marker位置錯誤
             * 調整Zoom可以使marker位置恢復正常
             * */
            gMap.Zoom++;
            gMap.Zoom--;
            
            initProjectDataViewFlyout(project);
        }


        #region -- Tab Table 新增、修改方法 --
        private void TagTableAdd(List<TagData> tags)
        {
            //新增多個Tag至Tag Table
            foreach (TagData tag in tags)
            {
                TagDataSource.AddNewItem(tag);
            }
            // 儲存 新增項目 並暫停 "新增執行階段"
            TagDataSource.CommitNew();
        }

        private void TagTableAdd(TagData tag)
        {
            // 新增Tag至Tag Tabled
            TagDataSource.AddNewItem(tag);
            // 儲存 新增項目 並暫停 "新增執行階段"
            TagDataSource.CommitNew();
        }

        // 清除Tag Table
        private void TagTableClear()
        {
            // 不斷移除首項直到清空
            while (TagDataSource.Count > 0)
            {
                TagDataSource.RemoveAt(0);
            }
        }

        #endregion
    }
}
