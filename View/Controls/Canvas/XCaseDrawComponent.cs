using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace XCase.View.Controls
{
    /// <summary>
    /// Encompasses <see cref="XCaseCanvas"/> in a <see cref="ScrollViewer"/>.
    /// </summary>
    public partial class XCaseDrawComponent
    {
        /// <summary>
        /// Gets encompassed <see cref="XCaseCanvas"/>
        /// </summary>
        /// <value><see cref="XCaseCanvas"/></value>
        public XCaseCanvas Canvas
        {
            get
            {
                return canvas;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XCaseDrawComponent"/> class.
        /// </summary>
        public XCaseDrawComponent()
        {
            InitializeComponent();


            //DoubleAnimation d = new DoubleAnimation(1, 100, new Duration(TimeSpan.FromSeconds(3)));
            //s.Children.Add(d);
            //Storyboard.SetTargetName();
            //s.Completed += s_Completed;
        }

        private void dockPanel_MouseMove(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(dockPanel);

            if (sliderPanel.Visibility == Visibility.Collapsed
                && dockPanel.ActualHeight - position.Y < 70)
            {
                sliderPanel.Visibility = Visibility.Visible;
            }

            if (sliderPanel.Visibility == Visibility.Visible
                && dockPanel.ActualHeight - position.Y >= 70)
            {
                sliderPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void dockPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(dockPanel);
            if (position.X >= 0 && position.X <= dockPanel.ActualWidth
                && position.Y >= 0 && position.Y <= dockPanel.ActualHeight)
            {

            }
            else
            {
                sliderPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void zoomSlider_MouseLeave(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(sliderPanel);
            if (position.X >= 0 && position.X <= sliderPanel.ActualWidth
                && position.Y >= 0 && position.Y <= sliderPanel.ActualHeight)
            {

            }
            else
            {
                sliderPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void bReset_Click(object sender, RoutedEventArgs e)
        {
            zoomSlider.Value = 1;
            scaleTransform.ScaleX = zoomSlider.Value;
            scaleTransform.ScaleY = zoomSlider.Value;
        }

        private void viewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var zsv = zoomSlider.Value; 
            var ho = scrollViewer.HorizontalOffset;

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                zoomSlider.Value += e.Delta > 0 ? 0.1 : -0.1;
                e.Handled = true;
            }
            
            scrollViewer.ScrollToHorizontalOffset(ho * (1 + (zoomSlider.Value - zsv)));
            scaleTransform.ScaleX = zoomSlider.Value;
            scaleTransform.ScaleY = zoomSlider.Value;
        }

        private void bZoomIn_Click(object sender, RoutedEventArgs e)
        {
            zoomSlider.Value += 0.1;
            scaleTransform.ScaleX = zoomSlider.Value;
            scaleTransform.ScaleY = zoomSlider.Value;
        }

        private void bZoomOut_Click(object sender, RoutedEventArgs e)
        {
            zoomSlider.Value -= 0.1;
            scaleTransform.ScaleX = zoomSlider.Value;
            scaleTransform.ScaleY = zoomSlider.Value;
        }

        private void zoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (scaleTransform != null)
            {
                scaleTransform.ScaleX = zoomSlider.Value;
                scaleTransform.ScaleY = zoomSlider.Value;
            }
        }
    }
}

