using System.Text;
using System.Text.Json;
namespace Bot_Library;

public class JSONProcessing
{
    /// <summary>
    /// Метод для Десериализации данных
    /// </summary>
    /// <returns></returns>
    /// <exception cref="FormatException">Ошибка в случае некорректных данных</exception>
    public static List<WifiCC> Read(Stream stream)
    {
        StringBuilder jsonFileInString = new StringBuilder();
        string line;
        using (StreamReader reader = new StreamReader(stream))
        {
            line = reader.ReadLine();
            while (line != null)
            {
                if (line.Length != 0)
                {
                    jsonFileInString.Append(line);
                }
                line = reader.ReadLine();
            }
        }
        
        List<WifiCC> allInf = JsonSerializer.Deserialize<List<WifiCC>>(jsonFileInString.ToString());
        if (allInf == null || allInf.Count == 0) // если файл пуст
        {
            throw new FormatException("Ошибка! Файл пуст!");
        }
        
        return allInf;
    }

    /// <summary>
    /// Метод для сериализации данных 
    /// </summary>
    /// <param name="products">Список объектов из json</param>
    /// <param name="path">Путь для записи</param>
    public static MemoryStream Write(List<WifiCC> allInf)
    {
        string jsonString = JsonSerializer.Serialize<List<WifiCC>>(allInf);
        
        MemoryStream stream = new MemoryStream();
        StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);

        writer.WriteLine(jsonString);

        writer.Flush();
        stream.Position = 0;

        return stream;
    }
}