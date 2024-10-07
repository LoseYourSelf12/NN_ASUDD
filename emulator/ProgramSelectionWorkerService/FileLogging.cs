using System;
using System.Collections.Generic;
//using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ProgramSelectionWorkerService
{
    internal class FileLogging
    {
        //StreamWriter sw; //{ get; set; }
        string dir = ""; 
        internal FileLogging(string? direct) // StreamWriter swriter)
        {
            if (String.IsNullOrEmpty(direct))
            {
                dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/log.txt";
            }
            else
            {
                dir = direct;
            }
            // sw = new StreamWriter(direct,true);
        }

        ////----------------------------------------------------------
        //// Статический метод записи строки в файл лога без переноса
        ////----------------------------------------------------------
        //public static void Write(string dir, string text)
        //{
        //    using (StreamWriter sw = new StreamWriter(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\log.txt", true))
        //    {
        //        sw.Write(DateTime.Now.ToString() + " " + text);
        //        sw.Close();
        //        sw.Dispose();

        //    }
        //}

        ////---------------------------------------------------------
        //// Статический метод записи строки в файл лога с переносом
        ////---------------------------------------------------------
        //public static void WriteLine(string? dir, string message)
        //{
        //    if (String.IsNullOrEmpty(dir)) { dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }

        //    using (StreamWriter sw = new StreamWriter(dir + "\\log.txt", true))
        //    {
        //        sw.WriteLine(String.Format("{0,-23} {1}", DateTime.Now.ToString() + ":", message));
        //        sw.Close();
        //        sw.Dispose();
        //    }
        //}

        ////----------------------------------------------------------
        //// Статический метод записи строки в файл лога без переноса
        ////----------------------------------------------------------
        //public void Write(string dir, string text)
        //{
        //    using (sw)
        //    {
        //        sw.Write(DateTime.Now.ToString() + " " + text);
        //        sw.Close();
        //        sw.Dispose();

        //    }
        //}

        //---------------------------------------------------------
        // Статический метод записи строки в файл лога с переносом
        //---------------------------------------------------------
        internal void WriteLineToFile(string message)
        {
            using (StreamWriter sw = new StreamWriter(dir, true))
            {
                sw.WriteLine(DateTime.Now.ToString() + " : " + message);
                sw.Flush();
                sw.Close();
            }
        }

        internal void WriteLineToFile2(string message) //, string dir)
        {
            using (StreamWriter sw = new StreamWriter(dir, true))
            {
                sw.WriteLineAsync( DateTime.Now.ToString() + " : " + message);
                sw.Flush();
                sw.Close();
            }
        }

        internal void WriteLinesListToFile2(List<string> logStrings) //, string dir)
        {
            using (StreamWriter sw = new StreamWriter(dir, true))
            {
                sw.Write(String.Join("",logStrings));                
                //sw.Flush();
                //sw.Dispose();
                sw.Close();
            }
        }

        internal void DisposeStreamWriter()
        {
           // this.sw.Close();
        }
    }
}
