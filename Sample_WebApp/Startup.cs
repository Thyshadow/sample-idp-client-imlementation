using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Sample_WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string ShowCode = "TST000";

            services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeFolder("/");
            });

            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = $"AuthCookie{ShowCode}";
                options.DefaultChallengeScheme = "oidc";
            })
                .AddJwtBearer()
                .AddCookie($"AuthCookie{ShowCode}", options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);
                    options.Events = new CookieAuthenticationEvents()
                    {
                        OnValidatePrincipal = context =>
                        {

                            if (context.Properties.Items.ContainsKey(".Token.expires_at"))
                            {
                                var expire = DateTime.Parse(context.Properties.Items[".Token.expires_at"]);
                                if (expire > DateTime.Now) //TODO:change to check expires in next 5 mintues.
                                {
                                    //logger.Warn($"Access token has expired, user: {context.HttpContext.User.Identity.Name}");

                                    //TODO: send refresh token to ASOS. Update tokens in context.Properties.Items
                                    //context.Properties.Items["Token.access_token"] = newToken;
                                    context.ShouldRenew = true;
                                }
                            }
                            return Task.FromResult(0);
                        }
                    };
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    //options.Authority = "https://localhost:5001/";
                    options.Authority= "https://identity.sandbox.mge360.com/";
                    options.ClientId = "Sample_WebApp";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("event");
                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.Events = new Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectEvents()
                    {
                        OnRedirectToIdentityProvider = context =>
                        {
                            context.ProtocolMessage.SetParameter("EventID", ShowCode);
                            //context.ProtocolMessage.
                            return Task.FromResult(0);
                        },
                        OnTokenResponseReceived = context =>
                        {
                            var response = context.TokenEndpointResponse;
                            return Task.FromResult(0);
                        }
                    };

                    options.ClaimActions.MapJsonKey("EventID", "EventID");
                    options.ClaimActions.MapJsonKey("ExternalID", "ExternalID");
                });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ShowCode", policy => policy.RequireClaim("EventID", "TST000"));
                options.AddPolicy("ExternalID", policy => policy.RequireClaim("ExternalID"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
