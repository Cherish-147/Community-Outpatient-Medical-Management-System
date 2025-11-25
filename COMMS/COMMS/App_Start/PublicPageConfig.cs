using Antlr.Runtime.Misc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace COMMS.App_Start
{
    public class PublicPageConfig
    {
        private static Dictionary<string, HashSet<string>> _publicPages;

        static PublicPageConfig()
        {
            LoadConfig();
        }
        private static void LoadConfig()
        {

            string path = HttpContext.Current.Server.MapPath("~/App_Data/publicPages.json");

            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);

                var tempDict = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
                //转换List<string>为HashSet<string>
                _publicPages = new Dictionary<string, HashSet<string>>();
                foreach (var kvp in tempDict)
                { 
                _publicPages[kvp.Key] = new HashSet<string>(kvp.Value);
                }
            }
            else
            {
                _publicPages =new Dictionary<string, HashSet<string>>();
            }
        }
        public static bool IsPublicPage(string controller, string action) 
        { 
            return _publicPages.ContainsKey(controller) &&_publicPages[controller].Contains(action);
        }

    }
}