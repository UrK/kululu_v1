﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Practices.Unity;

namespace Kululu.Web.Startup
{
    public class ContextLifetimeManager<T> : LifetimeManager, IDisposable
    {
        public override object GetValue()
        {
            return HttpContext.Current.Items[typeof(T).AssemblyQualifiedName];
        }
        public override void RemoveValue()
        {
            HttpContext.Current.Items.Remove(typeof(T).AssemblyQualifiedName);
        }
        public override void SetValue(object newValue)
        {
            HttpContext.Current.Items[typeof(T).AssemblyQualifiedName] = newValue;
        }
        public void Dispose()
        {
            RemoveValue();
        }
    }
}