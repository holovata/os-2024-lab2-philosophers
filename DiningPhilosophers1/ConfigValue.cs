using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;

namespace philosophers_try2
{
    public class ConfigValue
    {
        private static readonly ConfigValue instance = new ConfigValue();
        public static ConfigValue Inst => instance;
        private readonly Dictionary<string, string> _appSettings;

        private ConfigValue()
        {
            _appSettings = ConfigurationManager.AppSettings.AllKeys.ToDictionary(x => x, x => ConfigurationManager.AppSettings[x]);
        }

        public int PhilosopherCount => GetIntValue("Philosopher Count", 5);
        public int ForkCount => GetIntValue("Fork Count", PhilosopherCount);
        public int MaxPhilsophersToEatSimultaneously => GetIntValue("Max philosophers to eat simultaneously", 2);
        public int DurationPhilosophersEat => GetIntValue("Duration Allow Philosophers To Eat [seconds]", 20_000) * 1_000;
        public int MaxThinkDuration => GetIntValue("philosopher Max Think Duration [milliseconds]", 1000);
        public int MinThinkDuration => GetIntValue("philosopher Min Think Duration [milliseconds]", 50);
        public int DurationBeforeAskingPermissionToEat => GetIntValue("Duration Before Requesting Next Permission To Eat [milliseconds]", 20);

        private int GetIntValue(string key, int defaultValue)
        {
            string value = _appSettings.TryGetValue(key, out var val) ? val : null;

            if (!int.TryParse(value, out var intValue) || intValue <= 0)
            {
                Console.WriteLine($"Invalid or missing configuration value for '{key}'. Using default value: {defaultValue}");
                return defaultValue;
            }

            return intValue;
        }
    }
}
