using System;
using System.Web.Mvc;
using System.Globalization;
using Kululu.Web.Startup.Binders;

namespace Kululu.Web.Startup.Binders
{
    class DateTimeModelBinder : IModelBinder
    {

        #region IModelBinder Members

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            return CommonDateTime.ParseDate(value);
        }

        #endregion
    }
}