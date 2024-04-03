using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace ConsoleApp80
{
    class Program
    {
        static Dictionary<string, TcpClient> clients = new Dictionary<string, TcpClient>();
        public static void TwoMain()
        {
            TcpListener server = new TcpListener(IPAddress.Any, 3522);
            server.Start();
            Console.WriteLine("Сервер запущен. Ожидание подключений...");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();
            }
        }
        static void Main(string[] args)
        {
            //TwoMain();
            UdpClient server = new UdpClient(12345);
            Console.WriteLine("Сервер запущен. Ожидание запросов...");

            while (true)
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] receivedData = server.Receive(ref clientEndPoint);
                string request = Encoding.ASCII.GetString(receivedData);
                if (request == "getTime")
                {
                    // Получение текущего времени
                    string currentTime = DateTime.Now.ToString();
                    byte[] responseData = Encoding.ASCII.GetBytes(currentTime);
                    server.Send(responseData, responseData.Length, clientEndPoint);
                }
            }
        }
        static void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            string clientId = Guid.NewGuid().ToString();
            clients.Add(clientId, client);
            Console.WriteLine("Клиент подключен. ID: " + clientId);

            byte[] data = new byte[256];
            while (true)
            {
                try
                {
                    int bytesRead = stream.Read(data, 0, data.Length);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.ASCII.GetString(data, 0, bytesRead);
                        Console.WriteLine("Получено сообщение от клиента " + clientId + ": " + message);

                        // Отправка сообщения всем клиентам, кроме отправителя
                        foreach (var kvp in clients)
                        {
                            if (kvp.Value != client)
                            {
                                byte[] responseData = Encoding.ASCII.GetBytes(message);
                                kvp.Value.GetStream().Write(responseData, 0, responseData.Length);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при чтении данных от клиента " + clientId + ": " + ex.Message);
                    break;
                }
            }

            // Удаление клиента из списка после отключения
            clients.Remove(clientId);
            Console.WriteLine("Клиент отключен. ID: " + clientId);
            client.Close();
        }
    }
}