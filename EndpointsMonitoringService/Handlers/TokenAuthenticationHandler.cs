using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using EndpointsMonitoringService.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
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
        private IOwner _owner;


        public TokenAuthenticationHandler(
            IOwner owner,DatabaseContext databaseContext, IOptionsMonitor<TokenAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _logger = logger.CreateLogger<TokenAuthenticationHandler>();
            _databaseContext = databaseContext;
            _owner = owner;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {

            try
            {

                var headers = Request.Headers;
                if (headers == null || !headers.ContainsKey("Authorization"))
                {
                    _failureMessage = "No Authorization Header";
                    return AuthenticateResult.Fail(_failureMessage);
                }

                var authorization = AuthenticationHeaderValue.Parse(headers["Authorization"]);

                if (!authorization.Scheme.Equals("Bearer"))
                {
                    _failureMessage = "No Bearer Token => Use Bearer Token Authentication Specification";
                    return AuthenticateResult.Fail(_failureMessage);
                }


                var accessToken = authorization.Parameter;

                var foundUser = _databaseContext.User.
                    Where(x => x.AccessToken == accessToken).ToArray();

                if (foundUser.Length == 0)
                {
                    _failureMessage = "User With Token Not Found";
                    return AuthenticateResult.Fail(_failureMessage);
                }

                if (foundUser.Length > 1)
                {
                    _logger.LogWarning(String.Format("FOUND MULTIPLE USERS WITH ONE TOKEN, ID: "
                        + string.Join(",", foundUser.Select(x => x.Id).ToArray())
                        ));

                    _logger.LogWarning(String.Format("Getting first in collection"));
                }

                User user = foundUser.First();

                _owner.RegisterOwner(user);

                    Claim claim = new Claim("Id",user.Id.ToString());
                ClaimsIdentity idenity = new ClaimsIdentity(new List<Claim>() { claim },Scheme.Name);

                ClaimsPrincipal principal = new ClaimsPrincipal(idenity);
                AuthenticationTicket ticket = new AuthenticationTicket(principal, Scheme.Name);
                return AuthenticateResult.Success(ticket);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR HANDLING AUTHENTICATION");
                _failureMessage = "Service Internal Exception";
                return AuthenticateResult.Fail(_failureMessage);
            }


        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase += " - " + _failureMessage;
            return base.HandleChallengeAsync(properties);
        }


    }

    public class TokenAuthenticationOptions : AuthenticationSchemeOptions
    {




    }
}
