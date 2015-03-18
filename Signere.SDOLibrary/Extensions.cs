using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Signere.SDOLibrary
{
    public static class Extensions
    {
        public static Name GetFirstNameAndLastName(this BankIDSignature signature) {
            Name retur = new Name();
            var temptable = signature.Name.Split(',');
            retur.FirstName = temptable.Last().Trim();
            retur.LastName = temptable.First().Trim();

            TextInfo textInfo = new CultureInfo("nb-NO", false).TextInfo;
            retur.FirstName = textInfo.ToTitleCase(retur.FirstName.ToLowerInvariant());
            retur.LastName = textInfo.ToTitleCase(retur.LastName.ToLowerInvariant());
            return retur;
        }
      
    }

    internal static class InternalExtensions
    {
        private static string[] SplitStringOnCommaNotQouted(this string thastring)
        {
            return Regex.Split(thastring, ",(?=(?:[^']*'[^']*')*[^']*$)");
        }

        public static Dictionary<string, string> SplitString(this string text) {            
            var result = System.Text.RegularExpressions.Regex.Split(text, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");          
            Dictionary<string, string> list = new Dictionary<string, string>();
            foreach (var item in result) {
                var tmp = item.Split('=');
                string key = tmp.First().TrimEnd(' ').TrimStart(' ');
                string value = tmp.Last().Replace("\"", "").TrimEnd(' ').TrimStart(' ');
                list.Add(key, value);
            }

            return list;

        }

    
        public static string SerialNumber(this string certificate)
        {        
            var values = certificate.SplitString();

            return values["SERIALNUMBER"];
        }

        public static string Name(this string certificate)
        {
            var values = certificate.SplitString();

            return values["CN"];
        }

        public static string BankName(this string certificate) {
            var values = certificate.SplitString();
            return values["CN"].Replace("BankID ", string.Empty).Replace("Bank CA 2", string.Empty).TrimEnd(' ');
        }

        public static string Organization(this string certificate) {
            var values = certificate.SplitString();
            string retur=null;
            values.TryGetValue("OU", out retur);

            return retur;
        }

       
    }

    public class Name
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
