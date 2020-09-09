using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Sample_WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGetAsync()
        {

            var accessToken = await HttpContext.GetTokenAsync("access_token");


            var client = new HttpClient();

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://1qk7jkqhn0.execute-api.us-east-1.amazonaws.com/"),
                Method = HttpMethod.Get
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.SendAsync(request);
        }
    }
}
