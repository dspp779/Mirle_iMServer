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

namespace Mirle_GPLC.CustomeMarkers
{
    /// <summary>
    /// MarkerHollow.xaml 的互動邏輯
    /// </summary>
    public partial class ProjectMarker
    {
        Popup Popup;
        Label Label;
        GMapMarker Marker;
        GMapControl gMap;
        public ProjectData Project;

        public ProjectMarker(GMapControl gMap, GMapMarker marker, ProjectData project)
        {
            InitializeComponent();

            this.gMap = gMap;
            this.Marker = marker;
            this.Project = project;
        }

        private void initProjectMarker()
        {
            
            Popup = new Popup();
            Label = new Label();

            this.Loaded += new RoutedEventHandler(PojectMarker_Loaded);
            this.SizeChanged += new SizeChangedEventHandler(ProjectMarker_SizeChanged);
            this.MouseEnter += new MouseEventHandler(ProjectMarker_MouseEnter);
            this.MouseLeave += new MouseEventHandler(ProjectMarker_MouseLeave);
            this.MouseMove += new MouseEventHandler(ProjectMarker_MouseMove);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(ProjectMarker_MouseLeftButtonUp);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(ProjectMarker_MouseLeftButtonDown);

            Popup.Placement = PlacementMode.Mouse;
            {
                Label.Background = Brushes.Blue;
                Label.Foreground = Brushes.White;
                Label.BorderBrush = Brushes.WhiteSmoke;
                Label.BorderThickness = new Thickness(2);
                Label.Padding = new Thickness(5);
                Label.FontSize = 22;
                Label.Content = Project.name;
            }
            Popup.Child = Label;
        }

        void PojectMarker_Loaded(object sender, RoutedEventArgs e)
        {
            if (icon.Source.CanFreeze)
            {
                icon.Source.Freeze();
            }
        }

        void ProjectMarker_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Marker.Offset = new Point(-e.NewSize.Width / 2, -e.NewSize.Height / 2);
        }

        void ProjectMarker_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
            {
                Point p = e.GetPosition(gMap);
                Marker.Position = gMap.FromLocalToLatLng((int)p.X, (int)p.Y);
            }
        }

        void ProjectMarker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseCaptured)
            {
                Mouse.Capture(this);
            }
        }

        void ProjectMarker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                Mouse.Capture(null);
            }
        }

        void ProjectMarker_MouseLeave(object sender, MouseEventArgs e)
        {
            Marker.ZIndex -= 10000;
            Popup.IsOpen = false;
        }

        void ProjectMarker_MouseEnter(object sender, MouseEventArgs e)
        {
            Marker.ZIndex += 10000;
            Popup.IsOpen = true;
        }

        /* 滑鼠位於標記點上時，滾輪滑動事件處理
         * 
         * 說明：
         * 在GMap中，如果對Zoom值直接修，作畫面進行縮放時，中心點會維持不變
         * 而此事件情況為滑鼠滑鼠位於標記點上時，使用者通常希望觀看標記點的資訊
         * 所以滑鼠所位於的標記點，必須在畫面上保持不動
         * 
         * 以下作法為
         * 先記錄標記點在畫面的位置 (mouseLastZoom)
         * 以標記點做為中心進行縮放 (Zoom+-)
         * 
         * 算出先前已記錄畫面位置的點與畫面中心的像素差 (renderOffset)
         * 此像素差即為整張地圖所需的位移量
         * 
         * 算出新的中心點像素位置：當前標記點的像素位置+位移量
         * 算出中心點經緯座標：FromPixelToLatLng
         */
        private void ProjectMarker_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // latlng position of the project
            PointLatLng projectPosition = new PointLatLng(Project.lat, Project.lng);

            // local position of the project
            GPoint mouseLastZoom = gMap.FromLatLngToLocal(projectPosition);

            // center zoom to project
            gMap.Position = projectPosition;
            gMap.Zoom += (e.Delta > 0) ? 1 : -1;

            int zoom = (int)gMap.Zoom;

            // compute render offset
            GPoint renderOffset = GPoint.Empty;
            renderOffset.X = (int)gMap.RenderSize.Width/2 - mouseLastZoom.X;
            renderOffset.Y = (int)gMap.RenderSize.Height/2 - mouseLastZoom.Y;

            // current pixel position of the project
            GPoint positionPixel = gMap.MapProvider.Projection.FromLatLngToPixel(projectPosition, zoom);

            // new center position in pixel
            positionPixel.Offset(renderOffset);

            // compute and set the latlng of new center position
            gMap.Position = gMap.MapProvider.Projection.FromPixelToLatLng(positionPixel, zoom);
        }
    }
}
