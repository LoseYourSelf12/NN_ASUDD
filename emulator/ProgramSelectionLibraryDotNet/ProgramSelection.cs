using System;
using System.Collections.Generic;
//using System.Linq;
using System.Net.Mail;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ProgramSelectionLibraryDotNet
{
    public class ProgramSelection
    {

        /// <summary>
        /// Вариант с алгоритмом минимальных коэффициентов:
        /// Структура определяет связть между DIL-объектом и продолжительностью зелёного сигнала фазы.
        /// </summary>
        /// 
        public struct IntensityFromTo
        { 
            public int from;
            public int to;
        }

        public struct ProgramDilDur
        {   
            ////номер объекта детектирования
            //public int id_o { get; set; }
            //продолжительность зелёной фазы
            public int dur { get; set; }
            //коэффициент веса от общего времени горения зелёного
            public double duratCoef { get; set; }
            ////направление
            //public string sDir { get; set; }
            
            //Разница коэф-тов (зелёных сигналов и интенсивностей)
            public double deviationCoef { get; set; }

            ////Флаг необходимости проверки ??????????????????
            //public bool bCheck { get; set; }
        }

        /// <summary>
        /// Диапазон светофорной программы. 
        /// </summary>
        public struct ProgramDiapason
        {   //номер_программы
            public int pr_n;
            //номер детектора
            public int id_nd { get; set; }
            //номер объекта детектирования
            public int id_o { get; set; }
            //Флаг необходимости проверки
            public bool bCheck { get; set; }
            //Начальное значение диапазона интенсивностей для конкретного нейродетектора (с номером id_o)
            public int fromIntense { get; set; }
            //Конечное значение диапазона интенсивностей для конкретного нейродетектора (с номером id_o)
            public int toIntense { get; set; }
        }

        /// <summary>
        /// Ключ к списку диапазонов интенсиваностей светофорной программы
        /// </summary>
        public struct ProgramListKey
        {
            public int id_nd;
            public int pr_n;
            //!!!
            public int numLine;
            //public int id_o;
        }
        //internal int id_o;            
        public struct MeanKey
        {
            public int id_nd;
            public int id_o;
        }

        /// <summary>
        /// Измерение детектора. 
        /// E1 - полученное с нейродетектора за tim секунд.
        /// </summary>
        public struct DetectorMeasuring
        {
            //Номер детектора
            public int id_nd { get; set; }
            //Номер объекта детектирования
            public int id_o { get; set; }
            //Длительность интервала подсчёта, секунд
            public int tim { get; set; }
            ////ПОКА НЕ БРАЛ
            ////секунда от начала интервала заданного PTI
            //public int beg { get; set; }

            //направление - L, T, R, O, P, W
            public char direction { get; set; }
            //подход
            public int approach { get; set; }
            //Наименование
            public string name { get; set; }
            //расчётное значение - количество ТС "прямого" движения
            public int E1 { get; set; }
            //Значение приведенное к ЕД/Ч!
            public double E1PerHour { get; set; }
        }

        //public struct AverageL
        //{ 
        //}

        ///// <summary>
        ///// Вычислить средневзвешенное значение по словарю интенсивностей и количеству детекторов 
        ///// </summary>
        ///// <param name="detectorValues"> Словарь интенсивностей, ключ - номер цикла, Значение - список значение детектора с номером - Ключ </param>
        ///// <param name="detectorCount"> Число детекторов </param>
        ///// <param name="weightStep"> Весовой шаг </param>
        ///// <returns></returns>
        //public static List<double> GetWeightedAverageList(Dictionary<int, List<DetectorMeasuring>> detectorValues, int detectorCount, double weightStep)
        //{
        //    List<double> listWeightedMeans = new List<double>();

        //    //Добавляем новые элементы в список, в количестве detectorCount
        //    for (int i = 0; i < detectorCount; i++)
        //    {
        //        listWeightedMeans.Add(0);
        //    }

        //    //вычисляем начальный вес 
        //    double curWeight = weightStep * detectorValues.Count;
        //    double weightsSum = 0;
        //    foreach (var listDetectorValues in detectorValues)
        //    //for (int i=0; i < detectorValues.Count; i++)
        //    {
        //        List<DetectorMeasuring> list = listDetectorValues.Value;
        //        weightsSum += curWeight;
        //        for (int j = 0; j < list.Count; j++)
        //        {
        //            listWeightedMeans[j] += (list[j] * curWeight);
        //        }
        //        curWeight -= weightStep;
        //    }

        //    for (int i = 0; i < listWeightedMeans.Count; i++)
        //    {
        //        listWeightedMeans[i] = listWeightedMeans[i] / weightsSum;
        //    }

        //    return listWeightedMeans;
        //}

        /// <summary>
        /// Вычислить среднее значение по словарю интенсивностей и количеству детекторов
        /// </summary>
        /// <param name="detectorValues"> Словарь интенсивностей, ключ - номер детектора, Значение - список значение детектора с номером - Ключ </param>
        /// <param name="objectsCount"> Количество детекторов </param>
        /// <returns></returns>
        public List<double> GetArithmeticMeanList(Dictionary<int, List<DetectorMeasuring>> detectorValues, int objectsCount)
        {
            // detectorValues.Count - строк в таблице
            // detectorCount - столбцов в таблице

            List<double> listArithMeans = new List<double>();

            //Добавляем новые элементы в список, в количестве detectorCount
            for (int i = 0; i < objectsCount; i++)
            {
                listArithMeans.Add(0);
            }

            foreach (var listDetectorValues in detectorValues)
            //for (int i=0; i < detectorValues.Count; i++)
            {
                List<DetectorMeasuring> list = listDetectorValues.Value;
                for (int j = 0; j < list.Count; j++)
                {
                    listArithMeans[j] += list[j].E1PerHour;
                }
            }

            for (int i = 0; i < listArithMeans.Count; i++)
            {
                listArithMeans[i] = listArithMeans[i];// / detectorValues.Count;
            }

            return listArithMeans;
        }

        /// <summary>
        /// Вычислить среднее значение по словарю интенсивностей и количеству детекторов
        /// </summary>
        /// <param name="detectorValues"> Словарь интенсивностей, ключ - номер детектора, Значение - список значение детектора с номером - Ключ </param>
        /// <returns></returns>
        //Ключ внешнего словаря - id_n, ключ внутр. словаря - id_o, Значение внутр. словаря - среднее значение для объекта id_o
        //по всем циклам 

        //Всё таки сходил я в Сочинский Океанриум. Жаль, только снимать через стекло не всегда можно успешно, но сделали интересно, было на что посмотреть.
        public Dictionary<MeanKey, double> GetArithmeticMeanDict(List<DetectorMeasuring> detectorValuesList)
        //int neuroDetCount, int detObjectCount)
        {
            Dictionary<MeanKey, double> sumValues = new Dictionary<MeanKey, double>();
            Dictionary<MeanKey, double> arithMeans = new Dictionary<MeanKey, double>();

            //Словарь количеств
            Dictionary<MeanKey, int> dictCounts = new Dictionary<MeanKey, int>();

            //Проходя словарь, получаем суммарные значения
            foreach (var curDetMeasure in detectorValuesList)
            {
                //Формируем ключ
                MeanKey mk = new MeanKey();
                mk.id_nd = curDetMeasure.id_nd;
                mk.id_o = curDetMeasure.id_o;

                //Если нет ключа то формируем его
                if (!sumValues.ContainsKey(mk))
                {
                    sumValues.Add(mk, 0);
                    dictCounts.Add(mk, 0);
                }

                //Накручиваем общее суммарное значение
                sumValues[mk] = sumValues[mk] + curDetMeasure.E1PerHour;
                dictCounts[mk] = dictCounts[mk] + 1;
            }


            //Все суммарные значения делим на соответствующие количества
            foreach (var curArithMeans in sumValues)
            //for (int i=0; i< dictArithMeans.Count; i++)
            {
                //ar curArithMeans = dictArithMeans[i];
                //Формируем ключ
                MeanKey mk = new MeanKey();
                mk.id_nd = curArithMeans.Key.id_nd;
                mk.id_o = curArithMeans.Key.id_o;
                //Добавляем среднее арифметическое
                arithMeans.Add(mk, (double)(sumValues[mk] / dictCounts[mk]));
            }

            return arithMeans;
        }
        /// <summary>
        /// Вычислить среднее значение по словарю интенсивностей и количеству детекторов
        /// </summary>
        /// <param name="detectorValuesList"> Список значений детектора DetectorMeasuring</param>
        /// <returns> Словарь средних арифметических. Ключ - номер DIL-объекта (id_o). 
        ///           Значение - среднее арифметическое по каждому DIL-объекту </returns>
        //Ключ внешнего словаря - id_n, ключ внутр. словаря - id_o, Значение внутр. словаря - среднее значение для объекта id_o
        //по всем циклам 
        public Dictionary<int, double> GetArithmeticMean_Dict_IdOb_Aver(List<DetectorMeasuring> detectorValuesList)
        //int neuroDetCount, int detObjectCount)
        {
            Dictionary<int, double> sumValues = new Dictionary<int, double>();
            Dictionary<int, double> arithMeans = new Dictionary<int, double>();

            //Словарь количеств
            Dictionary<int, int> dictCounts = new Dictionary<int, int>();

            //Проходя словарь, получаем суммарные значения
            foreach (var curDetMeasure in detectorValuesList)
            {
                ////Формируем ключ
                //MeanKey mk = new MeanKey();
                //mk.id_nd = curDetMeasure.id_nd;
                //mk.id_o = curDetMeasure.id_o;

                //Если нет ключа то формируем его
                if (!sumValues.ContainsKey(curDetMeasure.id_o))
                {
                    sumValues.Add(curDetMeasure.id_o, 0);
                    dictCounts.Add(curDetMeasure.id_o, 0);
                }

                //Накручиваем общее суммарное значение
                sumValues[curDetMeasure.id_o] = sumValues[curDetMeasure.id_o] + curDetMeasure.E1PerHour;
                dictCounts[curDetMeasure.id_o] = dictCounts[curDetMeasure.id_o] + 1;
            }


            //Все суммарные значения делим на соответствующие количества
            foreach (var curArithMeans in sumValues)
            //for (int i=0; i< dictArithMeans.Count; i++)
            {
                //ar curArithMeans = dictArithMeans[i];
                ////Формируем ключ
                //MeanKey mk = new MeanKey();
                //mk.id_nd = curArithMeans.Key.id_nd;
                //mk.id_o = curArithMeans.Key.id_o;
                //Добавляем среднее арифметическое
                arithMeans.Add(curArithMeans.Key, (double)(sumValues[curArithMeans.Key] / dictCounts[curArithMeans.Key]));
            }

            return arithMeans;
        }

        ///// <summary>
        ///// Получить список подходящих номеров программ ТЗПП 
        ///// </summary>
        ///// <param name="trafficLightsPrograms"> Словарь програм. Ключ - номер программы. Значение - список программ ( структур Program, содержащих значения bCheck,fromIntense,toIntense )  </param>
        ///// <param name="averageValuesList"> Список средних (средневзвешенных) значений </param>
        ///// <returns> Список подходящих номеров программ ТЗПП </returns>
        ///// 
        //public List<ProgramKey> GetSuitableProgramsList(Dictionary<ProgramKey, List<ProgramSelection.Program>> trafficLightsPrograms, List<double> averageValuesList)
        //{
        //    List<ProgramKey> resList = new List<ProgramKey>();
        //    int i;
        //    bool isCommonMatch;
        //    bool isCurMatch;
        //    bool bAreWorkingPrograms;
        //    foreach (var curTrProgramList in trafficLightsPrograms)
        //    {
        //        i = 0;
        //        //Здесь берём де-факто true, просто страховка от пустого списка
        //        isCommonMatch = (curTrProgramList.Value.Count > 0);
        //        bAreWorkingPrograms = false;
        //        foreach (var Program in curTrProgramList.Value)
        //        {
        //            // Это чтобы отбросить тот случай, когда везде НЕ проставлен флаг проверки
        //            bAreWorkingPrograms = bAreWorkingPrograms || Program.bCheck;
        //            //Program.bCheck - если false, ЗНАЧИТ НЕ НАДО ПРОВЕРЯТЬ, то есть - "берём"!
        //            isCurMatch = (!Program.bCheck ||
        //                (Program.fromIntense <= averageValuesList[i] && averageValuesList[i] < Program.toIntense));

        //            isCommonMatch = isCommonMatch && isCurMatch;
        //            i++;
        //            if (!isCommonMatch) break;
        //        }

        //        if (isCommonMatch && bAreWorkingPrograms) resList.Add(curTrProgramList.Key);
        //    }
        //    return resList;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="intenseCoefsDict"></param>
        /// <param name="numPhaseToDils"></param>
        /// <param name="phasesCount"></param>
        /// <returns></returns>
        private double[]? GetMaxIntenseCoefsForEveryPhase(Dictionary<int, double> averageValuesDict, Dictionary<int, List<Dil_LanesCount_CorrCoef>> numPhaseToDils, int phasesCount, ref List<string> logStrings)
        {
            //Если сумма всех значений средних интенсивностей меньше или равно 0, то выходим
            double averageValuesSum = averageValuesDict.Values.Sum();
            if (averageValuesSum <= 0) return null;
            
            Dictionary<int, double> intenseCoefsDict = new Dictionary<int, double>();

            foreach (var curIntensePair in averageValuesDict)
            {
                //<id_o, intenseCoef>
                intenseCoefsDict.Add(curIntensePair.Key, (double)curIntensePair.Value / averageValuesSum);
            }

            //Инициализируем массив максимальных интенсивностей в рамках каждой фазы
            double[] maxIntenseCoefs = new double[phasesCount + 1]; // +1 - чисто из-за человеческого фактора =)

            try
            {
                //Началь. присвоения:
                for (int i = 0; i <= phasesCount; i++) maxIntenseCoefs[i] = 0;

                double curMaxIntenseCoef = 0;

                //Перебираем все фазы с первой по phasesCount
                for (int jPhase = 1; jPhase <= phasesCount; jPhase++)
                {
                    //Получаем список ДИЛ объектов соответствующих фазе
                    if (numPhaseToDils.TryGetValue(jPhase, out List<Dil_LanesCount_CorrCoef>? DilWithTrLanesForCurPhase_list))
                    {
                        curMaxIntenseCoef = 0;
                        //Перебираем каждый ДИЛ объект 
                        foreach (Dil_LanesCount_CorrCoef curDilWithTrLane in DilWithTrLanesForCurPhase_list)
                        {
                            //...и выбираем максимальный коэффициент 
                            if (intenseCoefsDict.TryGetValue(curDilWithTrLane.dil, out double curIntenseCoef))
                            {
                                double curIntenseCoefForLANESCOUNT = (double)((curIntenseCoef)/ curDilWithTrLane.TrLanesCount) ;
                                if (curIntenseCoefForLANESCOUNT > curMaxIntenseCoef)
                                {
                                    curMaxIntenseCoef = curIntenseCoefForLANESCOUNT;
                                }
                            }
                        }
                        maxIntenseCoefs[jPhase] = curMaxIntenseCoef;
                    }
                }

                //!!! Пересчитываем коэффициенты в расчёте на доли от 100%
                double sumMaxIntense = maxIntenseCoefs.Sum();
                for (int jPhase = 1; jPhase < maxIntenseCoefs.Count(); jPhase++)
                {
                    maxIntenseCoefs[jPhase] = maxIntenseCoefs[jPhase] / sumMaxIntense;
                }

            }
            catch (Exception e) {
                logStrings.Add("!!!" + e.TargetSite + " " + e.Source + " " + e.Message);
            }
            return maxIntenseCoefs;
        }
        //<id_o, intenseCoef>                     <Phase_№, DILs_list>
        //private List<double> GetDiffList(ProgramDilDur[,] arrProgsDils, Dictionary<int, double> intenseCoefsDict, Dictionary<int, List<int>> numPhaseToDils)
        //{
        //    int strCount = arrProgsDils.GetLength(0); 
        //    int colCount = arrProgsDils.GetLength(1);
        //    double curMaxIntenseCoef = 0;
        //    for (int iPr_n = 0; iPr_n < strCount; iPr_n++)
        //    {
        //        for (int jPhase = 1; jPhase < colCount; jPhase++)
        //        {
        //            if (numPhaseToDils.TryGetValue(jPhase, out List<int> DILsForCurPhase_list))
        //            {
        //                curMaxIntenseCoef = 0;
        //                foreach (int curDil in DILsForCurPhase_list)
        //                {
        //                    if (intenseCoefsDict.TryGetValue(curDil, out double curIntenseCoef))
        //                    {
        //                        if (curIntenseCoef > curMaxIntenseCoef) curMaxIntenseCoef = curIntenseCoef;
        //                        arrProgsDils[iPr_n,jPhase].coefDiff = curIntenseCoef;
        //                    }
        //                }
        //            }
        //            //Math.Abs(arrProgsDils[iPr_n, jPhase].greenCoef - )

        //        }
        //    }
        //}

        ///// <summary>
        ///// Метод возвращает список подходящих номеров программ ТЗПП, полученный на основании метода минимальных коэфициентов.
        ///// Общая суть метода:
        ///// 1) Посчитать сумму продолжительностей зелёных сигналов, посчитать отношения каждой к этой сумме.
        ///// 2) Посчитать сумму интенсивностей, посчитать отношения каждой к сумме интенсивностей.
        ///// 3) Сравнить коэффициенты 
        ///// </summary>
        ///// <param name="trafficLightsPrograms"></param>
        ///// <param name="averageValuesDict"></param>
        ///// <returns></returns>
        //public List<int> GetSuitableProgramBasedOnMinCoefsOLD(
        //           // Dictionary<int,List<ProgramDilGreen>> dilPrograms, //<номер программы, список данных программы>
        //            Dictionary<int,List<int>> numPhaseToDils, //<номер фазы,cписок id_o(DIL)> слрь определяет очередность DIL-объектов(т.е. и направлений тоже), как они идут в массиве ProgramDilDur                    
        //            int[,] arrPhaseDurs, //Двумерный массив продолжительностей  сигналов
        //            Dictionary<int, double> averageValuesDict)
        //{
        //    //Dictionary<ProgramListKey, List<ProgramSelection.ProgramDiapason>> по сути <pr_n, >
        //    //Dictionary<int, double> averageValuesDict  - Словарь<№ объекта, среднее значение интенсивности>
        //    List<int> curResList = new List<int>();

        //    Dictionary<int, double> resDeviationCoefDict = new Dictionary<int, double>();            

        //    int strCount = arrPhaseDurs.GetLength(0); // число строк 
        //    int colCount = arrPhaseDurs.GetLength(1); // столбцов

        //    //Расчитываем коэфициенты для интенсивностей Dictionary<int, double> averageValuesDict
        //    Dictionary<int, double> intenseCoefsDict = new Dictionary<int, double>();//будем писать:<id_o, (cur/sumIntense)>
        //    double averageValuesSum = averageValuesDict.Values.Sum();



        //    foreach (var curIntensePair in averageValuesDict)
        //    {                                   //<id_o, intenseCoef>
        //        intenseCoefsDict.Add(curIntensePair.Key, (double)curIntensePair.Value / averageValuesSum);
        //    }
        //    //Получаем список максимальных коэф-тов интенсивности для каждой фазы
        //    double[] maxIntenseCoefsForEveryPhase = GetMaxIntenseCoefsForEveryPhase(intenseCoefsDict, numPhaseToDils, colCount);

        //    ProgramDilDur[,] arrProgsDils = new ProgramDilDur[strCount, colCount];


        //    double minSumDeviationCoef = 0;
        //    //----
        //    //Общая продолжительность всех фаз для тек программы
        //    double curCommonDur; double curSumDeviationCoef;
        //    for (int iPr_n = 0; iPr_n < strCount; iPr_n++)
        //    {
        //        //Расчитываем Общую продолжительность ДЛЯ ТЕК ПРОГРАММЫ
        //        curCommonDur = 0;
        //        for (int jPhase = 1; jPhase <= colCount; jPhase++)
        //        {
        //            curCommonDur += arrPhaseDurs[iPr_n, jPhase];
        //        }
        //        curSumDeviationCoef = 0;
        //        minSumDeviationCoef = double.MaxValue;
        //        //Находим коэф-та разницы
        //        for (int jPhase = 1; jPhase <= colCount; jPhase++)
        //        {
        //            //переписываем продолжительность зелёного в arrProgsDils
        //            arrProgsDils[iPr_n, jPhase].dur = arrPhaseDurs[iPr_n, jPhase];

        //            //считаем коэфф-ты - отношение продолжительности 
        //            arrProgsDils[iPr_n, jPhase].duratCoef = (double)(arrProgsDils[iPr_n, jPhase].dur / curCommonDur);
        //            //получаем разницу коэф-тов по АБСОЛЮТНОЙ ВЕЛИЧИНЕ!
        //            arrProgsDils[iPr_n, jPhase].deviationCoef = Math.Abs(maxIntenseCoefsForEveryPhase[jPhase] - arrProgsDils[iPr_n, jPhase].duratCoef);
        //            curSumDeviationCoef += arrProgsDils[iPr_n, jPhase].deviationCoef;
        //        }
        //        //Добавляем коэф-т отклонения в результирующий список (номер Фазы = индексу в массиве)
        //        resDeviationCoefDict.Add(iPr_n,curSumDeviationCoef);
        //        if (minSumDeviationCoef > curSumDeviationCoef) minSumDeviationCoef = curSumDeviationCoef;
        //    }

        //    //Из всех выбираем ВСЕ те программы, которые с минимальным значением коэф-та отклонения
        //    foreach (var keypair in resDeviationCoefDict)
        //    {
        //        if (keypair.Value == minSumDeviationCoef)
        //        { curResList.Add(keypair.Key); }
        //    }

        //    //----
        //    return curResList;
        //}

        //internal int GetCountInDictPhasesDurs(Dictionary<int, List<int>> dictPhasesDurs)
        //{ 
        //    int count = 0;
        //    foreach (var keypair in dictPhasesDurs)
        //    {
        //         count = (((List<int>)keypair.Value).Count);
        //        break;
        //    }
        //    return count;
        //}

        internal List<int> GetSuitableProgramBasedOnComIntensity(Dictionary<int, IntensityFromTo> progIntensitiesFromTo, //Nprog - (From,To)
                                                                 Dictionary<int, double> averageValuesDict, //id_o-(aveIntens)
                                                                 Dictionary<int, List<Dil_LanesCount_CorrCoef>> ?numPhaseToDils,
                                                                          //Nphase-(List<id_o,lanes,corrCoef>)
                                                                 ref List<string> logStrings)
        {
            logStrings.Add("Вариант метода - III. Выбираем программу на основании общей интенсивности");
            List<int> resProgList = new List<int>();
            List<string> sCurMaxAveVals = new List<string>();
            if (numPhaseToDils == null) return resProgList;

            double curMaxAveVal;
            double sumIntense = 0;
            foreach (var curPhaseDLC in numPhaseToDils)
            {
                curMaxAveVal = 0;
                //logStrings.Add($"Фаза:{curPhaseDLC.Key}");
                List <Dil_LanesCount_CorrCoef> curList = ((List<Dil_LanesCount_CorrCoef>)curPhaseDLC.Value);                               
                //Находим максимальное среди тех, которые "задействуются" в рамках одной фазы
                foreach (Dil_LanesCount_CorrCoef dlc in curList)
                {                    
                    if (averageValuesDict.TryGetValue(dlc.dil,out double curAveVal))
                    {
                       // curAveVal = (double)(curAveVal / dlc.TrLanesCount);
                        if (curMaxAveVal < curAveVal) curMaxAveVal = curAveVal;
                    }
                }
                logStrings.Add($"Фаза {curPhaseDLC.Key}: Макс.средняя интенсивность: {curMaxAveVal} ед/ч");
                //sCurMaxAveVals.Add($"{dlc.} - {curMaxAveVal}");

                sumIntense += curMaxAveVal;
            }
            logStrings.Add($"СУММАРНАЯ(!) интенсивность средних: {sumIntense} ед/ч");
            //logStrings.Add(
            //смотрим в какой интервал попали
            foreach (var curProgIFromTo in progIntensitiesFromTo)
            {
                logStrings.Add($"Диапазоны интенсивностей для программы №{curProgIFromTo.Key}: [{curProgIFromTo.Value.from}-{curProgIFromTo.Value.to}]");
                if (curProgIFromTo.Value.from <= sumIntense && sumIntense < curProgIFromTo.Value.to)
                {
                    resProgList.Add(curProgIFromTo.Key);
                }
            }
            return resProgList;
        }

        internal List<int> GetSuitableProgramBasedOnMinCoefs(
                    // Dictionary<int,List<ProgramDilGreen>> dilPrograms, //<номер программы, список данных программы>
                    Dictionary<int, List<Dil_LanesCount_CorrCoef>> ?numPhaseToDils, //<номер фазы,cписок id_o(DIL)> слрь определяет очередность DIL-объектов(т.е. и направлений тоже), как они идут в массиве ProgramDilDur                    
                    Dictionary<int, List<int>> dictPhasesDurs, //для каждой программы - продолжительности 
                    Dictionary<int, double> averageValuesDict,  //id_o, value
                    ref List<string> logStrings                 //Список лога
                    )
        {
            List<int> curResList = new List<int>();
            if (numPhaseToDils == null) return curResList;
            try
            {
                logStrings.Add("Enter - GetSuitableProgramBasedOnMinCoefs");
                //Dictionary<ProgramListKey, List<ProgramSelection.ProgramDiapason>> по сути <pr_n, >
                //Dictionary<int, double> averageValuesDict  - Словарь<№ объекта, среднее значение интенсивности>
                int phasesCountInDictPhasesDursCOUNT = numPhaseToDils.Count; // GetCountInDictPhasesDurs( dictPhasesDurs);//((List<int>)dictPhasesDurs.FirstOrDefault().Value).Count;
                logStrings.Add($"12) Entering the metod based on MIN Coefs" + Environment.NewLine +
                   $"dictPhasesDurs.Count={dictPhasesDurs.Count}; numPhaseToDils.Count={numPhaseToDils.Count};Кол-во пр. в программах {phasesCountInDictPhasesDursCOUNT}!");

                if (dictPhasesDurs.Count <= 0 || phasesCountInDictPhasesDursCOUNT <= 0)
                { return new List<int> { }; }

                int strCount = dictPhasesDurs.Count; // число элементов в словаре, то есть число программ!

                int colCount = phasesCountInDictPhasesDursCOUNT; // число строк в первом списке(впрочем, как и в остальных) - т.е число фаз!

                logStrings.Add($"13) colCount={colCount}; strCount ={strCount} ");
                //Расчитываем коэфициенты для интенсивностей Dictionary<int, double> averageValuesDict
                Dictionary<int, double> intenseCoefsDict = new Dictionary<int, double>(); //будем писать:<id_o, (cur/sumIntense)>

                //Получаем список максимальных коэф-тов интенсивности для каждой фазы
                logStrings.Add($"14) intenseCoefsDict={String.Join("/", intenseCoefsDict.Values)} ");
               
                double[]? maxIntenseCoefsForEveryPhase = GetMaxIntenseCoefsForEveryPhase(averageValuesDict, numPhaseToDils, colCount, ref logStrings);
                if (maxIntenseCoefsForEveryPhase == null || maxIntenseCoefsForEveryPhase.Count() <= 0) return curResList;


                ////ProgramDilDur[,] arrProgsDils = new ProgramDilDur[strCount, colCount];
                //Dictionary<int, ProgramDilDur> dictProgsDils = new Dictionary<int, ProgramDilDur>();

                double minSumDeviationCoef = 0;
                //----
                //Общая продолжительность всех фаз для тек программы
                //List<double> listCommonDur = new List<double> { };
                double curCommonDur;
                double curSumDeviationCoef;

                Dictionary<int, double> resDeviationCoefDict = new Dictionary<int, double>();

                foreach (var curProg in dictPhasesDurs)
                //for (int iPr_n = 0; iPr_n < strCount; iPr_n++)
                {
                    //Расчитываем Общую продолжительность ДЛЯ ТЕК ПРОГРАММЫ
                    curCommonDur = 0;
                    foreach (int curPhaseDur in (List<int>)curProg.Value)
                    {
                        //listCommonDur.Add(curPhaseDur); 
                        curCommonDur += curPhaseDur;
                    }

                    //ЕСЛИ СУММА ВСЕХ ПРОДОЛЖИТЕЛЬНОСТЕЙ ДЛЯ ОДНОЙ ИЗ ПРОГРАММ МЕНЬШЕ РАВНА 0, то пропускаем
                    if (curCommonDur <= 0)
                    {
                        return curResList;
                    }

                    curSumDeviationCoef = 0;
                    minSumDeviationCoef = double.MaxValue;

                    int jPhase = 0;

                    logStrings.Add($"15)Deviation Coefs Calculation cycle... " + String.Join(",", (List<int>)curProg.Value));
                    //Находим коэф-та разницы
                    foreach (int curPhaseDur in (List<int>)curProg.Value)
                    //    for (int jPhase = 1; jPhase <= colCount; jPhase++)
                    {
                        jPhase++;
                        ProgramDilDur arrProgsDils = new ProgramDilDur();
                        //переписываем продолжительность зелёного в arrProgsDils
                        arrProgsDils.dur = curPhaseDur;
                        //считаем коэфф-ты - отношение продолжительности 
                        arrProgsDils.duratCoef = (double)(curPhaseDur / curCommonDur);
                        //получаем разницу коэф-тов по АБСОЛЮТНОЙ ВЕЛИЧИНЕ!
                        arrProgsDils.deviationCoef = Math.Abs(maxIntenseCoefsForEveryPhase[jPhase] - arrProgsDils.duratCoef);

                        curSumDeviationCoef += arrProgsDils.deviationCoef;
                        ////                Номер программы, список 
                        //dictProgsDils.Add(curProg.Key,arrProgsDils);
                    }
                    //Добавляем коэф-т отклонения в результирующий список (НОМЕР ПРОГРАММЫ -> СУММАРНЫЙ КОЭФ-Т ОТКЛОНЕНИЯ)
                    resDeviationCoefDict.Add(curProg.Key, curSumDeviationCoef);
                    // if (minSumDeviationCoef > curSumDeviationCoef) minSumDeviationCoef = curSumDeviationCoef;

                }
                //Выбираем программу с наименьшим суммарным отклонением
                minSumDeviationCoef = resDeviationCoefDict.Values.Min();
                logStrings.Add($"16) Select TLProgram numbers with MIN Coefs");

                foreach (var keypair in resDeviationCoefDict)
                {
                    if (keypair.Value == minSumDeviationCoef)
                    {
                        curResList.Add(keypair.Key);
                    }
                }
                //curResList = resDeviationCoefDict.Where(s => s.Value.Equals(minSumDeviationCoef)).Select(s => s.Key).ToList();

              
            }
            catch (Exception e)
            {
                logStrings.Add("!!!" + e.TargetSite + " " + e.Source + " " + e.Message);
            }
            //return new List<int>();
            return curResList;
        }

        /// <summary>
        /// Получить список подходящих номеров программ ТЗПП
        /// </summary>
        /// <param name="trafficLightsPrograms"> Словарь программ. Ключ - ProgramListKey. Значение - список программ ( структур Program, содержащих значения bCheck,fromIntense,toIntense )  </param>
        /// <param name="averageValuesDict"> Словарь средних значений. КЛЮЧ - НОМЕР ОБЪЕКТА ДЕТЕКТИРОВАНИЯ id_o. Значение - среднее для объекта детектирования. </param>
        /// <returns> Список номеров программ. </returns>
        public List<int> GetSuitableProgramsList(Dictionary<ProgramListKey, List<ProgramSelection.ProgramDiapason>> trafficLightsPrograms, Dictionary<int, double> averageValuesDict)
        {
            ////Результирующий словарь списков подходящих программ, ключ - идентификатор нейродетектора id_n
            //Dictionary<int, List<int>> resDict = new Dictionary<int, List<int>>();

            List<int> curResList = new List<int>();

            if (averageValuesDict == null || averageValuesDict.Count == 0) { return curResList; }

            // int i;
            bool isCommonMatch;
            bool isCurMatch;
            bool bAreWorkingPrograms;
            //Были ли вообще ПОДХОДЯЩИЕ программы для тек. id_o
            bool bWasPrograms;

            //MeanKey mk = new MeanKey();
            //mk.id_nd = 0;
            //mk.id_o = 0;
            int lastId_nd = 0;
            //Идём по строкам (программам)
            foreach (var curTrProgramDict in trafficLightsPrograms)
            {

                //ProgramListKey - curTrProgramDict.Key
                //curTrProgramDict.Key.id_nd
                //curTrProgramDict.Key.pr_n

                //public struct Program (curTrProgramDict.Value - List<Program>)
                //public int pr_n;
                //public int id_nd 
                //public int id_o 
                //----------------------------------------------------

                //Если сменился номер детектора, то записываем список выбранных программ для предыдущего
                //На первой итерации цикла, когда mk.id_nd = 0, ничего не записываем :
                //if (lastId_nd > 0 && lastId_nd != curTrProgramDict.Key.id_nd)
                //{
                //    ////if (!bWasPrograms)
                //    ////{
                //    ////    Console.WriteLine($"Для нейродетектора с №{curTrProgramDict.Key.id_nd} не обнаружены значения интенсивностей!");
                //    ////}
                //    //resDict.Add(lastId_nd, curResList);
                //    //curResList = new List<int>();
                //    bWasPrograms = false;
                //}
                bWasPrograms = false;
                lastId_nd = curTrProgramDict.Key.id_nd;
                //Здесь берём де-факто true, просто страховка от пустого списка
                isCommonMatch = (curTrProgramDict.Value.Count > 0);
                bAreWorkingPrograms = false;

                // Идём по диапазонам ВНУТРИ текущей светофорной ПРОГРАММЫ
                foreach (var curProgramDiapason in (List<ProgramDiapason>)curTrProgramDict.Value)
                {
                    //MeanKey mk = new MeanKey();
                    //mk.id_nd = curTrProgramDict.Key.id_nd;
                    //mk.id_o = curProgramDiap.id_o;                    
                    if (averageValuesDict.TryGetValue(curProgramDiapason.id_o, out double curAverageValue))
                    {
                        // Это чтобы отбросить тот случай, когда везде НЕ проставлен флаг проверки
                        bAreWorkingPrograms = bAreWorkingPrograms || curProgramDiapason.bCheck;
                        //Program.bCheck - если false, ЗНАЧИТ НЕ НАДО ПРОВЕРЯТЬ, то есть - "берём"!
                        isCurMatch = (!curProgramDiapason.bCheck ||
                            (curProgramDiapason.fromIntense <= curAverageValue && curAverageValue < curProgramDiapason.toIntense));
                        isCommonMatch = isCommonMatch && isCurMatch;
                        bWasPrograms = true;
                    }
                    else
                    {
                        //??? Остался вопрос - а что делать если среди сообщений нейродетектора
                        //не было ни одного значения для тек. объекта id_o?? 
                        Console.WriteLine($"Для №объекта - {curProgramDiapason.id_o} - не обнаружены средние значения интенсивностей!");
                    }
                    //Если было хотя бы одно непопадание в диапазон (при bCheck = true), то переходим к след диапазону
                    if (!isCommonMatch) break;
                }
                // && bAreWorkingPrograms - убрал из условия, чтобы если даже на всех детект. объектах bCheck - false , то всё равно берём
                if (isCommonMatch && bWasPrograms && !curResList.Contains(curTrProgramDict.Key.pr_n))
                { 
                    curResList.Add(curTrProgramDict.Key.pr_n);
                    //!Сделал для скорости:
                    return curResList; 
                }
            }
            return curResList;
        }

        ///// <summary>
        ///// Получить список подходящих номеров программ ТЗПП
        ///// </summary>
        ///// <param name="trafficLightsPrograms"> Словарь програм. Ключ - идентификатор нейродетектора id_n. Значение - список программ ( структур Program, содержащих значения bCheck,fromIntense,toIntense )  </param>
        ///// <param name="averageValuesList"> Список средних (средневзвешенных) значений </param>
        ///// <returns></returns>
        //public Dictionary<int, List<int>> GetSuitableProgramsDict(Dictionary<ProgramListKey, List<ProgramSelection.ProgramDiapason>> trafficLightsPrograms, Dictionary<MeanKey, double> averageValuesDict)
        //{
        //    //Результирующий словарь списков подходящих программ, ключ - идентификатор нейродетектора id_n
        //    Dictionary<int, List<int>> resDict = new Dictionary<int, List<int>>();

        //    if (averageValuesDict == null || averageValuesDict.Count == 0) { return resDict; }

        //    List<int> curResList = new List<int>();
        //    // int i;
        //    bool isCommonMatch;
        //    bool isCurMatch;
        //    bool bAreWorkingPrograms;
        //    //Были ли вообще программы для тек. id_nd и id_o
        //    bool bWasPrograms = false;

        //    //MeanKey mk = new MeanKey();
        //    //mk.id_nd = 0;
        //    //mk.id_o = 0;
        //    int lastId_nd = 0;
        //    //Идём по строкам (программам)
        //    foreach (var curTrProgramDict in trafficLightsPrograms)
        //    {

        //        //ProgramListKey - curTrProgramDict.Key
        //        //curTrProgramDict.Key.id_nd
        //        //curTrProgramDict.Key.pr_n

        //        //public struct Program (curTrProgramDict.Value - List<Program>)
        //        //public int pr_n;
        //        //public int id_nd 
        //        //public int id_o 
        //        //----------------------------------------------------

        //        //Если сменился номер детектора, то записываем список выбранных программ для предыдущего
        //        //На первой итерации цикла, когда mk.id_nd = 0, ничего не записываем :
        //        if (lastId_nd > 0 && lastId_nd != curTrProgramDict.Key.id_nd)
        //        {
        //            //if (!bWasPrograms)
        //            //{
        //            //    Console.WriteLine($"Для нейродетектора с №{curTrProgramDict.Key.id_nd} не обнаружены значения интенсивностей!");
        //            //}
        //            resDict.Add(lastId_nd, curResList);
        //            curResList = new List<int>();
        //            bWasPrograms = false;
        //        }

        //        lastId_nd = curTrProgramDict.Key.id_nd;

        //        //Здесь берём де-факто true, просто страховка от пустого списка
        //        isCommonMatch = (curTrProgramDict.Value.Count > 0);
        //        bAreWorkingPrograms = false;

        //        // Идём по диапазонам ВНУТРИ текущей светофорной ПРОГРАММЫ
        //        foreach (var curProgramDiap in (List<ProgramDiapason>)curTrProgramDict.Value)
        //        {
        //            MeanKey mk = new MeanKey();
        //            mk.id_nd = curTrProgramDict.Key.id_nd;
        //            mk.id_o = curProgramDiap.id_o;

        //            if (averageValuesDict.TryGetValue(mk, out double curAverageValue))
        //            {
        //                // Это чтобы отбросить тот случай, когда везде НЕ проставлен флаг проверки
        //                bAreWorkingPrograms = bAreWorkingPrograms || curProgramDiap.bCheck;
        //                //Program.bCheck - если false, ЗНАЧИТ НЕ НАДО ПРОВЕРЯТЬ, то есть - "берём"!
        //                isCurMatch = (!curProgramDiap.bCheck ||
        //                    (curProgramDiap.fromIntense <= curAverageValue && curAverageValue < curProgramDiap.toIntense));
        //                isCommonMatch = isCommonMatch && isCurMatch;
        //                bWasPrograms = true;
        //            }
        //            else
        //            {
        //                //??? Остался вопрос - а что делать если среди сообщений нейродетектора
        //                //не было ни одного значения для тек. нейродетектора id_nd и для тек. объекта id_o?? 
        //                Console.WriteLine($"Для нейродетектора с №{mk.id_nd} и с №объекта {mk.id_o} не обнаружены средние значения интенсивностей! (№программы - {curTrProgramDict.Key.pr_n})");
        //            }
        //            //Если было хотя бы одно непопадание в диапазон (при bCheck = true), то переходим к след диапазону
        //            if (!isCommonMatch) break;
        //        }
        //        // && bAreWorkingPrograms - убрал из условия, чтобы если даже на всех детект. объектах bCheck - false , то всё равно берём
        //        if (isCommonMatch) curResList.Add(curTrProgramDict.Key.pr_n);
        //    }

        //    //Для последнего номера нейродетектора lastId_nd
        //    if (curResList.Count > 0)
        //    {
        //        resDict.Add(lastId_nd, curResList);
        //    }
        //    return resDict;
        //}

        /// <summary>
        /// Получить номер подходящей программы из списка подходящих програм ТЗПП 
        /// </summary>
        /// <param name="suitableProgramsList"></param>
        /// <returns>Номер выбранной программы.</returns>
        public int GetChoosenProgram(List<int> suitableProgramsList, int lastChoosenProgram = 0, bool bSmoothProgTransition = false)    
        {
            if (suitableProgramsList!=null && suitableProgramsList.Count > 0) 
            { 
                if (bSmoothProgTransition && lastChoosenProgram > 0)
                {
                        //Если новая выбранная программа > выбранной в прошлый раз
                        if (suitableProgramsList[0] > lastChoosenProgram) { return (lastChoosenProgram + 1); }
                        //Если новая выбранная программа < выбранной в прошлый раз
                        else if (suitableProgramsList[0] < lastChoosenProgram) { return (lastChoosenProgram - 1); }
                        else return suitableProgramsList[0];

                }
                else
                {
                    return suitableProgramsList[0];
                }
            }
            else return 0;
        }


        public Dictionary<int, int> GetChoosenProgramsForEveryDetector(Dictionary<int, List<int>> suitableProgramsDict)
        {
            Dictionary<int, int> choosenProgramsDict = new Dictionary<int, int>();
            if (suitableProgramsDict.Count > 0)
            {
                foreach (var keyVal in suitableProgramsDict)
                {
                    choosenProgramsDict.Add(keyVal.Key, GetChoosenProgram(keyVal.Value));
                }
            }
            return choosenProgramsDict;
        }

        /// <summary>
        /// Функция возвращает каталог, в котором находится сборка
        /// </summary>
        /// <returns> Строку пути каталога, в котором находится сборка </returns>
        public static string GetExeDirectory()
        {
            //string codeBase = Assembly.GetExecutingAssembly().Location;
            string codeBase = Assembly.GetExecutingAssembly().CodeBase; //-устарел
            UriBuilder uri = new UriBuilder(codeBase);
            string ?path = Uri.UnescapeDataString(uri.Path);
            //if (String.IsNullOrEmpty(path)) return "";
            path = System.IO.Path.GetDirectoryName(path);
            return String.IsNullOrEmpty(path)?"":path;
        }

        /// <summary>
        /// Считывает из файла светофорные программы с диапазонами интенсивностей
        /// </summary>
        /// <param name="filename"> Путь и имя файла. Пример: "D:\\TrafficLightsPrograms.cnf"</param>
        /// <returns> Словарь светофорных программ с диапазонами интенсивностей.
        /// Ключ в словаре ProgramListKey, Значение список значений типа ProgramDiapason</returns>
        //Примеры: (В каждой строке — отдельная программа) 
        //PRGRM=4[1:0-800;1][2:0-800;1][3:0-800;0]
        //PRGRM=5[1:800-1000;1][2:800-1000;1][3:800-1000;1]
        //PRGRM=6[1:1000-1200;1][2:1000-1200;1][3:1000-1200;1]
        public Dictionary<ProgramListKey, List<ProgramSelection.ProgramDiapason>> ImportTrafficLightsProgramsFromFile(string filename, ref List<string> logStrings)
        {
            Dictionary<ProgramListKey, List<ProgramSelection.ProgramDiapason>> dTrafficLightsPrograms = new Dictionary<ProgramListKey, List<ProgramSelection.ProgramDiapason>>();
            //string dir = GetExeDirectory();
            int schStrok = 0;
            try
            {
                //Console.WriteLine();
                FileStream fs = File.Open(filename, FileMode.Open);

                //StreamReader sr = new StreamReader(fs);// dir + "\\out.txt",true);

                // Create an instance of StreamReader to read from a file.
                // The using statement also closes the StreamReader.
                StreamReader sr = new StreamReader(fs);
                string line;
                // Read and display lines from the file until the end of
                // the file is reached.

                //Console.WriteLine($"Read file: '{filename}'...");
                logStrings.Add($"Read the file: '{filename}'...");  
                while ((line = sr.ReadLine()) != null)
                {
                    schStrok++;
                    line = line.Trim();
                    line = line.Replace("\u202c" ,"");
                    if (line == "") { continue; }
                    //Console.WriteLine(line);
                    logStrings.Add(line);
                    //PRGRM:1=4[1:0-800;1][2:0-800;1][3:0-800;0]
                    int colonPos = line.IndexOf(":");
                    int newPos = line.IndexOf("=");
                    int id_nd = 0; //Convert.ToInt32( line.Substring(colonPos + 1, newPos-colonPos-1));
                    //удаляем то, что было до "="
                    line = line.Remove(0, newPos + 1);
                    //4[1:0-800;1][2:0-800;1][3:0-800;0]
                    newPos = line.IndexOf("[");
                    int pr_n = Convert.ToInt32(line.Substring(0, newPos));

                    //line = line.Remove(1, newPos);
                    //[1:0-800;1][2:0-800;1][3:0-800;0]

                    List<ProgramDiapason> progList = new List<ProgramDiapason>();
                    ProgramDiapason program;
                    int bracketPos1 = line.IndexOf("[");
                    int semicolon = 0;
                    int id_o = 0;
                    while (bracketPos1 >= 0)
                    {
                        //int bracketpos2 = line.IndexOf("]");
                        colonPos = line.IndexOf(":");
                        newPos = line.IndexOf("-");
                        semicolon = line.IndexOf(";");

                        //Номер DIL объекта
                        id_o = Convert.ToInt32(line.Substring(bracketPos1 + 1, colonPos - bracketPos1 - 1));

                        program = new ProgramSelection.ProgramDiapason();
                        program.id_nd = id_nd;
                        program.fromIntense = Convert.ToInt32(line.Substring(colonPos + 1, newPos - colonPos - 1));
                        program.toIntense = Convert.ToInt32(line.Substring(newPos + 1, semicolon - newPos - 1));
                        program.bCheck = Convert.ToBoolean(Convert.ToInt16(line.Substring(semicolon + 1, 1)));
                        program.id_o = id_o;
                        program.pr_n = pr_n;

                        //ДОБАВЛЯЕМ В СПИСОК!
                        progList.Add(program);

                        if (line.IndexOf("]") >= 0)
                        {
                            line = line.Remove(0, line.IndexOf("]") + 1);
                        }

                        bracketPos1 = line.IndexOf("[");
                    }
                    //Console.WriteLine(pr_n.ToString());
                    //Console.WriteLine(String.Join("|", progList)) ;

                    ProgramListKey prKey = new ProgramListKey();
                    prKey.id_nd = id_nd;
                    prKey.pr_n = pr_n;
                    prKey.numLine = schStrok;
                    //!!!
                    //prKey.id_o = id_o;
                    dTrafficLightsPrograms.Add(prKey, progList);
                }
                fs.Close();
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.                
                //Console.WriteLine($"При чтении файла: {filename} возникла ошибка:");
                //Console.WriteLine(e.Message);
                logStrings.Add($"При чтении файла: {filename} возникла ошибка:");
                logStrings.Add(e.Message);

                return new Dictionary<ProgramListKey, List<ProgramSelection.ProgramDiapason>>();
            }

            ////Вывод результатов разбора
            //if (dTrafficLightsPrograms.Count > 0)
            //{
            //    Console.WriteLine();
            //    Console.WriteLine("Результат разбора:");
            //}
            //foreach (var curPr in dTrafficLightsPrograms)
            //{
            //    Console.WriteLine($"Номер программы - {curPr.Key.pr_n}:");
            //    foreach (var prog in (List<ProgramSelection.ProgramDiapason>)curPr.Value)
            //    {
            //        Console.WriteLine($" Объект дет.№: {prog.id_o}; [{prog.fromIntense} - {prog.toIntense}],  Флаг необ. проверки: {prog.bCheck}"); //№ детектора - {prog.id_nd},
            //    }
            //}
            return dTrafficLightsPrograms;
        }

                        //program№, from - to intensity
        public Dictionary<int,ProgramSelection.IntensityFromTo> ImportProgramsOnComIntens(string filename, ref List<string> logStrings)
        {
            Dictionary<int, ProgramSelection.IntensityFromTo> dTrafficLightsPrograms = new Dictionary<int, ProgramSelection.IntensityFromTo>();
            try
            {
                FileStream fs = File.Open(filename, FileMode.Open);
                StreamReader sr = new StreamReader(fs);
                string line;
                logStrings.Add($"Read the file: '{filename}'...");
                while ((line = sr.ReadLine()) != null)
                {

                    line = line.Trim();
                    line = line.Replace("\u202c", "");
                    if (line == "") { continue; }
                    //7[1-190]
                    
                    int pos = line.IndexOf("]");
                    //Удаляем ] и до конца строки
                    if (pos >=0) line = line.Remove(pos);
                    pos = line.IndexOf("[");
                    int posDefis = line.IndexOf("-");
                    int prog = int.Parse(line.Substring(0,pos));
                    int from = int.Parse(line.Substring(pos + 1, posDefis - pos - 1));
                    int to = int.Parse(line.Substring(posDefis+1));
                    IntensityFromTo iFromTo = new IntensityFromTo();
                    iFromTo.from = from;
                    iFromTo.to = to;    
                    dTrafficLightsPrograms.Add(prog, iFromTo);
                }
                fs.Close();
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.                
                //Console.WriteLine($"При чтении файла: {filename} возникла ошибка:");
                //Console.WriteLine(e.Message);
                logStrings.Add($"При чтении файла: {filename} возникла ошибка:");
                logStrings.Add(e.Message);
                return new Dictionary<int, ProgramSelection.IntensityFromTo>();
            }

            return dTrafficLightsPrograms;
        }

        public struct Dil_LanesCount_CorrCoef
        {
            public int dil; //идентификатор dil
            public int TrLanesCount; //число полос, которое "накрывает" dil
            public double correctingCoeffsForDil; //корректирующий коэффициент
        }

        //private Dictionary<int, List<Dil_LanesCount_CorrCoef>> ImportNumPhaseToDilsFromFile(string fileName, ref List<string> logStrings)
        //{
        //    Dictionary<int, List<Dil_LanesCount_CorrCoef>> dictPhaseToDils = new Dictionary<int, List<Dil_LanesCount_CorrCoef>>();
        //    try
        //    {
        //        FileStream fs = File.Open(fileName, FileMode.Open);
        //        StreamReader sr = new StreamReader(fs);
        //        string line;
        //        int linesSch = 0;
        //        logStrings.Add($"Read the phase-to-dils correspondence file: '{fileName}'...");
        //        int colonPos = -1;
        //        while ((line = sr.ReadLine()) != null)
        //        {
        //            if (String.IsNullOrEmpty(line.Trim())) continue;

        //            linesSch++;
        //            string[] tmp = line.Split(',');
        //            List<Dil_LanesCount_CorrCoef> dilTrLanesList = new List<Dil_LanesCount_CorrCoef>();
        //            foreach (string s in tmp) 
        //            {
        //                Dil_LanesCount_CorrCoef dilTrLanes = new Dil_LanesCount_CorrCoef();
        //                colonPos = s.IndexOf(':');
        //                dilTrLanes.TrLanesCount = 1;
        //                if (colonPos < 0)
        //                {
        //                    dilTrLanes.dil = int.Parse(s);                            
        //                }
        //                else
        //                {
        //                    dilTrLanes.dil = int.Parse(s.Substring(0, colonPos));
        //                    dilTrLanes.TrLanesCount = int.Parse(s.Substring(colonPos+1,s.Length-colonPos-1));
        //                }
        //                dilTrLanesList.Add( dilTrLanes );   
        //            }
        //            //dictPhaseToDils.Add(linesSch, tmp.ToList());
        //            dictPhaseToDils.Add(linesSch, dilTrLanesList);
        //        }                
        //        fs.Close();
        //        logStrings.Add($"The the phase-to-dils correspondence file  '{fileName}' was SUCCESSFULLY loaded!");
        //        return dictPhaseToDils;
        //    }
        //    catch (Exception e)
        //    {
        //        // Let the user know what went wrong.                
        //        //Console.WriteLine($"При чтении файла: {filename} возникла ошибка:");
        //        //Console.WriteLine(e.Message);
        //        logStrings.Add($"При чтении файла: {fileName} возникла ошибка:");
        //        logStrings.Add(e.Message);
                
        //        return new Dictionary<int, List<Dil_LanesCount_CorrCoef>>();
        //    }

        //}

        private Dictionary<int,List<int>> ImportDictPhaseDursFromFile(string fileName, ref List<string> logStrings)
        {
            Dictionary<int, List<int>> phaseDurs = new Dictionary<int, List<int>>();
            //try
            //{
            //    FileStream fs = File.Open(fileName, FileMode.Open);
            //    StreamReader sr = new StreamReader(fs);
            //    string line="";
            //    int progNum=0;
            //    logStrings.Add($"Read the phase durations file: '{fileName}'...");

            //    while ((line = sr.ReadLine()) != null)
            //    {
            //        if (String.IsNullOrEmpty(line.Trim())) continue;
            //        //Берём номер фазы и удаляем его из строки
            //        progNum = Convert.ToInt32(line.Substring(0, line.IndexOf(':')));
            //        line= line.Remove(0, line.IndexOf(':')+1);

            //        //Переводим считанную строку в массив значений
            //        int[] phasesDurs = line.Split(',').
            //            Where(x => !string.IsNullOrWhiteSpace(x)).
            //            Select(x => int.Parse(x)).ToArray();

            //        phaseDurs.Add(progNum, phasesDurs.ToList());
            //    }
            //    fs.Close();
            //    logStrings.Add($"The the phase durations file  '{fileName}' was SUCCESSFULLY loaded!");
                return phaseDurs;
            //}
            //catch (Exception e)
            //{
            //    // Let the user know what went wrong.                
            //    //Console.WriteLine($"При чтении файла: {filename} возникла ошибка:");
            //    //Console.WriteLine(e.Message);
            //    logStrings.Add($"При чтении файла: {fileName} возникла ошибка:");
            //    logStrings.Add(e.Message);

            //    return null;
            //}
        }

            /// <summary>
            /// Муляж - заполняется словарь значений детекторов
            /// </summary>
            /// <param name="detectorsCount"></param>
            /// <returns></returns>
        public static Dictionary<int, List<int>> InitializeDetectorValues(int detectorsCount)
        {
            Dictionary<int, List<int>> detectorsValues = new Dictionary<int, List<int>>();
            //Номер детектора
            int i = 0;

            List<int> cycleDetectValues = new List<int>();
            cycleDetectValues.Add(600); cycleDetectValues.Add(901); cycleDetectValues.Add(1050); cycleDetectValues.Add(965);
            detectorsValues.Add(i, cycleDetectValues);
            Console.WriteLine($"Значения циклов для детектора - {i}:");
            Console.WriteLine(String.Join("|", cycleDetectValues));
            i++;

            cycleDetectValues = new List<int>();
            cycleDetectValues.Add(800); cycleDetectValues.Add(713); cycleDetectValues.Add(1165); cycleDetectValues.Add(541);
            detectorsValues.Add(i, cycleDetectValues);
            Console.WriteLine($"Значения циклов для детектора - {i}:");
            Console.WriteLine(String.Join("|", cycleDetectValues));
            i++;

            cycleDetectValues = new List<int>();
            cycleDetectValues.Add(1000); cycleDetectValues.Add(1059); cycleDetectValues.Add(360); cycleDetectValues.Add(1138);
            detectorsValues.Add(i, cycleDetectValues);
            Console.WriteLine($"Значения циклов для детектора - {i}:");
            Console.WriteLine(String.Join("|", cycleDetectValues));
            i++;
            return detectorsValues;
        }

        /// <summary>
        /// ОСНОВНАЯ ФУНКЦИЯ ВЫЗОВА ДЛЯ ПОЛУЧЕНИЯ НОМЕРА ОДНОЙ - САМОЙ ПОДХОДЯЩЕЙ ПРОГРАММЫ
        /// </summary>
        /// <param name="trLiProgramsFilename"> Имя файла для загрузки программ </param>
        /// <param name="neuroValuesStringsRMC"> Список строк - RMC-сообщений нейродетектора </param>
        /// <returns></returns>
        private int GetProgramNumber(string trLiProgramsFilename, List<string> neuroValuesStringsRMC)
        {
            //Dictionary<ProgramListKey, List<ProgramSelection.ProgramDiapason>> trafficLightsPrograms = this.ImportTrafficLightsProgramsFromFile(trLiProgramsFilename);    // "d:\\TrafficLightsProgram.txt");
            //                                                                                                                                                              //  Dictionary<int, List<int>> detectorValues = this.GetNeurodetectorsValuesFromList(neuroValuesStringsRMC);

            return 0;
        }
        //private Dictionary<int, List<int>> GetDetectorValues(int detectorsCount)
        //{
        //    Dictionary<int, List<int>> detectorsValues = new Dictionary<int, List<int>>();
        //}

        /// <summary>
        /// Функция разбирает строку RMC-сообщения curS и выдаёт значения параметров записанных в DetectorMeasuring
        /// </summary>
        /// <param name="curS">  Строка RMC-сообщения </param>
        /// <returns> Значения параметров записанных в DetectorMeasuring </returns>
        private DetectorMeasuring GetMeasFromRMCMessageParsing(string curS)
        {
            DetectorMeasuring meas = new DetectorMeasuring();

            int colonInd = 0;
            int newPos = 0;
            //int neuroN = 0;
            //int ind_o = 0;
            int beg = 0;
            //int tim = 0;
   
            //int tsCount = 0;
            int nextColonInd = 0;
            //int E1 = 0;

            //ПРИМЕР: @RMC:3{CTL:1:36000:60>DIL:1:NaimObj_2:T1}24:0
            string s = curS.Replace("\n\r", "");
            s = s.Replace("\r\n", "");
            s = s.Replace(" ", "");
            colonInd = s.IndexOf(":", colonInd);
            colonInd++;
            newPos = s.IndexOf("{");

            //id  нейродетектора
            meas.id_nd = Convert.ToInt32(s.Substring(colonInd, newPos - colonInd));

            //colonInd = s.IndexOf(":", colonInd);
            if (s.Substring(s.IndexOf("{") + 1, 3) == "CTL")
            {
                //сквозной номер для объекта детектирования пересечения на линии
                colonInd = s.IndexOf(":", colonInd);
                colonInd++;
                nextColonInd = s.IndexOf(":", colonInd);
                //НОМЕР ОБЪЕКТА ИЗМЕРЕНИЯ
                meas.id_o = Convert.ToInt32(s.Substring(colonInd, nextColonInd - colonInd));

                //секунда от начала интервала
                colonInd = nextColonInd + 1;
                nextColonInd = s.IndexOf(":", colonInd);
                beg = Convert.ToInt32(s.Substring(colonInd, nextColonInd - colonInd));

                //Длительность интервала подсчёта
                colonInd = nextColonInd + 1;
                meas.tim = Convert.ToInt32(s.Substring(colonInd, s.IndexOf(">", colonInd) - colonInd));

                //Пропускаем "DIL:19:"
                colonInd = s.IndexOf(":", colonInd);
                colonInd++;
                colonInd = s.IndexOf(":", colonInd);
                colonInd++;

                //Наименование 
                nextColonInd = s.IndexOf(":", colonInd);
                meas.name = s.Substring(colonInd, nextColonInd - colonInd);
                colonInd = nextColonInd + 1;

                //Направление
                newPos = s.IndexOf("}", colonInd);
                string tmpDirAppr = s.Substring(colonInd, newPos - colonInd);
                meas.direction = tmpDirAppr[0];
                //Подход
                meas.approach = Convert.ToInt32(tmpDirAppr.Substring(1,tmpDirAppr.Length-1));// - берем всё кроме символа направления

                //Расчётное значение прямого движения
                colonInd = s.IndexOf(":", colonInd);
                if (colonInd >= 0)
                {
                    //ИЗМЕРЕННОЕ ЗНАЧЕНИЕ прямого движения:
                    meas.E1 = Convert.ToInt32(s.Substring(newPos + 1, colonInd - newPos - 1));
                    //Расчётное значение обратного движения
                    //meas.E2 = Convert.ToInt32(s.Substring(colonInd + 1, s.Length - colonInd - 1));
                }
                else
                {
                    //ИЗМЕРЕННОЕ ЗНАЧЕНИЕ прямого движения:
                    meas.E1 = Convert.ToInt32(s.Substring(newPos + 1, s.Length - newPos - 1));
                }

                //ПРИВОДИМ К ЗНАЧЕНИЮ В ЧАС
                if (meas.tim > 0)
                {
                    meas.E1PerHour = 3600 * (double)meas.E1 / meas.tim;
                }
                else
                {
                    //Борода!!!
                    meas.E1PerHour = 0;
                }
                //colonInd = 0;
            }
            return meas;
        }
        /// <summary>
        /// Получает список расчётных значений нейродетекторов. 
        /// Номера объектов (DIL) записанные в строках регулярных сообщений могут быть различные.
        /// </summary>
        /// <param name="neuroValuesStringsRMC">Список строк - регулярных сообщений нейродетекторов, 
        /// пример - @RMC:1{CTL:19:20:15> DIL:19:ул.Сивки_к_ТЦ_‘Хромая_лошадь’:R1}13:7 </param>
        /// <returns> Список DetectorMeasuring </returns>
        public List<DetectorMeasuring> GetNeurodetectorsValuesFromList(List<string> neuroValuesStringsRMC, ref List<string> logStrings, Dictionary<int, List<Dil_LanesCount_CorrCoef>> ?numPhaseToDils, bool consoleOutput = true)
        {

            List<DetectorMeasuring> detectorsMeasOfCurCycle = new List<DetectorMeasuring>();
            //Dictionary<int, List<DetectorMeasuring>> detectorsValues = new Dictionary<int, List<DetectorMeasuring>>();

            //Console.WriteLine();
            //Console.WriteLine("Разбор сообщений(RMC) нейродетектора:");
            logStrings.Add( "Neuro messages(RMC) parsing: ");
            try
            {
                foreach (string curS in neuroValuesStringsRMC)
                {
                    // logStrings.Add("Текущее сообщение(RMC): " + Environment.NewLine + curS);
                    if (curS.IndexOf("CTL") >=0) 
                    { 
                        DetectorMeasuring meas = GetMeasFromRMCMessageParsing(curS);
                        //Находим корректирующие коэфф-ы ДЛЯ id_o(DIL-объекта) meas
                        if (numPhaseToDils != null)
                        {
                            foreach (var phDils in numPhaseToDils)
                            {
                                List<Dil_LanesCount_CorrCoef> dwlList = (List<Dil_LanesCount_CorrCoef>)phDils.Value;
                                foreach (Dil_LanesCount_CorrCoef dwl in dwlList)
                                {
                                    if (dwl.dil == meas.id_o)
                                    {
                                        //Применяем корректирующий коэф-т
                                        meas.E1PerHour = (double) (meas.E1PerHour * dwl.correctingCoeffsForDil );
                                        goto LoopEnd;  // Выходим из внутр цикла во внешний цикл
                                    }
                                }
                            }
                            LoopEnd:;
                        }
                        detectorsMeasOfCurCycle.Add(meas);
                    }
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine($"При разборе регулярных сообщений нейродетекторов возникла ошибка:");
                //Console.WriteLine(e.Message);
                logStrings.Add("An error occurred while parsing regular messages from neurodetectors: ");
                logStrings.Add(e.Message);
            }

            //detectorsValues.Count+1 - увеличиваем номер ЦИКЛА!
            //if (detectorsMeasOfCurCycle.Count > 0)  detectorsValues.Add(detectorsValues.Count+1, detectorsMeasOfCurCycle);

            //foreach (var val in detectorsValues)
            //{
            //Console.WriteLine("Номер цикла: " + val.Key.ToString());

            if (consoleOutput)
            {
                foreach (var mea in detectorsMeasOfCurCycle)
                {
                    //Console.WriteLine($"Нейродетектор №: {mea.id_nd}; № объекта: {mea.id_o} ");
                    //Console.WriteLine($"Длительность,с: {mea.tim}; Кол-во,ед: {mea.E1}; Расчётная интенсивность, Ед./ч: {mea.E1PerHour}");
                    logStrings.Add($"Det-r №: {mea.id_nd}; № object: {mea.id_o}; Duration,sec: {mea.tim}; Count, units: {mea.E1}; Calculated intensity, units/h: {mea.E1PerHour}");
                }
            }

            //}
            //return detectorsValues;
            return detectorsMeasOfCurCycle;
        }

        //Общая функция выбора программы
        /// <summary>
        /// Функция выбора программы в зависимости от интенсивностей движения.
        /// </summary>
        /// <param name="confProgramsFileName"> Путь и имя файла конфигурации программ. Пример: "D:\\TrafficLightsProgram.txt"</param>
        /// <param name="neuroValuesStringsRMC"> Список строк - регулярных сообщений нейродетекторов, 
        /// Пример строки: @RMC:1{CTL:19:20:15> DIL:19:ул.Сивки_к_ТЦ_‘Хромая_лошадь’:R1}13:7 </param>
        /// <param name="detectorsCount"> Число детекторов. </param>
        /// <returns></returns>
        private int ChooseProgramFull(string confProgramsFileName, List<string> neuroValuesStringsRMC, int detectorsCount)
        {
            //Dictionary<ProgramKey, List<ProgramSelection.Program>> trafficLightsPrograms = this.ImportTrafficLightsProgramsFromFile(confProgramsFileName);
            //if (trafficLightsPrograms == null) return 0;
            //Dictionary<int, List<ProgramSelection.DetectorMeasuring>> detectorsValues;
            //detectorsValues = this.GetNeurodetectorsValuesFromList(neuroValuesStringsRMC);
            //if (detectorsValues == null) return 0;
            //List<double> averageValuesList = null;
            //averageValuesList = this.GetArithmeticMeanList(detectorsValues, detectorsCount);
            //Console.WriteLine("Средние значения интенсивностей:");
            //Console.WriteLine(String.Join(" | ", averageValuesList));

            //List<int> suitableProgramsList = this.GetSuitableProgramsList(trafficLightsPrograms, averageValuesList);            
            //if (suitableProgramsList != null || suitableProgramsList.Count > 0)
            //{
            //    Console.WriteLine("Список подходящих программ (по номерам):"); 
            //    Console.WriteLine(String.Join(" | ", suitableProgramsList));
            //    Console.WriteLine("Выбираем программу из списка:");
            //    int num = this.GetChoosenProgram(suitableProgramsList);
            //    Console.WriteLine(num);
            //    return num;
            //}
            //else Console.WriteLine("Подходящие программы не найдены!");
            return 0;
        }

        //Общая функция выбора программы
        //Функция выбора программы в зависимости от значений интенсивностей движения, полученных c нейродетекторов
        // <summary>
        // 
        // </summary>
        // <param name="neuroValuesStringsRMC"> </param>
        // <param name="FTPUser"> Логин пользователя (FTP) - необходим для загрузки файла светофорных программ с FTP</param>
        // <param name="FTPPass"> Пароль пользователя (FTP) - необходим для загрузки файла светофорных программ c FTP </param>
        // <param name="FilePath"> Путь к файлу на </param>
        // <param name="FileName"></param>
        // <returns></returns>
        /// <summary>
        /// Общая функция выбора программы. Функция выбора программы в зависимости от значений интенсивностей движения, полученных c нейродетекторов
        /// </summary>
        /// <param name="neuroValuesStringsRMC"> 
        /// Список строк - регулярных сообщений нейродетекторов, 
        /// Пример строки: @RMC:1{CTL:19:20:15> DIL:19:ул.Сивки_к_ТЦ_‘Хромая_лошадь’:R1}13:7 
        /// </param>
        /// <param name="FTPUser"> Логин пользователя (FTP) - необходим для загрузки файла светофорных программ с FTP </param>
        /// <param name="FTPPass"> Пароль пользователя (FTP) - необходим для загрузки файла светофорных программ c FTP </param>
        /// <param name="FilePath"> Путь к файлу на локальном диске - для сохранения файла, загруженного с FTP. По умолчанию - берётся путь к сборке.</param>
        /// <param name="FileName"> Имя файла. По умолчанию - "TrafficLightsPrograms.cnf" </param>
        /// <param name="Host"> Имя хоста FTP. По умолчанию - "194.87.111.128" </param>
        /// <param name="CatOnFTPServer"> Каталог на FTP. По умолчанию - @"/home/dan/vanproject/"</param>
        /// <returns></returns>
        public int ChooseProgramTZPP(List<string> neuroValuesStringsRMC, 
                                    ref List<string> logStrings,
                                    //string FTPUser, string FTPPass,
                                    Dictionary<int, List<Dil_LanesCount_CorrCoef>> ?numPhaseToDils,
                                    Dictionary<int, List<int>> dictPhasesDurs,
                                    int algoritmNumber,
                                    List<int> BIntList,
                                    bool bSmoothProgTransition,
                                    int lastChoosenProgram = 0,
                                    string FilePath = "", 
                                    string FileName = "",                                    
                                    string FileNamePhaseToDILs = "PhaseToDils.phdils",
                                    string FileNamePhaseTimes = "PhaseTimes.phtim" 
                                    //, string Host = "", string CatOnFTPServer = ""
                                    )
        {
            //создаём экземпляр класса ProgramSelection - для выбора программы
            //ProgramSelection ps = new ProgramSelection();               

            if (String.IsNullOrEmpty(FilePath)) { FilePath = GetExeDirectory(); }
            Console.WriteLine($"FilePath: {FilePath}");
            if (FilePath.Length > 0 && FilePath.Substring(FilePath.Length - 1) != Path.DirectorySeparatorChar.ToString())//@"\") 
            { FilePath = FilePath + Path.DirectorySeparatorChar; };
            
            //FilePath = @"\upload";
            //FilePath = GetExeDirectory();
            //FilePath = FilePath + @"\";

            Dictionary<ProgramSelection.ProgramListKey, List<ProgramSelection.ProgramDiapason>> trafficLightsPrograms;
            Dictionary<int, ProgramSelection.IntensityFromTo> progIntensitiesFromTo;
            ////Если входящий словарь trafficLightsPrograms пуст  
            //if (trafficLightsPrograms == null || trafficLightsPrograms.Count < 1 )
            //{
            //то загружаем ИЗ ФАЙЛа всю пачку и разбираем её

            //}

            List<ProgramSelection.DetectorMeasuring> detectorsValues;

            Console.WriteLine($"8) Getting Neurodetectors Values From List...");
            detectorsValues = this.GetNeurodetectorsValuesFromList(neuroValuesStringsRMC, ref logStrings,numPhaseToDils); // - последний 

            //List<double> averageValuesList = null;
            Dictionary<int, double> averageValuesDict = new Dictionary<int, double>();

            Console.WriteLine($"9) Getting Arithmetic Mean for every of DIL-objects...");
            averageValuesDict = this.GetArithmeticMean_Dict_IdOb_Aver(detectorsValues);

            if (averageValuesDict.Count > 0)
            {
                //Console.WriteLine("Средние значения интенсивностей:");
                //Console.WriteLine("Average intensities:");
                logStrings.Add("Average intensities:");
                foreach (var MeanKeyAverValue in averageValuesDict)
                {
                    //Console.WriteLine($" №дет.объекта: {MeanKeyAverValue.Key}; Среднее - {MeanKeyAverValue.Value}");
                    //Console.WriteLine($" Object №: {MeanKeyAverValue.Key}; Average - {MeanKeyAverValue.Value}");
                    logStrings.Add($" Object №: {MeanKeyAverValue.Key}; Average - {MeanKeyAverValue.Value}");
                }
            }
            else 
            {
                //{ Console.WriteLine("Нет средних значений интенсивностей!"); }
                //Console.WriteLine("There are NO aver. intensities!"); 
                logStrings.Add("There are NO aver. intensities!");                
            }

            //Ищем подходящие программы:            
            if (averageValuesDict.Count > 0)
            {
                //Console.WriteLine("Список подходящих программ (по номерам):");
                //Console.WriteLine("List of suitable traff. light programs (pr. numbers):");
                logStrings.Add("List of suitable traff. light programs (pr. numbers):");
            }
            List<int> suitableProgramsList;

            //!!!ПОЛУЧАЕМ СПИСОК ПОДХОДЯЩИХ ПРОГРАММ МЕТОДОМ МИНИМАЛЬНЫХ КОЭФФИЦИЕНТОВ
            Console.WriteLine($"10) Calculate suitable TL Program Numbers List...");
            switch (algoritmNumber)
            {
                case 1:
                    //ОСНОВНОЙ АЛГОРИТМ:
                    trafficLightsPrograms = this.ImportTrafficLightsProgramsFromFile(FilePath + FileName, ref logStrings);
                    suitableProgramsList = this.GetSuitableProgramsList(trafficLightsPrograms, averageValuesDict);
                break;
                case 3:
                    progIntensitiesFromTo = this.ImportProgramsOnComIntens(FilePath + "ProgramsOnComIntens.prIntens", ref logStrings);
                    suitableProgramsList = GetSuitableProgramBasedOnComIntensity(progIntensitiesFromTo, averageValuesDict, numPhaseToDils, ref logStrings);
                break;

                default:
                    suitableProgramsList = GetSuitableProgramBasedOnMinCoefs(numPhaseToDils, dictPhasesDurs, averageValuesDict, ref logStrings);
                break;
            }


            //Console.WriteLine(String.Join(" | ", suitableProgramsList));
            logStrings.Add(String.Join(" | ", suitableProgramsList));

            //Console.WriteLine("Выбираем программу из списка:");
            //Console.WriteLine("Get the traff. light program from the list:");
            
            logStrings.Add("Get the traff. light program from the list:");
            
            Console.WriteLine($"99) Choose one of suitable TL Program Numbers...");
            lastChoosenProgram = this.GetChoosenProgram(suitableProgramsList, lastChoosenProgram,bSmoothProgTransition);
            //Console.WriteLine(choosenProgram);
            
            //!!!ВРЕМЕННАЯ ЗАГЛУШКА:
            //logStrings.Clear();

            logStrings.Add(lastChoosenProgram.ToString());
            logStrings.Add("==================================================================");
            return lastChoosenProgram;
        }


    //    /// <summary>
    //    /// Возвращает интенсивность на текущий момент времени
    //    /// </summary>
    //    /// <param name="neuroValuesStringsRMC">Список строк - регулярных сообщений нейродетекторов,
    //    /// Пример строки: @RMC:1{CTL:19:20:15> DIL:19:ул.Сивки_к_ТЦ_‘Хромая_лошадь’:R1}13:7 </param>
    //    /// <param name="sDateTime"></param>
    //    /// <returns></returns>
    //    public List<string> GetIntensitiesForNow(List<string> neuroValuesStringsRMC, ref List<string> logStrings,
    //                                             string FTPUser, string FTPPass,
    //                                             string FilePath, string FileName = "",
    //                                             string Host = "", string CatOnFTPServer = "",
    //                                             string sDateTime = "")
    //    {
    //        List<string> ls = new List<string>();

    //        //Дата/время
    //        if (String.IsNullOrEmpty(sDateTime))
    //        {
    //            sDateTime = DateTime.Now.ToString();
    //            Console.WriteLine("Текущие дата и время: " + sDateTime);
    //        }
    //        Console.WriteLine();
    //        Console.WriteLine("Сообщения (RMC) нейродетектора:");

    //        List<ProgramSelection.DetectorMeasuring> detectorsValues;
    //        detectorsValues = GetNeurodetectorsValuesFromList(neuroValuesStringsRMC, ref logStrings,null,false);

    //        Dictionary<int, double> averageValuesDict;
    //        averageValuesDict = this.GetArithmeticMean_Dict_IdOb_Aver(detectorsValues);

    //        //Выводим на консоль и в файл
    //        if (averageValuesDict.Count > 0)
    //        {
    //            //bool res = false;
    //            try
    //            {
    //                if (FilePath.Trim() == "") FilePath = ProgramSelection.GetExeDirectory();
    //                if (FilePath.Length > 0 && FilePath.Substring(FilePath.Length - 1) != Path.DirectorySeparatorChar.ToString()) //@"\") 
    //                { FilePath = FilePath + Path.DirectorySeparatorChar; }
    //                //Если даже файл существует, то он будет перезаписан:
    //                FileStream fcreate = File.Open(FilePath + FileName, FileMode.Create);
    //                StreamWriter sw = new StreamWriter(fcreate);

    //                Console.WriteLine("Средние значения интенсивностей:");
    //                foreach (var MeanKeyAverValue in averageValuesDict)
    //                {
    //                    Console.WriteLine($" №дет.объекта: {MeanKeyAverValue.Key}; Средняя интенсивность - {MeanKeyAverValue.Value}");
    //                    ls.Add(sDateTime + "| Детект.объект №" + MeanKeyAverValue.Key + "| Сред.интенсивность: " + MeanKeyAverValue.Value);
    //                    sw.WriteLine(sDateTime + "| Детект.объект №" + MeanKeyAverValue.Key + "| Сред.интенсивность: " + MeanKeyAverValue.Value);
    //                }

    //                sw.Close();
    //                Console.WriteLine($"Файл интенсивностей {FilePath + FileName} был УСПЕШНО записан!");
    //            }
    //            catch (Exception ex)
    //            {
    //                //Console.WriteLine($"Файл интенсивностей {FilePath + FileName} НЕ удалось записать!" +
    //                //                  "Произошла ошибка: " + ex.Message);
    //            }

    //            //Кидаем файл на FTP
    //            string sResUpload = FTPWorking.FTPUploadFile(FTPUser, FTPPass,
    //                                                         FilePath, FileName,
    //                                                         Host, CatOnFTPServer);
    //            if (String.IsNullOrEmpty(sResUpload))
    //            {
    //                Console.WriteLine($" Файл интенсивностей {FilePath + FileName} " +
    //                                  $" был УСПЕШНО выгружен с FTP {Host} {CatOnFTPServer}");
    //            }
    //            else
    //            {
    //                Console.WriteLine($" При выгрузке файла интенсивностей {FilePath + FileName} " +
    //                                  $" на FTP {Host} {CatOnFTPServer} возникли ошибки: " + Environment.NewLine + sResUpload);
    //            }

    //        }
    //        else
    //        {
    //            Console.WriteLine($"Значения интенсивностей не были получены!");
    //        }
    //        Console.WriteLine();


    //        ////Словарь средних значений
    //        //Dictionary<MeanKey, double> MeanE1PerHour = new Dictionary<MeanKey, double>();
    //        //foreach (var mea in detectorsMeas)
    //        //{
    //        //    MeanKey curKey = new MeanKey();
    //        //    curKey.id_nd = mea.id_nd;
    //        //    curKey.id_o = mea.id_o;

    //        //    //   // Console.WriteLine
    //        //}

    //        return ls;
    //    }
    }
}
