namespace AfkParse;

using System.Text.RegularExpressions;

public class Helpers
{
    public class EventType
    {
        private EventType(string value)
        {
            Value = value;
        }
        public string Value { get; }

        public static EventType NEW_INSTANCE { get { return new EventType("SupercruiseExit"); } }
        public static EventType END_INSTANCE { get { return new EventType("SupercruiseEntry"); } }
        public static EventType SHIP_DEATH { get { return new EventType("Died"); } }
        public static EventType BOUNTY_AWARDED { get { return new EventType("Bounty"); } }
        public static EventType SCAN { get { return new EventType("Scanned"); } }
        public static EventType FSS_SIGNAL { get { return new EventType("FSSSignalDiscovered"); } }
        public static EventType SHUTDOWN { get { return new EventType("Shutdown"); } }
        public static EventType JUMP_OUT { get { return new EventType("StartJump"); } }
    }

    public static DateTime timestamp(string line)
    {
        var match = Regex.Match(line, "timestamp..\"(.*?)\"");
        return DateTime.Parse(match.Groups[1].Value);
    }

    public static bool isEventType(string line, EventType type)
    {
        return line.Contains($"\"event\":\"{type.Value}\"");
    }

    public static bool isResDetection(string line)
    {
        if (line.Contains("$MULTIPLAYER_SCENARIO14_TITLE")
        || line.Contains("$MULTIPLAYER_SCENARIO77_TITLE")
        || line.Contains("$MULTIPLAYER_SCENARIO78_TITLE")
        || line.Contains("$MULTIPLAYER_SCENARIO79_TITLE"))
        {
            return true;
        }
        return false;
    }

    public static string getStringValue(string line, string valueName)
    {
        var match = Regex.Match(line, $"{valueName}..\"(.*?)\"");
        return match.Groups[1].Value;
    }

    public static int getIntValue(string line, string valueName)
    {
        var match = Regex.Match(line, $"{valueName}..(.*?),");
        return Int32.Parse(match.Groups[1].Value);
    }
}