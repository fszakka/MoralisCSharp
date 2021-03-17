namespace MoralisDotNet.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Parse.Abstractions.Infrastructure;
    using Parse.Abstractions.Infrastructure.Execution;
    using Parse.Infrastructure;
    using Parse.Infrastructure.Utilities;
    using BCLWebClient = System.Net.Http.HttpClient;

    public class MoralisWebClient : IWebClient
    {
        static HashSet<string> ContentHeaders { get; } = new HashSet<string>
        {
            { "Allow" },
            { "Content-Disposition" },
            { "Content-Encoding" },
            { "Content-Language" },
            { "Content-Length" },
            { "Content-Location" },
            { "Content-MD5" },
            { "Content-Range" },
            { "Content-Type" },
            { "Expires" },
            { "Last-Modified" }
        };


        public MoralisWebClient() : this(new BCLWebClient { }) { }

        public MoralisWebClient(BCLWebClient client) => Client = client;

        BCLWebClient Client { get; set; }

        public async Task<Tuple<HttpStatusCode, string>> ExecuteAsync(Parse.Infrastructure.Execution.WebRequest httpRequest, IProgress<IDataTransferLevel> uploadProgress, IProgress<IDataTransferLevel> downloadProgress,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var headersToRemove = new[]
            {
                "x-parse-windows-key",
                "x-parse-os-version"
            };
            uploadProgress ??= new Progress<IDataTransferLevel> { };
            downloadProgress ??= new Progress<IDataTransferLevel> { };
            HttpRequestMessage message = new HttpRequestMessage(new HttpMethod(httpRequest.Method), httpRequest.Target);

            // Fill in zero-length data if method is post.
            if ((httpRequest.Data is null && httpRequest.Method.ToLower().Equals("post") ? new MemoryStream(new byte[0]) : httpRequest.Data) is Stream { } data)
            {
                message.Content = new StreamContent(data);
            }

            if (httpRequest.Headers != null)
            {
                foreach (KeyValuePair<string, string> header in httpRequest.Headers)
                {
                    if (ContentHeaders.Contains(header.Key))
                    {
                        message.Content.Headers.Add(header.Key, header.Value);
                    }
                    else
                    {
                        message.Headers.Add(header.Key, header.Value);
                    }
                }
            }

            // Avoid aggressive caching on Windows Phone 8.1.

            message.Headers.Add("Cache-Control", "no-cache");
            foreach (var header in headersToRemove)
            {
                message.Headers.Remove(header); 
            }
            // TODO: (richardross) investigate progress here, maybe there's something we're missing in order to support this.

            uploadProgress.Report(new DataTransferLevel { Amount = 0 });

            return await Client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ContinueWith(httpMessageTask =>
            {
                HttpResponseMessage response = httpMessageTask.Result;
                uploadProgress.Report(new DataTransferLevel { Amount = 1 });

                return response.Content.ReadAsStreamAsync().ContinueWith(streamTask =>
                {
                    MemoryStream resultStream = new MemoryStream { };
                    Stream responseStream = streamTask.Result;

                    int bufferSize = 4096, bytesRead = 0;
                    byte[] buffer = new byte[bufferSize];
                    long totalLength = -1, readSoFar = 0;

                    try
                    {
                        totalLength = responseStream.Length;
                    }
                    catch { };

                    return InternalExtensions.WhileAsync(() => responseStream.ReadAsync(buffer, 0, bufferSize, cancellationToken).OnSuccess(readTask => (bytesRead = readTask.Result) > 0), () =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        return resultStream.WriteAsync(buffer, 0, bytesRead, cancellationToken).OnSuccess(_ =>
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            readSoFar += bytesRead;

                            if (totalLength > -1)
                            {
                                downloadProgress.Report(new DataTransferLevel { Amount = 1.0 * readSoFar / totalLength });
                            }
                        });
                    }).ContinueWith(_ =>
                    {
                        responseStream.Dispose();
                        return _;
                    }).Unwrap().OnSuccess(_ =>
                    {
                        // If getting stream size is not supported, then report download only once.

                        if (totalLength == -1)
                        {
                            downloadProgress.Report(new DataTransferLevel { Amount = 1.0 });
                        }

                        byte[] resultAsArray = resultStream.ToArray();
                        resultStream.Dispose();

                        // Assume UTF-8 encoding.

                        return new Tuple<HttpStatusCode, string>(response.StatusCode, Encoding.UTF8.GetString(resultAsArray, 0, resultAsArray.Length));
                    });
                });
            }).Unwrap().Unwrap();
        }
    }
    /// <summary>
    /// <code>IHttpRequest</code> is an interface that provides an API to execute HTTP request data.
    /// </summary>
    public class WebRequest
    {
        public Uri Target => new Uri(new Uri(Resource), Path);

        public string Resource { get; set; }

        public string Path { get; set; }

        public IList<KeyValuePair<string, string>> Headers { get; set; }

        /// <summary>
        /// Data stream to be uploaded.
        /// </summary>
        public virtual Stream Data { get; set; }

        /// <summary>
        /// HTTP method. One of <c>DELETE</c>, <c>GET</c>, <c>HEAD</c>, <c>POST</c> or <c>PUT</c>
        /// </summary>
        public string Method { get; set; }
    }
}
