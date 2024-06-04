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

                    // Mostrar la imagen procesada con los números en displayResultImage
                    Bitmap resultBitmap = mat.ToBitmap();
                    displayResultImage.Source = BitmapToBitmapImage(resultBitmap);
                }
                else
                {
                    Sms_error = new SmsError("Por favor, carga una imagen primero.");
                    Sms_error.Show();
                }
            }
            private BitmapImage BitmapToBitmapImage(Bitmap bitmap)
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                    memory.Position = 0;
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    return bitmapImage;
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
                int shapeCounter = 1; // Contador para los números de las figuras

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
                        shapeInfo.ShapeNumber = shapeCounter; // Asignar el número de la figura
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

                        // Dibujar el número de la figura en el centro del contorno
                        var moments = Cv2.Moments(contour);
                        int centerX = (int)(moments.M10 / moments.M00);
                        int centerY = (int)(moments.M01 / moments.M00);
                        Cv2.PutText(mat, shapeCounter.ToString(), new OpenCvSharp.Point(centerX, centerY), HersheyFonts.HersheySimplex, 1, Scalar.Black, 2);

                        shapeCounter++;
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
        }
    #region Class ShapeInfo
    public class ShapeInfo
    {
        public int ShapeNumber { get; set; } // Nueva propiedad para el número de la figura
        public string ShapeType { get; set; }
        public double Area { get; set; }
        public double Perimeter { get; set; }
        public int NumberOfEdges { get; set; }
        public List<double> EdgeDistances { get; set; }

        public ShapeInfo()
        {
            EdgeDistances = new List<double>();
        }

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

        public string EdgeDistancesAsString => string.Join(", ", EdgeDistances);

        private double CalculateDistance(Point point1, Point point2)
        {
            double deltaX = point2.X - point1.X;
            double deltaY = point2.Y - point1.Y;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

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



