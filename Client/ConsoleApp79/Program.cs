using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
namespace ConsoleApp79
{
    class Program
    {
        public static void TCPMain()
        {
            TcpClient client = new TcpClient();
            client.Connect("127.0.0.1", 3522);
            NetworkStream stream = client.GetStream();

            Console.WriteLine("Введите ваше имя:");
            string userName = Console.ReadLine();

            byte[] data = Encoding.ASCII.GetBytes(userName);
            stream.Write(data, 0, data.Length);

            Console.WriteLine("Вы подключены к серверу. Можете начинать общение.");

            // Поток для приема сообщений от сервера
            byte[] responseData = new byte[1024];
            while (true)
            {
                int bytesRead = stream.Read(responseData, 0, responseData.Length);
                string message = Encoding.ASCII.GetString(responseData, 0, bytesRead);
                Console.WriteLine(message);
            }
        }
        static void Main(string[] args)
        {
            //TCPMain();
            UdpClient client = new UdpClient();
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345);

            // Время, когда нужно совершить звонок (например, 10:00)
            TimeSpan callTime = new TimeSpan(10, 0, 0);

            while (true)
            {
                // Отправка запроса на сервер
                byte[] requestData = Encoding.ASCII.GetBytes("getTime");
                client.Send(requestData, requestData.Length, serverEndPoint);

                // Получение ответа от сервера
                byte[] responseData = client.Receive(ref serverEndPoint);
                string currentTime = Encoding.ASCII.GetString(responseData);

                Console.WriteLine("Текущее время: " + currentTime);

                // Проверка времени для звонка
                TimeSpan currentTimeOfDay = DateTime.Now.TimeOfDay;
                if (currentTimeOfDay.Hours == callTime.Hours && currentTimeOfDay.Minutes == callTime.Minutes)
                {
                    Console.WriteLine("Звонок!");
                    // Здесь можно добавить логику для звонка, например, проигрывание звонка или отправку уведомления
                }

                // Ожидание перед следующим запросом
                Thread.Sleep(1000);
            }
        }
    }
}