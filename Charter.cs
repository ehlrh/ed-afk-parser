namespace AfkParse;

using AfkParse.Model;
using System.Drawing;

public static class Charter
{
    private static ZedGraph.GraphPane pane = new ZedGraph.GraphPane();

    public static void drawChart(Session sesh)
    {
        pane.Title.Text = $"{sesh.SiteName} system, {sesh.EntryTime.ToUniversalTime()} (UTC) for {sesh.ExitTime.Subtract(sesh.EntryTime).TotalMinutes:n1} mins";
        pane.XAxis.Title.Text = "minutes";
        pane.YAxis.Title.Text = "number";

        pane.CurveList.Clear();
        pane.AddCurve("Pirate Scans",
            sesh.ScanTimes.Keys.Select((x, y) => x.Subtract(sesh.EntryTime).TotalMinutes).ToArray(),
            sesh.ScanTimes.Values.Select(x => (double)x).ToArray(),
            Color.Blue
        )
        .Line.IsVisible = false;
        pane.AddCurve("Pirate Attacks",
            sesh.AttackTimes.Keys.Select((x, y) => x.Subtract(sesh.EntryTime).TotalMinutes).ToArray(),
            sesh.AttackTimes.Values.Select(x => (double)x).ToArray(),
            Color.Red
        )
        .Line.IsVisible = false;
        if (sesh.FighterDeaths.Count != 0)
        {
            pane.AddCurve("Marks a Fighter Death",
                sesh.FighterDeaths.Select(x => x.Subtract(sesh.EntryTime).TotalMinutes).ToArray(),
                Enumerable.Repeat(25d, sesh.FighterDeaths.Count).ToArray(),
                Color.Black,
                ZedGraph.SymbolType.XCross
            )
            .Line.IsVisible = false;
        }

        pane.AxisChange();
        pane.GetImage(1920, 1080, 250, true).Save($"output/{sesh.SiteName}-{(sesh.EntryTime.ToFileTime()) / 100000000}.png");
    }
}