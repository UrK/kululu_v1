using System;
using System.Collections.Generic;
using System.Globalization;

namespace Kululu.Web.Facebook
{
    public class FbmlFacebookSession
    {
        private Dictionary<string, string> dictionary = new Dictionary<string, string>();

        public FbmlFacebookSession()
        { 
        }

        public FbmlFacebookSession(Dictionary<string, string> valueDict)
        {
            Dictionary = valueDict;
        }

        /// <summary>
        /// Gets the underlying dictionary store.
        /// </summary>
        /// <value>The dictionary.</value>
        public Dictionary<string, string> Dictionary
        {
            get
            {
                return this.dictionary;
            }
            private set
            {
                dictionary = value;
            }
        }

        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        /// <value>The access token.</value>
        public string Issued_at
        {
            get
            {
                if (dictionary.ContainsKey("issued_at"))
                {
                    return dictionary["issued_at"];
                }
                return null;
            }
            set
            {
                dictionary["issued_at"] = value;
            }
        }

        public string AppData
        {
            get
            {
                if (dictionary.ContainsKey("app_data"))
                {
                    return dictionary["app_data"];
                }
                return null;
            }
            set
            {
                dictionary["app_data"] = value;
            }
        }
        /// <summary>
        /// Gets or sets the profile id.
        /// </summary>
        /// <value>The profile id.</value>
        public long UserId
        {
            get
            {
                if (dictionary.ContainsKey("user_id"))
                {
                    return long.Parse(dictionary["user_id"], CultureInfo.InvariantCulture);
                }
                return default(long);
            }
            set
            {
                dictionary["user_id"] = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets or sets the algorithm.
        /// </summary>
        /// <value>The algorithm.</value>
        public string Algorithm
        {
            get
            {
                if (dictionary.ContainsKey("algorithm"))
                {
                    return dictionary["algorithm"];
                }
                return null;
            }
            set
            {
                dictionary["algorithm"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the algorithm.
        /// </summary>
        /// <value>The algorithm.</value>
        public int Age
        {
            get
            {
                if (dictionary.ContainsKey("min"))
                {
                    return Convert.ToInt32(dictionary["min"]);
                }
                return 0;
            }
            set
            {
                dictionary["min"] = value.ToString();
            }
        }


        /// <summary>
        /// Gets or sets the algorithm.
        /// </summary>
        /// <value>The algorithm.</value>
        public long PageId
        {
            get
            {
                if (dictionary.ContainsKey("id"))
                {
                    return Convert.ToInt64(dictionary["id"]);
                }
                return 0;
            }
            set
            {
                dictionary["id"] = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the algorithm.
        /// </summary>
        /// <value>The algorithm.</value>
        public bool IsLiked
        {
            get
            {
                if (dictionary.ContainsKey("liked"))
                {
                    return Convert.ToBoolean(dictionary["liked"]);
                }
                return false;
            }
            set
            {
                dictionary["liked"] = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the algorithm.
        /// </summary>
        /// <value>The algorithm.</value>
        public bool IsAdmin
        {
            get
            {
                if (dictionary.ContainsKey("admin"))
                {
                    return Convert.ToBoolean(dictionary["admin"]);
                }
                return false;
            }
            set
            {
                dictionary["admin"] = value.ToString();
            }
        }

        public string Token
        {
            get
            {
                if (dictionary.ContainsKey("oauth_token"))
                {
                    return Convert.ToString(dictionary["oauth_token"]);
                }
                return string.Empty;
            }
            set { dictionary["oauth_token"] = value; }
        }

        /// <summary>
        /// name of timezone element in facebook data
        /// </summary>
        private const string ELEMENT_TIME_OFFSET = "timezone";

        public int TimeZone
        {
            get
            {
                return dictionary.ContainsKey(ELEMENT_TIME_OFFSET) ?
                    int.Parse(dictionary[ELEMENT_TIME_OFFSET]) : 0;
            }
            set
            {
                dictionary[ELEMENT_TIME_OFFSET] = value.ToString();
            }
        }

        /// <summary>
        /// name of user locale element in signed request
        /// </summary>
        private const string ELEMENT_LOCALE = "locale";

        /// <summary>
        /// value of user locale
        /// </summary>
        public string Locale
        {
            get
            {
                return dictionary.ContainsKey(ELEMENT_LOCALE) ?
                    Convert.ToString(dictionary[ELEMENT_LOCALE]) :
                    null;
            }
            set { dictionary[ELEMENT_LOCALE] = value; }
    }
    }
}