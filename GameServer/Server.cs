using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Servertest
{
    class Server
    {
        public static int max_Player { get; private set; }
        public static int Port{ get; private set; }
        public static Dictionary<int, Client> client = new Dictionary<int , Client>();
        public delegate void PacketHandler(int _fClient, Packet _Packet);
        public static Dictionary<int, PacketHandler> PacketHandle;

        
        private static UdpClient udp_Listenner;
        private static TcpListener tcp_Listener;  

        public static void Start(int _max_Player , int _Port )
        {
            max_Player = _max_Player;
            Port = _Port;
            Console.WriteLine("Start Server");
            initialize_Server_Data();
            tcp_Listener = new TcpListener(IPAddress.Any, Port);
            tcp_Listener.Start();
            tcp_Listener.BeginAcceptTcpClient(new AsyncCallback(tcp_Connect_Callback), null);

            udp_Listenner = new UdpClient(Port);
            udp_Listenner.BeginReceive(udp_Receive_Callback, null);

            Console.WriteLine($"Server Start :) on {Port}.");
        }

        private static void tcp_Connect_Callback(IAsyncResult _Result)
        {
            TcpClient _Client = tcp_Listener.EndAcceptTcpClient(_Result);
            tcp_Listener.BeginAcceptTcpClient(new AsyncCallback(tcp_Connect_Callback), null);
            Console.WriteLine($"Incoming connection from {_Client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= max_Player; i++)
            {
            
                if(client[i].tcp.Socket == null)
                {
                    client[i].tcp.Connect(_Client);
                    return; 
                }
            }
            Console.WriteLine($"{_Client.Client.RemoteEndPoint}... Server is Full");
        }

        private static void udp_Receive_Callback(IAsyncResult _Result)
        {
            try
            {
                IPEndPoint _client_EndPoint = new IPEndPoint(IPAddress.Any,0);
                byte[] _Data = udp_Listenner.EndReceive(_Result, ref _client_EndPoint);
                udp_Listenner.BeginReceive(udp_Receive_Callback, null);
                if (_Data.Length < 4)
                {
                    Console.WriteLine("Error Connect");
                    return;
                }
                using (Packet _Packet = new Packet(_Data))
                {
                    int _cId = _Packet.ReadInt();
                    if (_cId == 0)
                    {
                        Console.WriteLine("Error Connect 0");
                        return;
                    }
                    if (client[_cId].udp.end_Point == null)
                    {
                        client[_cId].udp.Connect(_client_EndPoint);
                        return;
                    }
                    if (client[_cId].udp.end_Point.ToString() == _client_EndPoint.ToString())
                    {
                        client[_cId].udp.handle_Data(_Packet);
                    }
                }

            }
            catch (Exception _Ex)
            {
                Console.WriteLine($"Error Udp sending : {_Ex}");
                
                
            }

           
        }
        public static void send_Udp_Data(IPEndPoint _cEndPoint ,Packet _Packet)
        {
            try
            {
                if(_cEndPoint != null)
                {
                    udp_Listenner.BeginSend(_Packet.ToArray(), _Packet.Length(), _cEndPoint, null, null);
                }
            }
            catch (Exception _Ex)
            {
                Console.WriteLine($"Error sending {_cEndPoint} Udp : {_Ex}");
            }
        }
        private static void initialize_Server_Data()

        {
            for (int i = 1; i <= max_Player; i++)
            {
                client.Add(i, new Client(i));
            }
            PacketHandle = new Dictionary<int, PacketHandler>()
            {
                {
                    (int)ClientPackets.welcomeReceived , ServerHandle.Welcom_Received

             },{   (int)ClientPackets.playerMovement , ServerHandle.PlayerMovement},
                {   (int)ClientPackets.disConnectClient , ServerHandle.disconnectReceive}

            };
        }
        

    }
}
