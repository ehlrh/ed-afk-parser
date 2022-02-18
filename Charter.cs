namespace AfkParse;

using AfkParse.Model;

public static class Charter
{
    private static ZedGraph.GraphPane pane = new ZedGraph.GraphPane();

    public static void drawChart(Session sesh)
    {
        pane.Title.Text = $"{sesh.SiteName} system, {sesh.EntryTime.ToUniversalTime()} (UTC) for {sesh.ExitTime.Subtract(sesh.EntryTime).TotalMinutes:n1} mins";
        pane.XAxis.Title.Text = "minutes";
        pane.YAxis.Title.Text = "number";

        pane.CurveList.Clear();
        pane.AddCurve(label: "Pirate Scans",
            sesh.ScanTimes.Keys.Select<DateTime, double>((x, y) => x.Subtract(sesh.EntryTime).TotalMinutes).ToArray(),
            sesh.ScanTimes.Values.Select(x => (double)x).ToArray(),
            System.Drawing.Color.Blue
        )
        .Line.IsVisible = false;
        pane.AddCurve(label: "Pirate Attacks",
            sesh.AttackTimes.Keys.Select<DateTime, double>((x, y) => x.Subtract(sesh.EntryTime).TotalMinutes).ToArray(),
            sesh.AttackTimes.Values.Select(x => (double)x).ToArray(),
            System.Drawing.Color.Red
        )
        .Line.IsVisible = false;

        pane.AxisChange();
        pane.GetImage(1920, 1080, 250, true).Save($"output/{sesh.SiteName}-{(sesh.EntryTime.ToFileTime()) / 100000000}.png");
    }
}