using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

using System.Runtime.InteropServices;

namespace AramDashboard
{
    internal static class Native
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint access, bool inherit, uint pid);
        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr handle);
        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr process, int infoClass,
            IntPtr buffer, int bufferLength, out int returnLength);

        public static string ReadProcessCommandLine(uint pid)
        {
            IntPtr process = OpenProcess(0x1000, false, pid);
            if (process == IntPtr.Zero) return null;
            IntPtr buffer = IntPtr.Zero;
            try
            {
                buffer = Marshal.AllocHGlobal(65536);
                int returned;
                if (NtQueryInformationProcess(process, 60, buffer, 65536, out returned) != 0) return null;
                int length = Marshal.ReadInt16(buffer, 0);
                IntPtr text = Marshal.ReadIntPtr(buffer, IntPtr.Size == 8 ? 8 : 4);
                return length > 0 && text != IntPtr.Zero ? Marshal.PtrToStringUni(text, length / 2) : null;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (buffer != IntPtr.Zero) Marshal.FreeHGlobal(buffer);
                CloseHandle(process);
            }
        }
    }

    public class MatchRow
    {
        public string Time { get; set; }
        public string Champion { get; set; }
        public string Result { get; set; }
        public string Kda { get; set; }
        public string Queue { get; set; }
        public string Duration { get; set; }
    }

    public static class LcuClient
    {
        public static Dictionary<int, string> ChampionNames = new Dictionary<int, string>();
        public static readonly string[] Lockfiles = new string[]
        {
            @"C:\Program Files (x86)\Tencent\LoL\LOL(26)\LeagueClient\lockfile",
            @"C:\Program Files\Tencent\LoL\LOL(26)\LeagueClient\lockfile",
            @"C:\Program Files (x86)\Tencent\LoL\LOL\LeagueClient\lockfile",
            @"C:\WeGameApps\英雄联盟\LeagueClient\lockfile",
            @"D:\WeGameApps\英雄联盟\LeagueClient\lockfile"
        };

        public static async Task<List<MatchRow>> GetRecentMatchesAsync(int count)
        {
            List<MatchRow> rows = new List<MatchRow>();
            int port;
            string token;
            if (!TryCredentials(out port, out token))
            {
                return rows;
            }
            try
            {
                using (HttpClientHandler handler = new HttpClientHandler())
                using (HttpClient client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    client.BaseAddress = new Uri("https://127.0.0.1:" + port + "/");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes("riot:" + token)));

                    string summonerJson = await client.GetStringAsync("lol-summoner/v1/current-summoner").ConfigureAwait(false);
                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    Dictionary<string, object> summoner = serializer.DeserializeObject(summonerJson) as Dictionary<string, object>;
                    if (summoner == null)
                    {
                        return rows;
                    }
                    string puuid = summoner.ContainsKey("puuid") ? Convert.ToString(summoner["puuid"]) : "";
                    if (string.IsNullOrEmpty(puuid))
                    {
                        return rows;
                    }

                    string url = "lol-match-history/v1/products/lol/" + Uri.EscapeDataString(puuid) +
                        "/matches?begIndex=0&endIndex=" + Math.Max(0, count - 1);
                    string json = await client.GetStringAsync(url).ConfigureAwait(false);
                    Dictionary<string, object> root = serializer.DeserializeObject(json) as Dictionary<string, object>;
                    if (root == null || !root.ContainsKey("games"))
                    {
                        return rows;
                    }
                    Dictionary<string, object> gamesRoot = root["games"] as Dictionary<string, object>;
                    if (gamesRoot == null || !gamesRoot.ContainsKey("games"))
                    {
                        return rows;
                    }
                    object[] games = gamesRoot["games"] as object[];
                    if (games == null)
                    {
                        return rows;
                    }
                    for (int i = 0; i < games.Length && rows.Count < count; i++)
                    {
                        Dictionary<string, object> game = games[i] as Dictionary<string, object>;
                        if (game == null) continue;
                        Dictionary<string, object> participant = FindParticipant(game, puuid);
                        if (participant == null) continue;
                        MatchRow row = new MatchRow();
                        long created;
                        if (game.ContainsKey("gameCreation") && long.TryParse(Convert.ToString(game["gameCreation"]), out created))
                        {
                            row.Time = DateTimeOffset.FromUnixTimeMilliseconds(created).LocalDateTime.ToString("MM-dd HH:mm");
                        }
                        else
                        {
                            row.Time = "--";
                        }
                        int duration;
                        row.Duration = game.ContainsKey("gameDuration") && int.TryParse(Convert.ToString(game["gameDuration"]), out duration)
                            ? (duration / 60).ToString() + " 分钟" : "--";
                        int championId = participant.ContainsKey("championId") ? Convert.ToInt32(participant["championId"]) : 0;
                        string championName;
                        row.Champion = championId > 0 && ChampionNames.TryGetValue(championId, out championName)
                            ? championName : ("英雄 " + championId);
                        row.Queue = game.ContainsKey("queueId") && Convert.ToString(game["queueId"]) == "2400" ? "海克斯大乱斗" : "其他模式";
                        Dictionary<string, object> stats = participant.ContainsKey("stats") ? participant["stats"] as Dictionary<string, object> : null;
                        if (stats != null)
                        {
                            int kills = ToInt(stats, "kills");
                            int deaths = ToInt(stats, "deaths");
                            int assists = ToInt(stats, "assists");
                            row.Kda = kills + " / " + deaths + " / " + assists;
                            row.Result = stats.ContainsKey("win") && Convert.ToBoolean(stats["win"]) ? "胜利" : "失败";
                        }
                        else
                        {
                            row.Kda = "--";
                            row.Result = "--";
                        }
                        rows.Add(row);
                    }
                }
            }
            catch
            {
            }
            return rows;
        }

        private static Dictionary<string, object> FindParticipant(Dictionary<string, object> game, string puuid)
        {
            object value;
            if (!game.TryGetValue("participants", out value))
            {
                return null;
            }
            object[] participants = value as object[];
            if (participants == null)
            {
                return null;
            }
            for (int i = 0; i < participants.Length; i++)
            {
                Dictionary<string, object> p = participants[i] as Dictionary<string, object>;
                if (p != null && p.ContainsKey("puuid") && Convert.ToString(p["puuid"]) == puuid)
                {
                    return p;
                }
            }
            return null;
        }

        private static int ToInt(Dictionary<string, object> map, string key)
        {
            object value;
            int result;
            return map.TryGetValue(key, out value) && int.TryParse(Convert.ToString(value), out result) ? result : 0;
        }

        private static bool TryCredentials(out int port, out string token)
        {
            port = 0;
            token = null;
            for (int i = 0; i < Lockfiles.Length; i++)
            {
                try
                {
                    if (!File.Exists(Lockfiles[i])) continue;
                    string text = File.ReadAllText(Lockfiles[i]).Trim();
                    string[] parts = text.Split(':');
                    int parsed;
                    if (parts.Length >= 5 && int.TryParse(parts[2], out parsed) && !string.IsNullOrEmpty(parts[3]))
                    {
                        port = parsed;
                        token = parts[3];
                        return true;
                    }
                }
                catch
                {
                }
            }

            try
            {
                Process[] processes = Process.GetProcessesByName("LeagueClientUx");
                if (processes.Length == 0) return false;
                string command = Native.ReadProcessCommandLine((uint)processes[0].Id);
                if (string.IsNullOrEmpty(command)) return false;
                System.Text.RegularExpressions.Match pm =
                    System.Text.RegularExpressions.Regex.Match(command, "--app-port=(\\d+)");
                System.Text.RegularExpressions.Match tm =
                    System.Text.RegularExpressions.Regex.Match(command, "--remoting-auth-token=([^\\s\"]+)");
                if (!pm.Success || !tm.Success) return false;
                if (!int.TryParse(pm.Groups[1].Value, out port)) return false;
                token = tm.Groups[1].Value;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
