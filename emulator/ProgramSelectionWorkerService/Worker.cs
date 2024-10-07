using ProgramSelectionLibraryDotNet;
using ProgramSelectionWorkerService;
using Renci.SshNet.Messages;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Sockets;
using Microsoft.Extensions.Options;
using static ProgramSelectionLibraryDotNet.ProgramSelection;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

using LibraryTLCalc;
using TLparamsCalc;

using System.Net.Mail;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.CompilerServices;
//==========================================================================================================
//||  Кроссплатформенная служба WorkerService (.NET 6.0)
//==========================================================================================================
//||  ProgramSelectionWorkerService - Служба, к которой можно обратиться по протоколу http через POST-запрос 
//||  При запуске происходит обращение к ftp серверу   DownloadTraffProgramsFromFile
//||  
//||  параметры:
//||  1) string neuroValuesStringsRMC - набор RMC-сообщений объединённых в одну строку c разделителями ("|")
//||  2) string param2
//||  appsettings.json - файл конфигурации приложения 
//||  "ServerPrefix" - адрес для отправки POST-запросов, например, "http://127.0.0.1:8888/connection/"
//||  "bLoadTraffProgramsFileFromFTP" - флаг необходимости загрузки файла светофорных программ с FTP-сервера;
//||                                     если значение 0, то подгружаем свет.программы из файла
//||                                     локальной директории (где лежит служба).
//||    
//==========================================================================================================
namespace clProgramSelection //ProgramsSelectionEmulator //
{
    /// <summary>
    /// Класс Worker для запуска процесса Linux-системы, который при обращении к нему по http 
    /// через POST-запрос возвращает номер выбранной светофорной программы 
    /// </summary>
    /// 
    public class WorkerOptions
    {
        public string ?ServerPrefix { get; set; }
        public bool bLoadTraffProgramsFileFromFTP { get; set; }

        public int AlgoritmNumber { get; set; }        
        public bool bSmoothProgTransition { get; set; }
    }
    public class Worker : BackgroundService
    {

        const string constFTPUser = "dan";  
        const string constFTPPass = "xuBdyEF20";
        const string constFilePath = "";
        //const string constFileName = @"TrafficLightsProgram.cnf";
        const string constFTPHost = "";
        const string constCatOnFTPServer = "";

        private readonly ILogger<Worker> _logger;
        private readonly WorkerOptions options;

        //private readonly FileLogger _fileLogger;

        //public Worker(ILogger<Worker> logger)
        //{
        //    _logger = logger;
        //}

        public Worker(ILogger<Worker> logger, WorkerOptions options)
        {
            this._logger = logger;
            this.options = options;
        }

        //public Worker(FileLogger loggerF)
        //{
        //    _fileLogger = loggerF;
        //}


        private static string ReadFormData(HttpListenerRequest request)
        {
            using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                return reader.ReadToEnd();
            }
        }

        internal int TrafficLightProgramCalculation(ref List<string> logStrings,string[] neuroValuesStringsRMCList, FileLogging fl, string FilePath, string FileName, bool bMinCoefsAlgoritm )
        {

            //Console.WriteLine("============================================================");
            //Console.WriteLine("=        Calculation of traffic light program number       =");
            //Console.WriteLine("============================================================");


         
            return 0; 
        }

        private void WriteConsoleAndLogFile(FileLogging fl, ILogger<Worker> _logger, string message, bool bLogInfo = true)
        {
            //Пишем в лог файла:
            fl.WriteLineToFile2(message);
            message = message.TrimEnd('\n').TrimEnd('\r');
            //Если не пришёл false в bLogInfo, то пишем в консоль:
            if (bLogInfo)
            {
                //Console.WriteLine(DateTime.Now.ToString() + ": " + curS);
                _logger.LogInformation(message, DateTime.Now);
            }
        }

        private void WriteConsoleAndLogFile_curS(FileLogging fl, ILogger<Worker> _logger, ref string curS, bool bLogInfo = true)
        {
            //Пишем в лог файла:
            fl.WriteLineToFile2(curS);
            curS = curS.TrimEnd('\n').TrimEnd('\r');
            //Если не пришёл false в bLogInfo, то пишем в консоль:
            if (bLogInfo)
            {
                //Console.WriteLine(DateTime.Now.ToString() + ": " + curS);
                _logger.LogInformation(curS, DateTime.Now);
            }
            curS = "";
        }

        private void WriteListToConsoleAndLogFile2(FileLogging fl, ref List<string> logStrings)
        {
            //List<string> _logStrings = new List<string>();
            //Выводим сначала на консоль - список строк для лога
            //foreach (string curLogString in logStrings)
            for (int i=0; i<logStrings.Count; i++)
            {
                //_logStrings.Add(String.Format("{0,-23} {1}", DateTime.Now.ToString() + " : " + logStrings[i] + Environment.NewLine));
                logStrings[i] = DateTime.Now.ToString() + " : " + logStrings[i] + Environment.NewLine;
                //Console.Write(logStrings[i]);
                if (logStrings[i].IndexOf("|") > 0)
                {
                    string[] lst = logStrings[i].Split("|");
                    foreach (string str in lst)
                    {
                        _logger.LogInformation(str, DateTime.Now);
                    }
                }
                else
                { 
                    _logger.LogInformation(logStrings[i], DateTime.Now);
                }
            }

            //Выводим в файл лога
            fl.WriteLinesListToFile2(logStrings);

            //Обнуляем logStrings
            logStrings.Clear();
        }

