using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void send_Tcp_Data(Packet _Packet)
    {
        _Packet.WriteLength();
        Client.instance.tcp.send_Data(_Packet);
    }
    private static void send_Udp_Data(Packet _Packet)
    {
        _Packet.WriteLength();
        Client.instance.udp.send_Data(_Packet);
    }

    public static void WelcomReceive()
    {
        using (Packet _Packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            _Packet.Write(Client.instance.my_Id);
            _Packet.Write("Aum");
            send_Tcp_Data(_Packet);
        }
    }

    public static void PlayerMoveMent(Vector3 _position)
    {
        using (Packet _Packet = new Packet((int)ClientPackets.playerMovement))
        {
            
            _Packet.Write(_position);
           // _Packet.Write(GameManager.players[Client.instance.my_Id].transform.rotation);
            send_Udp_Data(_Packet);
        }
    }

    public static void disconnectSend(int _id)
    {
        using (Packet _Packet = new Packet((int)ClientPackets.disConnectClient))
        {

            _Packet.Write(_id);
            // _Packet.Write(GameManager.players[Client.instance.my_Id].transform.rotation);
            send_Tcp_Data(_Packet);
        }
    }




}
