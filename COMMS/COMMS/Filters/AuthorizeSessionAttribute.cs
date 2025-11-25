using COMMS.App_Start;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace COMMS.Filters
{
    public class AuthorizeSessionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            var action = filterContext.ActionDescriptor.ActionName;
            var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            //如果当前控制器在公开页面配置中，并且当前Action是公开页面，直接返回
            //if (PublicPages.ContainsKey(controller) && PublicPages[controller].Contains(action) { return; }

            //使用封装类判断是否公开页面
            if (PublicPageConfig.IsPublicPage(controller, action) ) { return; }            
            //如果未登录，跳转到Login页面
            if (session["UserID"] == null)
            {
                filterContext.Result = new RedirectResult("/Home/Login");
               
            } 
            base.OnActionExecuting(filterContext);

        }

        
        //定义公开页面配置：控制器->公开的Action列表
        private static readonly Dictionary<string, HashSet<string>> PublicPages = new Dictionary<string, HashSet<string>>()
        {
            { "Home",new HashSet<string> {"Login","Index","About","Contact","UserRegister"}},

        //如果以后有其他控制器，比如Admin，也可以加：
        //{"Admin",new HashSet<string>{"Help"，"Info”}

        };


    }
}