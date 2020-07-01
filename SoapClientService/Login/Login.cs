using AdysTech.CredentialManager;
using Microsoft.Extensions.DependencyInjection;
using Org.Visiontech.Commons;
using Org.Visiontech.Credential;
using Org.Visiontech.CredentialGrouping;
using SoapClientService.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Timers;
using Visiontech.Services.Utils;
using VisiontechCommons;

namespace SoapClientService.Login
{
    public class LoginManager : IDisposable
    {

        private readonly Logger Logger;
        private readonly Timer LoginTimer = new Timer();
        private readonly string ServiceName;
        private readonly IAuthenticationService AuthenticationService;
        private readonly IAuthenticatingMessageInspector AuthenticatingMessageInspector;
        private readonly CredentialSoap CredentialSoap;
        private readonly CredentialGroupingSoap CredentialGroupingSoap;
        private string TicketGrantingTicket;
        public string XGrant;
        private Action Callback;

        public LoginManager(Logger Logger, string ServiceName, IAuthenticationService AuthenticationService, IAuthenticatingMessageInspector AuthenticatingMessageInspector, CredentialSoap CredentialSoap, CredentialGroupingSoap CredentialGroupingSoap) {
            this.Logger = Logger;
            this.ServiceName = ServiceName;
            this.AuthenticationService = AuthenticationService;
            this.AuthenticatingMessageInspector = AuthenticatingMessageInspector;
            this.CredentialSoap = CredentialSoap;
            this.CredentialGroupingSoap = CredentialGroupingSoap;

            LoginTimer.Elapsed += new ElapsedEventHandler(LoginTime);
        }

        public void RefreshTicketGrantingTicket()
        {
            if (TicketGrantingTicket is object && AuthenticationService.VerifyTicketGrantingTicket(TicketGrantingTicket).GetAwaiter().GetResult())
            {
                return;
            }
            NetworkCredential NetworkCredential = CredentialManager.GetCredentials(ServiceName);

            TicketGrantingTicket = AuthenticationService.GetTicketGrantingTicket(NetworkCredential.UserName, NetworkCredential.Password).GetAwaiter().GetResult();
            if (string.IsNullOrWhiteSpace(TicketGrantingTicket))
            {
                Logger.LogEvent("Wrong Credentials !", EventLogEntryType.Error);
            }
        }

        public string RefreshServiceTicket()
        {
            RefreshTicketGrantingTicket();

            string token = AuthenticationService.GetServiceTicket(TicketGrantingTicket).GetAwaiter().GetResult();
            if (string.IsNullOrWhiteSpace(TicketGrantingTicket))
            {
                Logger.LogEvent("Wrong TGT !", EventLogEntryType.Error);
            }
            AuthenticatingMessageInspector.HttpRequestMessageProperty.Headers.Set(HttpRequestHeader.Authorization, "Bearer " + token);
            return token;
        }

        public void Login(double Interval, Action Callback)
        {
            if (!LoginTimer.Enabled)
            {
                LoginTimer.Interval = Interval;
                LoginTimer.Start();
                this.Callback = Callback;
            }
        }

        public void LoginTime(object source, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                Logger.LogEvent("Authenticating");

                RefreshServiceTicket();

                myResponse myResponse = CredentialSoap.myAsync(new my()).GetAwaiter().GetResult();
                if (myResponse is null || myResponse.@return is null)
                {
                    Logger.LogEvent("Could not authenticate !", EventLogEntryType.Error);
                    return;
                }

                Org.Visiontech.CredentialGrouping.simpleCriteriaDTO criteria = new Org.Visiontech.CredentialGrouping.simpleCriteriaDTO
                {
                    criteria = new Org.Visiontech.CredentialGrouping.credentialGroupingDTO
                    {
                        credential = new Org.Visiontech.CredentialGrouping.credentialDTO
                        {
                            id = myResponse.@return.id
                        },
                        role = new Org.Visiontech.CredentialGrouping.roleDTO
                        {
                            code = "FACTORY"
                        },
                        group = new Org.Visiontech.CredentialGrouping.groupDTO
                        {
                        },
                        valid = true,
                        deleted = false
                    }
                };

                Org.Visiontech.CredentialGrouping.findResponse findResponse = CredentialGroupingSoap.findAsync(new Org.Visiontech.CredentialGrouping.find(new[] { criteria }, Array.Empty<Org.Visiontech.CredentialGrouping.findOrderDTO>(), 1, 0)).GetAwaiter().GetResult();

                if (findResponse is null || findResponse.@return.count <= 0 || findResponse.@return.results is null)
                {
                    Logger.LogEvent("Could not find the factory grant !", EventLogEntryType.Error);
                    return;
                }

                Logger.LogEvent("Authenticated as " + myResponse.@return.username);
                if (findResponse.@return.results.First() is Org.Visiontech.CredentialGrouping.credentialGroupingDTO credentialGroupingDTO)
                {
                    Logger.LogEvent("Grant from group " + credentialGroupingDTO.group.code);
                }

                XGrant = findResponse.@return.results.First().id;

                SharedServiceProvider.ServiceProvider.GetRequiredService<IAuthenticatingMessageInspector>().HttpRequestMessageProperty.Headers.Set("X-Grant", XGrant);

                LoginTimer.Stop();
                Callback.Invoke();
            } catch (Exception e) {
                Logger.LogEvent("Error " + e.Message);
                Logger.LogEvent("StackTrace " + e.StackTrace);
            }
        }

        public void Dispose()
        {
            LoginTimer.Dispose();
        }
    }
}
