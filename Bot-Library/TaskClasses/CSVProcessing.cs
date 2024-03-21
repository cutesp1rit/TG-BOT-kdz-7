using System.Text;
namespace Bot_Library;

public class CSVProcessing
{
    /// <summary>
    /// Метод считывания csv файла
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static List<WifiCC> Read(Stream stream)
    {
        string line;
        List<string> arrayStrings = new List<string>();
        using (StreamReader reader = new StreamReader(stream)) // по потоку считываем все в одну строку
        {
            line = reader.ReadLine();
            while (line != null)
            {
                if (line.Length != 0)
                {
                    arrayStrings.Add(line);    
                }
                line = reader.ReadLine();
            }
            if (arrayStrings.Count == 0)
            {
                throw new ArgumentNullException();
            }
        }
        CheakString(arrayStrings.ToArray()); // проверяем на корректность
        return SortOfInformation(arrayStrings.ToArray()); // возвращаем готовый список
    }
    
    /// <summary>
    /// Метод записи csv файлов
    /// </summary>
    /// <param name="allInf"></param>
    /// <returns></returns>
    public static MemoryStream Write(List<WifiCC> allInf)
    {
        MemoryStream stream = new MemoryStream();
        StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);

        // записываем первые две строки
        writer.WriteLine("\"ID\";\"CulturalCenterName\";\"AdmArea\";\"District\";\"Address\";\"NumberOfAccessPoints\";" +
                         "\"WiFiName\";\"CoverageArea\";\"FunctionFlag\";\"AccessFlag\";\"Password\";\"Latitude_WGS84\";" +
                         "\"Longitude_WGS84\";\"global_id\";\"geodata_center\";\"geoarea\";\n\"Код\";" +
                         "\"Наименование культурного центра\";\"Административный округ\";\"Район\";\"Адрес\";" +
                         "\"Количество точек доступа\";\"Имя Wi-Fi сети\";\"Зона покрытия, в метрах\";" +
                         "\"Признак функционирования\";\"Условия доступа\";\"Пароль\";\"Широта в WGS-84\";" +
                         "\"Долгота в WGS-84\";\"global_id\";\"geodata_center\";\"geoarea\";");
        // записываем все объекты
        foreach (var obj in allInf)
        {
            writer.WriteLine(obj.ToCSV());
        }

        writer.Flush();
        stream.Position = 0;

