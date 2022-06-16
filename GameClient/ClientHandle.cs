using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
public class ClientHandle : MonoBehaviour
{
  public static void  Welcom(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();
        Debug.Log($"Message from server : {_msg}");
        Client.instance.my_Id = _myId;
        ClientSend.WelcomReceive();

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.Socket.Client.LocalEndPoint).Port);
    }
    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();

        GameManager.instance.SpawnPlayer(_id, _username, _position, _rotation);
    }
    public static void PlayerPosition(Packet _Packet)
    {
        int _id = _Packet.ReadInt();
        Vector3 _Position = _Packet.ReadVector3();
       // Quaternion _rotation = _Packet.ReadQuaternion();
        GameManager.players[_id].transform.position = _Position;
        //   GameManager.players[_id].transform.rotation = _rotation;
        
    }

    public static void disconnectReceive(Packet _Packet)
    {
        int _id = _Packet.ReadInt();
        Destroy(GameManager.players[_id].gameObject);
        GameManager.players.Remove(_id);
        Debug.Log(_id);

    }









}
