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

    public User(string charId)
    {
        ChatId = charId;
        UserState = UserEnum.Starting;
    }
    
    public User()
    {
        
    }

    public void Sorting()
    {
        
    }

    public List<WifiCC> TakingOverField(string field, string value)
    {
        return _listFromFile.Where(x => x.GetRightProperty(field).Contains(value)).ToList();
    }
}