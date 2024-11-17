using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.Tests
{
    public interface ITest
    {
        Task Test();
    }
    internal class TestClientDisconnected : ITest
    {
        public async Task Test()
        {
            using TcpClient tcpClient = new TcpClient("127.0.0.1", 37065);
            using NetworkStream stream = tcpClient.GetStream();
            byte[]  buffer = Encoding.UTF8.GetBytes("Test user");
            await stream.WriteAsync(buffer.AsMemory());
            await Task.Delay(4000);
            tcpClient.Close();
            await Task.CompletedTask;
        }
    }    
}
