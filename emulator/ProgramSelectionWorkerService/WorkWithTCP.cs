using System;
using System.Collections.Generic;
//using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;

namespace ProgramSelectionWorkerService
{
    internal class WorkWithTCP
    {

        //private async void FuncSendRequestToSocket(object sender, EventArgs e)
        //{

        //    IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, 8888);
        //    using Socket tcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //    {
        //        try
        //        {
        //            tcpListener.Bind(ipPoint);
        //            tcpListener.Listen(1000);//Listen();    // запускаем сервер
        //            Console.WriteLine("Сервер запущен. Ожидание подключений... ");

        //            var tcpClient = await tcpListener.AcceptAsync();
        //            while (true)
        //            {
        //                // получаем входящее подключение
        //                using (var tcpClient = await tcpListener.AcceptAsync()) ;
        //                //{
        //                // определяем данные для отправки - текущее время
        //                byte[] data = Encoding.UTF8.GetBytes(DateTime.Now.ToLongTimeString());
        //                // отправляем данные
        //                await tcpClient.SendAsync(data);
        //                Console.WriteLine($"Клиенту {tcpClient.RemoteEndPoint} отправлены данные");
        //                //}
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //        }
        //    }
        //}

        //public static async Task<int> FuncSendRequestToSocket2()
        //{
        //    //Console.WriteLine();
        //    //Console.WriteLine($"НАЧИНАЕМ ПЕРЕДАВАТЬ НА СЕРВЕР СООБЩЕНИЕ!");
        //    //var port = 1031;
        //    //var url = "194.87.111.128";

        //    //int tlProg = 4;
        //    //var dictStr = new Dictionary<string, string>
        //    //  {
        //    //    ////Данные запроса для домодедово
        //    //    { "userId", "27"},
        //    //    { "userName", "userName" },
        //    //    { "userProjId","4" },
        //    //    { "userRoleInd","1" },
        //    //    { "userType","ingeneer" },
        //    //    { "trLightId","1" },
        //    //    { "client_name","domoded_test" }, //"sym_kar_koz" - для "боевого" домодедово
        //    //    { "show_name","show_name" },
        //    //    { "command","setprogram" },
        //    //    { "programNo", tlProg.ToString() }
        //    //    ////  //{ "", strList },
        //    //    ////  //{ "", param }
        //    //  };

        //    ////BinaryFormatter serialization methods are obsolete and prohibited in ASP.NET apps
        //    //var binFormatter = new BinaryFormatter();
        //    //var mStream = new MemoryStream();
        //    //binFormatter.Serialize(mStream, dictStr);            
        //    //byte[] dictStrArray = mStream.ToArray();

        //    //using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream,ProtocolType.Tcp);

        //    //int bytesSent = 0 ;
        //    //try
        //    //{
        //    //    await socket.ConnectAsync(url, port);
        //    //    // определяем отправляемые данные
        //    //    //var message = $"GET / HTTP/1.1\r\nHost: {url}\r\nConnection: close\r\n\r\n";
        //    //    string message2 = "{\"userId\":\"27\", \"userName\":\"userName\", \"userProjId\":\"4\", \"userRoleInd\":\"1\", \"userType\":\"ingeneer\", \"trLightId\":\"1\", \"client_name\":\"domoded_test\",\"show_name\":\"domoded_test\", \"command\":\"setprogram\", \"programNo\":\"" + tlProg.ToString()+"\"} ";
        //    //   // string message3 = "{userId:27, userName:userName, userProjId:4, userRoleInd:1, userType:ingeneer, trLightId:1, client_name:domoded_test,show_name:show_name, command:setprogram, programNo:" + tlProg.ToString() + "} ";
        //    //    // конвертируем данные в массив байтов
        //    //    byte[] messageBytes = Encoding.UTF8.GetBytes(message2);

        //    //    bytesSent = await socket.SendAsync(dictStrArray, SocketFlags.None);
        //    //    Console.WriteLine($"на адрес {url} отправлено {bytesSent} байт(а)");

        //    //    // буфер для получения данных
        //    //    var responseBytes = new byte[512];
        //    //    // получаем данные
        //    //    var bytes = await socket.ReceiveAsync(responseBytes, SocketFlags.None);
        //    //    // преобразуем полученные данные в строку
        //    //    string response = Encoding.UTF8.GetString(responseBytes, 0, bytes);


        //    //    // выводим данные на консоль
        //    //    Console.WriteLine(response);
        //    //}
        //    //catch (SocketException ex)
        //    //{
        //    //    Console.WriteLine(ex.Message);
        //    //}
        //    //finally
        //    //{
        //    //    await socket.DisconnectAsync(true);

        //    //}
        //    //return bytesSent;
        //}

        public static async Task<string> SendDataToClient(string ipAddr, int port)
        {
            IPAddress ip = IPAddress.Parse(ipAddr);
            var tcpListener = new TcpListener(ip, port);

            try
            {
                tcpListener.Start();    // запускаем сервер
                Console.WriteLine("Сервер запущен. Ожидание подключений... ");

                while (true)
                {
                    // получаем подключение в виде TcpClient
                    using var tcpClient = await tcpListener.AcceptTcpClientAsync();
                    // получаем объект NetworkStream для взаимодействия с клиентом
                    var stream = tcpClient.GetStream();
                    // определяем данные для отправки - отправляем текущее время
                    byte[] data = Encoding.UTF8.GetBytes(DateTime.Now.ToLongTimeString());
                    // отправляем данные
                    await stream.WriteAsync(data);
                    Console.WriteLine($"Клиенту {tcpClient.Client.RemoteEndPoint} отправлены данные");
                }
            }
            finally
            {
                tcpListener.Stop();
            }
        }
    }
}
