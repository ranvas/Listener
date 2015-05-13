using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using Microsoft.Win32;

namespace Server.Model
{
       
    public class MyServer : IServer
    {

        //private string myAdress = "127.0.0.1";
        //private string myPort = "1050";


        #region конструктор и свойства
        public MyServer()
        {
            _running = false;
        }
        /// <summary>
        /// Запущено ли?
        /// </summary>
        public bool Running
        {
            get
            {
                return _running;
            }
        }
        /// <summary>
        /// сокет, которым будем слушать
        /// </summary>
        public Socket ServerSocket
        {
            get
            {
                return _serverSocket;
            }
        }
        /// <summary>
        /// базовый поток в котором будет выполняться сервер
        /// </summary>
        public Thread RequestListenerT
        {
            get
            {
                return _requestListenerT;
            }
        }
        /// <summary>
        /// неважные события
        /// </summary>
        /// <param name="_sea"></param>
        public event EventHandler<ServerEventArgs> ServerTracing;


        /// <summary>
        /// событие получения ошибки
        /// </summary>
        /// <param name="_sea"></param>
        public event EventHandler<ServerEventArgs> ServerError;

        // Кодировка
        private Encoding _charEncoder = Encoding.UTF8;

        // Поодерживаемый контент нашим сервером
        // Вы можете добавить больше
        // Смотреть здесь: http://www.webmaster-toolkit.com/mime-types.shtml
        private Dictionary<string, string> _extensions = new Dictionary<string, string>
        { 
            //{ "extension", "content type" }
            { "htm", "text/html"},
            { "html", "text/html" },
            { "xml", "text/xml" },
            { "txt", "text/plain" },
            { "css", "text/css" },
            { "png", "image/png" },
            { "gif", "image/gif" },
            { "jpg", "image/jpg" },
            { "jpeg", "image/jpeg" },
            { "zip", "application/zip"},
            { "json", "application/json"}
        };



        private bool _running;
        private Socket _serverSocket;
        private Thread _requestListenerT;

        #endregion

        #region сервис

        /// <summary>
        /// запуск сервиса с настройками по умолчанию
        /// </summary>
        public void start()
        {
            IPAddress ipAddress_ = IPAddress.Any;
            int port_ = 1050;
            int _maxNOfCon = 10;
            int _timeout = 100;
            start(ipAddress_, port_, _maxNOfCon, _timeout);
        }

        /// <summary>
        /// Запускаем сервер
        /// </summary>
        /// <param name="_ipAddress">прослушиваемый адрес</param>
        /// <param name="port">прослушиваемый порт</param>
        /// <param name="maxNOfCon">максимальное количество обрабатываемых соединений</param>
        private void start(IPAddress ipAddress, int port, int maxNOfCon, int timeout)
        {
            if (_running)
            {
                throw new Exception("попытка включить включнный сервер");
            }
            //создаем tcp/ip сокет (ipv4) для прослушивания сети
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(new IPEndPoint(ipAddress, port));
            _serverSocket.Listen(maxNOfCon);
            _serverSocket.ReceiveTimeout = timeout;
            _running = true;
            _serverSocket.SendTimeout = timeout;
            OnServerTracing(new ServerEventArgs
            {
                text = String.Format("Создан новый сервер-сокет, который слушает {0}:{1}, с максимальным количеством одновременных потоков {2} и таймаутом {3} ", ipAddress, port, maxNOfCon, timeout)
            });
            Thread requestHandler;
            // Наш поток ждет новые подключения и создает новые потоки.
            _requestListenerT = new Thread(() =>
            {
                OnServerTracing(new ServerEventArgs
                {
                    text = String.Format("запущен новый основной поток с идентификатором {0}", _requestListenerT.Name)
                });
                while (_running)
                {
                    Socket clientSocket;
                    try
                    {
                        OnServerTracing(new ServerEventArgs
                        {
                            text = String.Format("Слушаем сервер-сокет, пока не получим клиент-сокет")
                        });
                        //получаем запрос с прослушиваемого сокета
                        clientSocket = _serverSocket.Accept();
                        OnServerTracing(new ServerEventArgs
                        {
                            text = String.Format("Получен клиент-сокет")
                        });
                        // Создаем новый поток для нового клиента и продолжаем слушать сокет.
                        requestHandler = new Thread(() =>
                        {
                            OnServerTracing(new ServerEventArgs
                            {
                                text = String.Format("запущен новый поток для обработки клиентского запроса")
                            });
                            //обработка запроса
                            clientSocket.ReceiveTimeout = timeout;
                            clientSocket.SendTimeout = timeout;
                            try
                            {
                                //запускаем обработчик
                                handleTheRequest(clientSocket);
                            }
                            catch (Exception e)
                            {
                                OnServerError(new ServerEventArgs
                                {
                                    text = String.Format("При обработке запроса произошла ошибка: {0}", e.Message),
                                    ServerSocket = clientSocket
                                });
                            }
                            clientSocket.Close();
                        });
                        requestHandler.Start();
                    }
                    catch (SocketException se)
                    {
                        OnServerError(new ServerEventArgs
                        {
                            text = String.Format("Произошла ошибка: {0}", se.Message),
                            
                        });
                    }
                    catch (Exception e)
                    {
                        OnServerError(new ServerEventArgs
                        {
                            text = String.Format("Произошла ошибка: {0}", e.Message),
                        });
                    }
                }
            });
            _requestListenerT.Start();
        }

