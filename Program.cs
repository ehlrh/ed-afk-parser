var logFileList = Directory.EnumerateFiles(Environment.ExpandEnvironmentVariables(@"%UserProfile%\Saved Games\Frontier Developments\Elite Dangerous\"))
    .Where(x => x.Contains("Journal") && x.EndsWith(".log"));

using (StreamWriter writer = new StreamWriter("afk-bounty-rollup.txt"))
{
    long grandTotalBounties = 0;
    long grandBounties = 0;
    long grandTotalScared = 0;
    long grandTotalScarable = 0;

    foreach (string file in logFileList)
    {
        try
        {
            var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (StreamReader reader = new StreamReader(fileStream))
            {
                bool inRing = false;
                var sesh = new Session();
                string line;
                
                while (!String.IsNullOrEmpty(line = reader.ReadLine()))
                {
                    if (!inRing)
                    {
                        if (Helpers.isEventType(line, Helpers.EventType.NEW_INSTANCE)
                        && Helpers.getStringValue(line, "BodyType").Equals("PlanetaryRing"))
                        {
                            inRing = true;

                            sesh.SiteName = Helpers.getStringValue(line, "StarSystem");
                            sesh.EntryTime = Helpers.timestamp(line);
                        }
                    }
                    else
                    {

                        if (Helpers.isEventType(line, Helpers.EventType.SHIP_DEATH))
                        {
                            sesh.Deaths++;
                        }
                        if (reader.Peek() == -1)
                        {
                            sesh.Disconnects++;
                        }
                        if (line.Contains("$Pirate_OnStartScanCargo"))
                        {
                            sesh.CargoScans++;
                        }
                        if (line.Contains("$Pirate_OnDeclarePiracyAttack"))
                        {
                            sesh.PirateAttacks++;

                        }
                        if (line.Contains("$Pirate_ThreatTooHigh"))
                        {
                            sesh.ScaredOff++;
                        }
                        if (Helpers.isEventType(line, Helpers.EventType.BOUNTY_AWARDED))
                        {
                            sesh.Bounties++;
                            sesh.TotalBounties += Helpers.getIntValue(line, "TotalReward");

                            var shipname = Helpers.getStringValue(line, "Target");
                            if (sesh.ShipCount.ContainsKey(shipname))
                            {
                                sesh.ShipCount[shipname]++;
                            }
                            else
                            {
                                sesh.ShipCount[shipname] = 1;
                            }
                        }
                        if (Helpers.isEventType(line, Helpers.EventType.END_INSTANCE)
                        || Helpers.isEventType(line, Helpers.EventType.SHIP_DEATH)
                        || reader.Peek() == -1)
                        {
                            if (sesh.CargoScans > 10 && sesh.PirateAttacks > 10 && sesh.Bounties > 10 && sesh.Bounties < sesh.PirateAttacks * 1.5)
                            {
                                var exitTime = Helpers.timestamp(line);

                                grandBounties += sesh.Bounties;
                                grandTotalBounties += sesh.TotalBounties;
                                if (sesh.ScaredOff > 0)
                                {
                                    grandTotalScarable += sesh.CargoScans;
                                    grandTotalScared += sesh.ScaredOff;
                                }

                                var hoursInInstance = (exitTime - sesh.EntryTime).TotalHours;
                                writer.WriteLine($"AFK-like bounty hunting found in {file}");
                                writer.WriteLine($"Site: {sesh.SiteName}");
                                writer.WriteLine($"Entry time (local): {sesh.EntryTime}");
                                writer.WriteLine($"Exit time (local): {exitTime}");
                                writer.WriteLine($"Exit via death: {sesh.Deaths}");
                                writer.WriteLine($"Exit via disconnect: {sesh.Disconnects}");
                                writer.WriteLine($"Number of pirate scans: {sesh.CargoScans:n0}");
                                writer.WriteLine($"Number of pirate attacks: {sesh.PirateAttacks:n0}");
                                writer.WriteLine($"Number of pirates scared off: {sesh.ScaredOff:n0} ({100.0 * sesh.ScaredOff / sesh.CargoScans:n}% of scans)");
                                writer.WriteLine($"Scans per hour: {sesh.CargoScans / hoursInInstance:n1}");
                                writer.WriteLine($"Attacks per hour: {sesh.PirateAttacks / hoursInInstance:n1}");
                                writer.WriteLine($"Attacks per scan: {(float)sesh.PirateAttacks / sesh.CargoScans:n2}");
                                writer.WriteLine($"Number of bounties awarded: {sesh.Bounties:n0}");
                                writer.WriteLine($"Total value of bounties: {sesh.TotalBounties:n0}");
                                writer.WriteLine($"Bounty per scan: {sesh.TotalBounties / sesh.CargoScans:n0}");
                                writer.WriteLine($"Bounty per attack: {sesh.TotalBounties / sesh.PirateAttacks:n0}");
                                writer.WriteLine($"Bounty per hour: {sesh.TotalBounties / hoursInInstance:n0}");
                                writer.WriteLine($"Average bounty value: {sesh.TotalBounties / sesh.Bounties:n0}");
                                writer.WriteLine($"Destroyed ship counts:");
                                foreach (var ship in sesh.ShipCount)
                                {
                                    writer.WriteLine($"{ship.Key}: {ship.Value}");
                                }
                                writer.WriteLine("");
                            }

                            inRing = false;
                            sesh = new Session();
                        }
                    }
                }
            }
        }
        catch (IOException) { }
    }

    writer.WriteLine($"Percent of pirates scared off (where at least one pirate was scared off): {grandTotalScared:n0}/{grandTotalScarable:n0}={100.0 * grandTotalScared / grandTotalScarable:n}%");
    writer.WriteLine($"Overall bounty count: {grandBounties:n0}");
    writer.WriteLine($"Overall bounty total: {grandTotalBounties:n0}");
    writer.WriteLine($"Overall average bounty: {grandTotalBounties / grandBounties:n0}");
}