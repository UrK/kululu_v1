using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Kululu.Web.Attributes
{
    public sealed class DateGreaterThanAttribute : ValidationAttribute
    {
        private const string _defaultErrorMessage = "תאריך {0}  חייב להיות יותר גדול מ {1}";
        private string _basePropertyName;
        private string SecondDateDecodedTime {get;set;}
        private string FirstDateDecodedName {get;set;}

        public DateGreaterThanAttribute(string basePropertyName, string firstDateDecodedName, string secondDateDecodedTime)
            : base(_defaultErrorMessage)
        {
            _basePropertyName = basePropertyName;
            FirstDateDecodedName = firstDateDecodedName;
            SecondDateDecodedTime = secondDateDecodedTime;
        }

        //Override default FormatErrorMessage Method  
        public override string FormatErrorMessage(string name)
        {
            return string.Format(_defaultErrorMessage, SecondDateDecodedTime, FirstDateDecodedName);
        }

        //Override IsValid  
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //Get PropertyInfo Object  
            var basePropertyInfo = validationContext.ObjectType.GetProperty(_basePropertyName);

            if (basePropertyInfo == null || value == null)
            {
                return ValidationResult.Success;
            }

            //Get Value of the property  
            DateTime startDate;
            if (!DateTime.TryParse(basePropertyInfo.GetValue(validationContext.ObjectInstance, null).ToString(), out startDate))
            {
                return ValidationResult.Success;
            }


            DateTime thisDate;
            if (!DateTime.TryParse(value.ToString(), out thisDate))
            {
                return ValidationResult.Success;
            }

            //Actual comparision  
            if (thisDate <= startDate)
            {
                var message = FormatErrorMessage(validationContext.DisplayName);
                return new ValidationResult(message);
            }

            //Default return - This means there were no validation error  
            return null;
        }
    }  
}