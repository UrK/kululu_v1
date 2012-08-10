using System;
using System.Text;
using System.Web;

namespace Dror.Common.Utils
{
    public class UrlUtils
    {
        /// <summary>
        /// build absolute path based on any string passed to the method
        /// </summary>
        ///
        /// <param name="baseUrl">
        /// base application URL
        /// </param>
        ///
        /// <param name="input">
        /// path to the image from the database
        /// </param>
        ///
        /// <param name="appPath">
        /// absolute path to the application
        /// </param>
        ///
        /// <param name="defVal">
        /// default URL value
        /// </param>
        ///
        /// <returns>
        /// absolute URL of the image
        /// </returns>
        public static string BuildAbsoluteImageUrl(
            Uri baseUrl,
            string input,
            string appPath,
            string defVal)
        {
            input = input ?? defVal;

            /* if the input URL is already absolute, just return it */
            if (input.StartsWith("http://", System.StringComparison.CurrentCultureIgnoreCase) ||
                input.StartsWith("https://", System.StringComparison.CurrentCultureIgnoreCase))
            {
                return input;
            }

            if (input.StartsWith("/"))
            {
                input = input.Insert(0, "~");
            }
            if (!input.StartsWith("~/"))
            {
                input = input.Insert(0, "~/");
            }

            /* builder used to  create the URL */
            var sb = new StringBuilder(baseUrl.Scheme);
            sb.Append("://");
            sb.Append(baseUrl.Host);
            if (!baseUrl.IsDefaultPort)
            {
                sb.Append(":");
                sb.Append(baseUrl.Port);
            }
            sb.Append(appPath);
            sb.Append(VirtualPathUtility.ToAbsolute(input));

            return sb.ToString();
        }

        /// <summary>
        /// build application path based on any string passed to the method
        /// </summary>
        ///
        /// <param name="input">
        /// path to the image from the database
        /// </param>
        ///
        /// <param name="appPath">
        /// absolute path to the application
        /// </param>
        ///
        /// <param name="defVal">
        /// default URL value
        /// </param>
        ///
        /// <returns>
        /// absolute URL of the image
        /// </returns>
        public static string BuildApplicationImageUrl(
            string input,
            string appPath,
            string defVal)
        {
            input = input ?? defVal;

            /* if the input URL is already absolute, just return it */
            if (input.StartsWith("http://", System.StringComparison.CurrentCultureIgnoreCase) ||
                input.StartsWith("https://", System.StringComparison.CurrentCultureIgnoreCase))
            {
                return input;
            }

            /* builder used to  create the URL */
            var sb = new StringBuilder(appPath);
            if (appPath.EndsWith("/") && input.StartsWith("/"))
            {
                sb.Remove(sb.Length - 1, 1);
            }
            else if (!appPath.EndsWith("/") && !input.StartsWith("/"))
            {
                sb.Append("/");
            }
            sb.Append(input);
            return sb.ToString();
        }
    }
}
