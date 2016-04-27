using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
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

        protected enum PixelDrawingState { Draw, Clear };

        private Font mSelectedFont;

        private PixelDrawingState mDrawingState;
        private bool mMouseMove;

        public MainWindow()
        {
            InitializeComponent();

            mNumRow = 8;
            mNumColumn = 8;

            rowText.Text = mNumRow.ToString();
            columnText.Text = mNumColumn.ToString();

            UpdateGrid(mNumRow, mNumColumn);

            matrixGrid.MouseLeftButtonDown += MatrixGrid_MouseDown;
            matrixGrid.MouseRightButtonDown += MatrixGrid_MouseDown;
            matrixGrid.MouseMove += MatrixGrid_MouseMove;
            matrixGrid.MouseLeftButtonUp += MatrixGrid_MouseUp;
            matrixGrid.MouseRightButtonUp += MatrixGrid_MouseUp;

            mMouseMove = false;
            mDrawingState = PixelDrawingState.Draw;
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

                panel.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                panel.VerticalAlignment = VerticalAlignment.Stretch;
                panel.Orientation = System.Windows.Controls.Orientation.Vertical;
                panel.Margin = new Thickness(0, 2, 0, 0);
                panel.SetValue(Grid.ColumnProperty, i);

                //aggiungo gli elementi in riga

                for (int j = 0; j < row; j++)
                {
                    PixelControl bt = new PixelControl();
                    bt.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                    bt.Height = 20;
                    bt.Margin = new Thickness(2, 0, 0, 2);
                    bt.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                    bt.Tag = false;

                    panel.Children.Add(bt);
                }
                
                //aggiungo
                matrixGrid.Children.Add(panel);
            }
        }

        private void MatrixGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mMouseMove)
            {
                //ok abilito
                mMouseMove = false;
                matrixGrid.ReleaseMouseCapture();
                
            }
        }

        private void MatrixGrid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if ( mMouseMove )
            {
                //avvio disegno se il click avviene sopra un pixel
                System.Windows.Point pt = e.GetPosition(matrixGrid);

                HitTestResult res = VisualTreeHelper.HitTest(matrixGrid, pt);

                if (res == null) return;

                PixelControl pixel = null;

                DependencyObject obj = res.VisualHit;

                //verifico se si tratta di un pixel container
                while (true)
                {
                    if (obj is PixelControl)
                    {
                        pixel = obj as PixelControl;
                        break;
                    }
                    else if (obj != null)
                    {
                        obj = VisualTreeHelper.GetParent(obj);
                    }
                    else break;
                }

                if (pixel != null)
                {
                    bool pixel_val = pixel.PixelValue;

                    if ( mDrawingState == PixelDrawingState.Draw && !pixel_val )
                    {
                        pixel.PixelValue = true;
                    }
                    else if ( mDrawingState == PixelDrawingState.Clear && pixel_val )
                    {
                        pixel.PixelValue = false;
                    }
                }
            }
        }

        private void MatrixGrid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if ( !mMouseMove )
            {
                //avvio disegno se il click avviene sopra un pixel
                System.Windows.Point pt = e.GetPosition(matrixGrid);
                
                HitTestResult res = VisualTreeHelper.HitTest(matrixGrid, pt);
                
                if ( res != null && res.VisualHit != null )
                {
                    //ok abilito
                    if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released) mDrawingState = PixelDrawingState.Draw;
                    else if (e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Pressed) mDrawingState = PixelDrawingState.Clear;
                    else return; //doppio click non ammesso 
                    
                    mMouseMove = true;
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

        private void fontDialogButton_Click(object sender, RoutedEventArgs e)
        {
            FontDialog dialog = new FontDialog();

            if ( System.Windows.Forms.DialogResult.OK == dialog.ShowDialog() )
            {
                //ottengo il font selezionato
                Font font = dialog.Font;

                selectedFontText.Text = font.Name;
                mSelectedFont = font;
            }
        }

        private void loadFontButton_Click(object sender, RoutedEventArgs e)
        {
            if (mSelectedFont == null) return;

            string format_string = "a";
            double font_height = 200;

            //creo la 
            Typeface typeface = new Typeface(mSelectedFont.Name);

            FormattedText formatted_text = new FormattedText(format_string,                 //stringa
                CultureInfo.CurrentUICulture,                                               //informazioni sul formato
                System.Windows.FlowDirection.LeftToRight,                                   //direzione di scrittura
                typeface,                                                                   //tipo di font
                font_height,                                                                //altezza del font
                new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0)));    //colore

            formatted_text.TextAlignment = TextAlignment.Left;
            
            double width = 20 * mNumColumn;
            double height = 20 * mNumRow; 

            int width_int = (int)width;
            int height_int = (int)height;

            System.Windows.Point string_origin = new System.Windows.Point();

            string_origin.X = 0;
            string_origin.Y = 0;
            //string_origin.X = (width - formatted_text.Width) / 2;
            string_origin.Y = (height - formatted_text.Height) / 2;
            
            //apro un contesto grafico su cui andro a disegnare il font
            DrawingVisual drawing_visual = new DrawingVisual();
            
            //apro la sessione grafica
            DrawingContext drawing_context = drawing_visual.RenderOpen();
            //disegno lo sfondo bianco
            drawing_context.DrawRectangle(new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255)),
                null, new Rect(0, 0, width_int, height_int));
            //disegno la stringa
            drawing_context.DrawText(formatted_text, string_origin);
            //chiudo la sessione grafica
            drawing_context.Close();
            
            RenderTargetBitmap bm = new RenderTargetBitmap(
                width_int, height_int, 96, 96, PixelFormats.Pbgra32);
            bm.Render(drawing_visual);


            //debug start
            FileStream fp = File.Create("prova.bmp");

            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bm));
            encoder.Save(fp);


            fp.Close();
            //debug end


            //provo a tassellare la bmp
            byte[] pixels = new byte[bm.PixelWidth * bm.PixelHeight * 4];

            bm.CopyPixels(pixels, 4 * bm.PixelWidth, 0);



        }
    }
}
