using AfkParse.Model;
using AfkParse;

if (!Directory.Exists("output"))
{
    Directory.CreateDirectory("output");
}

var logFileList = Directory.EnumerateFiles(Environment.ExpandEnvironmentVariables(@"%UserProfile%\Saved Games\Frontier Developments\Elite Dangerous\"))
    .Where(x => x.Contains("Journal") && x.EndsWith(".log"));

using (StreamWriter writer = new StreamWriter("output/afk-bounty-rollup.txt"))
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

                if (reader.Peek() == -1)
                {
                    continue;
                }

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
                        if (Helpers.isEventType(line, Helpers.EventType.FIGHTER_DEATH))
                        {
                            sesh.FighterDeaths.Add(Helpers.timestamp(line));
                        }
                        if (Helpers.isEventType(line, Helpers.EventType.SHUTDOWN) || reader.Peek() == -1)
                        {
                            sesh.Disconnects++;
                        }
                        if (line.Contains("$Pirate_OnStartScanCargo"))
                        {
                            sesh.CargoScans++;
                            sesh.ScanTimes.AddOrUpdate(Helpers.timestamp(line), sesh.CargoScans, (_, _) => sesh.CargoScans);
                        }
                        if (line.Contains("$Pirate_OnDeclarePiracyAttack"))
                        {
                            sesh.PirateAttacks++;
                            sesh.AttackTimes.AddOrUpdate(Helpers.timestamp(line), sesh.PirateAttacks, (_, _) => sesh.PirateAttacks);
                        }
                        if (line.Contains("$Pirate_ThreatTooHigh"))
                        {
                            sesh.ScaredOff++;
                        }
                        if (Helpers.isEventType(line, Helpers.EventType.BOUNTY_AWARDED))
                        {
                            sesh.BountyCount++;
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
                        || Helpers.isEventType(line, Helpers.EventType.JUMP_OUT)
                        || Helpers.isEventType(line, Helpers.EventType.SHIP_DEATH)
                        || Helpers.isEventType(line, Helpers.EventType.SHUTDOWN)
                        || reader.Peek() == -1)
                        {
                            if (sesh.CargoScans > 10 && sesh.PirateAttacks > 10 && sesh.BountyCount > 10 && sesh.BountyCount < sesh.PirateAttacks * 1.5)
                            {
                                sesh.ExitTime = Helpers.timestamp(line);

                                grandBounties += sesh.BountyCount;
                                grandTotalBounties += sesh.TotalBounties;
                                if (sesh.ScaredOff > 0)
                                {
                                    grandTotalScarable += sesh.CargoScans;
                                    grandTotalScared += sesh.ScaredOff;
                                }

                                Reporter.writeReport(sesh, writer, file);
                                Charter.drawChart(sesh);
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