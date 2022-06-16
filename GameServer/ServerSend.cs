using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
namespace Servertest
{
    public class ServerSend
    {
        private static void Send_Tcp_Data(int _toClient,Packet _packet)
        {
            _packet.WriteLength();
            Server.client[_toClient].tcp.Send_Data(_packet);
        }
        private static void Send_Udp_Data(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.client[_toClient].udp.send_Data(_packet);
        }
        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.max_Player; i++)
            {
                Server.client[i].tcp.Send_Data(_packet);
            }
        }
        private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.max_Player; i++)
            {
                if (i != _exceptClient)
                {
                    Server.client[i].tcp.Send_Data(_packet);
                }
            }
        }

        private static void SendUDPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.max_Player; i++)
            {
                Server.client[i].udp.send_Data(_packet);
            }
        }
        private static void SendUDPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.max_Player; i++)
            {
                if (i != _exceptClient)
                {
                    Server.client[i].udp.send_Data(_packet);
                }
            }
        }
        public static void Welcome(int _toClient ,string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);

                Send_Tcp_Data(_toClient, _packet);

            }
        }
        public static void spawnPlayer(int _toClient, Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
            {
                _packet.Write(_player.Id);
                _packet.Write(_player.user_Name);
                _packet.Write(_player.position);
                _packet.Write(_player.rotation);

                Send_Tcp_Data(_toClient, _packet);

            }
        }
        public static void PlayerPosition(Player _player )
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
            {
                _packet.Write(_player.Id);
                _packet.Write(_player.position);
               


                SendUDPDataToAll(_player.Id,_packet);
            }
        }

        public static void disconnect_Send(int _fClient)
        {
            using (Packet _packet = new Packet((int)ServerPackets.disConnectSv))
            {
             
                _packet.Write(_fClient);

                SendTCPDataToAll(_fClient,_packet);
            }
        }





    }
}
