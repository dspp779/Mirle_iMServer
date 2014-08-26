using Mirle.iMServer.Model;
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
    /// ProjectView.xaml 的互動邏輯
    /// </summary>
    public partial class DeviceSimpleView : UserControl
    {

        private MainWindow mainWindow;
        private DeviceData project;

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

        public void set(DeviceData p)
        {
            this.project = p;
            textBlock_projectName.Text = project.alias;
        }

        private void textBox_searchTag_TextChanged(object sender, TextChangedEventArgs e)
        {
            mainWindow.searchTag(textBox_searchTag.Text);
        }

        private void button_close_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void button_expand_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.initProjectDataViewFlyout(project);
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            var draggableControl = sender as UserControl;

            if (isDragging && draggableControl != null)
            {
                Point currentPosition = e.GetPosition(this.Parent as UIElement);

                var tranform = draggableControl.RenderTransform as TranslateTransform;
                if (tranform == null)
                {
                    tranform = new TranslateTransform();
                    draggableControl.RenderTransform = tranform;
                }
                tranform.X = currentPosition.X - clickPosition.X + renderTransform.X;
                tranform.Y = currentPosition.Y - clickPosition.Y + renderTransform.Y;
            }
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            var draggableControl = sender as UserControl;
            clickPosition = e.GetPosition(this.Parent as UIElement);
            draggableControl.CaptureMouse();
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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
