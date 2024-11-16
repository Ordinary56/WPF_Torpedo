using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    static internal class SocketUtils
    {
        /// <summary>
        /// Constantly checks wether a client is still connected or not
        /// </summary>
        /// <param name="client">The connected client</param>
        /// <returns>True - if the client is still available and connencted, False - otherwise or if there was an error when checking</returns>
        public static bool IsConnected(this TcpClient client)
        {
            try
            {
                return client != null && client.Client != null &&
                   !(client.Client.Poll(1, SelectMode.SelectRead) && client.Client.Available == 0);
            }
            catch
            {
                return false;
            }
        }
    }
}
