using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Model
{


    /// <summary>
    /// запустить с параметрами и без результата
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public delegate void implementation(Dictionary<string, string> args);


    /// <summary>
    /// Аргументы в событие
    /// </summary>
    public class ServerEventArgs : EventArgs
    {
        public string text;
        public System.Net.Sockets.Socket ServerSocket;
    }

    public class Request
    {
        public string requestedUrl;
        public string httpMethod;
        public string methodName;
        private IList<string> args;

        public IList<string> Args
        {
            get 
            {
                if (args==null)
                {
                    args = new List<string>();
                }
                return args;
            }
            set 
            {
                if (args == null)
                {
                    args = new List<string>();
                }
                args = value; 
            }
        }
    }
}
