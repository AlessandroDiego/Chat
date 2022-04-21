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



using System.Net.Sockets;
using System.Net;
using System.Windows.Threading;

namespace ChatSocket
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Socket socket = null;//INIZIALIZIAMO IL SOCKET
        const int porta= 65500;

        public MainWindow()
        {
            InitializeComponent();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);//Internetwork=IPV4 , Dgram=UDP
            DispatcherTimer dTimer = null; //Creiamo il Timer

            IPAddress local_address = IPAddress.Any; //Any Prendo l'ip dal sistema operativo
            IPEndPoint local_endpoint = new IPEndPoint(local_address.MapToIPv4(),porta); //maptoipv4 lo mappa in ipv4 - 65000 è la porta

            socket.Bind(local_endpoint);//Unisce il socket all'endpoint

            dTimer = new DispatcherTimer(); //creo l'oggetto
            dTimer.Tick += new EventHandler(aggiornamento_dTimer); // l'evento che parte quando scade il tick;
            dTimer.Interval= new TimeSpan(0,0,0,0,250);//ogni quanto fare il tick
            dTimer.Start();//parte il timer


            LblPorta.Content = " " + porta;



        }

        private void aggiornamento_dTimer(object sender, EventArgs e)
        {
          int nBytes = 0;//Quanti bytes ho dentro
          
            if((nBytes=socket.Available)>0)
            {
                byte[] buffer = new byte[nBytes]; //vettore di n bytes

                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); //ipendpoint derivata di endpoint Serve per ricevere il messaggio

                nBytes = socket.ReceiveFrom(buffer, ref remoteEndPoint);//prende l'ip e la porta e li mette nell'endpoint

                string from = ((IPEndPoint)remoteEndPoint).Address.ToString(); //recupero l'ip
                string from2 = ((IPEndPoint)remoteEndPoint).Port.ToString();
                string messaggio = Encoding.UTF8.GetString(buffer, 0, nBytes); // creo il messaggio dai bytes

                ListMex.Items.Add(from + ":" + from2 + ": " + messaggio);
            }
        }

        private void BtnInvia_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IPAddress remote_adress = IPAddress.Parse(TxtIP.Text);//prendo l'ip
                IPEndPoint remote_endpoint = new IPEndPoint(remote_adress, int.Parse(TxtPort.Text));//creo l'endpoint(destinatario)
                byte[] messaggio = Encoding.UTF8.GetBytes(TxtMess.Text);//Prendo il messaggio

                socket.SendTo(messaggio, remote_endpoint);

                ListMex.Items.Add("TU: " + TxtMess.Text);
            } 
            catch
            {
                MessageBox.Show("Hai inserito un indirizzo ip o una porta incompatibili");
            }
        }

        private void ListMex_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string[] IPePorta = new string[3];
            IPePorta=ListMex.SelectedItem.ToString().Split(":");

            if (IPePorta[0] != "TU")
            {
                TxtIP.Text = IPePorta[0];
                TxtPort.Text = IPePorta[1];
            }
        }
    }
}
