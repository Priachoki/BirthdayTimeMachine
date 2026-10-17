using System.Net;
using System.Text;

namespace BirthdayTimeMachine.Test
{
    // Pretends to be an API: returns a fixed response instead of calling the internet.
    // Lets us test how each service maps JSON into our models.
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _content;
        private readonly HttpStatusCode _statusCode;

        public Uri? LastRequestUri { get; private set; }
        public int CallCount { get; private set; }

        public FakeHttpMessageHandler(string content, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            _content = content;
            _statusCode = statusCode;
        }

        // gives the service an HttpClient that uses this fake instead of the network
        public HttpClient CreateClient()
        {
            return new HttpClient(this) { BaseAddress = new Uri("https://test.local/") };
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri;
            CallCount++;

            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content, Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }
}