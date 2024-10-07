using LibraryTLCalc;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TLparamsCalc;

namespace ProgramSelectionWorkerService
{
    //Данный класс служит для удобства установления значений экземпляра структуры ConSO
    
    internal class Reflection
    {
        /// <summary>
        /// Задаём новые значения тех полей в структуре, наименования которых есть в списке.
        /// </summary>
        /// <param name="conSO"> Структура </param>
        /// <param name="valuesList">Словарь новых значений (наименование поля - новое значение)</param>
        public static void SetValuesForAllFieldsConSO(ref ClassConSO.ConSO conSO, Dictionary<string, object> valuesList)
        {
            //Получаем список всех полей структуры!
            List<FieldInfo> allFields = conSO.GetType().GetFields().ToList<FieldInfo>();
            //Упаковываем в объект для задания значение (it's a trick! We need it!)
            object boxed = (object)conSO;
            foreach (FieldInfo field in allFields)
            {
                //Если есть значение в списке для данного поля, то устанавливаем это значение
                if (valuesList.TryGetValue(field.Name, out var newValue))
                {
                    if (null != field)// && field.IsPublic)
                    {
                        field.SetValue(boxed, newValue);
                    }
                }
            }
            //Распаковываем изменённый объект в структуру
            conSO = (ClassConSO.ConSO)boxed;
        }

        /// <summary>
        /// Задаём новые значения тех полей в структуре, наименования которых есть в списке.
        /// </summary>
        /// <param name="conSO"> Структура </param>
        /// <param name="valuesList">Словарь новых значений (наименование поля - новое значение)</param>
        public static bool SetValuesForAllFieldsConSO(ref ClassConSO.ConSO conSO, List<string> headerList, List<string> valuesList)
        {
            if (headerList.Count != valuesList.Count)
            {
                return false; 
            }

            //Получаем список всех полей структуры!
            List<FieldInfo> allFields = conSO.GetType().GetFields().ToList<FieldInfo>();
            //Упаковываем в объект для задания значение (it's a trick! We need it!)
            object boxed = (object)conSO;

            //На всякий пожарный:
            for (int i=0; i<valuesList.Count; i++)
                valuesList[i] = valuesList[i].Trim();

            foreach (FieldInfo field in allFields)
            {
                int headerPos = headerList.IndexOf(field.Name);
                //Если есть значение в списке для данного поля, то устанавливаем это значение
                if (headerPos >= 0)
                {
                    if (null != field)// && field.IsPublic)
                    {
                        //int, bool, string, double
                        if (field.FieldType == typeof(int)) field.SetValue(boxed, Convert.ToInt32(valuesList[headerPos]));
                        else if (field.FieldType == typeof(bool))
                        {
                            //if (valuesList[headerPos] == "да" || valuesList[headerPos] == "yes" || valuesList[headerPos] == "true")
                            field.SetValue(boxed, (valuesList[headerPos].ToLower() == "да" || valuesList[headerPos].ToLower() == "yes"
                                || valuesList[headerPos].ToLower() == "true" || valuesList[headerPos] == "1"));// Convert.ToBoolean(valuesList[headerPos]));
                        }
                        else if (field.FieldType == typeof(string)) 
                        {   if (valuesList[headerPos].ToLower() == "нет") 
                                field.SetValue(boxed, "");
                            else  
                                field.SetValue(boxed, valuesList[headerPos]); }
                        else if (field.FieldType == typeof(double)) field.SetValue(boxed, Convert.ToDouble(valuesList[headerPos]));                        
                    }
                }
            }
            //Распаковываем изменённый объект в структуру
            conSO = (ClassConSO.ConSO)boxed;
            return true;
        }

