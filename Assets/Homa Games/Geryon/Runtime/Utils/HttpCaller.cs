using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace HomaGames.Geryon
{
    /// <summary>
    /// Caller to perform HTTP calls to the API
    /// </summary>
    public class HttpCaller
    {
        #region IHttpCaller implementation


        /// <summary>
        /// Asynchronous HTTP GET request returning the result as a string if succeed.
        /// If not, default string is returned
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<string> Get(string uri)
        {
            HomaGamesLog.DebugFormat("Get: {0}", uri);
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                HomaGamesLog.ErrorFormat("Http Caller error: internet not reachable");
                return default;
            }

            using (HttpClient client = GetHttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(uri).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    string resultString = await response.Content.ReadAsStringAsync();
                    return resultString;
                }
                else
                {
                    HomaGamesLog.ErrorFormat("Http Caller error: {0}", response.RequestMessage.ToString());
                }
            }

            return default;
        }

        /// <summary>
        /// Synchronous HTTP GET request returning the result as a string if succeed.
        /// If not, default string is returned
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public string GetSynchronous(string uri)
        {
            HomaGamesLog.DebugFormat("Get: {0}", uri);
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                HomaGamesLog.ErrorFormat("Http Caller error: internet not reachable");
                return default;
            }

            using (HttpClient client = GetHttpClient())
            {
                HttpResponseMessage response = client.GetAsync(uri).Result;
                if (response.IsSuccessStatusCode)
                {
                    string resultString = response.Content.ReadAsStringAsync().Result;
                    return resultString;
                }
                else
                {
                    HomaGamesLog.ErrorFormat("Http Caller error: {0}", response.RequestMessage.ToString());
                }
            }

            return default;
        }

        #endregion

        #region Private helpers

        private HttpClient GetHttpClient()
        {
#if UNITY_EDITOR && CHARLES_PROXY
            var httpClientHandler = new HttpClientHandler()
            {
                Proxy = new System.Net.WebProxy("http://localhost:8888", false),
                UseProxy = true
            };

            HttpClient client = new HttpClient(httpClientHandler);
            return client;
#else
            HttpClient client = new HttpClient();
            return client;
#endif
        }

        #endregion
    }
}