        //private void WriteConsoleAndLogFileSW(StreamWriter sw, ILogger<Worker> _logger, string message)
        //{
        //    //_logger.LogInformation("Worker running at: {time} ", DateTime.Now);
        //    //FileLoggerStatic.WriteLine("Worker running at: {time} ");

        //    _logger.LogInformation(message, DateTime.Now);
        //    sw.WriteLine(String.Format("{0,-23} {1}", DateTime.Now.ToString() + ":", message));
        //}

        string _curS = "";
        
        private bool DownloadTraffProgramsFromFile(ref string sMessage, string FTPUser, string FTPPass,
                                    string FilePath, string FileName,
                                    string FTPHost = "", string CatOnFTPServer = "")
        {            
            //1) Скачиваем файл с светофорными программами с ftp
            string resDownFile = FTPWorking.FTPDownloadFile(FTPUser, FTPPass, ref FilePath, ref FileName, FTPHost, CatOnFTPServer);
            if (!String.IsNullOrEmpty(resDownFile))
            {
                //Console.WriteLine("Программы не были загружены!");                
                sMessage = "Attention! Traffic light programs were NOT LOADED FROM FTP!" + Environment.NewLine;
                sMessage += resDownFile;   
                return false;
            }
            else
            {
                //Console.WriteLine("Файл светофорных программ '" + FilePath + FileName + "' был успешно загружен с FTP!");                
                sMessage = $"The file of traffic light programs '{FileName}' was SUCCESSFULLY loaded from FTP !" + Environment.NewLine;                
                
                //ПОКА НЕТ - 2 Загружаем в память светофорные программы                 
                return true;
            }
           
        }

