using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Kululu.Web.Common
{
    public enum BrowserName
    { 
        IE,
        Chrome,
        Firefox,
        Safari
    }
    public class BrowserInfo
    {
        public BrowserName Name { get; set; }
        public int Version { get; set; }

        public BrowserInfo(BrowserName name, int version)
        {
            Name = name;
            Version = version;
        }
    }
}