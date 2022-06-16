using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int data_BufferSize = 4096;
    public string ip = "127.0.0.1";
    public int port = 808;
    public int my_Id = 0;
    public TCP tcp ;
    public UDP udp;

    private bool isConnected = false;
    private delegate void packet_Handler(Packet _packet);
    private static Dictionary<int, packet_Handler> packet_Handle; 
    private void Awake()
    {
        if (instance == null)
        {
            instance = this; 
        }else if(instance != this)
        {
            Debug.Log($"Destroy");
            Destroy(this);
        }
    }
    private void Start()
    {
        tcp = new TCP();
        udp = new UDP();
    }
    private void OnApplicationQuit()
    {
       ClientSend.disconnectSend(my_Id);
       Disconnect();
    }
    public void connect_to_Server()
    {
       
        Initialize_Client_Data();
        isConnected = true; 
        tcp.Connect();
    }
    public class TCP
    {
        public TcpClient Socket;
        private Packet receive_Data;
        private NetworkStream Stream;
        private byte[] receive_Buffer;
        public void Connect()
        {
            Socket = new TcpClient { ReceiveBufferSize = data_BufferSize, SendBufferSize = data_BufferSize };

            receive_Buffer = new byte[data_BufferSize];
            Socket.BeginConnect(instance.ip, instance.port, connect_Callback, Socket);

        }
        private void connect_Callback(IAsyncResult _Result)
        {
            if (!Socket.Connected)
            {
                Debug.Log("Not server down");
                return;
            }


            Stream = Socket.GetStream();
            receive_Data = new Packet();
            Stream.BeginRead(receive_Buffer, 0, data_BufferSize, receive_Callback, null);
        }

        public void send_Data(Packet _Packet)
        {
            try
            {
                if (Socket != null)
                {
                    Stream.BeginWrite(_Packet.ToArray(), 0, _Packet.Length(), null, null);
                }
            }
            catch (Exception _Ex)
            {
                Console.WriteLine($"Error sendig :{_Ex}");
            }
        }
        private void receive_Callback(IAsyncResult _Result)
        {
            try
            {
                int _byte_Length = Stream.EndRead(_Result);
                if (_byte_Length <= 0)
                {
                    
               instance.Disconnect();
                    ClientSend.disconnectSend(Client.instance.my_Id);
                    return;
                }
                byte[] _Data = new byte[_byte_Length];
                Array.Copy(receive_Buffer, _Data, _byte_Length);

                receive_Data.Reset(handle_Data(_Data));
                Stream.BeginRead(receive_Buffer, 0, data_BufferSize, receive_Callback, null);
            }
            catch
            {
                ClientSend.disconnectSend(Client.instance.my_Id);
                Disconnect();
            }
        }

        private bool handle_Data(byte[] _Data)
        {
            int _packet_Lenght = 0;

            receive_Data.SetBytes(_Data);
            if (receive_Data.UnreadLength() >= 4)
            {
                _packet_Lenght = receive_Data.ReadInt();
                if (_packet_Lenght <= 0)
                {
                    return true;
                }
            }
            while (_packet_Lenght > 0 && _packet_Lenght <= receive_Data.UnreadLength())
            {
                byte[] _packet_Byte = receive_Data.ReadBytes(_packet_Lenght);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packet_Byte))
                    {
                        int _packet_Id = _packet.ReadInt();
                        packet_Handle[_packet_Id](_packet);
                    }

                });
                _packet_Lenght = 0;
                if (receive_Data.UnreadLength() >= 4)
                {
                    _packet_Lenght = receive_Data.ReadInt();
                    if (_packet_Lenght <= 0)
                    {
                        return true;
                    }
                }
            }
            if (_packet_Lenght <= 1)
            {
                return true;
            }
            return false;
        }

        private void Disconnect()
        {
           instance.Disconnect();

            Stream = null;
            receive_Data = null;
            receive_Buffer = null;
            Socket = null;

        }

    }

    public class UDP
    {
        public UdpClient Socket;
        public IPEndPoint end_Point;
        public UDP()
        {
            end_Point = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }
        public void Connect(int local_Port)
        {
            Socket = new UdpClient(local_Port);
            Socket.Connect(end_Point);
            Socket.BeginReceive(receive_Callback, null);

            using (Packet _Packet = new Packet())
            {
                send_Data(_Packet);
            }

        }
        public void send_Data(Packet _Packet)
        {
            try
            {
                _Packet.InsertInt(instance.my_Id);
                if(Socket != null)
                {
                    Socket.BeginSend(_Packet.ToArray(), _Packet.Length(), null, null);
                }
            }
            catch(Exception _Ex)
            {
                Debug.Log($"Erorr Sending to data {_Ex}");
            }
        }
        private void receive_Callback(IAsyncResult _Result)
        {
            try 
            {
                byte[] _Data = Socket.EndReceive(_Result, ref end_Point);
                Socket.BeginReceive(receive_Callback, null);
                if(_Data.Length<4)
                {
                    ClientSend.disconnectSend(Client.instance.my_Id);
                    instance.Disconnect();
                    return; 
                }
                handle_Data(_Data);
            }
            catch 
            {
                ClientSend.disconnectSend(Client.instance.my_Id);
                Disconnect();
            }
        }
        private void handle_Data(byte[] _data)
        {
            using (Packet _packet = new Packet(_data))
            {
                int _packetLength = _packet.ReadInt();
                _data = _packet.ReadBytes(_packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_data))
                {
                    int _packetId = _packet.ReadInt();
                    packet_Handle[_packetId](_packet);
                }
            });
        }



        private void Disconnect()
        {
            instance.Disconnect();

            end_Point = null;
            Socket = null;
        }

    }
    private void Initialize_Client_Data()
    {
        packet_Handle = new Dictionary<int, packet_Handler>()
        {
            { (int)ServerPackets.welcome ,ClientHandle.Welcom},
           { (int)ServerPackets.spawnPlayer ,ClientHandle.SpawnPlayer},
           { (int)ServerPackets.playerPosition ,ClientHandle.PlayerPosition}
            ,{ (int)ServerPackets.disConnectSv ,ClientHandle.disconnectReceive}

        };
       
    }
    private void Disconnect()
    {
        if(isConnected)
        {
           
            isConnected = false;
            tcp.Socket.Close();
            //udp.Socket.Close();
            Debug.Log("Disconnect from Server");
        }
    }
}
