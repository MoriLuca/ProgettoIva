using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using LMOpcuaConnector.Data;
using System.Linq;
using System.IO;
using System.Globalization;
using Microsoft.Extensions.Hosting;

namespace LMOpcuaConnector.Model
{
    public partial class OPCUAClient : IHostedService
    {
        const int ReconnectPeriod = 10;
        Session session;
        SessionReconnectHandler reconnectHandler;
        string endpointURL;
        int publishingInterval;
        int clientRunTime = Timeout.Infinite;
        static bool autoAccept = false;
        static ExitCode exitCode;
        ReferenceDescriptionCollection subscriptionArray = new ReferenceDescriptionCollection();


        /// <summary>
        /// Evento per la gestione del push di modifca stato di una tag da parte del server opc ua
        /// </summary>
        public EventHandler<Tag> OnTagChange;

        /// <summary>
        /// Evento per la gestione del cambio di stato della connessione al server opcua
        /// </summary>
        public EventHandler<bool> OnConnectionStatusChange;

        private TagConfigurator tagConfigurator;
        private ServerExportMethod serverExportMethod;
        private string rootTagFolder;
        public ListOfTags ListOfTags;
        public bool ConnectionSatus { get; private set; }


        #region contructor
        public OPCUAClient(string _endpointURL, bool _autoAccept, int _stopTimeout, TagConfigurator _tagConfigurator,
            ServerExportMethod _serverExportMethod, int _publishingInterval = 1000, string _rootTagsFolder = null)
        {
            tagConfigurator = _tagConfigurator;
            endpointURL = _endpointURL;
            autoAccept = _autoAccept;
            publishingInterval = _publishingInterval;
            clientRunTime = _stopTimeout <= 0 ? Timeout.Infinite : _stopTimeout * 1000;
            serverExportMethod = _serverExportMethod;
            rootTagFolder = _rootTagsFolder; 
            ListOfTags = new ListOfTags();
        }
        #endregion

        public ExitCode ExitCode { get => exitCode; }

        public void Run()
        {
            try
            {
                RunClient().Wait();
            }
            catch (Exception ex)
            {
                Utils.Trace("ServiceResultException:" + ex.Message);
                Console.WriteLine("Exception: {0}", ex.Message);
                return;
            }

            //ManualResetEvent quitEvent = new ManualResetEvent(false);
            //try
            //{
            //    Console.CancelKeyPress += (sender, eArgs) =>
            //    {
            //        quitEvent.Set();
            //        eArgs.Cancel = true;
            //        Console.WriteLine("cancel premuto");
            //    };
            //}
            //catch
            //{
            //}

            //// wait for timeout or Ctrl-C
            //quitEvent.WaitOne(clientRunTime);

            // return error conditions
            if (session.KeepAliveStopped)
            {
                exitCode = ExitCode.ErrorNoKeepAlive;
                return;
            }

            exitCode = ExitCode.Ok;
        }

        private async Task RunClient()
        {
            #region client configuration
            exitCode = ExitCode.ErrorCreateApplication;
            ApplicationInstance application = new ApplicationInstance
            {
                ApplicationName = "UA Core Sample Client",
                ApplicationType = ApplicationType.Client,
                ConfigSectionName = Utils.IsRunningOnMono() ? "Opc.Ua.MonoSampleClient" : "Opc.Ua.SampleClient"
            };


            // load the application configuration.
            string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            ApplicationConfiguration config = await application.LoadApplicationConfiguration(directoryName + "//Opcua.Client.Config.xml", false);

            // check the application certificate.
            bool haveAppCertificate = await application.CheckApplicationInstanceCertificate(false, 0);
            if (!haveAppCertificate)
            {
                throw new Exception("Application instance certificate invalid!");
            }

            if (haveAppCertificate)
            {
                config.ApplicationUri = Utils.GetApplicationUriFromCertificate(config.SecurityConfiguration.ApplicationCertificate.Certificate);
                if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                {
                    autoAccept = true;
                }
                config.CertificateValidator.CertificateValidation += new CertificateValidationEventHandler(CertificateValidator_CertificateValidation);
            }
            else
            {
                Console.WriteLine("    WARN: missing application certificate, using unsecure connection.");
            }

            //Discover endpoints of {0}.", endpointURL);
            exitCode = ExitCode.ErrorDiscoverEndpoints;
            var selectedEndpoint = CoreClientUtils.SelectEndpoint(endpointURL, haveAppCertificate, 15000);
            //Console.WriteLine("    Selected endpoint uses: {0}",
                //selectedEndpoint.SecurityPolicyUri.Substring(selectedEndpoint.SecurityPolicyUri.LastIndexOf('#') + 1));

            //Create a session with OPC UA server.
            exitCode = ExitCode.ErrorCreateSession;
            var endpointConfiguration = EndpointConfiguration.Create(config);
            var endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);
            session = await Session.Create(config, endpoint, false, "OPC UA Console Client", 60000, new UserIdentity(new AnonymousIdentityToken()), null);

            // register keep alive handler
            session.KeepAliveInterval = 5000;
            session.KeepAlive += Client_KeepAlive;
            #endregion

            #region Browse the OPC UA server namespace
            exitCode = ExitCode.ErrorBrowseNamespace;
            ReferenceDescriptionCollection references;
            Byte[] continuationPoint;

            references = session.FetchReferences(ObjectIds.ObjectsFolder);

