

using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fabron.Mando;
using HoneyComb.Commands;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HoneyComb.Server.CommandHandlers
{
    public class InvokeHttpRequestHandler : ICommandHandler<InvokeHttpRequest, InvokeHttpRequestResult>
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<InvokeHttpRequest> _logger;

        public InvokeHttpRequestHandler(ILogger<InvokeHttpRequest> logger, IHttpClientFactory httpClient)
        {
            _httpClient = httpClient.CreateClient(Constants.HttpClientNames.Default);
            _logger = logger;
        }

        public async Task<InvokeHttpRequestResult> Handle(InvokeHttpRequest command, CancellationToken token)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(command.Url)
            };
            if (command.Headers is not null)
            {
                foreach ((var key, var value) in command.Headers)
                {
                    request.Headers.Add(key, value);
                }
            }
            request.Method = new HttpMethod(command.HttpMethod);
            if (command.PayloadJson is not null)
            {
                request.Content = new StringContent(command.PayloadJson, Encoding.UTF8, "application/json");
            }

            HttpResponseMessage res = await _httpClient.SendAsync(request, token);
            var resBody = await res.Content.ReadAsStringAsync(token);

            return new InvokeHttpRequestResult((int)res.StatusCode, resBody);
        }
    }
}
