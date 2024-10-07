using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  ProgramSelectionLibraryDotNet
{
    public class FTPWorking
    {
        // 194.87.111.128/home/dan/vanproject/
        const string constHost = "194.87.111.128";
        const string constCatOnFTPServer = @"/home/dan/vanproject/";
        // ***         Dim address As String = "ftp://" & Host & "/home/dan/vanproject/" & FiName 'Путь к файлу, который необходимо загрузить, включая имя файла и адрес сайта
        // Dim address As String = "ftp://" & Host & "/vanproject/" & FiName 'Путь к файлу, который необходимо загрузить, включая имя файла и адрес сайта        
        const string constFileName = "TrafficLightsPrograms.cnf";        
        //const string constFilePath = @"D:\"; // Имя загружаемого файла и путь к нему.
        //const string User = "dan"; // - Имя пользователя, подлинность которого необходимо проверить
        //const string Pass = "xuBdyEF20"; // - Пароль для проверки подлинности

        public static void GetParamsLocalPathFile(out string FilePath, out string FileName)
        {
            FilePath = "";
            FileName = "";
            if (String.IsNullOrEmpty(FilePath)) { FilePath = ProgramSelection.GetExeDirectory(); }            
            if (FilePath.Length > 0 && FilePath.Substring(FilePath.Length - 1) != Path.DirectorySeparatorChar.ToString()) //@"\" 
            { 
                FilePath += Path.DirectorySeparatorChar; 
            };
            if (String.IsNullOrEmpty(FileName)) { FileName = constFileName; }
        }

        private static void GetParams(ref string FilePath , ref string FileName,
                               ref string Host, ref string CatOnFTPServer)
        {
            if (String.IsNullOrEmpty(FilePath)) { FilePath = ProgramSelection.GetExeDirectory(); }
            if (String.IsNullOrEmpty(FileName)) { FileName = constFileName; }
            if (String.IsNullOrEmpty(Host)) { Host = constHost; }
            if (String.IsNullOrEmpty(CatOnFTPServer)) { CatOnFTPServer = constCatOnFTPServer; }

            string res = "Catalog: " + FilePath + Environment.NewLine;
            res += "Filename: " + FileName + Environment.NewLine;
            res += "Host(FTP): " + Host + Environment.NewLine;
            res += "Catalog(FTP):" + CatOnFTPServer + Environment.NewLine;
            Console.WriteLine(res);
        }

        /// <summary>
        /// Функция для выгрузки файла на FTP-сервер.
        /// </summary>
        /// <param name="User"> Логин пользователя (FTP) - необходим для загрузки файла светофорных программ с FTP </param>
        /// <param name="Pass"> Пароль пользователя (FTP) - необходим для загрузки файла светофорных программ c FTP </param>
        /// <param name="FilePath"> Путь к файлу на локальном диске - для сохранения файла, загруженного с FTP. По умолчанию - берётся путь к сборке.</param>
        /// <param name="FileName"> Имя файла. По умолчанию - "TrafficLightsPrograms.cnf" </param>
        /// <param name="Host"> Имя хоста FTP. По умолчанию - "194.87.111.128" </param>
        /// <param name="CatOnFTPServer"> Каталог на FTP. По умолчанию - @"/home/dan/vanproject/"</param>
        /// <returns> Истина - если файл выгружен без ошибок.</returns>
        internal static string FTPUploadFile(string User, string Pass,
                                    string FilePath = "", string FileName = "",
                                    string Host = "", string CatOnFTPServer = ""
                                    )
        {
            GetParams(ref FilePath, ref FileName,
                      ref Host, ref CatOnFTPServer);

            string res = "";
            var authMethod = new Renci.SshNet.PasswordAuthenticationMethod(User, Pass);
            var connectionInfo = new Renci.SshNet.ConnectionInfo(Host, User, authMethod);
            var client = new Renci.SshNet.SftpClient(connectionInfo);
            //var client2 = new Renci.SshNet.SftpClient()

            if (FilePath.Length > 0 && FilePath.Substring(FilePath.Length - 1) != Path.DirectorySeparatorChar.ToString()) //@"\") 
            { FilePath = FilePath + Path.DirectorySeparatorChar; };
            try
            {
                client.Connect();
                using (var fileStream = System.IO.File.OpenRead(FilePath + FileName))
                {
                    client.UploadFile(fileStream, CatOnFTPServer + FileName, true);
                }
            }
            catch (Exception ex)
            {
                res = $"Ошибка при выгрузке файла '{FilePath + FileName}' на FTP-сервер - '{Host + CatOnFTPServer}'" + Environment.NewLine +
                                  $"{ex.Message}";
                Console.WriteLine(res); // 
            }
            finally
            {
                client.Disconnect();
                client.Dispose();
            }
            return res;
        }

        //internal static string ASD(string User, string Pass,
        //                            string FilePath = "", string FileName = "",
        //                            string Host = "", string CatOnFTPServer = ""
        //                            )
        //{
        //    //Кидаем файл на FTP
        //    string sResUpload = FTPWorking.FTPUploadFile(FTPUser, FTPPass,
        //                                                 FilePath, FileName,
        //                                                 Host, CatOnFTPServer);
        //    if (String.IsNullOrEmpty(sResUpload))
        //    {
        //        Console.WriteLine($" Файл интенсивностей {FilePath + FileName} " +
        //                          $" был УСПЕШНО выгружен на FTP {Host} {CatOnFTPServer}");
        //    }
        //    else
        //    {
        //        Console.WriteLine($" При выгрузке файла интенсивностей {FilePath + FileName} " +
        //                          $" на FTP {Host} {CatOnFTPServer} возникли ошибки: " + Environment.NewLine + sResUpload);
        //    }
        //}

        /// <summary>
        /// Функция для загрузки файла с FTP-сервера.
        /// </summary>
        /// <param name="User"> Логин пользователя (FTP) </param>
        /// <param name="Pass"> Пароль пользователя (FTP) </param>
        /// <param name="FilePath"> Путь к файлу на локальном диске - для сохранения файла, загруженного с FTP. По умолчанию - берётся путь к сборке.</param>
        /// <param name="FileName"> Имя файла. По умолчанию - "TrafficLightsPrograms.cnf" </param>
        /// <param name="Host"> Имя хоста FTP. По умолчанию - "194.87.111.128" </param>
        /// <param name="CatOnFTPServer"> Каталог на FTP. По умолчанию - @"/home/dan/vanproject/"</param>
        /// <returns> Истина - если файл загружен без ошибок.</returns>
        public static string FTPDownloadFile(string User, string Pass,
                                    ref string FilePath, ref string FileName,
                                    string Host = "", string CatOnFTPServer = "")
        {
            GetParams(ref FilePath, ref FileName,
                      ref Host, ref CatOnFTPServer);

            string res = "";

            var authMethod = new Renci.SshNet.PasswordAuthenticationMethod(User, Pass);
            var connectionInfo = new Renci.SshNet.ConnectionInfo(Host, User, authMethod);
            var client = new Renci.SshNet.SftpClient(connectionInfo);
            if (FilePath.Length > 0 && FilePath.Substring(FilePath.Length - 1) != Path.DirectorySeparatorChar.ToString()) //@"\") 
            { FilePath = FilePath + Path.DirectorySeparatorChar.ToString(); };
            try
            {
                
                client.Connect();
                using (Stream fileStream = System.IO.File.Create(FilePath + FileName))
                {
                    client.DownloadFile(CatOnFTPServer + FileName, fileStream);

                }
            }
            catch (Exception ex)
            {
                res = $"Ошибка при загрузке файла '{FileName}' с FTP-сервера - '{Host + CatOnFTPServer}'" + Environment.NewLine +
                      $"{ex.Message}";
                Console.WriteLine(res); // 
            }
            finally
            {
                client.Disconnect();
                client.Dispose();
            }
            return res;
        }
    }

}
