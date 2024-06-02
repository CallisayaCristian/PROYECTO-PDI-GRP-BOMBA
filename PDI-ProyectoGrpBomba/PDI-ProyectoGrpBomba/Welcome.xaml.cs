using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PDI_ProyectoGrpBomba
{
    /// <summary>
    /// Lógica de interacción para Welcome.xaml
    /// </summary>
    public partial class Welcome : Window
    {
        private Timer closeTimer;
        public Welcome()
        {
            InitializeComponent();

            Loaded += WelcomeLoaded;

            // Inicializar el temporizador
            closeTimer = new Timer(2000); // 5000 milisegundos = 5 segundos
            closeTimer.Elapsed += CloseTimerElapsed;
            closeTimer.AutoReset = false; // Para que no se repita automáticamente
            closeTimer.Start();
        }
        private void CloseTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Cerrar la ventana en el hilo de la interfaz de usuario
            Application.Current.Dispatcher.Invoke(() =>
            {
               
                MainWindow mainWindow = new MainWindow();
                mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            });
        }
        private void WelcomeLoaded(object sender, RoutedEventArgs e)
        {
            // Crear una animación para aumentar el tamaño de la imagen
            DoubleAnimation widthAnimation = new DoubleAnimation();
            widthAnimation.From = 100; // Tamaño inicial de la imagen
            widthAnimation.To = 500; // Tamaño final de la imagen
            widthAnimation.Duration = TimeSpan.FromSeconds(1.5); // Duración de la animación en segundos

            DoubleAnimation heightAnimation = new DoubleAnimation();
            heightAnimation.From = 100; // Tamaño inicial de la imagen
            heightAnimation.To = 500; // Tamaño final de la imagen
            heightAnimation.Duration = TimeSpan.FromSeconds(1.5); // Duración de la animación en segundos

            // Asignar las animaciones al Image
            imagen.BeginAnimation(Image.WidthProperty, widthAnimation);
            imagen.BeginAnimation(Image.HeightProperty, heightAnimation);
        }
    }
}
