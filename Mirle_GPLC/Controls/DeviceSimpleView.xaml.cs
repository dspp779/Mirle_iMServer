using Mirle.iMServer.Model;
using Mirle.iMServer.Model.Utility;
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

namespace Mirle_GPLC.Controls
{
    /// <summary>
    /// DeviceSimpleView.xaml 的互動邏輯
    /// </summary>
    public partial class DeviceSimpleView : UserControl
    {

        private MainWindow mainWindow;
        private DeviceData device;

        private bool isDragging;
        private Point clickPosition;
        private TranslateTransform renderTransform = new TranslateTransform();

        public DeviceSimpleView()
        {
            InitializeComponent();
        }

        public void init(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            this.projectTagTable.LostMouseCapture += (sender, e) => { this.isDragging = false; };
        }

        public void set(DeviceData d)
        {
            this.device = d;
            textBlock_projectName.Text = device.alias;
            device.reload();
            TrendDataManager.registerDeviceTagRefresh(device.tags);
        }

        private void textBox_searchTag_TextChanged(object sender, TextChangedEventArgs e)
        {
            mainWindow.searchTag(textBox_searchTag.Text);
        }

        private void button_close_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            TrendDataManager.cancelDeviceTagRefresh();
        }

        private void button_expand_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.initProjectDataViewFlyout(device);
        }

        private void DeviceSimpleView_MouseMove(object sender, MouseEventArgs e)
        {
            var draggableControl = sender as UserControl;

            if (isDragging && draggableControl != null)
            {
                UIElement parent = this.Parent as UIElement;
                Point currentPosition = e.GetPosition(parent);

                var tranform = draggableControl.RenderTransform as TranslateTransform;
                if (tranform == null)
                {
                    tranform = new TranslateTransform();
                    draggableControl.RenderTransform = tranform;
                }
                tranform.X = currentPosition.X - clickPosition.X + renderTransform.X;
                tranform.Y = currentPosition.Y - clickPosition.Y + renderTransform.Y;
                // draggable control boundary condition
                double boundary;
                tranform.Y = (tranform.Y > -draggableControl.Margin.Top) ?
                    tranform.Y : -draggableControl.Margin.Top;
                boundary = draggableControl.Margin.Bottom + draggableControl.RenderSize.Height - 20;
                tranform.Y = (tranform.Y < boundary) ?
                    tranform.Y : boundary;
                boundary = draggableControl.Margin.Left + draggableControl.RenderSize.Width - 10;
                tranform.X = (tranform.X < boundary) ?
                    tranform.X : boundary;
                boundary = draggableControl.Margin.Left + draggableControl.RenderSize.Width
                    - parent.RenderSize.Width;
                tranform.X = (tranform.X > boundary) ?
                    tranform.X : boundary;
            }
        }

        private void DeviceSimpleView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            var draggableControl = sender as UserControl;
            clickPosition = e.GetPosition(this.Parent as UIElement);
            draggableControl.CaptureMouse();
        }

        private void DeviceSimpleView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            var draggableControl = sender as UserControl;
            draggableControl.ReleaseMouseCapture();

            var tranform = draggableControl.RenderTransform as TranslateTransform;
            if (tranform == null)
            {
                tranform = new TranslateTransform();
                draggableControl.RenderTransform = tranform;
            }
            renderTransform.X = tranform.X;
            renderTransform.Y = tranform.Y;
        }

    }
}
