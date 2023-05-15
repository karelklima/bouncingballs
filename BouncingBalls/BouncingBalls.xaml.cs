using System;
using System.Collections.Generic;
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
using System.Windows.Threading;

namespace BouncingBalls
{
    /// <summary>
    /// Interaction logic for Screen.xaml
    /// </summary>
    public partial class Screen : Window
    {

        private Engine Engine;
        
        public Screen()
        {
            InitializeComponent();
            InitializeBouncingBalls();
        }

        public void InitializeBouncingBalls()
        {
            this.Engine = new Engine(this.Canvas);
            this.Engine.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Engine.InitializeSoundEngine();
            this.Engine.PrecalculateSoundProperties();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Engine.PrecalculateSoundProperties();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Engine.Stop();
            Application.Current.Shutdown();
        }

        private void BouncingBalls_Click(object sender, RoutedEventArgs e)
        {
            this.Overlay.Visibility = this.Overlay.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Gravity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.Engine != null)
                this.Engine.SetGravity((double) this.Gravity.Value);
        }

        private void DropRate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.Engine != null)
                this.Engine.SetDropRate((Int16) this.DropRate.Value);
        }

        private void BallSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.Engine != null)
            {
                this.Engine.SetBallSize((double) this.BallSize.Value);
                Resources["BallSize"] = this.BallSize.Value;
                Resources["DropZoneSize"] = this.BallSize.Value + 6;
            }
            //MessageBox.Show(Convert.ToString(Resources["BallSize"]));
        }

        private void LineSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.Engine != null)
            {
                this.Engine.SetLineSize((double) this.LineSize.Value);
                Resources["LineSize"] = this.LineSize.Value;
                Resources["AnchorSize"] = this.LineSize.Value * 1.6;
            }
        }

        private void BounceAcceleration_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.Engine != null)
                this.Engine.SetBounceAcceleration((double) this.BounceAcceleration.Value);
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            Button Button = (Button)sender;
            Resources["BackgroundBrush"] = (SolidColorBrush)Button.Background;
        }

        private void ClearContentButton_Click(object sender, RoutedEventArgs e)
        {
            this.Engine.ClearAll();
        }

        private void ResetSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            this.Gravity.Value = 0.2;
            this.BounceAcceleration.Value = 0.95;
            this.DropRate.Value = 850;
            this.BallSize.Value = 10;
            this.LineSize.Value = 3;
        }

        private void SoundCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            this.Engine.SetSoundEnabled(true);
        }

        private void SoundCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            this.Engine.SetSoundEnabled(false);
        }

    }

}
