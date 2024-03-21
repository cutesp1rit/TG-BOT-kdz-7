namespace Bot_Library.Bots;

public class User
{
    private string _chatId;
    private List<WifiCC> _listFromFile;
    private UserEnum _userState;

    public string ChatId
    {
        get { return _chatId; }
        set { _chatId = value;  }
    }

    public List<WifiCC> ListFromFile
    {
        get { return _listFromFile; }
        set { _listFromFile = value; }
    }

    public UserEnum UserState
    {
        get { return _userState; }
        set { _userState = value;  }
    }

    public string TmpField
    {
        get;
        set;
    }
    
    public List<WifiCC> TmpList
    {
        get;
        set;
    }

    public User(string charId)
    {
        ChatId = charId;
        UserState = UserEnum.Starting;
    }
    
    public User()
    {
        
    }

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
            else
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
            else
            {
                return list.Where(x => x.GetRightProperty(field).Contains(value)).ToList();    
            }    
        }
    }
}