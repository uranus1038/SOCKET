using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
namespace Servertest
{
    class GameLogic
    {
        public static void Update()
        {
            foreach (Client _client in Server.client.Values)
            {
                if (_client.player != null)
                {
                    _client.player.Update();
                  
                }
               
            }
            ThreadManager.UpdateMain();
        }
    }
}
