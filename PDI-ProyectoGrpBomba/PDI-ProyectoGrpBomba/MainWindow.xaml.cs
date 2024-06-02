using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Point = OpenCvSharp.Point;
using System.Drawing.Imaging;
using AForge.Imaging;
using AForge.Imaging.Filters;
using Pen = System.Drawing.Pen;




namespace PDI_ProyectoGrpBomba
{
    using static OpenCvSharp.ConnectedComponents;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    using WpfWindow = System.Windows.Window;
    public partial class MainWindow : WpfWindow
    {
        BitmapImage original_img;
        BitmapImage labeledImage;
        int objetosDetectados;


        SmsError Sms_error;

        public MainWindow()
        {
            InitializeComponent();
        }
        #region BOTONES DE CERRAR Y MINIMIZAR
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void btnMinizar_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        #endregion

        #region CARGAR IMAGEN
        private void btnLoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            try
            {
                if (openFileDialog.ShowDialog() == true)
                {
                    string filename = openFileDialog.FileName;
                    original_img = new BitmapImage(new Uri(filename));
                    displayLoadOriginal.Source = original_img;
                    displayResultImage.Source = original_img;
                }
            }
            catch (Exception)
            {
                Sms_error = new SmsError("Error al cargar la imagen");
                Sms_error.Show();
            }
        }
        #endregion

        #region DETECTAR FIGURAS GEOMETRICAS CON LIBRERIA OpenCV
        private void btnGeoShapesInfoShow_Click(object sender, RoutedEventArgs e)
        {
            if (displayLoadOriginal.Source != null)
            {
                Bitmap bitmap = BitmapImageToBitmap((BitmapImage)displayLoadOriginal.Source);
                Mat mat = BitmapToMat(bitmap);
                var shapesInfo = ProcessImageAndGetShapesInfo(mat);
                displayInfoDataFigures.ItemsSource = shapesInfo;

                /// Detedectar Objetos
                DetectarObjetos();
            }
            else
            {
                Sms_error = new SmsError("Por favor, carga una imagen primero.");
                Sms_error.Show();
                
            }
        }
        private Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
        private Mat BitmapToMat(Bitmap bitmap)
        {
            return bitmap.ToMat();
        }

