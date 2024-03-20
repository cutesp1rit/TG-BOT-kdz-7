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

    public User(string charId)
    {
        ChatId = charId;
        UserState = UserEnum.Starting;
    }
    
    public User()
    {
        
    }
}