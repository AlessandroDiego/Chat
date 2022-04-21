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
using System.IO;

namespace ChatSocket
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Socket socket = null;//INIZIALIZIAMO IL SOCKET
        const int porta= 65500;
        List<string> contatti;

        public MainWindow()
        {
            InitializeComponent();

            LeggiContattiRecenti();

            VediLista();

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);//Internetwork=IPV4 , Dgram=UDP
            DispatcherTimer dTimer = null; //Creiamo il Timer

            IPAddress local_address = IPAddress.Any; //Any Prendo l'ip dal sistema operativo
            IPEndPoint local_endpoint = new IPEndPoint(local_address.MapToIPv4(),porta); //maptoipv4  mappa l 'indirizzo in ipv4 

            socket.Bind(local_endpoint);//Unisce il socket all'endpoint

            dTimer = new DispatcherTimer(); //creo l'oggetto timer 
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
                string from2 = ((IPEndPoint)remoteEndPoint).Port.ToString();    //recupero la porta
                string messaggio = Encoding.UTF8.GetString(buffer, 0, nBytes); // creo il messaggio dai bytes

                ListMex.Items.Add(from + ":" + from2 + ": " + messaggio); //Aggiungo alla chat il mittente identificato da indirizzo e porta
            }
        }

        private void BtnInvia_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IPAddress remote_adress = IPAddress.Parse(TxtIP.Text);//prendo l'ip
                IPEndPoint remote_endpoint = new IPEndPoint(remote_adress, int.Parse(TxtPort.Text));//creo l'endpoint(destinatario)
                byte[] messaggio = Encoding.UTF8.GetBytes(TxtMess.Text);//Prendo il messaggio

                socket.SendTo(messaggio, remote_endpoint);//Invio il  messaggio

                ListMex.Items.Add("TU: " + TxtMess.Text); //così posso vedere il messaggio che ho invitato come se fosse una vera chat

                contatti.Add(remote_adress + "-" + TxtPort.Text); //aggiungo il contatto alla lista di contatti del registro

                using (StreamWriter writer = new StreamWriter("contatti.txt")) //riscrivo il file con il contatto nuovo
                {
                    for (int i = 0; i < contatti.Count; i++)
                    {
                        writer.WriteLine(contatti[i]);
                    }
                }
                LeggiContattiRecenti();
                VediLista();
            } 
            catch
            {
                MessageBox.Show("Hai inserito un indirizzo ip o una porta incompatibili");
            }
        }

        private void ListMex_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //serve per far si che cliccando il messaggio vengono compilate automaticamente le textbox con i dati del mittente per potergli rispondere

            string[] IPePorta = new string[3];
            IPePorta=ListMex.SelectedItem.ToString().Split(":");

            if (IPePorta[0] != "TU")
            {
                TxtIP.Text = IPePorta[0];
                TxtPort.Text = IPePorta[1];
                contatti.Add(IPePorta[0] + "-" + IPePorta[1]);
            }
        }


        private void LeggiContattiRecenti()
        {
            //Leggo il file del registro dei contatti
             
            string riga;
            contatti = new List<string>();

            using (StreamReader sr = new StreamReader("contatti.txt"))
            {
                while (sr.EndOfStream == false)
                {
                    riga = sr.ReadLine();
                    contatti.Add(riga);
                }
            }
        }

        private void ListRubrica_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //serve per far si che cliccando il contatto in rubrica possa scrivergli direttamente compilando automaticamente le textbox
            if (ListRubrica.SelectedIndex!= -1)
                {
                    string[] IPePorta = new string[3];
                    IPePorta = ListRubrica.SelectedItem.ToString().Split("-");
                    TxtIP.Text = IPePorta[0];
                    TxtPort.Text = IPePorta[1];
                }
            
        }


        public void VediLista()
        {
            //Vedo la lista del registro
            ListRubrica.Items.Clear();
            for (int i = 0; i < contatti.Count; i++)
            {
                ListRubrica.Items.Add(contatti[i]);
            }
        }
    }
}