        private List<ShapeInfo> ProcessImageAndGetShapesInfo(Mat mat)
        {
            var shapesInfo = new List<ShapeInfo>();

            Mat gray = new Mat();
            Cv2.CvtColor(mat, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.GaussianBlur(gray, gray, new OpenCvSharp.Size(5, 5), 0);
            Cv2.Canny(gray, gray, 50, 150);

            OpenCvSharp.Point[][] contours;
            Cv2.FindContours(gray, out contours, out HierarchyIndex[] hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            foreach (var contour in contours)
            {
                var epsilon = 0.02 * Cv2.ArcLength(contour, true);
                var approx = Cv2.ApproxPolyDP(contour, epsilon, true);

                if (Cv2.ContourArea(approx) > 100)
                {
                    var shapeInfo = new ShapeInfo();
                    shapeInfo.NumberOfEdges = approx.Length;

                    if (approx.Length == 3)
                    {
                        shapeInfo.ShapeType = "Triángulo";
                    }
                    else if (approx.Length == 4)
                    {
                        var rect = Cv2.BoundingRect(approx);
                        var aspectRatio = Math.Abs((double)rect.Width / rect.Height - 1);
                        shapeInfo.ShapeType = aspectRatio <= 0.05 ? "Cuadrado" : "Rectángulo";
                    }
                    else if (approx.Length == 5)
                    {
                        shapeInfo.ShapeType = "Pentágono";
                    }
                    else
                    {
                        shapeInfo.ShapeType = "Círculo";
                    }

                    shapeInfo.Area = Cv2.ContourArea(approx);
                    shapeInfo.Perimeter = Cv2.ArcLength(approx, true);
                    shapeInfo.CalculateEdgeDistances(ConvertContourToPoints(approx));
                    shapesInfo.Add(shapeInfo);
                }
            }

            return shapesInfo;
        }
        private List<OpenCvSharp.Point> ConvertContourToPoints(OpenCvSharp.Point[] contour)
        {
            List<OpenCvSharp.Point> points = new List<OpenCvSharp.Point>();
            foreach (var point in contour)
            {
                points.Add(point);
            }
            return points;
        }


        #endregion

        #region DETECTAR OBJETOS CON LIBRERIA AForge
        private Bitmap BitmapImage2Bitmap(BitmapSource bitmapSource)
        {
            Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapSource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }
        void DetectarObjetos()
        {
            Bitmap auxImage = BitmapImage2Bitmap(original_img);

            Grayscale grayscaleFilter = new Grayscale(0.3, 0.59, 0.11);
            Bitmap grayscaleImage = grayscaleFilter.Apply(auxImage);
            SobelEdgeDetector edgeDetector = new SobelEdgeDetector();
            Bitmap edgeImage = edgeDetector.Apply(grayscaleImage);
            Threshold thresholdFilter = new Threshold(100);
            thresholdFilter.ApplyInPlace(edgeImage);

            //Grayscale grayscaleFilter = new Grayscale(0.3, 0.59, 0.11);
            //Bitmap grayscaleImage = grayscaleFilter.Apply(auxImage);
            //Threshold thresholdFilter = new Threshold(245);
            //thresholdFilter.ApplyInPlace(grayscaleImage);
            //Invert invert = new Invert();
            //grayscaleImage = invert.Apply(grayscaleImage);

            BlobCounter blobCounter = new BlobCounter();
            blobCounter.ProcessImage(edgeImage);
            //blobCounter.ProcessImage(grayscaleImage);
            AForge.Imaging.Blob[] blobs = blobCounter.GetObjectsInformation();
            int minSize = 1000;
            objetosDetectados = 0;
            foreach (var blob in blobs)
            {
                if (blob.Area >= minSize)
                {
                    System.Drawing.Rectangle rect = blob.Rectangle;
                    DrawRectangle(auxImage, rect, System.Drawing.Color.Red);
                    objetosDetectados++;
                }
            }
            labeledImage = BitmapToBitmapImage(auxImage);
            displayResultImage.Source = labeledImage;
            lblMensajes.Text = $"  Objetos detectados: {objetosDetectados}  ";

        }
        private BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
        private void DrawRectangle(Bitmap bitmap, System.Drawing.Rectangle rect, System.Drawing.Color color)
        {
            for (int x = rect.Left; x < rect.Right; x++)
            {
                bitmap.SetPixel(x, rect.Top, color);
                bitmap.SetPixel(x, rect.Bottom - 1, color);
            }

            for (int y = rect.Top; y < rect.Bottom; y++)
            {
                bitmap.SetPixel(rect.Left, y, color);
                bitmap.SetPixel(rect.Right - 1, y, color);
            }
        }
        #endregion

    }
    #region Class ShapeInfo
    public class ShapeInfo
    {
        public string ShapeType { get; set; }
        public double Area { get; set; }
        public double Perimeter { get; set; }
        public int NumberOfEdges { get; set; }

        // Podrías calcular y almacenar las distancias entre los vértices aquí
        public List<double> EdgeDistances { get; set; }

        public ShapeInfo()
        {
            EdgeDistances = new List<double>();
        }

        // Método para calcular y almacenar las distancias entre los vértices
        public void CalculateEdgeDistances(List<Point> vertices)
        {
            if (vertices == null || vertices.Count < 2)
                return;

            EdgeDistances.Clear();

            for (int i = 0; i < vertices.Count - 1; i++)
            {
                for (int j = i + 1; j < vertices.Count; j++)
                {
                    double distance = CalculateDistance(vertices[i], vertices[j]);
                    EdgeDistances.Add(distance);
                }
            }
        }
        public string EdgeDistancesAsString
        {
            get
            {
                return string.Join(", ", EdgeDistances);
            }
        }

        // Método para calcular la distancia entre dos puntos
        private double CalculateDistance(Point point1, Point point2)
        {
            double deltaX = point2.X - point1.X;
            double deltaY = point2.Y - point1.Y;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        // Método para obtener las distancias como una cadena formateada
        public string GetFormattedEdgeDistances()
        {
            return string.Join(", ", EdgeDistances);
        }
    }
    #endregion

    #region Class MyPoint
    public class MyPoint
    {
        public double X { get; set; }
        public double Y { get; set; }

        public MyPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
    #endregion


}


