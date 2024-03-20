using System.Text;
using System.Text.Json.Serialization;

namespace Bot_Library;

[Serializable]
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

    [JsonPropertyName("ID")]
    public string Id
    {
        get => _id;
        init => _id = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_widgetId\"");
    }
    
    [JsonPropertyName("CulturalCenterName")]
    public string CulturalCenterName
    {
        get => _culturalCenterName;
        set => _culturalCenterName = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_culturalCenterName\"");
    }

    [JsonPropertyName("AdmArea")]
    public string AdmArea
    {
        get => _admArea;
        set => _admArea = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_admArea\"");
    }

    [JsonPropertyName("District")]
    public string District
    {
        get => _district;
        set => _district = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_district\"");
    }

    [JsonPropertyName("Address")]
    public string Address
    {
        get => _address;
        set => _address = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_address\"");
    }

    [JsonPropertyName("NumberOfAccessPoints")]
    public string NumberOfAccessPoints
    {
        get => _numberOfAccessPoints;
        set => _numberOfAccessPoints = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_numberOfAccessPoints\"");
    }

    [JsonPropertyName("WiFiName")]
    public string WiFiName
    {
        get => _wiFiName;
        set => _wiFiName = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_wiFiName\"");
    }

    [JsonPropertyName("CoverageArea")]
    public string CoverageArea
    {
        get => _coverageArea;
        set => _coverageArea = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_coverageArea\"");
    }

    [JsonPropertyName("FunctionFlag")]
    public string FunctionFlag
    {
        get => _functionFlag;
        set => _functionFlag = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_functionFlag\"");
    }

    [JsonPropertyName("AccessFlag")]
    public string AccessFlag
    {
        get => _accessFlag;
        set => _accessFlag = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_accessFlag\"");
    }

    [JsonPropertyName("Password")]
    public string Password
    {
        get => _password;
        set => _password = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_password\"");
    }

    [JsonPropertyName("Latitude_WGS84")]
    public string LatitudeWGS84
    {
        get => _latitudeWGS84;
        set => _latitudeWGS84 = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_latitudeWGS84\"");
    }

    [JsonPropertyName("Longitude_WGS84")]
    public string LongitudeWGS84
    {
        get => _longitudeWGS84;
        set => _longitudeWGS84 = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_longitudeWGS84\"");
    }

    [JsonPropertyName("global_id")]
    public string GlobalId
    {
        get => _globalId;
        set => _globalId = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_globalId\"");
    }

    [JsonPropertyName("geodata_center")]
    public string GeodataCenter
    {
        get => _geodataCenter;
        set => _geodataCenter = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_geodataCenter\"");
    }

    [JsonPropertyName("geoarea")]
    public string Geoarea
    {
        get => _geoarea;
        set => _geoarea = value ?? throw new ArgumentNullException(nameof(value),
            "Ошибка инициализации \"_geoarea\"");
    }
    
    [JsonConstructor]
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

    public string ToCSV()
    {
        return $"\"{Id}\";\"{CulturalCenterName}\";\"{AdmArea}\";\"{District}\";\"{Address}\";\"{NumberOfAccessPoints}\";" +
               $"\"{WiFiName}\";\"{CoverageArea}\";\"{FunctionFlag}\";\"{AccessFlag}\";" +
               $"\"{Password}\";\"{LatitudeWGS84}\";\"{LongitudeWGS84}\";\"{GlobalId}\";\"{GeodataCenter}\";\"{Geoarea}\";";
    }
}