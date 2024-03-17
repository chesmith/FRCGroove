using RestSharp;

using FRCGroove.Lib.Models.Statboticsv2;

using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;

namespace FRCGroove.Lib
{
    public static class StatboticsAPIv2
    {
        private static readonly RestClient _client = new RestClient("https://api.statbotics.io/v2");
        public static string CacheFolder { get; set; }
        public static Dictionary<int, EPA> EPACache { get; set; }

        public static void InitializeEPACache()
        {
            if (CacheFolder.Length > 0)
            {
                string cachePath = $@"{CacheFolder}\EPACache.{DateTime.Now.Year}.json";
                if (!File.Exists(cachePath))
                {
                    EPACache = new Dictionary<int, EPA>();
                    List<EPA> epas = new List<EPA>();
                    int offset = 0;
                    while (true)
                    {
                        var request = new RestRequest($"/team_years?year={DateTime.Now.Year}&limit=100&offset={offset}");
                        var resp = _client.Execute(request);
                        List<EPA> results = JsonConvert.DeserializeObject<List<EPA>>(resp.Content);
                        if (results.Count == 0) break;
                        epas.AddRange(results);
                        offset += 100;
                    }
                    EPACache = epas.ToDictionary(v => v.team, v => v);
                    File.WriteAllText(cachePath, JsonConvert.SerializeObject(EPACache));
                }

                try
                {
                    string cachedData = File.ReadAllText(cachePath);
                    EPACache = JsonConvert.DeserializeObject<Dictionary<int, EPA>>(cachedData);
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
