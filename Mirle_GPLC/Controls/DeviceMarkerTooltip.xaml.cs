using Mirle.iMServer.Model;
using System.Windows.Controls;

namespace Mirle_GPLC.Controls
{
   /// <summary>
   /// Interaction logic for DeviceMarkerTooltip.xaml
   /// </summary>
    public partial class DeviceMarkerTooltip : UserControl
    {
        public DeviceMarkerTooltip()
        {
            InitializeComponent();
        }

        public void SetValues(DeviceData device)
        {
            textBlock_DeviceName.Text = device.ToString();
            textBlock_TagNum.Text = device.tags.Count.ToString();
        }
    }
}
