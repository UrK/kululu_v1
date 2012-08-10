using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Web.Script.Serialization;
using Dror.Common.Extensions;

namespace Kululu.Web.Facebook
{
    public class FBSignedRequestManager
    {
        /// <summary>
        /// Parses the signed request string.
        /// </summary>
        /// <param name="signedRequestValue">The encoded signed request value.</param>
        /// <returns>The valid signed request.</returns>
        internal protected FbmlFacebookSession ParseSignedRequest(string signedRequestValue, string secrect)
        {
            string[] parts = signedRequestValue.Split('.');
            var encodedValue = parts[0];

            var sig = Base64UrlDecode(encodedValue);
            var payload = parts[1];

            using (var cryto = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secrect)))
            {
                var hash = Convert.ToBase64String(cryto.ComputeHash(Encoding.UTF8.GetBytes(payload)));
                var hashDecoded = Base64UrlDecode(hash);
                if (hashDecoded != sig)
                {
                    return null;
                }
            }

            var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(Base64UrlDecode(payload)));
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            var data = (IDictionary<string, object>)serializer.DeserializeObject(payloadJson);
            var sessionKeyValue = PopulateFbmlSessionObject(data);
            return new FbmlFacebookSession(sessionKeyValue);
        }

        Dictionary<string, string> PopulateFbmlSessionObject(IDictionary<string, object> sessionInfo)
        {
            var signedRequest = new FbmlFacebookSession();
            foreach (var keyValue in sessionInfo)
            {
                //checking if value is actually of generic type
                if (keyValue.Value.GetType().IsGenericType)
                {

                    signedRequest.Dictionary.AddRange(
                    PopulateFbmlSessionObject((IDictionary<string, object>)keyValue.Value));
                }
                else
                {
                    signedRequest.Dictionary.Add(keyValue.Key, keyValue.Value.ToString());
                }
            }
            return signedRequest.Dictionary;
        }

        /// <summary>
        /// Converts the base 64 url encoded string to standard base 64 encoding.
        /// </summary>
        /// <param name="encodedValue">The encoded value.</param>
        /// <returns>The base 64 string.</returns>
        private static string Base64UrlDecode(string encodedValue)
        {
            encodedValue = encodedValue.Replace('+', '-').Replace('/', '_').Trim();
            int pad = encodedValue.Length % 4;
            if (pad > 0)
            {
                pad = 4 - pad;
            }

            encodedValue = encodedValue.PadRight(encodedValue.Length + pad, '=');
            return encodedValue;
        }
    }
}