        //Получить ключ для словаря плана сигнализации (если bForPhases то работаем с табличкой $dtTime_0
        internal static string GetKey(string sID) //, string sNapr)
        {
            //В этом случае - в качестве идентификатора для направления будет ID из конфигурации
            string key = "";
            //Нужно чтобы в sID было хотя бы 2 символа - иначе это ошибка
            if (sID.Length <= 1) return key;
            string sNum = sID.Substring(0, sID.Length - 1);

            int lastRazryad = Convert.ToInt32(sID[sID.Length - 1] + "");

            if (lastRazryad == 0 || lastRazryad == 2 || lastRazryad == 3)
            {
                key = "Т";
                key += sNum + ((lastRazryad == 0) ? "1" : lastRazryad);
            }
            else if (lastRazryad == 6 || lastRazryad == 7 || lastRazryad == 8)
            {
                key = "П";
                key += sNum + "1";
            }
            else return "";
            //if (sNapr.Contains('Т')) key = "Т";
            //else if (sNapr.Contains('П')) key = "П";
            //else return "";                

            //key += sNum + (sID[sID.Length-1] == '0' || sID[sID.Length - 1] == '7') ? '1' : sID[sID.Length - 1];
            return key;

            // return sID; // - Берём ID
        }

        /// <summary>
        /// Переводит словарь  интенсивностей (ключ - значение) вида (XX - VALUE), где X - цифра,
        /// в (LXX - VALUE), L - литера типа направления "П" или "Л".
        /// </summary>
        /// <param name="Nij"></param>
        /// <returns></returns>
        internal static Dictionary<string, double> GetNormilizedNij(Dictionary<string, double> Nij)
        {            
            Dictionary<string, double> normNij = new Dictionary<string, double>();
            if (Nij == null || Nij.Count <= 0) return normNij;
            foreach (KeyValuePair<string, double> pair in Nij)
            { 
                normNij.Add(GetKey(pair.Key), pair.Value);
            }
            return normNij; 
        }

        //ID;Напр;Тзав;Ф1;Ф2;фф2;С1;фс1;Ф3;фф3;пф3;С2;фс2;пс2;Ф4;фф4;пф4;Eg;X
        //1;N_BMP;;;;;;;;;;;;;;;;;
        //2;Событие;;;-1;-1;1;1;-2;-2;-2;2;2;2;;;;;1
        //3;Нов.пр;;10;16;19;;;29;32;33;;;;48;51;53;;
        //4;Т.мин;;;;;;;;;;;;;5;;;;
        //5;Длит.Т;;10;6;3;6;3;10;3; 1;10;3; 1;15;3; 2;Цикл;53
        //6;Напр;Тзав;Ф1;Ф2;фф2;С1;фс1;Ф3;фф3;пф3;С2;фс2;пс2;Ф4;фф4;пф4;Eg;X
        //10;Тр-1;4;К;З;З;З;З;З;фж;пк;З;фж;пк;К;;;19;
        //12;Сл-1;3;К;К;;К;;З;фк;;З;фк;;К;;;10;
        //13;Сп-1;3;З;З;З;З;З;З;фк;;З;фк;;К;;;29;
        //20;Тр-2;5;К;К;;К;;К;;;К;;;З;фж;пк;15;
        //27;вП-2;3;К;К;;З;фк;К;;;К;;;К;;;0;
        //30;Тр-3;3;З;З;фж;З;фж;К;;;К;;;К;;;16;
        //40;Тр-4;3;К;К;;К;;К;;;К;;;З;фж;;15;
        //47;вП-4;3;К;К;;К;;К;;;З;фк;;К;;;0;
        /// <summary>
        /// Добавляем строку  в план сигнализации перекрёстка ccr.
        /// </summary>
        /// <param name="ccr">Экземпляр перекрёстка (изменяется). </param>
        /// <param name="headerList">Шапка.</param>
        /// <param name="valuesList">Список значений, которые становятся строкой плана сигнализации</param>        
        public static bool AddLineOfSignalPlan(ref ClassCrossRoad ccr, List<string> valuesList, List<int> badPosList )
        {

            void Replace_DSwLPesh_DSwRPesh(string sKey, string sNapr, ref ClassCrossRoad ccr)
            {
                for (int i=0; i < ccr.akSO.Length; i++)
                {
                    if (ccr.akSO[i].DSwLTr == sNapr)
                    {
                        ccr.akSO[i].DSwLTr = sKey;
                    }
                    if (ccr.akSO[i].DSwRTr == sNapr)
                    {
                        ccr.akSO[i].DSwRTr = sKey;
                    }
                    if (ccr.akSO[i].DSwLPesh == sNapr)
                    {
                        ccr.akSO[i].DSwLPesh = sKey;
                    }
                    if (ccr.akSO[i].DSwRPesh == sNapr)
                    {
                        ccr.akSO[i].DSwRPesh = sKey;
                    }
                    
                }
            }
            List<string> valuesList_NoCallPhases(List<string> valuesList, List<int> badPosList)
            {
                List<string> resList = new List<string>();
                for (int i = 0; i < valuesList.Count; i++) 
                {
                    if (!badPosList.Contains(i))
                    { 
                        resList.Add(valuesList[i].Replace('ф','<').Replace('п', '<'));
                    }
                }
                return resList.GetRange(2, resList.Count - 2);
            }

            //НАЧАЛО ТЕЛА Ф-ИИ ЗДЕСЬ!
            string sKey = "";
            if (valuesList.Count <= 0)
            { 
                return true;
            }
            //Составляем ключ
            sKey = GetKey(valuesList[0]);// , valuesList[1]);
            if (sKey == "") return false; //continue;

            //Здесь в самом экземпляре класса ccr меняем DSwLPesh и DSwRPesh (наприм , вП-2 или вП-4)
            //                                на удобоваримые для алгоритма, например, П21 или  П41
            Replace_DSwLPesh_DSwRPesh(sKey, valuesList[1], ref ccr);

            //Добавляем список сигналов для данного направления с ключом sKey
            ccr.signalPlan.Add(sKey, valuesList_NoCallPhases(valuesList, badPosList));

            return true;
            //КОНЕЦ ТЕЛА Ф-ИИ
        }



