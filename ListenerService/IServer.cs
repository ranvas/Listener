using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace Server.Model
{
    public interface IServer
    {
        bool Running
        {
            get;
        }
        Socket ServerSocket
        {
            get;
        }
        Thread RequestListenerT
        {
            get;
        }
        void start();
        void stop();
    }
}
