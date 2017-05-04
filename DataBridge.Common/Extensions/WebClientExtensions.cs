using System;
using System.Net;
using System.Text;

namespace DataBridge.Extensions
{
    public static class WebClientExtensions
    {
        /// <summary>
        /// Add credentials to the webclient. Just setting the Crediatials is not enough, the http Basic authentiaction header needs to be set, too
        /// </summary>
        /// <param name="webClient">The web client.</param>
        /// <param name="credential">The credential.</param>
        public static void SetCredentials(this WebClient webClient, NetworkCredential credential)
        {
            if (credential == null)
            {
                return;
            }

            webClient.Credentials = credential;
            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credential.UserName + ":" + credential.Password));
            webClient.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);
        }
    }
}