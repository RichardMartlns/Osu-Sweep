using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuSweep.Services.Localization
{
    public interface ILocalizationService
    {
        void SetLanguage(string cultureCode);
    }
}
