﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Codexzier.Wpf.ApplicationFramework.Components.UserSettings;
using Newtonsoft.Json;
using WpfAppTemplateForNuget.Components.Data;
using WpfAppTemplateForNuget.Components.UserSettings;

namespace WpfAppTemplateForNuget.Components.RkiCoronaLandkreise
{
    public class RkiCoronaLandkreiseComponent
    {
        public delegate void RkiDataErrorEventHandler(string message);

        private const string UrlGenCasesDeathsWeekIncidence =
            "https://services7.arcgis.com/mOBPykOjAyBO2ZKk/arcgis/rest/services/RKI_Landkreisdaten/FeatureServer/0/query?where=1%3D1&outFields=cases,deaths,county,last_update,cases7_per_100k,death_rate,GEN&returnGeometry=false&outSR=4326&f=json";

        private static RkiCoronaLandkreiseComponent _singleton;

        private RkiCoronaLandkreiseComponent()
        {
        }

        public static RkiCoronaLandkreiseComponent GetInstance()
        {
            return _singleton ??= new RkiCoronaLandkreiseComponent();
        }

        public Landkreise LoadData(out Action<bool> saveIf, bool loadForceFromInternet = false)
        {
            saveIf = null;
            var filename = HelperExtension.CreateFilename();

            var usl = UserSettingsLoader<CustomSettingsFile>.GetInstance(SerializeHelper.Serialize,
                SerializeHelper.Deserialize);

            if (!usl.Load().LoadRkiDataByApplicationStart &&
                !loadForceFromInternet)
                filename = GetLastLoadedData();

            if (!string.IsNullOrEmpty(filename) && File.Exists(filename))
            {
                var reload = this.LoadLocalOrReloadOnlineFromRki(filename);
                if (reload != null) return reload;
            }

            if (string.IsNullOrEmpty(filename))
                return new Landkreise
                {
                    Date = DateTime.MinValue,
                    Districts = new List<Landkreis>()
                };

            var result = this.ConvertToLandkreise(this.LoadActualData());
            if (result == null) return null;

            if (filename == HelperExtension.CreateFilename())
            {
                if (!File.Exists(filename))
                {
                    this.SaveToFile(result, filename);
                }
                else
                {
                    if (IsDifferent(filename, result))
                        saveIf = canSave =>
                        {
                            if (!canSave) return;
                            this.SaveToFile(result, filename);
                        };
                }
            }

            return result;
        }

        private static bool IsDifferent(string filename, Landkreise landkreise)
        {
            var rawResult = JsonConvert.SerializeObject(landkreise);
            var localRaw = File.ReadAllText(filename);
            return localRaw.Length != rawResult.Length;
        }

        private static string GetLastLoadedData()
        {
            var last = HelperExtension.GetFiles().Select(s => new FileInfo(s)).OrderBy(w =>
            {
                var dateStr = w.FullName.GetDate();
                if (DateTime.TryParse(dateStr, out var dt)) return dt;

                return DateTime.MinValue;
            }).ToArray();

            return !last.Any() ? string.Empty : last.Last().FullName;
        }

        private Landkreise LoadLocalOrReloadOnlineFromRki(string filename)
        {
            var t = this.LoadFromFile(filename);

            var actualDateTime = DateTime.Now.ToShortDateString();
            var lastUpdateTime = t.Date.ToShortDateString();

            if (actualDateTime.Equals(lastUpdateTime))
            {
                var resultFromFile = this.LoadFromFile(filename);

                if (resultFromFile
                    .Date
                    .ToShortTimeString()
                    .Equals(actualDateTime))
                    return resultFromFile;
            }

            return null;
        }

        private Landkreise ConvertToLandkreise(RkiCoronaLandkreiseResult result)
        {
            var lk = result.features.FirstOrDefault();
            if (lk == null) return new Landkreise();

            var strLastUpdate = lk.attributes.last_update.RemoveTimeFromLastUpdateString();
            if (DateTime.TryParse(strLastUpdate, out var lastUpdate))
            {
                var landkreise = new Landkreise
                {
                    Date = lastUpdate,
                    Districts = new List<Landkreis>()
                };

                foreach (var item in result.features)
                    landkreise.Districts.Add(new Landkreis
                    {
                        Name = item.attributes.GEN,
                        WeekIncidence = item.attributes.cases7_per_100k,
                        Cases = item.attributes.cases,
                        Deaths = item.attributes.deaths
                    });

                return landkreise;
            }

            return new Landkreise();
        }

        internal Landkreise LoadFromFile(string filename)
        {
            return JsonConvert.DeserializeObject<Landkreise>(File.ReadAllText(filename));
        }

        internal void SaveToFile(Landkreise landkreise, string filename)
        {
            var contentStr = JsonConvert.SerializeObject(landkreise);

            File.WriteAllText(filename, contentStr);
        }

        private RkiCoronaLandkreiseResult LoadActualData()
        {
            var client = new WebClient
            {
                Headers = {[HttpRequestHeader.ContentType] = "application/json"}
            };

            var rawResult = client.DownloadString(UrlGenCasesDeathsWeekIncidence);

            if (rawResult.ToLower().Contains("error") || string.IsNullOrEmpty(rawResult))
            {
                this.RkiDataErrorEvent?.Invoke("Fehler beim abrufen der Daten.");
                return null;
            }

            return JsonConvert.DeserializeObject<RkiCoronaLandkreiseResult>(rawResult);
        }

        public event RkiDataErrorEventHandler RkiDataErrorEvent;
    }
}