        /// <summary>
        /// останавливаем сервер
        /// </summary>
        public void stop()
        {
            OnServerTracing(new ServerEventArgs
                {
                    text = String.Format("попытка выключить сервер")
                });
            if (_running)
            {
                _running = false;
                _serverSocket.Close();
                _serverSocket = null;
                OnServerTracing(new ServerEventArgs { text = "server выключен" });
            }
            else
            {
                OnServerError(new ServerEventArgs
                {
                    text = String.Format("попытка остановить незапущенный сервер")
                });
            }
            
        }
        protected virtual void OnServerError(ServerEventArgs sea)
        {
            EventHandler<ServerEventArgs> handler = ServerError;
            if (handler != null)
            {
                handler(this, sea);
            }
            if (sea.ServerSocket != null)
            {
                notImplemented(sea.ServerSocket, sea.text);
            }
        }

        protected virtual void OnServerTracing(ServerEventArgs sea)
        {
            EventHandler<ServerEventArgs> handler = ServerTracing;
            if (handler != null)
            {
                handler(this, sea);
            }
        }
        


        /// <summary>
        /// обработка пользовательского запроса
        /// </summary>
        /// <param name="clientSocket"></param>
        private void handleTheRequest(Socket clientSocket)
        {
            
            OnServerTracing(new ServerEventArgs
            {
                text = String.Format("обработка клиентского запроса началась")
            });
            string responsebyAPI = null;
            string contentType;
            try
            {
                //буфер
                byte[] buffer = new byte[10240];
                // Получаем содержимое запроса в buffer
                int receivedBCount = clientSocket.Receive(buffer);
                // кодируем полученные даные
                string strReceived = _charEncoder.GetString(buffer, 0, receivedBCount);
                //Если запрос GET /favicon.ico не обрабатываем его
                string favicon = "GET /favicon.ico";
                if(String.Equals(strReceived.Substring(0, favicon.Length), favicon))
                {
                  OnServerTracing(new ServerEventArgs
                  {
                    text = String.Format("Получен запрос иконки страницы. Не обрабатываем его")
                  });
                  return;
                }
                //парсим полученные данные
                Request req = ParseRequest(strReceived);

                //если запрос не GET и не POST, то возвращаем 501 ошибку
                if (!req.httpMethod.Equals("GET"))
                {
                    OnServerError(new ServerEventArgs
                    {
                        text = String.Format("Полученный запрос не поддерживается"),
                        ServerSocket = clientSocket
                    });
                    return;
                }
                //запрашиваем запрос у API
                contentType = _extensions["html"];
                //responsebyAPI = protocol(req.methodName, req.Args);
                responsebyAPI = API.Api.ApiGenerateResponse(req.methodName, out contentType, req.Args);
                //отправка ответа
                sendOkResponse(clientSocket, responsebyAPI, contentType);
            }
            catch (Exception e)
            {
                OnServerError(new ServerEventArgs 
                { 
                    text = String.Format("Ошибка при обработке запроса с {0}:\n{1}", clientSocket.RemoteEndPoint, e.Message),
                    ServerSocket = clientSocket
                });
            }
        }


