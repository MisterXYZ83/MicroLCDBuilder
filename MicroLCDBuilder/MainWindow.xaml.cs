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

            UpdateGrid(mNumRow, mNumColumn, null);

            matrixGrid.MouseLeftButtonDown += MatrixGrid_MouseDown;
            matrixGrid.MouseRightButtonDown += MatrixGrid_MouseDown;
            matrixGrid.MouseMove += MatrixGrid_MouseMove;
            matrixGrid.MouseLeftButtonUp += MatrixGrid_MouseUp;
            matrixGrid.MouseRightButtonUp += MatrixGrid_MouseUp;

            mMouseMove = false;
            mDrawingState = PixelDrawingState.Draw;

            ///update combobox dei caratteri
            for ( int k = 33; k < 127; k++ )
            {
                byte char_b = (byte)k;
                fontCombo.Items.Add(Convert.ToChar(char_b));
            }

            fontCombo.SelectedIndex = 48-33; //parte da 0
        }


        private void UpdateGrid(int row, int col, byte [] px_data)
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

                    if ( px_data != null )
                    {
                        byte v = px_data[j * mNumColumn + i];
                        bt.PixelValue = (v == 1 ? true : false);

                    }

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

            UpdateGrid(mNumRow, mNumColumn, null);
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

            byte [] pixeled = PixelizeChar(fontCombo.SelectedItem.ToString(), mSelectedFont, mNumRow, mNumColumn, 20, (int)thresholdSlider.Value);

            if ( pixeled != null )
            {
                UpdateGrid(mNumRow, mNumColumn, pixeled);
            }

        }


        private byte[] PixelizeChar ( string c, Font font, int row, int col, int pix_dim, int thres )
        {
            byte[] pixel_char = null;

            string format_string = c.Substring(0,1);
            double font_height = 200;

            //creo la 
            Typeface typeface = new Typeface(font.Name);

            FormattedText formatted_text = new FormattedText(format_string,                 //stringa
                CultureInfo.CurrentUICulture,                                               //informazioni sul formato
                System.Windows.FlowDirection.LeftToRight,                                   //direzione di scrittura
                typeface,                                                                   //tipo di font
                font_height,                                                                //altezza del font
                new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0)));    //colore

            formatted_text.TextAlignment = TextAlignment.Left;

            double width = pix_dim * col;
            double height = pix_dim * row;

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

            //analizzo i pixel b/w per ogni blocco (20x20) a maggioranza
            pixel_char = new byte[row * col];
            Array.Clear(pixel_char, 0, pixel_char.Length);

            byte[] pixel_bmp = new byte[width_int * height_int * 4]; //codifica BGRA32
            Array.Clear(pixel_bmp, 0, pixel_bmp.Length);

            bm.CopyPixels(pixel_bmp, width_int * 4, 0);
           
            for (int p = 0; p < row; p++)
            {
                for (int q = 0; q < col; q++)
                {
                    //analizzo l'area di pixel relativi al blocco (p,q) (20x20)

                    pixel_char[p * col + q] = (CheckBlock(pixel_bmp, q, p, pix_dim, thres, width_int, 4) ? (byte)1 : (byte)0);

                }
            }

            bm.Clear();
            bm = null;

            pixel_bmp = null;

           
            return pixel_char;
        }


        private bool CheckBlock(byte[] pixels, int b_x, int b_y, int b_size, int thres, int w, int bpp)
        {
            bool ret = false;
            int cnt_val = 0;

            for ( int x = 0; x < b_size; x++ )
            {
                for ( int y = 0; y < b_size; y++ )
                {
                    int arr_pos = ((b_y * b_size + y) * w * bpp) + (b_x * b_size + x) * bpp;
                    if (pixels[arr_pos] == 0) cnt_val++;
                    if (cnt_val > thres) return true;
                }
            }


            return ret;
        }
    }
}
