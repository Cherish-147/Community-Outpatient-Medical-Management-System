using COMMS.Filters;
using System.Web;
using System.Web.Mvc;

namespace COMMS
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());

            filters.Add(new AuthorizeSessionAttribute());//全局应用
        }
    }
}