        //ccr.signalPlan = new Dictionary<string, List<string>>();
        //ccr.signalPlan.Add("Напр", new List<string>(){ "Тзав", "Ф1", "<", "Ф2", "<", "<<", "Ф3", "<", "<<", "Ф4", "<", "Ф5", "<", "Eg", "'X'" });
        //ccr.signalPlan.Add("Т11", new List<string>() { "3", "К", "", "К", "", "", "З", "<", "<ж", "К", "", "К", "", "19", "" });

        /// <summary>
        ///  
        /// </summary>
        /// <param name="list"></param>
        /// <param name="badPosList"></param>
        public static void NormalizeSHAPKU_SignalPlanStringList(ref List<string> list, ref List<int> badPosList, bool bNeedID = true)
        {
           // badPosList = new List<int>();

            List<string> resList = new List<string>();
            int signLessCount = 0;
            string s = "";

            bool bNeedTake = false;

            for (int i = 0; i < list.Count; i++)
            {
                s = list[i].Trim();

                if (s == "ID")
                {
                    if (bNeedID) resList.Add(s);
                }
                //для $dtTime_0
                else if (s == "Прогр")
                /* || s == "Смещ" || s == "Зан" */
                { 
                    resList.Add(s);                 
                } //continue;
                //$dtPhases_0
                else if (s == "Напр")
                {
                    resList.Add(s);
                }
                else if (s == "Тзав")
                {
                    resList.Add(s);
                }
                else if (s[0] == 'Ф')
                {
                    signLessCount = 0;
                    resList.Add(s);
                }
                //6;Напр;Тзав;Ф1;Ф2;фф2;С1;фс1;Ф3;фф3;пф3;С2;фс2;пс2;Ф4;фф4;пф4;Eg;X
                //10;Тр-1;4;К;З;З;З;З;З;фж;пк;З;фж;пк;К;;;19;
                else if ((s[0] == 'ф' && s[1] == 'ф') || (s[0] == 'п' && s[1] == 'ф'))
                {
                    signLessCount++;
                    resList.Add(string.Concat(Enumerable.Repeat("<", signLessCount)));
                }
                //else if ((s[0] == 'ф' && s[1] == 'ж') || (s[0] == 'п' && s[1] == 'к'))
                //{                    
                //    resList.Add("");
                //}
                else if ((s[0] == 'С' && Char.IsDigit(s[1])) || (s[0] == 'ф' && s[1] == 'с') || (s[0] == 'п' && s[1] == 'с'))
                {
                    badPosList.Add(i);
                    continue;
                }
                else // if (s == "Напр" || s == "Eg" || s == "X" )
                {   
                    resList.Add(s);
                }

                
            }
            //Для передачи в качестве результата функции
            list = resList;
        }

    }
}
