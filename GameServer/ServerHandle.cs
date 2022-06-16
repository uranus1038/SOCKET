using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
namespace Servertest
{
    class ServerHandle
    {
        public static void Welcom_Received(int _fClient , Packet _Packet)
        {
            int _id_User = _Packet.ReadInt();
            string txt = _Packet.ReadString();

            Console.WriteLine($"{Server.client[_fClient].tcp.Socket.Client.RemoteEndPoint} connected successfully and is now player {_id_User}");
            if(_fClient != _id_User)
            {
                Console.WriteLine($"Player {_id_User} id : {_fClient}");
            }
            Server.client[_id_User].send_Into_Game(txt);
        }
        public static void PlayerMovement(int _fClient , Packet packet)
        {
            Vector3 _position = packet.ReadVector3();
          

            Server.client[_fClient].player.set_Input(_position);
          
        }
        public static void disconnectReceive(int _fClient, Packet packet)
        {
            
            int _id = packet.ReadInt();
            Console.WriteLine(_id);

            ServerSend.disconnect_Send(_id);
            

        }



    }   
}
