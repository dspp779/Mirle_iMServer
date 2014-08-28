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
using System.Windows.Media;
using System.Diagnostics;
using Mirle.iMServer.Model.Utility;
using MahApps.Metro;

namespace Mirle_GPLC
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static MainWindow runningInstance;
        public static GplcSettings currentSetting;

        private readonly MainWindowViewModel _viewModel;

        // project data
        List<DeviceData> projectList;
        Dictionary<DeviceData, ProjectMarker> projectMarkerDictionary
            = new Dictionary<DeviceData, ProjectMarker>();
        List<TagData> tagList = new List<TagData>();

        ListCollectionView TagDataSource;

        ProjectMarker _currentMarker;
        protected ProjectMarker CurrentMarker
        {
            get { return _currentMarker; }
            set
            {
                // 更新地圖先前選擇的 project marker 的狀態
                if (_currentMarker != null)
                {
                    _currentMarker.IsSelected = false;
                }

                if (value != null)
                {
                    // 更新地圖的目前 marker
                    _currentMarker = value;
                    // 更新地圖新選擇的 project marker 的狀態
                    _currentMarker.IsSelected = true;
                    // 更新專案列表的 已選取專案
                    projectListView.SelectedItem = _currentMarker.Project;
                }
                else
                {
                    projectListView.SelectedItem = null;
                }
            }
        }

        public MainWindow()
        {
            // 初始化執行中個體
            runningInstance = this;

            // 設定 DataContext
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;

            // 初始化現在設定
            MainWindow.currentSetting = _viewModel.setting;

            InitializeComponent();

            // 初始化 Tag Table，使用空 Tag data
            tagList.Add(TagData.Empty);
            // 建立可分類的 Tag 資料
            var data = new ObservableCollection<TagData>(tagList);
            TagDataSource = new ListCollectionView(data);
            // 設定 data source
            _viewModel.DataSource = TagDataSource;

            // 初始化 project simple view
            projectSimpleView.init(this);

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 初始化地圖元件
            // 設定語言
            GMap.NET.MapProviders.GMapProvider.Language = GMap.NET.LanguageType.ChineseTraditional;
            // 設定圖塊取得機制: ServerOnly, ServerAndCache, CacheOnly.
            GMap.NET.GMaps.Instance.Mode = _viewModel.MapAccessMode;

            // 設定地圖來源
            //gMap.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            gMap.MapProvider = deviceEditControl.Map_SetPosition.MapProvider
                = _viewModel.MapProvider;
            //Binding mapProviderBinding = new Binding();
            //mapProviderBinding.Source = _viewModel.MapProvider;
            //BindingOperations.SetBinding(gMap.MapProvider, , mapProviderBinding;
            // 設定初始檢視大小
            gMap.Zoom = 8;
            // 關閉顯示中心紅十字
            gMap.ShowCenter = false;
            // 設定滑鼠滑動地圖按鈕為左鍵 (預設為右鍵)
            gMap.DragButton = MouseButton.Left;
            // 設定初始位置
            gMap.Position = new PointLatLng(23.8, 121);

            ChangeTheme(_viewModel.setting.AccentColor);

            refreshData();

            // 印出參數
            //string str = "";
            //foreach (string s in Environment.GetCommandLineArgs())
            //    str += s+"\n";
            //messageDialog("參數：", str);
        }

        public void refreshData()
        {
            // 初始化目前標記為空值
            _currentMarker = null;

            // 初始化 simple view 為隱藏
            projectSimpleView.Visibility = Visibility.Hidden;

            // 初始化地圖選擇方塊
            comboBox_maptype.SelectedItem = gMap.MapProvider;
            try
            {
                // 載入專案資料
                this.projectList = ModelUtil.getMapDeviceList();
                // 加入專案列表
                refreshProjectData(projectList);
            }
            catch (Exception ex)
            {
                messageDialog("發生錯誤", "載入專案資料時發生錯誤\n" + ex.Message);
            }
        }

        private void refreshProjectData(List<DeviceData> pList)
        {
            // 更新專案列表
            // 清空專案列表
            projectListView.Items.Clear();
            gMap.Markers.Clear();
            projectMarkerDictionary.Clear();
            // 反序地加入專案地圖標記，使得排序較前的專案有較高的z-index
            for (int i = pList.Count - 1; i >= 0; i--)
            {
                GMapMarker pm = newProjectMarker(pList[i]);
            }
            // 加入專案列表
            foreach (DeviceData p in pList)
            {
                projectListView.Items.Add(p);
            }
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

        #region -- marker adding functions --
        private GMapMarker newProjectMarker(DeviceData p)
        {
            return newProjectMarker(new PointLatLng(p.lat, p.lng), p);
        }
        private GMapMarker newProjectMarker(PointLatLng latlng, DeviceData p)
        {
            GMapMarker marker = new GMapMarker(latlng);
            ProjectMarker pm = new ProjectMarker(this, marker, p);
            // 設定 marker 為 project marker 的形狀
            marker.Shape = pm;
            // 將 marker 加入地圖中
            addMarker(marker);
            // 加入 project marker list 中
            projectMarkerDictionary.Add(p, pm);
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
            {
                // 將 Trend data worker 關閉
                // (非必要，關閉應用程式會自動關閉背景工作)
                TrendDataManager.cancelDeviceTagRefresh();
                Application.Current.Shutdown();
            }
        }

        // 專案選項變更事件處理
        private void projectListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //ProjectData p = projectListView.SelectedItem as ProjectData;
            //if (p != null)
        }

        /* selection change does not trigger when mouse down on the selected item
         * and mouse down event does not trigger when mouse down on listbox item,
         * so use preview mouse down instead.
         * selectionChanged 在使用者對已選擇項目按下按鈕時不會觸發
         * mouseDown 則不會在項目被按下時觸發
         * 所以使用previewMouseDown
         * 專門針對使用者對已選擇項目按下按鈕，開啟專案資料瀏覽
         * 同時，專案項目點擊，會造成地圖位置改變，而非專案項目選擇改變
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
                DeviceData p = item.Content as DeviceData;

                if (p != null)
                {
                    tabControl_main.SelectedItem = tabItem_map;
                    selectProject(p);
                }
            }
            else
            {
                projectListView.SelectedItem = null;
                CurrentMarker = null;
                //projectListView.UnselectAll();
            }
        }

        #endregion

        // 地圖上專案標記的點擊事件處理 : 由project marker 呼叫
        public void projectMarkerClicked(ProjectMarker pm)
        {
            // project marker 不能為 null
            Debug.Assert(pm != null);

            // 設定 current marker
            CurrentMarker = pm;
            // 開啟 project simple view
            initProjectSimpleView(pm.Project);
            // 開啟project flyout
            //initProjectDataViewFlyout(pm.Project);
        }

        // 選擇專案
        private void selectProject(DeviceData device)
        {
            // project 不能為 null
            Debug.Assert(device != null);

            // 更新專案列表的已選取專案
            projectListView.SelectedItem = device;

            // 更新地圖的目前 project marker
            ProjectMarker pm = null;
            projectMarkerDictionary.TryGetValue(device, out pm);
            CurrentMarker = pm;

            // 將專案位置設為中心並調整zoom大小
            gMap.Position = new PointLatLng(device.lat, device.lng);

            // 如果Zoom沒改變，標記位置可能會出錯
            gMap.Zoom = 17;
            gMap.Zoom = 16;

            //開啟simple view
            initProjectSimpleView(device);
        }

        // 開啟專案內容瀏覽 simple view
        public void initProjectSimpleView(DeviceData device)
        {
            // 關閉flyout
            projectFlyout.IsOpen = false;

            // 設定 simple view 之 device
            projectSimpleView.set(device);

            // 更新專案 Tag Table 內容
            initProjectTagTable(device);

            // 重置 Tag Table scroll 為頂端
            dataGridScrollToTop(projectSimpleView.projectTagTable);
            //開啟 simple view
            projectSimpleView.Visibility = Visibility.Visible;
        }

        // 開啟專案內容瀏覽 flyout
        public void initProjectDataViewFlyout(DeviceData device)
        {
            // 隱藏 simple view
            projectSimpleView.Visibility = Visibility.Hidden;

            // 初始化專案內容瀏覽畫面
            projectFlyout.Header = device.alias;
            textBlock_projectAddr.Text = device.addr;
            textBox_searchTag.Text = "";

            // 更新專案 Tag Table 內容
            initProjectTagTable(device);

            // 重置 Tag Table scroll 為頂端
            dataGridScrollToTop(projectTagTable);
            // 開啟Flyout
            projectFlyout.IsOpen = true;
        }

        // 更新專案 Tag Table 內容
        private void initProjectTagTable(DeviceData device)
        {
            // 清空 Tag table
            TagTableClear();
            // 清空 Tag list
            tagList.Clear();

            // 加入裝置的所有點位到 taglist
            tagList.AddRange(device.tags);
            // 加入點位到Tag Table
            TagTableAdd(tagList);
            TagDataSource.Refresh();
        }

        // 將傳入的 DataGrid 控制項的 scroll bar 移至頂端
        private void dataGridScrollToTop(DataGrid control)
        {
            var border = VisualTreeHelper.GetChild(control, 0) as Decorator;
            if (border != null)
            {
                var scrollViewer = border.Child as ScrollViewer;
                scrollViewer.ScrollToTop();
            }
        }

        // 非同步對話方塊
        public async void messageDialog(string title, string message)
        {
            var mySettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "確定",
                AnimateShow = true,
                AnimateHide = false
            };

            await this.ShowMessageAsync(title, message, MessageDialogStyle.Affirmative, mySettings);
        }

        #region -- 專案、點位搜尋方法 --
        public void searchProject(string keywordCommand)
        {
            string[] keywordList = parseKeyword(keywordCommand);

            foreach (DeviceData p in projectList)
            {
                // removal phase
                if (projectListView.Items.Contains(p))
                {
                    if (!p.Contains(keywordList))
                        projectListView.Items.Remove(p);
                }
                // add back phase
                else
                {
                    if (p.Contains(keywordList))
                        projectListView.Items.Add(p);
                }
            }
        }

        public void searchTag(string keywordCommand)
        {
            string[] keywordList = parseKeyword(keywordCommand);

            foreach (TagData t in tagList)
            {
                // removal phase
                if (TagDataSource.Contains(t))
                {
                    if (!t.Contains(keywordList))
                        TagDataSource.Remove(t);
                }
                // add back phase
                else
                {
                    if (t.Contains(keywordList))
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
        #endregion

        /* 位置鎖定縮放
         * 
         * 作法為
         * 先記錄鎖定點在畫面的位置 (mouseLastZoom)
         * 以鎖定點做為中心進行縮放 (Zoom+-)
         * 
         * 算出先前已記錄畫面位置的點與畫面中心的像素差 (renderOffset)
         * 此像素差即為整張地圖所需的位移量
         * 
         * 算出新的中心點像素位置：鎖定點像素位置+位移量
         * 算出中心點經緯座標：FromPixelToLatLng
         */
        public void positionLockZoom(GMapControl mapControl, PointLatLng lockPosition, int delta)
        {
            // local position of lock position
            GPoint mouseLastZoom = mapControl.FromLatLngToLocal(lockPosition);

            // center zoom to project
            mapControl.Position = lockPosition;
            mapControl.Zoom += (delta > 0) ? 1 : -1;

            int zoom = (int)mapControl.Zoom;

            // compute render offset
            GPoint renderOffset = GPoint.Empty;
            renderOffset.X = (int)mapControl.RenderSize.Width / 2 - mouseLastZoom.X;
            renderOffset.Y = (int)mapControl.RenderSize.Height / 2 - mouseLastZoom.Y;

            // current pixel position of the project
            GPoint positionPixel = mapControl.MapProvider.Projection.FromLatLngToPixel(lockPosition, zoom);

            // new center position in pixel
            positionPixel.Offset(renderOffset);

            // compute and set the latlng of new center position
            mapControl.Position = mapControl.MapProvider.Projection.FromPixelToLatLng(positionPixel, zoom);
        }

        private void projectFlyout_IsOpenChanged(object sender, EventArgs e)
        {
            if (!projectFlyout.IsOpen)
            {
                initProjectSimpleView(CurrentMarker.Project);
            }
        }

        private void gMap_OnMapTypeChanged(GMapProvider type)
        {
            deviceEditControl.Map_SetPosition.MapProvider = type;
            _viewModel.setting.MapProvider = type;
        }

        private void ChangeTheme(AccentColorMenuData accentColor)
        {
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            var accent = ThemeManager.GetAccent(accentColor.Name);
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme.Item1);
            OnThemeChanged(accentColor);
        }


        public void OnThemeChanged(AccentColorMenuData accentColor)
        {
            _viewModel.setting.AccentColor = accentColor;
        }
    }
}
