using System;
using System.Threading;

namespace Servertest
{
    class Program
    {
        private static bool isRunnig = false;
        public const int TICK_PER_SEC = 30;
        public const int MS_PER_TICK = 1000 / TICK_PER_SEC;
        static void Main(string[] args)
        {
            Console.Title = "GameServer";
            isRunnig = true;
            Thread main_Thread = new Thread(new ThreadStart(_main_Thread));
            main_Thread.Start();

            Server.Start(100, 8881);
         
            
        }

        private static void _main_Thread()
        {
            DateTime _next_Loop = DateTime.Now;
            while (isRunnig)
            {
                while (_next_Loop < DateTime.Now)
                {
                    GameLogic.Update();
                    _next_Loop = _next_Loop.AddMilliseconds(MS_PER_TICK);
                }
                if (_next_Loop > DateTime.Now)
                {
                    Thread.Sleep(_next_Loop - DateTime.Now);
                }   
            }
        }
    }
}