        return stream;
    }
    
    
    /// все следующие методы по парсингу файла взяты из старого кдз
    
    
    /// <summary>
    /// Алгоритм проверки файла на корректность данных
    /// </summary>
    /// <param name="arrayStrings">массив строк</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void CheakString(string[] arrayStrings)
    {
        for (int v = 0; v < arrayStrings.Length; v++) // проверяем каждую строку на корректность данных
        {
            if (arrayStrings[v].Length == 0)
            {
                continue;
            }
            // этот цикл нужен в случае пробелов в конце строки. однако я не пользуюсь им, так как
            // записываю строки подряд \n. но он у меня есть в случае другой задачи
            // в этой я считаю, что наличие символа НЕ ";" в конце строки - являяется исключением
            /* if (massivStrings[v][^1] != ';')
            {
                while (massivStrings[v][^1] != ';')
                {
                    massivStrings[v] = massivStrings[v][..^1];
                }
            } */

            string someString = arrayStrings[v];
            int i = 0; // индекс для прохода по строке
            int kolProverka = 0; // количество разделенных элементов в строке
            if (someString[^1] != ';')
            {
                // в таком случае запись данных в файле некорректна
                throw new ArgumentNullException();
            }
            while (i < someString.Length)
            {
                // если идет ячейка с =", то мы переходим к шагу с кавычками
                if ((someString[i] == '=') && (someString[i + 1] == '"'))
                {
                    i++;
                }
                if (kolProverka == 16) // то выборок в строк больше 16, что не соотвествует первой строке, завершаем цикл, чтобы выкинуть предупреждение ниже
                {
                    kolProverka++; // чтобы сработало предупреждение ниже
                }
                if (someString[i] == '\"')
                {
                    if (kolProverka == 15)
                    { // если последняя выборка заходит с '"', то предпоследний элемент должен быть '"'
                        if (someString[^2] != '"')
                        {
                            // в таком случае запись данных в файле некорректна
                            throw new ArgumentNullException();
                        }
                    }
                    i++;
                    // цикл для сборки одной из выборок
                    while (someString[i] != '\"' && someString[i + 1] != ';')
                    {
                        i++;
                    }
                    i += 2; // сдвигаемся на после ';'
                    kolProverka++;
                    continue;
                }
                // берем выборку, если она идет без кавычек
                while (someString[i] != ';')
                {
                    i++;
                }
                kolProverka++; // занесли выборку
                i++; // сдвигаемся с ";"
                continue;
            }
            if (kolProverka != 16) // если записалось не 16, то данные в файле записаны неверно, предупреждаем об этом пользователя
            {
                throw new ArgumentNullException();
            }
        }
    }
    
    /// <summary>
    /// Отвечает за рассортировку всех данных, элементов по массивам. Возвращает массив массивов. 
    /// Работа метода происходит не через Split, чтобы избежать потери информации, так как данные в таблице заключены в кавычках
    /// </summary>
    public static List<WifiCC> SortOfInformation(string[] arrayStrings) {
        int kolPust = 0; // количество пустых строк
        // этот цикл нужен в случае пробелов в конце строки. однако я не пользуюсь им, так как
        // записываю строки подряд \n. но он у меня есть в случае другой задачи
        // в этой я считаю, что наличие символа НЕ ";" в конце строки - являяется исключением
        for (int i = 0; i < arrayStrings.Length; i++)
        {
            if (arrayStrings[i].Length == 0)
            {
                kolPust++;
            }
        }
        
        // имеем 16 выборок
        string[][] arrayArraysStrings = new string[arrayStrings.Length - kolPust][]; // пустые не учитываем
        int j = 0; // индекс для массива выше
        for (int i = 0; i < arrayStrings.Length; i++)
        {
            if (arrayStrings[i].Length != 0) // пустые строки не добавляем
            {
                arrayArraysStrings[j] = ReadString(arrayStrings[i]);
                j++;
            }
        }
        
        List<WifiCC> allInf = new List<WifiCC>();
        foreach (var line in arrayArraysStrings[2..])
        {
            allInf.Add(new WifiCC(line[0], line[1], line[2], line[3], line[4], line[5], 
                line[6], line[7], line[8], line[9], line[10], line[11], 
                line[12], line[13], line[14], line[15]));
        }

        if (allInf == null || allInf.Count == 0) // если файл пуст
        {
            throw new FormatException("Ошибка! Файл пуст!");
        }
        
        return allInf;
    }
    /// <summary>
    /// Делит строку на массив данных выборок
    /// </summary>
    /// <param name="someString">Переданная строка из файла</param>
    /// <returns></returns>
    public static string[] ReadString(string someString)
    {
        // этот цикл нужен в случае пробелов в конце строки. однако я не пользуюсь им, так как
        // записываю строки подряд \n. но он у меня есть в случае другой задачи
        // в этой я считаю, что наличие символа НЕ ";" в конце строки - являяется исключением
        /* if (someString[^1] != ';')
        {
            while (someString[^1] != ';')
            {
                someString = someString[..^1];
            }
        } */

        string[] resultOfMassivString = new string[16]; // так как всего в таблице 16 выборок
        int i = 0; // индекс для прохода по строке
        int kolProverka = 0; // индекс для заноски выборок в массив
        StringBuilder part = new StringBuilder(""); // с помощью этой переменной будем составлять каждую выборку
        while (i < someString.Length)
        {
            // если идет ячейка с =", то мы переходим к шагу с кавычками
            if ((someString[i] == '=') && (someString[i+1] == '"'))
            {
                i++;
            }
            part = new StringBuilder("");
            if (someString[i] == '\"')
            {
                i++;
                // цикл для сборки одной из выборок
                while (someString[i] != '\"' && someString[i+1] != ';') {
                    part.Append(someString[i]);
                    i++;
                }
                i += 2; // сдвигаемся на после ';'
                resultOfMassivString[kolProverka] = part.ToString();
                kolProverka++;
                continue;
            }
            // берем выборку, если она идет без кавычек
            while (someString[i] != ';')
            {
                part.Append(someString[i]);
                i++;
            }
            resultOfMassivString[kolProverka] = part.ToString(); // заносим получанную выборку в масив
            kolProverka++; // занесли выборку
            i++; // сдвигаемся с ";"
            continue;
        }
        return resultOfMassivString; // возвращаем получанный массив
    }
}

