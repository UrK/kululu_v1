using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kululu.Web.Startup.Binders
{
    public class NullableLocalDateTimeBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (string.IsNullOrEmpty(value.AttemptedValue))
            {
                return null;
            }
            return CommonDateTime.ParseDate(value);
        }
    }
}