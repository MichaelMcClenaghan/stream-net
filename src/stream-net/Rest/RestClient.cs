using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Stream.Rest
{
    internal class RestClient
    {
        readonly Uri _baseUrl;
        private TimeSpan _timeout;

        public RestClient(Uri baseUrl, TimeSpan timeout)
        {
            _baseUrl = baseUrl;
            _timeout = timeout;
        }

        private UnityWebRequest BuildClient(RestRequest request)
        {
            //var client = new HttpClient();
            var unityClient = new UnityWebRequest();

            //client.DefaultRequestHeaders.Accept.Clear()
            //client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            unityClient.SetRequestHeader("Accept", "application/json");

            //client.Timeout = _timeout;
            unityClient.timeout = _timeout.Seconds;

            // add request headers
            request.Headers.ForEach(h =>
            {
                //client.DefaultRequestHeaders.Add(h.Key, h.Value);
                unityClient.SetRequestHeader(h.Key, h.Value);
            });

            return unityClient;
        }

        public TimeSpan Timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        private async Task<RestResponse> ExecuteGet(Uri url, RestRequest request)
        {
            using (var client = BuildClient(request))
            {
                client.method = UnityWebRequest.kHttpVerbGET;
                client.url = url.ToString();

                client.downloadHandler = new DownloadHandlerBuffer();

                await client.SendWebRequest();

                if (client.isNetworkError || client.isHttpError) {
                    return new RestResponse() { StatusCode = (HttpStatusCode)client.responseCode, ErrorMessage = client.error, ErrorException = new Exception(client.error), Content = client.downloadHandler.text };
                } else {
                    return await RestResponse.FromResponseMessage(client.downloadHandler.text, (HttpStatusCode)client.responseCode);
                }
            }
        }

        private async Task<RestResponse> ExecutePost(Uri url, RestRequest request)
        {
            using (var client = BuildClient(request))
            {
                client.method = UnityWebRequest.kHttpVerbPOST;
                client.url = url.ToString();

                client.downloadHandler = new DownloadHandlerBuffer();
                client.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(request.JsonBody));

                client.SetRequestHeader("Content-Type", "application/json");

                await client.SendWebRequest();

                if (client.isNetworkError || client.isHttpError) {
                    return new RestResponse() { StatusCode = (HttpStatusCode)client.responseCode, ErrorMessage = client.error, ErrorException = new Exception(client.error), Content = client.downloadHandler.text };
                } else {
                    return await RestResponse.FromResponseMessage(client.downloadHandler.text, (HttpStatusCode)client.responseCode);
                }
            }
        }

        private async Task<RestResponse> ExecuteDelete(Uri url, RestRequest request)
        {
            using (var client = BuildClient(request))
            {
                client.method = UnityWebRequest.kHttpVerbDELETE;
                client.url = url.ToString();

                client.downloadHandler = new DownloadHandlerBuffer();

                await client.SendWebRequest();

                if (client.isNetworkError || client.isHttpError) {
                    return new RestResponse() { StatusCode = (HttpStatusCode)client.responseCode, ErrorMessage = client.error, ErrorException = new Exception(client.error), Content = client.downloadHandler.text };
                } else {
                    return await RestResponse.FromResponseMessage(client.downloadHandler.text, (HttpStatusCode)client.responseCode);
                }
            }
        }

        private Uri BuildUri(RestRequest request)
        {
            var queryString = "";
            request.QueryParameters.ForEach((p) =>
            {
                queryString += (queryString.Length == 0) ? "?" : "&";
                queryString += string.Format("{0}={1}", p.Key, Uri.EscapeDataString(p.Value.ToString()));
            });
            return new Uri(_baseUrl, request.Resource + queryString);
        }

        public Task<RestResponse> Execute(RestRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request", "Request is required");

            Uri url = this.BuildUri(request);

            switch (request.Method)
            {
                case HttpMethod.DELETE:
                    return this.ExecuteDelete(url, request);
                case HttpMethod.POST:
                    return this.ExecutePost(url, request);
                default:
                    return this.ExecuteGet(url, request);
            }
        }
    }
}