        private Request ParseRequest(string strReceived)
        {
            OnServerTracing(new ServerEventArgs
            {
                text = String.Format("парсинг запроса {0}", strReceived)
            });
            Request req = new Request();//запарсенный запрос
            //узнаем httpMethod
            req.httpMethod = strReceived.Substring(0, strReceived.IndexOf(" "));
            //индекс начала запроса
            int start = strReceived.IndexOf(req.httpMethod) + req.httpMethod.Length + 1;
            //длина запроса
            int length = strReceived.LastIndexOf("HTTP") - start - 1;
            //запоминаем запрашиваемый URL
            req.requestedUrl = strReceived.Substring(start, length);
            int questindex;//индекс знака вопроса
            int argindex;//индекс начала параметра
            int ampersandindex;//индекс амперсанда
            //Переменная, в которую будем складывать параметры запроса для вывода
            string requestArgs = "Не обладает аргументами";
            //если нет знака вопроса, то подразумеваем, что параметров нет
            if ((questindex = req.requestedUrl.IndexOf("?")) == -1)
            {
                //запоминаем имя запрашиваемого метода
                req.methodName = req.requestedUrl;
            }
            else
            {
              requestArgs = "Обладает аргументами: ";
                //запоминаем строку до вхождения знака вопроса
                req.methodName = req.requestedUrl.Substring(0, questindex);
                //пропускаем знак вопроса
                argindex = questindex + 1;
                //пока находим амперсанд
                while ((ampersandindex = req.requestedUrl.IndexOf("&", argindex)) != -1)
                {
                    //записываем подстроку до амперсанда
                    req.Args.Add(req.requestedUrl.Substring(argindex, (ampersandindex - argindex)));
                    requestArgs += req.requestedUrl.Substring(argindex, (ampersandindex - argindex)) + " ";
                    //инкрементируем вхождение
                    argindex = ampersandindex + 1;
                }
                //проводим операцию записи последнего параметра до конца
                req.Args.Add(req.requestedUrl.Substring(argindex));
                requestArgs += req.requestedUrl.Substring(argindex);
            }
            OnServerTracing(new ServerEventArgs
            {
              text = String.Format("Распарсенный запрос носит имя {0}, {1}", req.methodName, requestArgs)
            });
            return req;
        }
        #endregion

        #region разные виды ответа сервера

        /// <summary>
        /// ответ для строк
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="strContent"></param>
        /// <param name="responseCode"></param>
        /// <param name="contentType"></param>
        private void sendResponse(Socket clientSocket, string strContent, string responseCode,
                                  string contentType)
        {
            byte[] bContent = _charEncoder.GetBytes(strContent);
            sendResponse(clientSocket, bContent, responseCode, contentType);
        }

        /// <summary>
        /// ответ для массива байт
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="bContent"></param>
        /// <param name="responseCode"></param>
        /// <param name="contentType"></param>
        private void sendResponse(Socket clientSocket, byte[] bContent, string responseCode,
                                  string contentType)
        {
            try
            {
                byte[] bHeader = _charEncoder.GetBytes(
                                    "HTTP/1.1 " + responseCode + "\r\n"
                                  + "Server: MyServer\r\n"
                                  + "Content-Length: " + bContent.Length.ToString() + "\r\n"
                                  + "Connection: close\r\n"
                                  + "Content-Type: " + contentType + "; charset=utf-8\r\n\r\n");
                clientSocket.Send(bHeader);
                clientSocket.Send(bContent);
                clientSocket.Close();
            }
            catch { }
        }

        /// <summary>
        /// ответ если 501 ошибка, не найден обработчик
        /// </summary>
        /// <param name="clientSocket"></param>
        private void notImplemented(Socket clientSocket, string message)
        {
            sendResponse(clientSocket, "<html><head><meta " +
            "http-equiv=\"Content-Type\" content=\"text/html; " +
            "charset=utf-8\"></head><body><h2>MyServer</h2><div>501 - Method Not Implemented" +
                "<br>" + message +
                "</div></body></html>",
                "501 Not Implemented", "text/html");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="clientSocket"></param>
        private void notFound(Socket clientSocket)
        {

            sendResponse(clientSocket, "<html><head><meta " +
                "http-equiv=\"Content-Type\" content=\"text/html; " +
                "charset=utf-8\"></head><body><h2>MyServer " +
                "Server</h2><div>404 - Not " +
                "Found</div></body></html>",
                "404 Not Found", "text/html");
        }

        /// <summary>
        /// ответ если все хорошо, передаем байты
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="bContent"></param>
        /// <param name="contentType"></param>
        private void sendOkResponse(Socket clientSocket, byte[] bContent, string contentType)
        {

            sendResponse(clientSocket, bContent, "200 OK", contentType);
        }

        /// <summary>
        /// ответ если все хорошо, передаем строку
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="bContent"></param>
        /// <param name="contentType"></param>
        private void sendOkResponse(Socket clientSocket, string bContent, string contentType)
        {
            //OnServerTracing(new ServerEventArgs { text = String.Format("отвечаем, что все ОК и передаем данные:\n {0}", bContent) });
            OnServerTracing(new ServerEventArgs { text = String.Format("отвечаем, что все ОК, данные где-то есть"), ServerSocket = clientSocket });
            sendResponse(clientSocket, bContent, "200 OK", contentType);
        }
        #endregion



    }


    

}
