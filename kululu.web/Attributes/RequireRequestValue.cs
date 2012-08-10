using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Reflection;

namespace Kululu.Web.Attributes
{
    public class RequireRequestValueAttribute : ActionMethodSelectorAttribute
    {
        public string[] ValueNames { get; private set; }

        public RequireRequestValueAttribute(string[] valueNames)
        {
            ValueNames = valueNames;
        }

        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
        {
            bool contains = false;
            foreach (var value in ValueNames)
            {
                contains = controllerContext.HttpContext.Request[value] != null;
                if (!contains) break;
            }
            return contains;
        }
    }
}
