using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using System.Web.Mvc;

namespace Kululu.Web.Startup.Binders
{
    public class CommonDateTime
    {
        public static DateTime ParseDate(ValueProviderResult value)
        {

            // Create an array of all supported standard date and time format specifiers.
            string[] formats = {"d", "D", "f", "F", "g", "G", "m", "o", "r", 
                          "s", "t", "T", "u", "U", "Y", "dd/MM/yyyy hh:mm:ss", "MM/dd/yyyy hh:mm:ss"};

            // 
            CultureInfo[] cultures = {CultureInfo.CreateSpecificCulture("de-DE"), 
                                CultureInfo.CreateSpecificCulture("en-US"), 
                                CultureInfo.CreateSpecificCulture("he-IL"),
                                CultureInfo.CreateSpecificCulture("es-ES"), 
                                CultureInfo.CreateSpecificCulture("fr-FR")};

            var parsedSuccesfuly = false;
            int cultureIndex = 0;
            DateTime foundDateTime = DateTime.MinValue;
            while (!parsedSuccesfuly && cultureIndex < cultures.Length)
            {
                parsedSuccesfuly = DateTime.TryParseExact(value.AttemptedValue, formats, cultures[cultureIndex], DateTimeStyles.None, out foundDateTime);
                cultureIndex++;
            }

            if (parsedSuccesfuly)
            {
                return foundDateTime;
            }
            throw new FormatException("Could not parse the specified date, culture is not supported");
        
        }
    }
}