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
        ProjectData Project;

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

        private void ProjectMarker_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //Point p = e.GetPosition(gMap);
            //gMap.Position = gMap.FromLocalToLatLng((int)p.X, (int)p.Y);
            gMap.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
            gMap.Position = new GMap.NET.PointLatLng(Project.lat, Project.lng);
            gMap.Zoom += (e.Delta > 0) ? 1 : -1;
            gMap.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionWithoutCenter;
        }
    }
}
