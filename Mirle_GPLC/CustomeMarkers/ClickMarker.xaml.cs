using GMap.NET;
using GMap.NET.WindowsPresentation;
using Mirle_GPLC.Controls;
using System;
using System.Collections.Generic;
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

namespace Mirle_GPLC.CustomeMarkers
{
    /// <summary>
    /// ClickMarker.xaml 的互動邏輯
    /// </summary>
    public partial class ClickMarker
    {
        GMapMarker Marker;
        DeviceEditControl Control;

        public ClickMarker(DeviceEditControl control, GMapMarker marker)
        {
            InitializeComponent();

            this.Control = control;
            this.Marker = marker;
        }

        private void ClickMarker_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Marker.Offset = new Point(-e.NewSize.Width / 2, -e.NewSize.Height / 2);
        }

        private void ClickMarker_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // hover position locking zoom
            Control.positionLockZoom(Control.Map_SetPosition, Marker.Position, (e.Delta > 0) ? 1 : -1);
        }

        private void ClickMarker_Loaded(object sender, RoutedEventArgs e)
        {
            if (icon.Source.CanFreeze)
            {
                icon.Source.Freeze();
            }
        }

    }
}
