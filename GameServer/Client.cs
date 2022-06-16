using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
namespace Servertest
{
    class Client
    {
        public int Id;
        public TCP tcp;
        public UDP udp; 
        public static int data_BufferSize = 4096;

        public Player player; 
        public Client(int _client_id)
        {
            Id = _client_id;
            tcp = new TCP(Id);
            udp = new UDP(Id);
        }
        public class TCP
        {
            public TcpClient Socket;
            public Packet receive_Data; 
            private readonly int Id;
            private byte[] receive_Buffer;
            private NetworkStream Stream;


            public TCP(int _Id)
            {
                Id = _Id;
            }
            public void Connect(TcpClient _socket)
            {
                Socket = _socket;
                Socket.ReceiveBufferSize = data_BufferSize;
                Socket.SendBufferSize = data_BufferSize;

                Stream = Socket.GetStream();
                receive_Buffer = new byte[data_BufferSize];
                Stream.BeginRead(receive_Buffer,0,data_BufferSize,receive_Callback,null);
                receive_Data = new Packet();
                ServerSend.Welcome(Id, "Hi you");
            }
            public void Send_Data(Packet _packet)
            {
                try
                {
                    if (Socket != null)
                    {
                        Stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception _Ex)
                {
                    Console.WriteLine($"Error sendig {Id} :{_Ex}");
                }
            }
            
            private void receive_Callback(IAsyncResult _Result)
            {
                try
                {
                    int _byte_Length = Stream.EndRead(_Result);
                    if(_byte_Length <= 0)
                    {
                        //Console.WriteLine(_byte_Length);
                        Server.client[Id].Disconnect();
                        return; 
                    }
                    byte[] _Data = new byte[_byte_Length];
                    Array.Copy(receive_Buffer, _Data, _byte_Length);

                    receive_Data.Reset(handle_Data(_Data));
                    Stream.BeginRead(receive_Buffer, 
                        0, data_BufferSize, receive_Callback, null);
                }
                catch(Exception _EX)
                {
                    Console.WriteLine("Error Receiv TCP Data " + _EX);
                    Server.client[Id].Disconnect();
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
                            Server.PacketHandle[_packet_Id](Id,_packet);
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

            public void Disconnect()
            {
                Socket.Close();
               
                Stream = null;
                receive_Buffer = null;
                receive_Data = null;
                Socket = null;
            }

        }

        public class UDP
        {
            public IPEndPoint end_Point;

            private int Id; 
            public UDP (int _Id)
            {
                Id = _Id; 
            }
            public void Connect(IPEndPoint _end_Point)
            {
                end_Point = _end_Point;
               
            }
            public void send_Data(Packet _Packet)
            {
                Server.send_Udp_Data(end_Point, _Packet);
            }

            public void handle_Data(Packet _packet_Data)
            {
                int _packet_Length = _packet_Data.ReadInt();
                byte[] _packet_Bytes = _packet_Data.ReadBytes(_packet_Length);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _Packet = new Packet(_packet_Bytes))
                    {
                        int _packet_Id = _Packet.ReadInt();
                        Server.PacketHandle[_packet_Id](Id, _Packet);
                    }
                });
              
            }

            public void Disconnect()
            {
                end_Point = null;
            }
        
        }

        public void Disconnect()
        {
            Console.WriteLine( $"{tcp.Socket.Client.RemoteEndPoint} has Disconnect");

            player = null;

            tcp.Disconnect();
            udp.Disconnect();
        }

        public  void send_Into_Game(string _playerName)
        {
            player = new Player(Id, _playerName, new Vector3(0, 0, 0));
            foreach (Client _client in Server.client.Values)
            {
                if (_client.player != null)
                {
                    if (_client.Id != Id)
                    {
                        ServerSend.spawnPlayer(Id, _client.player); 
                    }

                }
            }
            foreach (Client _client in Server.client.Values)
            {
                if (_client.player != null)
                {
                        ServerSend.spawnPlayer(_client.Id,player);
                    
                }
            }
        }
    }
}
