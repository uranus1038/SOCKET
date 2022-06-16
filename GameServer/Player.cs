using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics; 
namespace Servertest
{
    public class Player
    {
        public Player instance;
        public int Id;
   
        public string user_Name;

        public Vector3 position;
        public Quaternion rotation;

        

        public  Player(int _id , string _username , Vector3 _spawnPosition)
        {
            Id = _id;
            user_Name = _username;
            position = _spawnPosition;
            rotation = Quaternion.Identity;

        }
        public void Update()
        {
            ServerSend.PlayerPosition(this);
        }
        





        public void set_Input(Vector3 _position )
        {
            position = _position;
          
            
        }

    }
}
