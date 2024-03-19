namespace Bot_Library;

public class WifiCC
{
    private string _id;
    private string _culturalCenterName;
    private string _admArea;
    private string _district;
    private string _address;
    private string _numberOfAccessPoints;
    private string _wiFiName;
    private string _coverageArea;
    private string _functionFlag;
    private string _accessFlag;
    private string _password;
    private string _latitudeWGS84;
    private string _longitudeWGS84;
    private string _globalId;
    private string _geodataCenter;
    private string _geoarea;

    public string Id
    {
        get => _id;
        init => _id = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_widgetId\"");
    }
    
    public string CulturalCenterName
    {
        get => _culturalCenterName;
        set => _culturalCenterName = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_culturalCenterName\"");
    }

    public string AdmArea
    {
        get => _admArea;
        set => _admArea = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_admArea\"");
    }

    public string District
    {
        get => _district;
        set => _district = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_district\"");
    }

    public string Address
    {
        get => _address;
        set => _address = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_address\"");
    }

    public string NumberOfAccessPoints
    {
        get => _numberOfAccessPoints;
        set => _numberOfAccessPoints = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_numberOfAccessPoints\"");
    }

    public string WiFiName
    {
        get => _wiFiName;
        set => _wiFiName = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_wiFiName\"");
    }

    public string CoverageArea
    {
        get => _coverageArea;
        set => _coverageArea = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_coverageArea\"");
    }

    public string FunctionFlag
    {
        get => _functionFlag;
        set => _functionFlag = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_functionFlag\"");
    }

    public string AccessFlag
    {
        get => _accessFlag;
        set => _accessFlag = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_accessFlag\"");
    }

    public string Password
    {
        get => _password;
        set => _password = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_password\"");
    }

    public string LatitudeWGS84
    {
        get => _latitudeWGS84;
        set => _latitudeWGS84 = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_latitudeWGS84\"");
    }

    public string LongitudeWGS84
    {
        get => _longitudeWGS84;
        set => _longitudeWGS84 = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_longitudeWGS84\"");
    }

    public string GlobalId
    {
        get => _globalId;
        set => _globalId = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_globalId\"");
    }

    public string GeodataCenter
    {
        get => _geodataCenter;
        set => _geodataCenter = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_geodataCenter\"");
    }

    public string Geoarea
    {
        get => _geoarea;
        set => _geoarea = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_geoarea\"");
    }
    
    public WifiCC(string id, string culturalCenterName, string admArea, string district, string address,
        string numberOfAccessPoints, string wiFiName, string coverageArea, string functionFlag,
        string accessFlag, string password, string latitudeWGS84, string longitudeWGS84,
        string globalId, string geodataCenter, string geoarea)
    {
        Id = id;
        CulturalCenterName = culturalCenterName;
        AdmArea = admArea;
        District = district;
        Address = address;
        NumberOfAccessPoints = numberOfAccessPoints;
        WiFiName = wiFiName;
        CoverageArea = coverageArea;
        FunctionFlag = functionFlag;
        AccessFlag = accessFlag;
        Password = password;
        LatitudeWGS84 = latitudeWGS84;
        LongitudeWGS84 = longitudeWGS84;
        GlobalId = globalId;
        GeodataCenter = geodataCenter;
        Geoarea = geoarea;
    }

    // наличие пустого конструктора
    public WifiCC()
    {
        
    }
}