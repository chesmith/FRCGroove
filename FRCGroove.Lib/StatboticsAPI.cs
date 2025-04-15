using RestSharp;

using FRCGroove.Lib.Models.Statbotics;

using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Diagnostics;
using System.Text.Json;
using System.Text;

namespace FRCGroove.Lib
{
    public static class StatboticsAPI
    {
        private static readonly RestClient _client = new RestClient("https://api.statbotics.io/v3");
        public static string CacheFolder { get; set; }
        public static Dictionary<int, Statbotics_v3> EPACache { get; set; }

        public static void InitializeEPACache()
        {
            if (CacheFolder.Length > 0)
            {
                string cachePath = $@"{CacheFolder}\EPACache.{DateTime.Now.Year}.json";
                if (!File.Exists(cachePath))
                {
                    EPACache = new Dictionary<int, Statbotics_v3>();
                    List<Statbotics_v3> epas = new List<Statbotics_v3>();
                    int offset = 0;
                    while (true)
                    {
                        Debug.WriteLine($"{DateTime.Now:s} Initializing Statbotics Cache - Getting offset {offset}");

                        var request = new RestRequest($"/team_years?year={DateTime.Now.Year}&limit=500&offset={offset}");
                        var resp = _client.Execute(request);
                        List<Statbotics_v3> results = JsonSerializer.Deserialize<List<Statbotics_v3>>(resp.Content);
                        if (results.Count == 0) break;
                        epas.AddRange(results);
                        offset += 500;

                        Debug.WriteLine($"{DateTime.Now:s} Initializing Statbotics Cache - Got " + results.Count + " results");
                    }
                    EPACache = epas.ToDictionary(v => v.team, v => v);
                    File.WriteAllText(cachePath, JsonSerializer.Serialize(EPACache));

                    string csvPath = $@"{CacheFolder}\EPACache.{DateTime.Now.Year}.csv";
                    var s = EPACache.Select(e => $"{e.Key},{e.Value.epa.breakdown.auto_points},{e.Value.epa.breakdown.teleop_points},{e.Value.epa.breakdown.endgame_points},{e.Value.epa.breakdown.total_points}");
                    File.WriteAllText(csvPath, String.Join("\n", s), Encoding.Unicode);
                }

                try
                {
                    string cachedData = File.ReadAllText(cachePath);
                    EPACache = JsonSerializer.Deserialize<Dictionary<int, Statbotics_v3>>(cachedData);
                }
                catch (Exception) { /* do nothing */ }
            }
        }

        public static void ResetEPACache()
        {
            //TODO: perhaps automate resetting EPA cache once per day during off hours (how?)
            string cachePath = $@"{CacheFolder}\EPACache.{DateTime.Now.Year}.json";
            if (File.Exists(cachePath))
            {
                File.Move(cachePath, $@"{CacheFolder}\EPACache.{DateTime.Now.ToString("yyyy-MM-dd.HH-mm-ss")}.json");
            }

            InitializeEPACache();
        }
    }
}
