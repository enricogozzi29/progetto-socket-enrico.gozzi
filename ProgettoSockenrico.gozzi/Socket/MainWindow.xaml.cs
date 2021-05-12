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
//Aggiunta delle seguenti librerie
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace socket
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool Ricezione = true;
        public MainWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// crea un socket con all'interno il proprio indirizzo IP e avvia il processo di recezione
        /// </summary>
        private void btnCreaSocket_Click(object sender, RoutedEventArgs e)
        {
            IPEndPoint sourceSocket = new IPEndPoint(LocalIPAddress(), int.Parse(txtPortaMittente.Text));

            Ricezione = false;
                       
            btnInvia.IsEnabled = true;

            Thread ricezione = new Thread(new ParameterizedThreadStart(SocketReceive));

            ricezione.Start(sourceSocket);

        }
        /// <summary>
        /// aggiungere controlli sul contenuto delle textbox
        /// </summary>
        private void btnInvia_Click(object sender, RoutedEventArgs e)
        {
            string ipAddress = txtIP.Text;
            int port = int.Parse(txtPort.Text);

            SocketSend(IPAddress.Parse(ipAddress), port, txtMsg.Text);


        }
        /// <summary>
        /// elabora la ricezione del messaggio
        /// </summary>
        public async void SocketReceive(object socksource)
        {
            IPEndPoint ipendp = (IPEndPoint)socksource;

            Socket t = new Socket(ipendp.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            t.Bind(ipendp);

            Byte[] bytesRicevuti = new Byte[256];

            string message;

            int contaCaratteri = 0;

            await Task.Run(() =>
            {
                Ricezione = true;
                while (Ricezione)
                {
                    if(t.Available >0)
                    {
                        message = "";

                        contaCaratteri = t.Receive(bytesRicevuti, bytesRicevuti.Length,0);
                        message = message + Encoding.ASCII.GetString(bytesRicevuti, 0, contaCaratteri);

                        this.Dispatcher.BeginInvoke(new Action(() =>
                        {

                            lblRicevi.Content = message;
                        }));

                    }

                }
                t.Close();
                Ricezione = true;
            });

        }
        /// <summary>
        /// invia il messaggio al destinatario
        /// </summary>
        /// <param name="dest"> IP destinatario </param>
        /// <param name="destport"> porta destinatario </param>
        /// <param name="message"> messaggio </param>
        public void SocketSend(IPAddress dest, int destport, string message)
        {
            Byte[] byteInviati = Encoding.ASCII.GetBytes(message);

            Socket s = new Socket(dest.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint remote_endpoint = new IPEndPoint(dest, destport);

            s.SendTo(byteInviati, remote_endpoint);
        }
        private IPAddress LocalIPAddress()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return null;
            }

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            return host
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }

    }
}
