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
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private int mNumRow;
        private int mNumColumn;

        private bool mDrawingState;

        public MainWindow()
        {
            InitializeComponent();

            mNumRow = 8;
            mNumColumn = 8;

            rowText.Text = mNumRow.ToString();
            columnText.Text = mNumColumn.ToString();

            UpdateGrid(mNumRow, mNumColumn);

            matrixGrid.MouseDown += MatrixGrid_MouseDown;
            matrixGrid.MouseMove += MatrixGrid_MouseMove;
            matrixGrid.MouseUp += MatrixGrid_MouseUp;

            mDrawingState = false;
        }


        private void UpdateGrid(int row, int col)
        {
            //rimuovo tutto
            matrixGrid.ColumnDefinitions.Clear();
            matrixGrid.Children.Clear();

            for ( int i = 0; i < col; i++ )
            {
                ColumnDefinition colDef = new ColumnDefinition();

                colDef.Width = new GridLength(20);

                matrixGrid.ColumnDefinitions.Add(colDef);

                //aggiungo uno stack
                StackPanel panel = new StackPanel();

                panel.HorizontalAlignment = HorizontalAlignment.Stretch;
                panel.VerticalAlignment = VerticalAlignment.Stretch;
                panel.Orientation = Orientation.Vertical;
                panel.Margin = new Thickness(0, 2, 0, 0);
                panel.SetValue(Grid.ColumnProperty, i);

                //aggiungo gli elementi in riga

                for (int j = 0; j < row; j++)
                {
                    PixelControl bt = new PixelControl();
                    bt.HorizontalAlignment = HorizontalAlignment.Stretch;
                    bt.Height = 20;
                    bt.Margin = new Thickness(2, 0, 0, 2);
                    bt.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                    bt.Tag = false;

                    panel.Children.Add(bt);
                }
                
                //aggiungo
                matrixGrid.Children.Add(panel);
            }
        }

        private void MatrixGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mDrawingState)
            {
                
                //ok abilito
                mDrawingState = false;
                matrixGrid.ReleaseMouseCapture();
                
            }
        }

        private void MatrixGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if ( mDrawingState )
            {
                //avvio disegno se il click avviene sopra un pixel
                Point pt = e.GetPosition(matrixGrid);

                IInputElement elem = matrixGrid.InputHitTest(pt);

                if (elem != null)
                {
                    //switch del pixel
                    PixelControl pixel = elem as PixelControl;

                    bool pixel_val = pixel.PixelValue;

                    pixel.PixelValue = !pixel_val;
                }
            }
        }

        private void MatrixGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ( !mDrawingState )
            {
                //avvio disegno se il click avviene sopra un pixel
                Point pt = e.GetPosition(matrixGrid);

                IInputElement elem = matrixGrid.InputHitTest(pt);

                if ( elem != null )
                {
                    //ok abilito
                    mDrawingState = true;
                    matrixGrid.CaptureMouse();
                }

            }
        }

        private void updateGridButton_Click(object sender, RoutedEventArgs e)
        {
            int r = 0, c = 0;

            if (!int.TryParse(rowText.Text, out r)) return;
            if (!int.TryParse(columnText.Text, out c)) return;

            mNumRow = r;
            mNumColumn = c;

            UpdateGrid(mNumRow, mNumColumn);
        }
    }
}
