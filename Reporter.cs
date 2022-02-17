namespace AfkParse;

using AfkParse.Model;

public class Reporter
{
    public static void writeReport(Session sesh, StreamWriter writer, string file)
    {
        var hoursInInstance = (sesh.ExitTime - sesh.EntryTime).TotalHours;
        writer.WriteLine($"AFK-like bounty hunting found in {file}");
        writer.WriteLine($"Site: {sesh.SiteName}");
        writer.WriteLine($"Entry time (local): {sesh.EntryTime}");
        writer.WriteLine($"Exit time (local): {sesh.ExitTime}");
        writer.WriteLine($"Exit via death: {sesh.Deaths}");
        writer.WriteLine($"Exit via disconnect: {sesh.Disconnects}");
        writer.WriteLine($"Number of pirate scans: {sesh.CargoScans:n0}");
        writer.WriteLine($"Number of pirate attacks: {sesh.PirateAttacks:n0}");
        writer.WriteLine($"Number of pirates scared off: {sesh.ScaredOff:n0} ({100.0 * sesh.ScaredOff / sesh.CargoScans:n}% of scans)");
        writer.WriteLine($"Scans per hour: {sesh.CargoScans / hoursInInstance:n1}");
        writer.WriteLine($"Attacks per hour: {sesh.PirateAttacks / hoursInInstance:n1}");
        writer.WriteLine($"Attacks per scan: {(float)sesh.PirateAttacks / sesh.CargoScans:n2}");
        writer.WriteLine($"Number of bounties awarded: {sesh.BountyCount:n0}");
        writer.WriteLine($"Total value of bounties: {sesh.TotalBounties:n0}");
        writer.WriteLine($"Bounty per scan: {sesh.TotalBounties / sesh.CargoScans:n0}");
        writer.WriteLine($"Bounty per attack: {sesh.TotalBounties / sesh.PirateAttacks:n0}");
        writer.WriteLine($"Bounty per hour: {sesh.TotalBounties / hoursInInstance:n0}");
        writer.WriteLine($"Average bounty value: {sesh.TotalBounties / sesh.BountyCount:n0}");
        writer.WriteLine($"Destroyed ship counts:");

        foreach (var ship in sesh.ShipCount)
        {
            writer.WriteLine($"{ship.Key}: {ship.Value}");
        }
        writer.WriteLine("");
    }
}