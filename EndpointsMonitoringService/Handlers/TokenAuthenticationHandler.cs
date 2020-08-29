using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using EndpointsMonitoringService.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using Renci.SshNet.Messages.Authentication;

namespace EndpointsMonitoringService.Handlers
{
    public class TokenAuthenticationHandler : AuthenticationHandler<TokenAuthenticationOptions>
    {

        private readonly ILogger<TokenAuthenticationHandler> _logger;
        private readonly DatabaseContext _databaseContext;
        private string _failureMessage;


        public TokenAuthenticationHandler(DatabaseContext databaseContext, IOptionsMonitor<TokenAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) :base(options,logger,encoder, clock)
        {
            _logger = logger.CreateLogger<TokenAuthenticationHandler>();
            _databaseContext = databaseContext;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {


            var headers = Request.Headers;
            if (headers == null || !headers.ContainsKey("Authorization"))
            {
                _failureMessage = "No Authorization Header";
                return AuthenticateResult.Fail(_failureMessage);
            }

            var authorization = AuthenticationHeaderValue.Parse(headers["Authorization"]);

            if(!authorization.Parameter.Contains("Bearer"))
            {
                _failureMessage = "No Bearer Token => Use Bearer Token Authentication Specification";
                return AuthenticateResult.Fail(_failureMessage);
            }


            var principal = new ClaimsPrincipal();
            var ticket = new AuthenticationTicket(principal,Scheme.Name);

            return AuthenticateResult.Success(ticket);




        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase += " - " +  _failureMessage;
            return base.HandleChallengeAsync(properties);
        }


    }

    public class TokenAuthenticationOptions : AuthenticationSchemeOptions
    {
        



    }
}
