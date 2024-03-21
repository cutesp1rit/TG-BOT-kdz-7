namespace Bot_Library.Bots;

public class User
{
    private string _chatId;
    private List<WifiCC> _listFromFile;
    private UserEnum _userState;

    /// <summary>
    /// свойство для id диалога пользователя
    /// </summary>
    public string ChatId
    {
        get { return _chatId; }
        set { _chatId = value;  }
    }

    /// <summary>
    /// свойство со списком данных из файла
    /// </summary>
    public List<WifiCC> ListFromFile
    {
        get { return _listFromFile; }
        set { _listFromFile = value; }
    }

    /// <summary>
    /// его состояния
    /// </summary>
    public UserEnum UserState
    {
        get { return _userState; }
        set { _userState = value;  }
    }

    /// <summary>
    /// временное выбранное поле пользователем
    /// </summary>
    public string TmpField
    {
        get;
        set;
    }
    
    /// <summary>
    /// временный список для выборки
    /// </summary>
    public List<WifiCC> TmpList
    {
        get;
        set;
    }

    /// <summary>
    /// конструктор с параметрами
    /// </summary>
    /// <param name="charId"></param>
    public User(string charId)
    {
        ChatId = charId;
        UserState = UserEnum.Starting;
    }
    
    /// <summary>
    ///  конструктор без параметров
    /// </summary>
    public User()
    {
        
    }

    /// <summary>
    /// Метод сортировки по заданию
    /// </summary>
    /// <param name="field"></param>
    public void Sorting(string field)
    {
        if (field == "CulturalCenterName")
        {
            _listFromFile = _listFromFile.OrderBy(x => x.CulturalCenterName).ToList();
        }
        else // другое поле
        {
            _listFromFile = _listFromFile.OrderBy(x =>
            {
                // если там не число -- опускаем вниз
                if (!int.TryParse(x.NumberOfAccessPoints, out var value))
                    value = Int32.MaxValue - 1; 

                return value;
            }).ToList();

        }
    }

    /// <summary>
    /// Метод для выборки по заданию
    /// </summary>
    /// <param name="field"></param>
    /// <param name="value"></param>
    /// <param name="list"></param>
    /// <returns></returns>
    public List<WifiCC> TakingOverField(string field, string value, List<WifiCC> list = null)
    {
        if (list == null) // не передавали доп параметр 
        {
            // сначала берет для District
            if (field == "District и AccessFlag" && UserState==UserEnum.WaitingForTwoValue)
            {
                return _listFromFile.Where(x => x.GetRightProperty("District").ToLower().Contains(value.ToLower())).ToList();
            }
            // во втором случае для AccessFlag
            else if (field == "District и AccessFlag" && UserState == UserEnum.WaitingForValue)
            {
                return _listFromFile.Where(x => x.GetRightProperty("AccessFlag").ToLower().Contains(value.ToLower())).ToList();
            }
            else // если другие поля
            {
                return _listFromFile.Where(x => x.GetRightProperty(field).ToLower().Contains(value.ToLower())).ToList();    
            }    
        }
        else
        {
            // сначала берет для District
            if (field == "District и AccessFlag" && UserState==UserEnum.WaitingForTwoValue)
            {
                return list.Where(x => x.GetRightProperty("District").Contains(value)).ToList();
            }
            // во втором случае для AccessFlag
            else if (field == "District и AccessFlag" && UserState == UserEnum.WaitingForValue)
            {
                return list.Where(x => x.GetRightProperty("AccessFlag").Contains(value)).ToList();
            }
            else // если другие поля
            {
                return list.Where(x => x.GetRightProperty(field).Contains(value)).ToList();    
            }    
        }
    }
}