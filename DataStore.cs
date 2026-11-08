using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace AramDashboard
{
    public class ChampionRow
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public int Rank { get; set; }
        public double WinRate { get; set; }
        public double PickRate { get; set; }
    }

    public class AugmentRow
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Rarity { get; set; }
        public int Score { get; set; }
        public string Rating { get; set; }
        public double WinRate { get; set; }
        public string Source { get; set; }
        public int Games { get; set; }
    }

    public class DataStore
    {
        public string DataDir { get; private set; }
        public string DataDate { get; private set; }
        public int ChampionCount { get; private set; }
        public int AugmentCount { get; private set; }
        public List<ChampionRow> Champions { get; private set; }
        public Dictionary<int, List<AugmentRow>> ByChampion { get; private set; }
        public List<AugmentRow> TopAugments { get; private set; }

        public DataStore(string baseDir)
        {
            DataDir = Path.Combine(baseDir, "data");
            Champions = new List<ChampionRow>();
            ByChampion = new Dictionary<int, List<AugmentRow>>();
            TopAugments = new List<AugmentRow>();
            Load();
        }

        public void Load()
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;

            Dictionary<int, string> names = new Dictionary<int, string>();
            string championPath = Path.Combine(DataDir, "champions.json");
            if (File.Exists(championPath))
            {
                object[] rows = serializer.DeserializeObject(File.ReadAllText(championPath)) as object[];
                if (rows != null)
                {
                    for (int i = 0; i < rows.Length; i++)
                    {
                        Dictionary<string, object> row = rows[i] as Dictionary<string, object>;
                        if (row == null) continue;
                        int id = Convert.ToInt32(row["id"]);
                        names[id] = Convert.ToString(row["name"]);
                    }
                }
            }

            string latestPath = Path.Combine(DataDir, "latest.json");
            if (File.Exists(latestPath))
            {
                Dictionary<string, object> root = serializer.DeserializeObject(File.ReadAllText(latestPath)) as Dictionary<string, object>;
                if (root != null)
                {
                    DataDate = Convert.ToString(root.ContainsKey("date") ? root["date"] : "");
                    object[] list = root["champions"] as object[];
                    if (list != null)
                    {
                        for (int i = 0; i < list.Length; i++)
                        {
                            Dictionary<string, object> row = list[i] as Dictionary<string, object>;
                            if (row == null) continue;
                            ChampionRow item = new ChampionRow();
                            item.Id = Convert.ToInt32(row["championId"]);
                            item.Name = names.ContainsKey(item.Id) ? names[item.Id] : ("英雄 " + item.Id);
                            item.Rank = Convert.ToInt32(row["rank"]);
                            item.WinRate = Convert.ToDouble(row["winRate"]);
                            item.PickRate = Convert.ToDouble(row["pickRate"]);
                            Champions.Add(item);
                        }
                    }
                }
            }
            Champions.Sort(delegate(ChampionRow a, ChampionRow b) { return a.Rank.CompareTo(b.Rank); });
            ChampionCount = Champions.Count;

            string fitPath = Path.Combine(DataDir, "augment-fit.json");
            if (File.Exists(fitPath))
            {
                Dictionary<string, object> root = serializer.DeserializeObject(File.ReadAllText(fitPath)) as Dictionary<string, object>;
                if (root != null && root.ContainsKey("champions"))
                {
                    Dictionary<string, object> champions = root["champions"] as Dictionary<string, object>;
                    Dictionary<int, AugmentRow> unique = new Dictionary<int, AugmentRow>();
                    foreach (KeyValuePair<string, object> pair in champions)
                    {
                        int championId = Convert.ToInt32(pair.Key);
                        object[] rows = pair.Value as object[];
                        List<AugmentRow> list = new List<AugmentRow>();
                        if (rows != null)
                        {
                            for (int i = 0; i < rows.Length; i++)
                            {
                                Dictionary<string, object> row = rows[i] as Dictionary<string, object>;
                                if (row == null) continue;
                                AugmentRow item = new AugmentRow();
                                item.Id = Convert.ToInt32(row["augmentId"]);
                                item.Name = Convert.ToString(row["name"]);
                                item.Rarity = Convert.ToString(row["rarity"]);
                                item.Score = Convert.ToInt32(row["score"]);
                                item.Rating = Convert.ToString(row["rating"]);
                                item.WinRate = row.ContainsKey("resgWinRate") && row["resgWinRate"] != null
                                    ? Convert.ToDouble(row["resgWinRate"]) : 0.0;
                                item.Source = Convert.ToString(row["source"]);
                                item.Games = row.ContainsKey("resgGames") ? Convert.ToInt32(row["resgGames"]) : 0;
                                list.Add(item);
                                AugmentRow existing;
                                if (!unique.TryGetValue(item.Id, out existing) || item.Score > existing.Score)
                                {
                                    unique[item.Id] = item;
                                }
                            }
                        }
                        ByChampion[championId] = list;
                    }
                    TopAugments.AddRange(unique.Values);
                    TopAugments.Sort(delegate(AugmentRow a, AugmentRow b) { return b.Score.CompareTo(a.Score); });
                    AugmentCount = TopAugments.Count;
                }
            }
        }
    }
}