            session.Browse(
                null,
                null,
                ObjectIds.ObjectsFolder,
                0u,
                BrowseDirection.Forward,
                ReferenceTypeIds.HierarchicalReferences,
                true,
                (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method,
                out continuationPoint,
                out references);

            //Console.WriteLine(" DisplayName, BrowseName, NodeClass");

            foreach (var rd in references)
            {
                //Console.WriteLine(" {0}, {1}, {2}", rd.DisplayName, rd.BrowseName, rd.NodeClass);
                ReferenceDescriptionCollection nextRefs;
                byte[] nextCp;
                session.Browse(
                    null,
                    null,
                    ExpandedNodeId.ToNodeId(rd.NodeId, session.NamespaceUris),
                    0u,
                    BrowseDirection.Forward,
                    ReferenceTypeIds.HierarchicalReferences,
                    true,
                    (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method,
                    out nextCp,
                    out nextRefs);

                foreach (var nextRd in nextRefs)
                {

                    //Console.WriteLine("   + {0}, {1}, {2}", nextRd.DisplayName, nextRd.BrowseName, nextRd.NodeClass);
                    if (rd.DisplayName.Text == rootTagFolder) subscriptionArray.Add(nextRd);
                }
            }
            #endregion

            #region Create a subscription with publishing interval of {publishingInterval} milliseconds.");
            exitCode = ExitCode.ErrorCreateSubscription;
            var subscription = new Subscription(session.DefaultSubscription) { PublishingInterval = publishingInterval };

            exitCode = ExitCode.ErrorMonitoredItem;

            var list = new List<MonitoredItem>();

            if (tagConfigurator == TagConfigurator.BrowseTheServer)
            {
                try
                {
                    //Subscribe a tutte le tags trovate con il browsing, filtrate a monte
                    foreach (var tag in subscriptionArray)
                    {
                        if (tag.NodeClass != NodeClass.Variable) continue;

                        list.Add(
                            new MonitoredItem(subscription.DefaultItem)
                            {
                                DisplayName = tag.DisplayName.Text,
                            //StartNodeId = $"ns={tag.NodeId.NamespaceIndex};i={Convert.ToUInt32(tag.NodeId.Identifier)}"
                            StartNodeId = $"ns={tag.NodeId.NamespaceIndex};{((serverExportMethod == ServerExportMethod.Id) ? "i" : "s")}={tag.NodeId.Identifier}"
                            }
                        );
                    }
                }
                catch (Exception ex)
                {

                    throw new NotImplementedException();
                }
                
            }


            else if (tagConfigurator == TagConfigurator.FromTagClassList)
            {
                //Subscribe a tutte le tags trovate con il browsing, filtrate a monte
                foreach (var tag in ListOfTags.Tags)
                {
                    try
                    {
                        list.Add(
                            new MonitoredItem(subscription.DefaultItem)
                            {
                                DisplayName = tag.Name,
                                //StartNodeId = $"ns={tag.NodeId.NamespaceIndex};i={Convert.ToUInt32(tag.NodeId.Identifier)}"
                                StartNodeId = $"ns={tag.NodeId.NamespaceIndex};{((serverExportMethod == ServerExportMethod.Id) ? "i" : "s")}=tag.NodeId.Identifier"
                            }
                        );
                    }
                    catch (Exception ex)
                    {
                        throw new NotImplementedException();
                    }

                }
            }


            else if (tagConfigurator == TagConfigurator.ReadJSON)
            {
                //aggiunta variabili statiche
                list.Add(
                    new MonitoredItem(subscription.DefaultItem)
                    {
                        DisplayName = "FCSetopint",
                        StartNodeId = $"ns=4;i=1279"
                    }
                );
            }



            ////Aggiunta variabili da lista compilata
            //foreach (var tag in ListOfTags.Tags)
            //{
            //    list.Add(
            //        new MonitoredItem(subscription.DefaultItem)
            //        {
            //            DisplayName = tag.Name,
            //            StartNodeId = tag.NodeId
            //        }
            //    );
            //}



            list.ForEach(i => i.Notification += OnNotification);
            subscription.AddItems(list);
            #endregion

            #region Add the subscription to the session
            exitCode = ExitCode.ErrorAddSubscription;
            session.AddSubscription(subscription);
            subscription.Create();
            #endregion

            #region Running...Press Ctrl-C to exit.
            exitCode = ExitCode.ErrorRunning;
            ConnectionSatus = true;
            OnConnectionStatusChange?.Invoke(this, true);
            #endregion

            #region test
            //RunTest();
            #endregion

        }

        private void RunTest()
        {
#warning test only
            new Thread(() =>
            {
                while (33 == 33)
                {

                    Tag t = ListOfTags.GetTagByName("Numeraccio");
                    object value = (t.Value == null) ? 0 : (float)t.Value - 5.1;
                    WriteTag(t, value);
                    Thread.Sleep(10);
                }
            }).Start();
        }

        private static void CertificateValidator_CertificateValidation(CertificateValidator validator, CertificateValidationEventArgs e)
        {
            if (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
            {
                e.Accept = autoAccept;
                if (autoAccept)
                {
                    Console.WriteLine("Accepted Certificate: {0}", e.Certificate.Subject);
                }
                else
                {
                    Console.WriteLine("Rejected Certificate: {0}", e.Certificate.Subject);
                }
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Run();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            session?.Close();
            session?.Dispose();
            return Task.CompletedTask;
        }
    }
}
