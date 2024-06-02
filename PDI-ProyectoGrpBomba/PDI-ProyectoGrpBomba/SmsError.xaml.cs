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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PDI_ProyectoGrpBomba
{
    /// <summary>
    /// Lógica de interacción para SmsError.xaml
    /// </summary>
    public partial class SmsError : Window
    {
        private Timer closeTimer;
        public SmsError(string msm)
        {
            InitializeComponent();
            lblRespuesta.Text = msm;

            // Inicializar el temporizador
            closeTimer = new Timer(5000); // 5000 milisegundos = 5 segundos
            closeTimer.Elapsed += CloseTimerElapsed;
            closeTimer.AutoReset = false; // Para que no se repita automáticamente

            closeTimer.Start();
        }
        private void CloseTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Cerrar la ventana en el hilo de la interfaz de usuario
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.Close();
            });
        }

        private void btnIniciar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