        private Dictionary<int, List<Dil_LanesCount_CorrCoef>> ImportNumPhaseToDilsFromFile(string fileName, ref List<string> logStrings)
        {
            Dictionary<int, List<Dil_LanesCount_CorrCoef>> dictPhaseToDils = new Dictionary<int, List<Dil_LanesCount_CorrCoef>>();
            try
            {

                FileStream fs = File.Open(fileName, FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                string line = string.Empty;
                string sAfterColon = string.Empty;
                int linesSch = 0;
                logStrings.Add($"Read the phase-to-dils correspondence file: '{fileName}'...");
                int nextPos = -1;
                while ((line = sr.ReadLine()) != null)
                {
                    if (String.IsNullOrEmpty(line.Trim())) continue;

                    linesSch++;
                    string[] tmp = line.Split(';');
                    List<Dil_LanesCount_CorrCoef> dilTrLanesList = new List<Dil_LanesCount_CorrCoef>();
                    foreach (string s in tmp)
                    {
                        string curS = s.Replace('.',',');
                        Dil_LanesCount_CorrCoef dilTrLanes = new Dil_LanesCount_CorrCoef();
                        nextPos = curS.IndexOf(':');
                        dilTrLanes.TrLanesCount = 1;
                        dilTrLanes.correctingCoeffsForDil = 1;
                        //Eсли есть в подстроке только DIL-объекты соотв. фазе
                        if (nextPos < 0) 
                        {
                            dilTrLanes.dil = int.Parse(curS);
                            
                        }
                        else
                        {   //Если присутствует ещё и количество полос                        
                            dilTrLanes.dil = int.Parse(curS.Substring(0, nextPos));
                            sAfterColon = curS.Substring(nextPos + 1, curS.Length - nextPos - 1);
                            //Берём позицию *
                            nextPos = sAfterColon.IndexOf('*');
                            // Нет корректирующего коэф-та , а только число полос
                            if (nextPos < 0)
                            {
                                dilTrLanes.TrLanesCount = int.Parse(sAfterColon);
                            }
                            else 
                            //Есть и корректирующий коэф-т и число полос
                            {
                                dilTrLanes.TrLanesCount = int.Parse(sAfterColon.Substring(0, nextPos));
                                dilTrLanes.correctingCoeffsForDil = double.Parse(sAfterColon.Substring(nextPos + 1)); //sAfterColon.Length - nextPos - 1); ;
                            }
                        }
                        dilTrLanesList.Add(dilTrLanes);
                    }
                    //dictPhaseToDils.Add(linesSch, tmp.ToList());
                    dictPhaseToDils.Add(linesSch, dilTrLanesList);
                }
                fs.Close();
                logStrings.Add($"The the phase-to-dils correspondence file  '{fileName}' was SUCCESSFULLY loaded!");
                return dictPhaseToDils;
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.                
                //Console.WriteLine($"При чтении файла: {filename} возникла ошибка:");
                //Console.WriteLine(e.Message);
                logStrings.Add($"При чтении файла: {fileName} возникла ошибка:");
                logStrings.Add(e.Message);

                return new Dictionary<int, List<Dil_LanesCount_CorrCoef>>();
            }

        }

        /// <summary>
        /// Импортирует соответствие направлений - dil-объектам.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="crossRoad"></param>
        /// <param name="logStrings"></param>
        /// <returns></returns>
        private bool ImportDirectionsToDils(string fileName, ref ClassCrossRoad crossRoad, ref List<string> logStrings)
        {
            Dictionary<string,int> dictDirectionsToDils = new Dictionary<string, int>();
            try
            {
                FileStream fs = File.Open(fileName, FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                string line = string.Empty;
                //string sAfterColon = string.Empty;
                //int linesSch = 0;
                //int nextPos = -1;

                logStrings.Add($"Read the Directions To Dils correspondence file: '{fileName}'...");
                
                while ((line = sr.ReadLine()) != null)
                {
                    if (String.IsNullOrEmpty(line.Trim())) continue;
                    
                    string[] pairDircodeAndDil = line.Split('-');

                    //Если нет символа '-', то 
                    if (pairDircodeAndDil.Length < 2) continue;
                    if (int.TryParse(pairDircodeAndDil[1], out int dil))
                    {
                        dictDirectionsToDils.Add(Reflection.GetKey(pairDircodeAndDil[0].Trim()), dil);
                    }                    
                }
                fs.Close();

                crossRoad.dictDirectionsToDils = dictDirectionsToDils;

                logStrings.Add($"The the Directions-to-dils correspondence file  '{fileName}' was SUCCESSFULLY loaded!");
                return true;
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.                
                //Console.WriteLine($"При чтении файла: {filename} возникла ошибка:");
                //Console.WriteLine(e.Message);
                logStrings.Add($"При чтении файла: {fileName} возникла ошибка:");
                logStrings.Add(e.Message);

                return false;
            }

        }


        private List<int> GetIntListFromString(string sList)
        {
            string[] strArr = sList.Split(',');
            List<int> ints = new List<int>();
            foreach (string s in strArr)
            {
                ints.Add(int.Parse(s));
            }
            return ints;
        }

        private Dictionary<int, List<int>> ImportDictPhaseDursFromFile(string fileName, ref List<string> logStrings)
        {
            Dictionary<int, List<int>> phaseDurs = new Dictionary<int, List<int>>();
            try
            {
                FileStream fs = File.Open(fileName, FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                string line = "";
                int progNum = 0;
                logStrings.Add($"Read the phase durations file: '{fileName}'...");

                while ((line = sr.ReadLine()) != null)
                {
                    if (String.IsNullOrEmpty(line.Trim())) continue;
                    //Берём номер фазы и удаляем его из строки
                    progNum = Convert.ToInt32(line.Substring(0, line.IndexOf(':')));
                    line = line.Remove(0, line.IndexOf(':') + 1);

                    //Переводим считанную строку в массив значений
                    string[] sPhasesDurs = line.Split(',');
                    List<int> phasesDurs = new List<int>();
                    foreach (string sCurPhasesDur in sPhasesDurs)
                    {
                        phasesDurs.Add(int.Parse(sCurPhasesDur));
                    }

                        //Where(x => !string.IsNullOrWhiteSpace(x)).
                        //Select(x => int.Parse(x)).ToArray();

                    phaseDurs.Add(progNum, phasesDurs);
                }
                fs.Close();
                logStrings.Add($"The the phase durations file  '{fileName}' was SUCCESSFULLY loaded!");
                return phaseDurs;
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.                
                //Console.WriteLine($"При чтении файла: {filename} возникла ошибка:");
                //Console.WriteLine(e.Message);
                logStrings.Add($"При чтении файла: {fileName} возникла ошибка:");
                logStrings.Add(e.Message);

                return new Dictionary<int, List<int>>();
            }
        }
        //FFF!!!!!!!!!!!!!!!!!!!!!!!

        /// <summary>
        /// Импортирует дополнительные параметры конфигурации перекрёстка
        /// </summary>
        /// <param name="crossRoad"></param>
        /// <param name="fileName"></param>
        /// <param name="logStrings"></param>
        private void ImportAdditionalParamsForAdaptive(ref ClassCrossRoad crossRoad, string fileName, ref List<string> logStrings)
        {
            //Dictionary<int, List<int>> phaseDurs = new Dictionary<int, List<int>>();
            try
            {
                FileStream fs = File.Open(fileName, FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                string line = "";
               // int progNum = 0;
                logStrings.Add($"Read AdditionalParams file: '{fileName}'...");
                string[] sProportsPair;// =new string[2];
                while ((line = sr.ReadLine()) != null)
                {
                    if (String.IsNullOrEmpty(line.Trim())) continue;
                    //Берём номер фазы и удаляем его из строки

                    string[] sPair = line.Split("=");
                    //Если нет символа '-', то 
                    if (sPair.Length < 2) continue;
                    sPair[0] = sPair[0].Trim();
                    if (sPair[0] == "rounddecimals") { crossRoad.roundDecimals = Convert.ToInt32(sPair[1]); }
                    else if (sPair[0] == "proports") 
                    {
                        if (crossRoad.proports == null) crossRoad.proports = new Dictionary<string, List<double>> { };
                        sProportsPair = sPair[1].Split(":");
                        crossRoad.proports.Add(Reflection.GetKey(sProportsPair[0]),sProportsPair[1].Split(";")?.Select(Double.Parse)?.ToList());
                    }
                    else if (sPair[0] == "addInL") { crossRoad.addInL = Convert.ToDouble(sPair[1]); }
                    else if (sPair[0] == "addOutL") { crossRoad.addOutL = Convert.ToDouble(sPair[1]); }
                    else if (sPair[0] == "addInR") { crossRoad.addInR = Convert.ToDouble(sPair[1]); }
                    else if (sPair[0] == "addOutR") { crossRoad.addOutR = Convert.ToDouble(sPair[1]); }
                    else if (sPair[0] == "isRing") { crossRoad.isRing = Convert.ToBoolean(sPair[1]); }
                }
                fs.Close();
                logStrings.Add($"The the phase durations file  '{fileName}' was SUCCESSFULLY loaded!");
//                return phaseDurs;
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.                
                //Console.WriteLine($"При чтении файла: {filename} возникла ошибка:");
                //Console.WriteLine(e.Message);
                logStrings.Add($"При чтении файла: {fileName} возникла ошибка:");
                logStrings.Add(e.Message);

              //  return new Dictionary<int, List<int>>();
            }
        }

        /// <summary>
        /// Импортирует конфигурацию перекрёстка crossRoad из файла filename.
        /// </summary>
        /// <param name="crossRoad"></param>
        /// <param name="fileName"></param>
        /// <param name="logStrings"></param>
        private void _ImportAllForAdaptivConfig(ref ClassCrossRoad crossRoad, string fileName, ref List<string> logStrings)
        { 
            //создаём новый экземпляр класса для заполнения 
            ClassConSO.ConSO conSO = new ClassConSO.ConSO();            
            //Создаём временный массив перекрёстков
            List<ClassConSO.ConSO>  tmpAkSOlist = new List<ClassConSO.ConSO>();
            crossRoad.signalPlan = new Dictionary<string, List<string>>();
            //Dictionary<string, List<string>> minTimes = new Dictionary<string, List<string>>();
            List<string> minTimesShapka = new List<string>();
            List<string> minTimesValues = new List<string>();
            crossRoad.minTimesForPhases = new List<int>();

            try
            {
                FileStream fs = File.Open(fileName, FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                string line = "";
                string lineOrig = "";
                int progNum = 0;
                logStrings.Add($"Read configuration file: '{fileName}'...");
                //для $dtKSO
                Dictionary<string, object> valuesDict = new Dictionary<string, object>();
                //для "$dtPhases_0"
                //для "$dtTime_0"
                string curSection = "";

                //Номер строки внутри секции 
                int curSectionLineNumber = 0;

                List<string> headerFields = new List<string>();
                List<string> fieldValues = new List<string>();
                List<int> badPosList = new List<int>();

                while ((lineOrig = sr.ReadLine()) != null)
                {
                    line = lineOrig.Trim(); 
                    //Если строка(без незначащих символов) пуста
                    if (String.IsNullOrEmpty(line)) continue;

                    //Если строка начинает НОВЫЙ РАЗДЕЛ! 
                    if (line[0] == '$')
                    {
                        //Если мы уже до этой строки работали с каким-то разделом:
                        if (!String.IsNullOrEmpty(curSection))
                        {
                            //Если работали с секцией $dtKSO, то сохраняем то, что прочитали
                            if (curSection == "$dtKSO")                            
                            {
                                crossRoad.akSO = tmpAkSOlist.ToArray();
                                crossRoad.approachCount = tmpAkSOlist.Count;
                                tmpAkSOlist = new List<ClassConSO.ConSO>();
                                
                            }
                            //Если работали с секцией $dtPhases_0, то сохраняем то, что прочитали
                            else if (curSection == "$dtPhases_0")
                            {
                                headerFields.Clear();
                                fieldValues.Clear();
                                badPosList.Clear();

                                foreach (var kp in crossRoad.signalPlan)
                                { 
                                    Console.WriteLine( kp.Key + " : " + String.Join("; ", kp.Value));
                                }
                            }
                            //Если работали с мин значениями 
                            else if (curSection == "$dtTime_0")
                            {
                                //переводим minTimes в minTimesForPhases
                                //1;Осн-01;10;6;3;10;3;1;15;3; 2;53;;2

                                for (int i=0; i < minTimesShapka.Count; i++)
                                {
                                    if (minTimesShapka[i][0] == 'Ф') crossRoad.minTimesForPhases.Add(Convert.ToInt32(minTimesValues[i]));                                    
                                }
                                headerFields.Clear();
                                fieldValues.Clear();
                                badPosList.Clear();

                            }
                        }

                        //СОХРАНЯЕМ ТЕКУЩ. ЗАГОЛОВОК РАЗДЕЛА и номер строки внутри секции!
                        curSection = line;
                        curSectionLineNumber = 0;

                        //ВНИМАНИЕ! ЗДЕСЬ МЫ ПЕРЕХОДИМ С ТЕК. ИТЕРАЦИИ ЦИКЛА НА СЛЕДУЮЩУЮ СТРОКУ - К ШАПКЕ ПАРАМЕТРОВ
                        continue;
                    }
                    //ИНАЧЕ ЕСЛИ ЭТО СТРОКИ ВНУТРИ РАЗДЕЛА
                    else if (curSection != "")
                    {

                        //КОГДА получили список шапки, то обрабатываем строки
                        //ПАРАМЕТРЫ ПЕРЕКРЁСТКА (akSO)
                        if (curSection == "$dtKSO")
                        {
                            //Если работаем с шапкой
                            if (curSectionLineNumber <= 0)
                            {
                                headerFields = line.Split(';').ToList();
                            }
                            else
                            { 
                                fieldValues = line.Split(';').ToList();
                                if (Reflection.SetValuesForAllFieldsConSO(ref conSO, headerFields, fieldValues))
                                {
                                    tmpAkSOlist.Add(conSO);
                                    conSO = new ClassConSO.ConSO();
                                }
                                else
                                {
                                    tmpAkSOlist = new List<ClassConSO.ConSO>();
                                    throw new Exception("Несоответствие количества параметров количеству значений!" + Environment.NewLine +
                                                        "Секция - " + curSection + ", номер строки в секции - " + curSectionLineNumber);
                                }
                            }
                        }
                        //ПЛАН СИГНАЛИЗАЦИИ
                        else if (curSection == "$dtPhases_0")
                        {
                            //Если работаем с шапкой
                            if (curSectionLineNumber <= 0)
                            {
                                //ID;Напр;Тзав;Ф1;Ф2;фф2;С1;фс1;Ф3;фф3;пф3;С2;фс2;пс2;Ф4;фф4;пф4;Eg;X
                                headerFields = line.Split(';').ToList();
                                //удаляем "ID"
                                //headerFields.RemoveAt(0);
                                Reflection.NormalizeSHAPKU_SignalPlanStringList(ref headerFields, ref badPosList,false);
                                //Заносим шапку : ключ - 1й элемент "ID", список - остальные элементы                                
                                crossRoad.signalPlan.Add(headerFields[0],headerFields.GetRange(1,headerFields.Count-1));
                                
                                //ccr.signalPlan.Add("Напр", new List<string>(){ "Тзав", "Ф1", "<", "Ф2", "<", "<<", "Ф3", "<", "<<", "Ф4", "<", "Ф5", "<", "Eg", "'X'" });
                                //ccr.signalPlan.Add("Т11", new List<string>() { "3", "К", "", "К", "", "", "З", "<", "<ж", "К", "", "К", "", "19", "" });
                            }
                            else
                            {
                                fieldValues = line.Split(';').ToList();
                                //Если дошли до значимых строк Плана сигнализации
                                if (Convert.ToInt32(fieldValues[0]) >= 10)
                                {
                                    Reflection.AddLineOfSignalPlan(ref crossRoad, fieldValues, badPosList);
                                }                                 
                            }                                                        
                        }
                        else if (curSection == "$dtTime_0")
                        {
                            //Если работаем с шапкой
                            if (curSectionLineNumber <= 0)
                            {
                                headerFields = line.Split(';').ToList();
                                Reflection.NormalizeSHAPKU_SignalPlanStringList(ref headerFields, ref badPosList);
                                //Заносим шапку : ключ - 1й элемент "ID", список - остальные элементы                                
                                //minTimes.Add(headerFields[0], headerFields.GetRange(1, headerFields.Count - 1));
                                minTimesShapka = headerFields;
                            }
                            else
                            {
                                fieldValues = line.Split(';').ToList();
                                //Если дошли до значимых строк 
                                if (Convert.ToInt32(fieldValues[0]) == 1)
                                {
                                    AddLineOfMinTimes(ref minTimesValues, fieldValues, badPosList);
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }
                        //Увеличиваем номер строки внутри секции
                        curSectionLineNumber++;
                    }
                    ////Берём номер фазы и удаляем его из строки
                    //progNum = Convert.ToInt32(line.Substring(0, line.IndexOf(':')));
                    //line = line.Remove(0, line.IndexOf(':') + 1);

                    ////Переводим считанную строку в массив значений
                    //string[] sPhasesDurs = line.Split(',');
                    //List<int> phasesDurs = new List<int>();
                    //foreach (string sCurPhasesDur in sPhasesDurs)
                    //{
                    //    phasesDurs.Add(int.Parse(sCurPhasesDur));
                    //}

                    ////Where(x => !string.IsNullOrWhiteSpace(x)).
                    ////Select(x => int.Parse(x)).ToArray();

                    //phaseDurs.Add(progNum, phasesDurs);
                }
                fs.Close();
                logStrings.Add($"The the CONFIGURATION file '{fileName}' was SUCCESSFULLY loaded!");
                //return phaseDurs;
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.                
                //Console.WriteLine($"При чтении файла: {filename} возникла ошибка:");
                //Console.WriteLine(e.Message);
                logStrings.Add($"При чтении файла: {fileName} возникла ошибка:");
                logStrings.Add(e.Message);

               // return new Dictionary<int, List<int>>();
            }
        }

        public static bool AddLineOfMinTimes(ref List<string> minTimesValues, List<string> valuesList, List<int> badPosList)
        {
            string sKey = "";
            if (valuesList.Count <= 0)
            {
                return true;
            }
            
            //Составляем ключ
            sKey = valuesList[0];
            //valuesList.RemoveAt(0);

            List<string> resList = new List<string>();
            for (int i = 0; i < valuesList.Count; i++)
            {
                if (!badPosList.Contains(i))
                {
                    resList.Add(valuesList[i]);
                }
            }
            minTimesValues = resList;

            return true;
        }

        int lastChoosenProgram = 0;
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // WriteConsoleAndLogFile(fl, _logger, "!!!!!!!!!!!!!" + options.ServerPrefix);
            //await WorkWithTCP.FuncSendRequestToSocket2();
            _curS = "";
            //string? sLogPathFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);            

            //Получаем имя и каталог - локального файла св. программ (скаченного с FTP)
            FTPWorking.GetParamsLocalPathFile(out string FilePath, out string FileTLProgramsName);
            //Добавляем имя файла - чтобы писать в лог 
            string sLogPathFileWithFileName = FilePath + "log.txt";

            FileLogging fl = new FileLogging(sLogPathFileWithFileName);

            WriteConsoleAndLogFile(fl, _logger, "1) FilePath:" + FilePath);
            bool bTLProgramsDownloadedSuccessfull = false;

            Dictionary<int, List<Dil_LanesCount_CorrCoef>>? numPhaseToDils = null;// new Dictionary<int, List<DilWithTrLanesCount>>();
            Dictionary<int, List<int>> dictPhasesDurs = new Dictionary<int, List<int>>();
            List<int> BIntList = new List<int>();
            List<string> logStrings = new List<string>();

            ClassCrossRoad crossRoad = new ClassCrossRoad();

            //Если в настройках есть флаг , то Загружаем светофорные программы из файла
            if (options.AlgoritmNumber == 1)
            {
                if (options.bLoadTraffProgramsFileFromFTP)
                {
                    DownloadTraffProgramsFromFile(ref _curS, constFTPUser, constFTPPass, FilePath, FileTLProgramsName, constFTPHost, constCatOnFTPServer);
                    //Выводим на консоль и в файл
                    WriteConsoleAndLogFile_curS(fl, _logger, ref _curS);
                }

                //Если файла нет, то выходим ImportNumPhaseToDilsFromFile
                if (!File.Exists(FilePath + FileTLProgramsName))
                {
                    _curS = "File '" + FileTLProgramsName + "' does NOT exist in the directory: " + Environment.NewLine +
                           "'" + FilePath + "'!" + Environment.NewLine +
                           "We shall try to download it from FTP now..." + Environment.NewLine;
                    WriteConsoleAndLogFile_curS(fl, _logger, ref _curS);

                    bTLProgramsDownloadedSuccessfull = DownloadTraffProgramsFromFile(ref _curS, constFTPUser, constFTPPass, FilePath, FileTLProgramsName, constFTPHost, constCatOnFTPServer);
                    //Выводим на консоль и в файл
                    WriteConsoleAndLogFile_curS(fl, _logger, ref _curS);

                    if (!bTLProgramsDownloadedSuccessfull) return;
                }
            }
            else if (options.AlgoritmNumber == 2 || options.AlgoritmNumber == 3)
            {
                string FileNamePhaseToDILs = "PhaseToDils.phdils";
                numPhaseToDils = this.ImportNumPhaseToDilsFromFile(FilePath + FileNamePhaseToDILs, ref logStrings);
                if (options.AlgoritmNumber == 2)
                {
                    string FileNamePhaseTimes = "PhaseTimes.phtim";
                    ////Здесь надо бы с FTP также подгружать файлы PhaseTimes.phtim и PhaseToDils.phdils                    
                    dictPhasesDurs = this.ImportDictPhaseDursFromFile(FilePath + FileNamePhaseTimes, ref logStrings);
                }
                else if (options.AlgoritmNumber == 3)
                {
                    //BIntList = GetIntListFromString();
                }
            }
            //АДАПТИВ!
            else if (options.AlgoritmNumber == 99)
            {             
                string fileName = "DirectionsToDils.conf";
                if (ImportDirectionsToDils(FilePath + fileName, ref crossRoad, ref logStrings))
                { 
                    //numPhaseToDils = this.ImportNumPhaseToDilsFromFile(FilePath + FileNamePhaseToDILs, ref logStrings);
                    fileName = "Conf.tlop";
                    _ImportAllForAdaptivConfig(ref crossRoad, FilePath + fileName, ref logStrings);
                    fileName = "AdditionalParams.conf";
                    ImportAdditionalParamsForAdaptive(ref crossRoad, FilePath + fileName, ref logStrings);
                    //*************************************************************************************************
                    //----------------[ БЛОК ТОЛЬКО ДЛЯ ТЕСТИРОВАНИЯ! ]------------------------------------------------
                    /**/ crossRoad.GetCalculatedCrossroadResposeString(
                             Reflection.GetNormilizedNij(
                                 new Dictionary<string, double>()
                                 {
                                     { "10",480 },
                                     { "12",240 },
                                     { "13",330 },
                                     { "20",400 },
                                     { "30",410 },
                                     { "40",390 },
                                 }
                             )
                          );      
                        
                    /**/
                    //*************************************************************************************************
                }
            }
            
            WriteConsoleAndLogFile(fl, _logger, $"2) options.AlgoritmNumber = {options.AlgoritmNumber}");

            //Создаём сервер
            HttpListener server = new HttpListener();

            //!Берём префикс сервера из appsettings.json
            if (options.ServerPrefix is null) { options.ServerPrefix = "http://127.0.0.1:2050/connection/"; }
            server.Prefixes.Add(options.ServerPrefix);
            //server.Prefixes.Add("http://127.0.0.1:8888/connection/");
            //server.Prefixes.Add("http://192.168.3.129:8888/connection/");
            
            server.Start();
                 
            _curS += "The Worker is running!"; // + Environment.NewLine;
            WriteConsoleAndLogFile_curS(fl,  _logger, ref _curS);

            while (!stoppingToken.IsCancellationRequested)
            {
                // HttpListenerRequest request;

                try
                {
                    var context = await server.GetContextAsync();

                    //HttpListener listener = new HttpListener();
                    //HttpListenerContext context = listener.GetContext();

                    HttpListenerPrefixCollection prefixes = server.Prefixes;

                    _curS += "3) Prefixes:";// + Environment.NewLine;
                    foreach (string prefix in prefixes)
                    {
                        _curS += prefix;// + Environment.NewLine;                        
                    }
                    WriteConsoleAndLogFile_curS(fl, _logger, ref _curS);

                    // получаем данные запроса
                    var request = context.Request;

                    //if (request.HttpMethod == "GET")
                    //{
                    //    string sAnswer = $"<!DOCTYPE html lang=\"ru\"><head> O, DA! </head> <body> <h1> DANIIL - VSYO SUPER! </h1></body></html>";
                    //    // получаем объект для установки ответа
                    //    var response = context.Response;
                    //    byte[] buffer = Encoding.UTF8.GetBytes(sAnswer);
                    //    // получаем поток ответа и пишем в него ответ
                    //    response.ContentLength64 = buffer.Length;
                    //    using Stream output = response.OutputStream;
                    //    // отправляем данные
                    //    await output.WriteAsync(buffer);
                    //    await output.FlushAsync();
                    //    WriteConsoleAndLogFile2(fl, _logger, ref sAnswer);
                    //}

                    if (request.HttpMethod == "POST" && (request.ContentType == "application/x-www-form-urlencoded" ||
                                                         (request.ContentType != null && request.ContentType.StartsWith("multipart/form-data"))))
                    {
                       
                        //thing1 = first % 2Csecond % 2Cthird % 2Cforth & thing2 = sepParam
                        string readedData = ReadFormData(request);
                        WriteConsoleAndLogFile(fl, _logger, $"4) readedData: {readedData}");
                        //// Process the form data as needed                        
                        //Console.WriteLine("Received form data:");
                        //Console.WriteLine(readedData);

                        Dictionary<string, string> paramDict = GetParamDict(readedData,ref logStrings);

                        ////Выводим ключи:
                        //foreach (string curKey in request.QueryString.AllKeys) curS += request.QueryString[curKey] + Environment.NewLine;
                        //    //Console.WriteLine(request.QueryString[curKey]);
                        
                        //WriteConsoleAndLogFile2(fl, _logger, ref curS);
                        WriteConsoleAndLogFile(fl, _logger, $"5) paramDict: {paramDict}");
                        string[] neuroValuesStringsRMCList;

                        if (paramDict.TryGetValue("neuroValuesStringsRMC", out string? strNeuroValuesStringsRMC))
                        {
                            if (strNeuroValuesStringsRMC == null)
                            {
                                _curS += "Neurodetector Values List is null!";// + Environment.NewLine;
                                //WriteConsoleAndLogFile2(fl, _logger, ref curS);
                            }
                            else
                            {

                                string responseString = string.Empty;
                                //------------ЕСЛИ Адаптив------------------------------
                                if (options.AlgoritmNumber == 99)
                                {
                                    //responseString = crossRoad.GetCalculatedCrossroadResposeString();
                                    responseString = "Ф1=15;Ф2=20;Ф3=25;Ф4=30;Ф5=20";  
                                }
                                //-------ЕСЛИ ТЗПП---------------------------------
                                else
                                {
                                    neuroValuesStringsRMCList = strNeuroValuesStringsRMC.Split("|");
                                    WriteConsoleAndLogFile(fl, _logger, $"6) neuroValuesStringsRMCList: {neuroValuesStringsRMCList}");
                                    // WriteConsoleAndLogFile2(fl, _logger, ref curS, false);

                                    ////Выводим список нейросообщений
                                    //foreach (string curNeuroValueString in neuroValuesStringsRMCList)
                                    //{
                                    //    curS += curNeuroValueString;
                                    //}
                                    //====================================================================================
                                    //|     Вычисляем номер программы:                                                   |
                                    //==================================================================================== 
                                    //int prNum = TrafficLightProgramCalculation(ref logStrings, neuroValuesStringsRMCList, fl, FilePath, FileTLProgramsName, (options.AlgoritmNumber == 2));
                                    WriteConsoleAndLogFile(fl, _logger, $"7) =========[  Calculation of traffic light program number ]=========");
                                    //logStrings.Add("=========[  Calculation of traffic light program number ]=========");

                                    ProgramSelection ps = new ProgramSelection();
                                    lastChoosenProgram = ps.ChooseProgramTZPP(neuroValuesStringsRMCList.ToList(),
                                                                        ref logStrings,
                                                                        numPhaseToDils,
                                                                        dictPhasesDurs,
                                                                        options.AlgoritmNumber,
                                                                        BIntList,
                                                                        options.bSmoothProgTransition,
                                                                        lastChoosenProgram,
                                                                        FilePath, FileTLProgramsName
                                                                        );



                                    WriteListToConsoleAndLogFile2(fl, ref logStrings);
                                    responseString = lastChoosenProgram.ToString();
                                }
                                // получаем объект для установки ответа
                                var response = context.Response;
                                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                                // получаем поток ответа и пишем в него ответ
                                response.ContentLength64 = buffer.Length;
                                using Stream output = response.OutputStream;
                                // отправляем данные
                                await output.WriteAsync(buffer);
                                await output.FlushAsync();                                
                            };
                        }

                        ////Выводим информацию запроса:
                        //Console.WriteLine($"Application address: {request.LocalEndPoint}");
                        //Console.WriteLine($"Client address: {request.RemoteEndPoint}");
                        //Console.WriteLine(request.RawUrl);
                        //Console.WriteLine($"URL-address: {request.Url}");
                        //Console.WriteLine("Request headers:");
                        //foreach (string item in request.Headers.Keys) { Console.WriteLine($"{item}:{request.Headers[item]}"); }
                    }
                }
                catch (Exception ex)
                {
                    _curS += "==============[  ERROR! ]==============";
                    _curS += ex.Message + Environment.NewLine ;
                    _curS += "Source: " + ex.Source + Environment.NewLine;
                    WriteConsoleAndLogFile_curS(fl, _logger, ref _curS);
                   // _logger.LogInformation("Error! - " + ex.Message);
                    
                }
                await Task.Delay(300, stoppingToken);
            }
            //sw.Close();
            //sw.Dispose();
            //fl.DisposeStreamWriter();
            server.Stop();
        }

        private List<double> GetCorrCoeffsDoubleList(string sCorrCoeffs)
        {
            sCorrCoeffs = sCorrCoeffs.Replace('.', ',');
            List<double> list = new List<double>();
            string[] arr = (sCorrCoeffs.Split(';'));
            try
            {
                foreach (string str in arr)
                {
                    list.Add(Convert.ToDouble(str));
                }
            }
            catch 
            { 
                return new List<double>();
            }
            return list;    
        }
        private static Dictionary<string, string> GetParamDict(string readedData, ref List<string> logStrings)
        {
            logStrings = new List<string>();    
            //using System.Web and Add a Reference to System.Web
            Dictionary<string, string> postParams = new Dictionary<string, string>();
            string[] rawParams = readedData.Split('&');
            //Console.WriteLine("Retrieving values from the request:");
            logStrings.Add("Retrieving values from the request:");
            foreach (string param in rawParams)
            {
                string[] kvPair = param.Split('=');
                string key = kvPair[0];
                string value = HttpUtility.UrlDecode(kvPair[1]);
                postParams.Add(key, value);
                logStrings.Add($"  {key}: ");
                logStrings.Add(value);
            }

            return postParams;
        }
    }
}