using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace HomaGames.HomaBelly.Utilities
{
#if UNITY_EDITOR
    public class EditorHttpCaller<T>
    {
        #region IHttpCaller implementation

        public async Task<T> Get(string uri, IModelDeserializer<T> deserializer)
        {
            try
            {
                using (HttpClient client = GetHttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(uri).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        string resultString = await response.Content.ReadAsStringAsync();
                        return deserializer.Deserialize(resultString);
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"[Editor Http Caller] Exception while querying API {uri}: {e.Message}");
            }

            return default;
        }

        public async Task<string> DownloadFile(string uri, string outputFilePath)
        {
            using (HttpClient client = GetHttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                    {
                        using (Stream streamToWriteTo = File.Open(outputFilePath, FileMode.Create))
                        {
                            await streamToReadFrom.CopyToAsync(streamToWriteTo);
                        }
                    }

                    HomaBellyEditorLog.Debug($"Done");
                    return outputFilePath;
                }
                else
                {
                    throw new FileNotFoundException(response.ReasonPhrase);
                }
            }
        }
        #endregion

        #region Private helpers

        private HttpClient GetHttpClient()
        {
#if CHARLES_PROXY
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
#endif
}