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

namespace MicroLCDBuilder
{
    /// <summary>
    /// Logica di interazione per PixelControl.xaml
    /// </summary>
    public partial class PixelControl : UserControl
    {

        private bool mState;

        public PixelControl()
        {
            mState = false;

            InitializeComponent();

            pixelPanel.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

            IsHitTestVisible = true;

            Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            
        }

        private void StackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if ( !mState ) UpdatePixel(mState);
        }

        private void StackPanel_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if ( mState ) UpdatePixel(mState);
        }

        private void UpdatePixel(bool state)
        {
            if (mState)
            {
                pixelPanel.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                mState = false;
            }
            else
            {
                pixelPanel.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                mState = true;
            }
        }

        public bool PixelValue
        {
            get { return mState; }
            set
            {
                UpdatePixel(value);
            }
        }

        public byte PixelBit
        {
            get { return (byte)(mState ? 1 : 0); }
            set
            {
                UpdatePixel(value == 0 ? false : true);
            }
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            Point pt = hitTestParameters.HitPoint;
            
            return new PointHitTestResult(this, pt);
        }
    }
}
