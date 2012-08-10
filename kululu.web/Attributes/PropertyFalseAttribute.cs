using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Kululu.Web.Attributes
{
    public sealed class PropertyFalseAttribute : ValidationAttribute
    {
        private string DefaultErrorMessage {get;set;}
        private string BasePropertyName {get;set;}

        public PropertyFalseAttribute(string basePropertyName, string defaultErrorMessage)
        {
            BasePropertyName = basePropertyName;
            DefaultErrorMessage = defaultErrorMessage;
        }

        //Override IsValid  
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //Get PropertyInfo Object  
            var basePropertyInfo = validationContext.ObjectType.GetProperty(BasePropertyName);

            if (basePropertyInfo == null || value == null)
            {
                return null;
            }

            bool isOriginChecked;
            if (!bool.TryParse(basePropertyInfo.GetValue(validationContext.ObjectInstance, null).ToString(), out isOriginChecked))
            {
                return null;
            }

            bool isDestionationChecked;
            if (!bool.TryParse(value.ToString(), out isDestionationChecked ))
            {
                return null;
            }

            if (isOriginChecked && !isDestionationChecked)
                return new ValidationResult(DefaultErrorMessage);

            //Default return - This means there were no validation error  
            return null;
        }
    }
}