using System.Globalization;


namespace OsuSweep.Services.Localization
{
    public class LocalizationService : ILocalizationService
    {
        public void SetLanguage(string cultureCode)
        {
            var culture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }
}
