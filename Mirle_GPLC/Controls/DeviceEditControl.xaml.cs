﻿using GMap.NET;
using GMap.NET.WindowsPresentation;
using Mirle.iMServer.Model;
using Mirle_GPLC.CustomeMarkers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// projectEditControl.xaml 的互動邏輯
    /// </summary>
    public partial class DeviceEditControl : UserControl
    {
        private MainWindow mainWindow;
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

        private DeviceData _device = new DeviceData();
        public DeviceData Device
        {
            get { return _device; }
            set
            {
                if (value != null )
                {
                    _device = value;
                }
                else
                {
                    _device = DeviceData.Empty;
                }
                textBlock_deviceName.Text = _device.deviceName;
                textBox_deviceAlias.Text = _device.alias;
                textBox_deviceAddr.Text = _device.addr;
                textBox_lng.Text = _device.lng.ToString();
                textBox_lat.Text = _device.lat.ToString();
                if (_device.id > 0)
                {
                    PointLatLng position = new PointLatLng(_device.lat, _device.lng);
                    markerPosition = position;
                    Map_SetPosition.Position = position;
                    Map_SetPosition.Zoom = 18;
                }
            }
        }

        private GMapMarker currMarker;
        private PointLatLng markerPosition
        {
            get { return currMarker.Position; }
            set
            {
                if (currMarker == null)
                {
                    currMarker = new GMapMarker(value);
                    currMarker.Shape = new ClickMarker(this, currMarker);
                    Map_SetPosition.Markers.Add(currMarker);
                }
                currMarker.Position = value;
                textBox_lng.Text = currMarker.Position.Lng.ToString();
                textBox_lat.Text = currMarker.Position.Lat.ToString();
            }
        }
        public DeviceEditControl()
        {
            InitializeComponent();
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
            // 設定地圖來源
            Map_SetPosition.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            // 設定初始檢視大小
            Map_SetPosition.Zoom = 8;
            // 關閉顯示中心紅十字
            Map_SetPosition.ShowCenter = false;
            // 設定滑鼠滑動地圖按鈕為右鍵 (預設為右鍵)
            Map_SetPosition.DragButton = MouseButton.Right;
            // 設定初始位置
            Map_SetPosition.Position = new PointLatLng(23.8, 121);

            listBox_devices.UnselectAll();
        }

        public void init(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        private void refreshDeviceList()
        {
            deviceList = ModelUtil.getDeviceList();
        }

        private void button_reset_Click(object sender, RoutedEventArgs e)
        {
            Device = listBox_devices.SelectedItem as DeviceData;
        }

        private void listBox_devices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Device = listBox_devices.SelectedItem as DeviceData;
        }

        private void button_edit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string alias = textBox_deviceAlias.Text.Trim();
                string addr = textBox_deviceAddr.Text.Trim();
                float.TryParse(textBox_lng.Text.Trim(), out lng);
                float.TryParse(textBox_lat.Text.Trim(), out lat);
                DeviceData device =
                    new DeviceData(Device.id, alias, Device.deviceName, addr, lat, lng);
                insertUpdate(device);
            }
            catch (Exception ex)
            {
                mainWindow.messageDialog("編輯站位發生錯誤", ex.Message);
            }
            finally
            {
                MainWindow.runningInstance.refreshData();
            }
        }

        private void insertUpdate(DeviceData device)
        {
            // update
            if (device.id > 0)
            {
                ModelUtil.updateDervice(device);
                // apply to viewing content
                Device.apply(device);
            }
            // insert
            else
            {
                int id = ModelUtil.insertDervice(device);
                // apply to viewing content
                Device.apply(id, device);
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

        private void Map_SetPosition_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Map_SetPosition.IsDragging)
            {
                Point p = e.GetPosition(Map_SetPosition);
                markerPosition = Map_SetPosition.FromLocalToLatLng((int)p.X, (int)p.Y);
                if (Map_SetPosition.Zoom < 16)
                {
                    positionLockZoom(Map_SetPosition, markerPosition, 5);
                }
                else if (Map_SetPosition.Zoom < 18)
                {
                    positionLockZoom(Map_SetPosition, markerPosition, 18 - (int)Map_SetPosition.Zoom);
                }
            }
        }

        private void textBox_latlng_TextChanged(object sender, TextChangedEventArgs e)
        {
        }
        
    }
}