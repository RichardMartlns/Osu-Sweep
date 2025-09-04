using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuSweep.Core.Models
{
    public class LanguageModel
    {
        public required string DisplayName { get; set; }

        public required string CultureCode { get; set; }

        public required string IconPath { get; set; }
    }
}
