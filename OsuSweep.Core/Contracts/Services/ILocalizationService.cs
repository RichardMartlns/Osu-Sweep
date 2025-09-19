using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuSweep.Core.Contracts.Services
{
    public interface ILocalizationService
    {
        void SetLanguage(string cultureCode);
    }
}
