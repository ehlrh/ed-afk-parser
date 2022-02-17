namespace AfkParse.Model;

using System.Collections.Concurrent;

public class Session
{

    public Session()
    {
        SiteName = "";
        EntryTime = new DateTime();
        Deaths = 0;
        Disconnects = 0;
        CargoScans = 0;
        PirateAttacks = 0;
        ScaredOff = 0;
        BountyCount = 0;
        TotalBounties = 0;
        BountyTargets = new List<Ship>();
        FighterDeaths = new List<DateTime>();
        ShipCount = new Dictionary<string, int>();
        AttackTimes = new ConcurrentDictionary<DateTime, int>();
        ScanTimes = new ConcurrentDictionary<DateTime, int>();
    }

    public string SiteName { get; set; }
    public DateTime EntryTime { get; set; }
    public DateTime ExitTime { get; set; }
    public int Deaths { get; set; }
    public int Disconnects { get; set; }
    public int CargoScans { get; set; }
    public int PirateAttacks { get; set; }
    public int ScaredOff { get; set; }
    public int BountyCount { get; set; }
    public int TotalBounties { get; set; }
    public List<Ship> BountyTargets { get; private set; }
    public List<DateTime> FighterDeaths { get; private set; }
    public Dictionary<string, int> ShipCount { get; private set; }
    public ConcurrentDictionary<DateTime, int> AttackTimes { get; private set; }
    public ConcurrentDictionary<DateTime, int> ScanTimes { get; private set; }
}