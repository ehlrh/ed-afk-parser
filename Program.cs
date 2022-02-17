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
                                sesh.ExitTime = Helpers.timestamp(line);

                                grandBounties += sesh.Bounties;
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