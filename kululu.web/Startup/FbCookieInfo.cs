using System;
using System.Linq;
using System.Security.Cryptography;
using System.Collections.Specialized;
using System.Text;

namespace Kululu.Web.Startup
{
    public class FbCookieInfo
    {
        /// <summary>
        /// encryption key used to sign the cookie
        /// </summary>
        private static readonly byte[] MEncryptionKey =
            Encoding.ASCII.GetBytes("l;kjzxcvv#$Rzxv");

        /// <summary>
        /// algorithm used to has the cookie
        /// </summary>
        private readonly KeyedHashAlgorithm m_crypt;

        /// <summary>
        /// encryption key used to store variables in the session
        /// </summary>
        ///
        /// <remarks>
        /// The encryption is used is almost 'no-encryption'. We don't need
        /// full blown Rijandel here.
        /// </remarks>
        //private const int HIDE_FBID_KEY = 0x66BA27D0;

        internal FbCookieInfo()
        {
            m_crypt = new HMACSHA1(MEncryptionKey);
        }

        internal FbCookieInfo(NameValueCollection cookieInfo) : this()
        {
            if (cookieInfo == null || cookieInfo.Count <= 0)
            {
                return;
            }

            FacebookId = (cookieInfo["FacebookId"] != null)
                ? long.Parse(cookieInfo["FacebookId"]) : 0;

            AccessToken = cookieInfo["AccessToken"] ?? string.Empty;

            HasLikedPage = cookieInfo["HasLikedPage"] != null &&
                bool.Parse(cookieInfo["HasLikedPage"]);

            TimeOffset = (cookieInfo["TimeOffset"] != null) ?
                int.Parse(cookieInfo["TimeOffset"]) : 0;

            IsAdmin = cookieInfo["IsAdmin"] != null &&
                bool.Parse(cookieInfo["IsAdmin"]);

            IsCanvas = cookieInfo["IsCanvas"] != null &&
               bool.Parse(cookieInfo["IsCanvas"]);

            /* save cookie values only if the signature is valid */
            if (cookieInfo["Signature"] == null ||
                !VerifyCookie(Convert.FromBase64String(cookieInfo["Signature"])))
            {
                m_userId = default(long);
                m_accessToken = default(string);
                m_hasLikedPage = false;
                m_isAdmin = false;
                IsCanvas = false;
                m_timeOffset = 0;
            }
        }

        internal event Action<FbCookieInfo> ValueUpdated;

        /// <summary>
        /// signature of this cookie
        /// </summary>
        private byte[] m_signature;

        /// <summary>
        /// Get signature value
        /// </summary>
        public byte[] Signature { get { return m_signature;  } }

        private long m_userId;

        internal long FacebookId
        {
            get
            {
                //decrypting user value
                return m_userId;
            }
            set
            {
                //encrypting user value
                m_userId = value;
                NotifyChanged();
            }
        }

        private string m_accessToken;

        internal string AccessToken
        {
            get
            {
                return m_accessToken;
            }
            set
            {
                m_accessToken = value;
                NotifyChanged();
            }
        }

        private bool m_isAdmin;
        internal bool IsAdmin
        {
            get
            {
                return m_isAdmin;
            }
            set
            {
                m_isAdmin = value;
                NotifyChanged();
            }
        }

        private bool m_hasLikedPage;

        internal bool HasLikedPage
        {
            get
            {
                return m_hasLikedPage;
            }
            set
            {
                m_hasLikedPage = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// time offset of the current user according to Facebook
        /// </summary>
        private int m_timeOffset;

        /// <summary>
        /// Get the time offset of the current user
        /// </summary>
        internal int TimeOffset
        {
            get
            {
                return m_timeOffset;
            }
            private set
            {
                m_timeOffset = value;
                NotifyChanged();
            }
        }

        /// <summary>
        /// indication of content is viewed in canvas or in page
        /// </summary>
        private bool isCanvas;

        /// <summary>
        /// Get indication if content is viewed in canvas or in page
        /// </summary>
        internal bool IsCanvas
        {
            get
            {
                return isCanvas;
            }
            set
            {
                isCanvas = value;
                NotifyChanged();
            }
        }

        void NotifyChanged()
        {
            if (ValueUpdated != null)
            {
                SignCookie();
                ValueUpdated(this);
            }
        }

        /// <summary>
        /// calculates signature of all the cookie data
        /// </summary>
        ///
        /// <returns>
        /// cookie signature
        /// </returns>
        private byte[] CalcSignature()
        {
            /* builder of cookie hash */
            var sb = new StringBuilder(m_userId.ToString());
            sb.Append(m_accessToken);
            sb.Append(m_hasLikedPage.ToString());
            sb.Append(m_isAdmin.ToString());
            sb.Append(m_timeOffset.ToString());

            return m_crypt.ComputeHash(Encoding.ASCII.GetBytes(sb.ToString()));
        }

        /// <summary>
        /// sign the cookie
        /// </summary>
        private void SignCookie()
        {
            /* builder of cookie hash */
            var sb = new StringBuilder(m_userId.ToString());
            sb.Append(m_accessToken);
            sb.Append(m_hasLikedPage.ToString());
            sb.Append(m_isAdmin.ToString());
            sb.Append(m_timeOffset.ToString());

            m_signature = CalcSignature();
        }

        /// <summary>
        /// verify signature of the cookie
        /// </summary>
        ///
        /// <returns>
        /// true if the cookie signature is valid, false otherwise
        /// </returns>
        private bool VerifyCookie(byte[] sign)
        {
            var val = CalcSignature();
            if (sign == null || val.Length != sign.Length)
            {
                return false;
            }
            return !val.Where((t, i) => t != sign[i]).Any();
        }
    }
}