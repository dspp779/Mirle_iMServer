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
using System.Windows.Controls.Primitives;
using GMap.NET.WindowsPresentation;
using Mirle.iMServer.Model;
using GMap.NET;
using Mirle_GPLC.Controls;
using MahApps.Metro.Controls;

namespace Mirle_GPLC.CustomeMarkers
{
    /// <summary>
    /// MarkerHollow.xaml 的互動邏輯
    /// </summary>
    public partial class DeviceMarker
    {
        // marker image uri
        private static string markerHollowPath = "pack://application:,,,/Mirle_GPLC;component/CustomeMarkers/DeviceMarkerHollow.png";
        private static string markerFillPath = "pack://application:,,,/Mirle_GPLC;component/CustomeMarkers/DeviceMarkerFill.png";

        Popup Popup;
        GMapMarker Marker;
        MainWindow mainWindow;
        
        public DeviceData Device;
        bool selected;

        public string name
        {
            get { return Device.alias; }
        }
        public double lat
        {
            get { return Device.lat; }
        }
        public double lng
        {
            get { return Device.lng; }
        }
        public bool IsSelected
        {
            get { return selected; }
            set
            {
                selected = value;
                if (value)
                {
                    Marker.ZIndex += 10000;
                    icon.Source = new BitmapImage(new Uri(markerFillPath));
                }
                else
                {
                    Marker.ZIndex -= 10000;
                    icon.Source = new BitmapImage(new Uri(markerHollowPath));
                }
            }
        }
        

        public DeviceMarker(MainWindow mainWindow , GMapMarker marker, DeviceData device)
        {
            InitializeComponent();

            this.mainWindow = mainWindow;
            this.Marker = marker;
            this.Device = device;
            selected = false;

            initDeviceMarkerPopup();
        }

        private void initDeviceMarkerPopup()
        {
            Popup = new Popup();
            Popup.Placement = PlacementMode.Mouse;
        }

        void DeviceMarker_Loaded(object sender, RoutedEventArgs e)
        {
            if (icon.Source.CanFreeze)
            {
                icon.Source.Freeze();
            }
        }

        void DeviceMarker_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Marker.Offset = new Point(-e.NewSize.Width / 2, -e.NewSize.Height / 2);
        }

        void DeviceMarker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        void DeviceMarker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mainWindow.deviceMarkerClicked(this);
        }

        void DeviceMarker_MouseLeave(object sender, MouseEventArgs e)
        {
            if(!selected)
                icon.Source = new BitmapImage(new Uri(markerHollowPath));
            Marker.ZIndex -= 100000;
            Popup.IsOpen = false;
            Popup.Child = null;
        }

        void DeviceMarker_MouseEnter(object sender, MouseEventArgs e)
        {
            icon.Source = new BitmapImage(new Uri(markerFillPath));
            Marker.ZIndex += 100000;
            DeviceMarkerTooltip tooltip = new DeviceMarkerTooltip();
            tooltip.SetValues(Device);
            Popup.Child = tooltip;
            Popup.IsOpen = true;
        }

        /* 滑鼠位於標記點上時，滾輪滑動事件處理
         * 
         * 說明：
         * 在GMap中，如果對Zoom值直接修，作畫面進行縮放時，中心點會維持不變
         * 而此事件情況為滑鼠滑鼠位於標記點上時，使用者通常希望觀看標記點的資訊
         * 所以滑鼠所位於的標記點，必須在畫面上保持不動
         * 
         * 作法為
         * 先記錄標記點在畫面的位置 (mouseLastZoom)
         * 以標記點做為中心進行縮放 (Zoom+-)
         * 
         * 算出先前已記錄畫面位置的點與畫面中心的像素差 (renderOffset)
         * 此像素差即為整張地圖所需的位移量
         * 
         * 算出新的中心點像素位置：當前標記點的像素位置+位移量
         * 算出中心點經緯座標：FromPixelToLatLng
         */
        private void DeviceMarker_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            PointLatLng projectPosition = new PointLatLng(Device.lat, Device.lng);
            // hover position locking zoom
            mainWindow.positionLockZoom(mainWindow.gMap, projectPosition, (e.Delta > 0) ? 1 : -1);
        }

        public override string ToString()
        {
            return Device.ToString();
        }

        private void DeviceMarker_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PointLatLng projectPosition = new PointLatLng(Device.lat, Device.lng);
            // hover position locking zoom
            mainWindow.positionLockZoom(mainWindow.gMap, projectPosition, 1);
        }
    }
}
