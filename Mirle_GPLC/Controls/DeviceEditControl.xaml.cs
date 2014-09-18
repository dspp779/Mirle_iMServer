using GMap.NET;
using GMap.NET.WindowsPresentation;
using Mirle.iMServer.Model;
using Mirle_GPLC.CustomeMarkers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mirle_GPLC.Controls
{
    /// <summary>
    /// DeviceEditControl.xaml 的互動邏輯
    /// </summary>
    public partial class DeviceEditControl : UserControl
    {
        private float lat, lng;
        private List<DeviceData> deviceList;

        // Device Listbox 的資料來源
        private ListCollectionView _dataSource;
        public ListCollectionView DataSource
        {
            get { return _dataSource; }
            set
            {
                _dataSource = value;
            }
        }

        // 紀錄目前選擇的device
        private DeviceData _device = new DeviceData();
        public DeviceData Device
        {
            get { return _device; }
            set
            {
                if (value != null )
                {
                    _device = value;
                    button_edit.IsEnabled = true;
                }
                else
                {
                    _device = DeviceData.Empty;
                    button_edit.IsEnabled = false;
                }

                // 更新 textbox 的值
                textBlock_deviceName.Text = _device.deviceName;
                textBox_deviceAlias.Text = _device.alias;
                textBox_deviceAddr.Text = _device.addr;
                // 更新地圖選取位置與縮放，別名存在代表站位已設定
                if (!string.IsNullOrWhiteSpace(_device.alias))
                {
                    textBox_lng.Text = _device.lng.ToString();
                    textBox_lat.Text = _device.lat.ToString();
                    // 調整標記與地圖的位置
                    PointLatLng position = new PointLatLng(_device.lat, _device.lng);
                    MarkerPosition = position;
                    Map_SetPosition.Position = position;
                    // 如果Zoom沒改變，標記位置可能會出錯
                    Map_SetPosition.Zoom = 17;
                    Map_SetPosition.Zoom = 18;
                }
                else
                {
                    //textBox_lng.Text = textBox_lat.Text = "";
                }
            }
        }

        // 地圖的標記，及其位置變更處理
        private GMapMarker currMarker;
        private PointLatLng MarkerPosition
        {
            get { return currMarker.Position; }
            set
            {
                if (currMarker == null)
                {
                    // 新增 marker
                    currMarker = new GMapMarker(value);
                    // 設定 shape 為 click marker
                    currMarker.Shape = new ClickMarker(this, currMarker);
                    // 將 marker 加入地圖
                    Map_SetPosition.Markers.Add(currMarker);
                }
                // 設定 marker 的位置
                currMarker.Position = value;
            }
        }
        public DeviceEditControl()
        {
            InitializeComponent();

            // 設定 Control 的資料繫結物件
            DataContext = this;

            // load device list
            refreshDeviceList();

            // 設定 Device Listbox 資料來源
            var data = new ObservableCollection<DeviceData>(deviceList);
            // 設定 Device Listbox 的 datasource
            DataSource = new ListCollectionView(data);
        }

        private void DeviceEditUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            listBox_devices.UnselectAll();
            // 設定地圖來源
            //Map_SetPosition.MapProvider = MainWindow.currentSetting.MapProvider;
            // 設定初始檢視大小
            Map_SetPosition.Zoom = 8;
            // 關閉顯示中心紅十字
            Map_SetPosition.ShowCenter = false;
            // 設定滑鼠滑動地圖按鈕為右鍵 (預設為右鍵)
            Map_SetPosition.DragButton = MouseButton.Left;
            // 設定初始位置
            Map_SetPosition.Position = new PointLatLng(23.8, 121);
            // 設定初始經緯度文字方塊
            textBox_lng.Text = textBox_lat.Text = "";
        }

        private void refreshDeviceList()
        {
            try
            {
                deviceList = ModelUtil.getDeviceList();
            }
            catch (Exception)
            {
                // 錯誤發生
                deviceList = new List<DeviceData>();
            }
        }

        private void listBox_devices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 設定目前站位
            Device = listBox_devices.SelectedItem as DeviceData;
        }

        private void button_edit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 分析 輸入資料
                string alias = textBox_deviceAlias.Text.Trim();
                string addr = textBox_deviceAddr.Text.Trim();
                float.TryParse(textBox_lng.Text.Trim(), out lng);
                float.TryParse(textBox_lat.Text.Trim(), out lat);

                Debug.Assert(!String.IsNullOrWhiteSpace(Device.deviceName));

                // 新增站位物件包含輸入的資訊
                DeviceData device =
                    new DeviceData(alias, Device.deviceName, addr, lat, lng);

                // 插入 或 更新 站位資訊
                ModelUtil.insertUpdateDervice(device);
                // apply to viewing content
                Device.apply(device);
            }
            catch (Exception ex)
            {
                MainWindow.runningInstance.messageDialog("編輯站位發生錯誤", ex.Message);
            }
            finally
            {
                // 更新 MainWindow
                MainWindow.runningInstance.refreshData();
            }
        }

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

        private void Map_SetPosition_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // if not dragging, place marker
            if (!Map_SetPosition.IsDragging)
            {
                Point p = e.GetPosition(Map_SetPosition);
                MarkerPosition = Map_SetPosition.FromLocalToLatLng((int)p.X, (int)p.Y);
                refreshLatlngTextBox();
                // zoom adjuctment
                if (Map_SetPosition.Zoom < 16)
                {
                    positionLockZoom(Map_SetPosition, MarkerPosition, 5);
                }
                else if (Map_SetPosition.Zoom < 18)
                {
                    positionLockZoom(Map_SetPosition, MarkerPosition, 18 - (int)Map_SetPosition.Zoom);
                }
            }
        }

        #region -- textbox latlng refreshing --

        private void refreshLatlngTextBox()
        {
            textBox_lng.Text = currMarker.Position.Lng.ToString();
            textBox_lat.Text = currMarker.Position.Lat.ToString();
        }

        private void textBox_lat_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (float.TryParse(textBox_lat.Text.Trim(), out lat) && textBox_lat.IsFocused)
            {
                // 更新 marker 緯度
                PointLatLng position = new PointLatLng(lat, 0);
                if (currMarker != null)
                {
                    position.Lng = currMarker.Position.Lng;
                }
                MarkerPosition = position;
                Map_SetPosition.Position = position;
            }
        }

        private void textBox_lng_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (float.TryParse(textBox_lng.Text.Trim(), out lng) && textBox_lng.IsFocused)
            {
                // 更新 marker 經度
                PointLatLng position = new PointLatLng(0, lng);
                if (currMarker != null)
                {
                    position.Lat = currMarker.Position.Lat;
                }
                MarkerPosition = position;
                Map_SetPosition.Position = position;
            }
        }
        #endregion

    }
}
