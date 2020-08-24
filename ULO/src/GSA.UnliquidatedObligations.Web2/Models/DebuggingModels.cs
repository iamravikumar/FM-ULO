using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using RevolutionaryStuff.Core;

namespace GSA.UnliquidatedObligations.Web.Models
{
    public class SettingsModel
    {
        public IDictionary<string, string> GAS { get; private set; }
        public IDictionary<string, string> EnvironmentVariables { get; private set; }
        public IDictionary<string, string> Configs { get; private set; }
        public IDictionary<string, object> EnvironmentInfo => Program.EnvironmentInfo;
        public SettingsModel() { }
        public SettingsModel(IConfiguration config)
        {
            GAS = new Dictionary<string, string>();
            var gas = config.GetGsaAdministrativeSettings();
            foreach (var pi in gas.GetType().GetProperties(BindingFlags.Public|BindingFlags.Instance|BindingFlags.GetProperty))
            {
                GAS[pi.Name] = Stuff.ObjectToString(pi.GetValue(gas));
            }
            EnvironmentVariables = new Dictionary<string, string>();
            foreach (DictionaryEntry kvp in Environment.GetEnvironmentVariables())
            {
                EnvironmentVariables[kvp.Key.ToString()] = kvp.Value.ToString();
            }
            Configs = new Dictionary<string, string>(config.AsEnumerable());
        }
    }
}
