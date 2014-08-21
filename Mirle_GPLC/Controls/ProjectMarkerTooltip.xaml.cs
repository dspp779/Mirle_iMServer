using Mirle.iMServer.Model;
using System.Windows.Controls;

namespace Mirle_GPLC.Controls
{
   /// <summary>
   /// Interaction logic for TrolleyTooltip.xaml
   /// </summary>
    public partial class ProjectMarkerTooltip : UserControl
    {
        public ProjectMarkerTooltip()
        {
            InitializeComponent();
        }

        public void SetValues(ProjectData project)
        {
            textblock_ProjectName.Text = project.name;
            textBlock_ProjectAddress.Text = project.addr;
            textBlock_DeviceNum.Text = project.devices.Count.ToString();
            //TimeGps.Text = project.lat.ToString();
            //Area.Text = project.lng.ToString();
        }
    }
}
