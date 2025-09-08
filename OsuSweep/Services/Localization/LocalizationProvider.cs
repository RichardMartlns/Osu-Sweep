using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuSweep.Services.Localization
{
    public class LocalizationProvider
    {
        public string this[string key] => Resources.Strings.ResourceManager.GetString(key) ?? $"[{key}]";
    }
}
