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
        // 目前執行階段物件的指向
        public static MainWindow runningInstance;
        // 目前設定的指向
        public static GplcSettings currentSetting;

        // 資料繫結 model 物件
        private readonly MainWindowViewModel _viewModel;

        // 站位列表
        List<DeviceData> deviceList;
        // 站位與其標記的查詢字典
        Dictionary<DeviceData, DeviceMarker> deviceMarkerDictionary
            = new Dictionary<DeviceData, DeviceMarker>();
        
        // 目前點為列表
        List<TagData> tagList = new List<TagData>();

        // tag 表格的資料來源指向
        ListCollectionView TagDataSource;

        // 目前選擇的 marker
        DeviceMarker _currentMarker;
        protected DeviceMarker CurrentMarker
        {
            get { return _currentMarker; }
            set
            {
                // 更新地圖先前選擇的 device marker 的狀態
                if (_currentMarker != null)
                {
                    _currentMarker.IsSelected = false;
                }

                if (value != null)
                {
                    // 更新地圖的目前 marker
                    _currentMarker = value;
                    // 更新地圖新選擇的 device marker 的狀態
                    _currentMarker.IsSelected = true;
                    // 更新站位列表的 已選取站位
                    deviceListView.SelectedItem = _currentMarker.Device;
                }
                else
                {
                    deviceListView.SelectedItem = null;
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

            // 初始化 device simple view
            deviceSimpleView.init(this);

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
            deviceSimpleView.Visibility = Visibility.Hidden;

            // 初始化地圖選擇方塊
            comboBox_maptype.SelectedItem = gMap.MapProvider;
            try
            {
                // 載入站位資料
                this.deviceList = ModelUtil.getMapDeviceList();
                // 加入站位列表
                refreshDeviceData(deviceList);
            }
            catch (Exception ex)
            {
                messageDialog("發生錯誤", "載入站位資料時發生錯誤\n" + ex.Message);
            }
        }

        private void refreshDeviceData(List<DeviceData> pList)
        {
            // 更新站位列表
            // 清空站位列表
            deviceListView.Items.Clear();
            gMap.Markers.Clear();
            deviceMarkerDictionary.Clear();
            // 反序地加入站位地圖標記，使得排序較前的站位有較高的z-index
            for (int i = pList.Count - 1; i >= 0; i--)
            {
                GMapMarker pm = newDeviceMarker(pList[i]);
            }
            // 加入站位列表
            foreach (DeviceData p in pList)
            {
                deviceListView.Items.Add(p);
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
        private GMapMarker newDeviceMarker(DeviceData p)
        {
            return newDeviceMarker(new PointLatLng(p.lat, p.lng), p);
        }
        private GMapMarker newDeviceMarker(PointLatLng latlng, DeviceData p)
        {
            GMapMarker marker = new GMapMarker(latlng);
            DeviceMarker pm = new DeviceMarker(this, marker, p);
            // 設定 marker 為 device marker 的形狀
            marker.Shape = pm;
            // 將 marker 加入地圖中
            addMarker(marker);
            // 加入 device marker list 中
            deviceMarkerDictionary.Add(p, pm);
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

        // 搜尋 Device 文字方塊文字變更事件處理
        private void textBox_searchDevice_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchDevice(textBox_searchDevice.Text);
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
                Exit();
            }
        }

        private void Exit()
        {
            // 將 Trend data worker 關閉
            // (非必要，關閉應用程式會自動關閉背景工作)
            TrendDataManager.cancelDeviceTagRefresh();
            Application.Current.Shutdown();
        }

        // 站位選項變更事件處理
        private void deviceListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //DeviceData d = deviceListView.SelectedItem as DeviceData;
            //if (d != null)
        }

        /* selection change does not trigger when mouse down on the selected item
         * and mouse down event does not trigger when mouse down on listbox item,
         * so use preview mouse down instead.
         * selectionChanged 在使用者對已選擇項目按下按鈕時不會觸發
         * mouseDown 則不會在項目被按下時觸發
         * 所以使用previewMouseDown
         * 專門針對使用者對已選擇項目按下按鈕，開啟站位資料瀏覽
         * 同時，站位項目點擊，會造成地圖位置改變，而非站位項目選擇改變
         * */
        private void deviceListView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // get newly selected listbox item
            // PreviewMouseDown 的觸發在selection改變之前
            // 所以使用者所按下項目需由下面方法取得
            var item = ItemsControl.ContainerFromElement(deviceListView,
                e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
            {
                // get device data
                DeviceData p = item.Content as DeviceData;

                if (p != null)
                {
                    tabControl_main.SelectedItem = tabItem_map;
                    selectDevice(p);
                }
            }
            else
            {
                deviceListView.SelectedItem = null;
                CurrentMarker = null;
                //deviceListView.UnselectAll();
            }
            new TrendDataManager(_viewModel.PollingRate);
        }

        #endregion

        // 地圖上站位標記的點擊事件處理 : 由 device marker 呼叫
        public void deviceMarkerClicked(DeviceMarker dm)
        {
            // device marker 不能為 null
            Debug.Assert(dm != null);

            // 設定 current marker
            CurrentMarker = dm;
            // 開啟 device simple view
            initDeviceSimpleView(dm.Device);
            // 開啟device flyout
            //initDeviceDataViewFlyout(dm.Device);
        }

        // 選擇站位
        private void selectDevice(DeviceData device)
        {
            // Device 不能為 null
            Debug.Assert(device != null);

            // 更新站位列表的已選取站位
            deviceListView.SelectedItem = device;

            // 更新地圖的目前 Device marker
            DeviceMarker dm = null;
            deviceMarkerDictionary.TryGetValue(device, out dm);
            CurrentMarker = dm;

            // 將站位位置設為中心並調整zoom大小
            gMap.Position = new PointLatLng(device.lat, device.lng);

            // 如果Zoom沒改變，標記位置可能會出錯
            gMap.Zoom = 17;
            gMap.Zoom = 16;

            //開啟simple view
            initDeviceSimpleView(device);
        }

        // 開啟站位內容瀏覽 simple view
        public void initDeviceSimpleView(DeviceData device)
        {
            // 關閉flyout
            deviceFlyout.IsOpen = false;

            // 設定 simple view 之 device
            deviceSimpleView.set(device);

            // 更新站位 Tag Table 內容
            initDeviceTagTable(device);

            // 重置 Tag Table scroll 為頂端
            dataGridScrollToTop(deviceSimpleView.deviceTagTable);
            //開啟 simple view
            deviceSimpleView.Visibility = Visibility.Visible;
        }

        // 開啟站位內容瀏覽 flyout
        public void initDeviceDataViewFlyout(DeviceData device)
        {
            // 隱藏 simple view
            deviceSimpleView.Visibility = Visibility.Hidden;

            // 初始化站位內容瀏覽畫面
            deviceFlyout.Header = device.alias;
            textBlock_deviceAddr.Text = device.addr;
            textBox_searchTag.Text = "";

            // 更新站位 Tag Table 內容
            initDeviceTagTable(device);

            // 重置 Tag Table scroll 為頂端
            dataGridScrollToTop(deviceTagTable);
            // 開啟Flyout
            deviceFlyout.IsOpen = true;
        }

        // 更新站位 Tag Table 內容
        private void initDeviceTagTable(DeviceData device)
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

        #region -- 站位、點位搜尋方法 --
        public void searchDevice(string keywordCommand)
        {
            string[] keywordList = parseKeyword(keywordCommand);

            foreach (DeviceData p in deviceList)
            {
                // removal phase
                if (deviceListView.Items.Contains(p))
                {
                    if (!p.Contains(keywordList))
                        deviceListView.Items.Remove(p);
                }
                // add back phase
                else
                {
                    if (p.Contains(keywordList))
                        deviceListView.Items.Add(p);
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

            // center zoom to device
            mapControl.Position = lockPosition;
            mapControl.Zoom += (delta > 0) ? 1 : -1;

            int zoom = (int)mapControl.Zoom;

            // compute render offset
            GPoint renderOffset = GPoint.Empty;
            renderOffset.X = (int)mapControl.RenderSize.Width / 2 - mouseLastZoom.X;
            renderOffset.Y = (int)mapControl.RenderSize.Height / 2 - mouseLastZoom.Y;

            // current pixel position of the device
            GPoint positionPixel = mapControl.MapProvider.Projection.FromLatLngToPixel(lockPosition, zoom);

            // new center position in pixel
            positionPixel.Offset(renderOffset);

            // compute and set the latlng of new center position
            mapControl.Position = mapControl.MapProvider.Projection.FromPixelToLatLng(positionPixel, zoom);
        }

        private void deviceFlyout_IsOpenChanged(object sender, EventArgs e)
        {
            if (!deviceFlyout.IsOpen)
            {
                initDeviceSimpleView(CurrentMarker.Device);
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
