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

namespace LMOpcuaConnector.Model
{

    public partial class OPCUAClient
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
        //purtroppo creato stato gestito da me, perchè utilizzando quello interno della sessione non funzionava.
        //forse bisognerebbe guardare lo stato del keepalive
        public bool ConnectionSatus { get; private set; }

        public Task CloseSession()
        {
            if (session?.Connected == true)
            {
                session.Close();
                session.Dispose();
                ConnectionSatus = false;
                OnConnectionStatusChange?.Invoke(this, false);
            }
            return Task.CompletedTask;
        }
        public Task OpenSession()
        {
            if (session?.Connected == false || session == null)
            {
                Run();
            }
            return Task.CompletedTask;
        }


        #region configurator
        public void Init(OPCUAInitializer init)
        {
            tagConfigurator = init.TagConfigurator;
            endpointURL = init.EndpointURL;
            autoAccept = init.AutoAccept;
            publishingInterval = init.PublishingInterval;
            clientRunTime = init.StopTimeout <= 0 ? Timeout.Infinite : init.StopTimeout * 1000;
            serverExportMethod = init.ServerExportMethod;
            rootTagFolder = init.RootTagsFolder;
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
                ApplicationName = "LMOpcuaSession",
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

            //2 - Discover endpoints of {0}.", endpointURL);
            exitCode = ExitCode.ErrorDiscoverEndpoints;
            var selectedEndpoint = CoreClientUtils.SelectEndpoint(endpointURL, haveAppCertificate, 15000);
            Console.WriteLine("    Selected endpoint uses: {0}",
                selectedEndpoint.SecurityPolicyUri.Substring(selectedEndpoint.SecurityPolicyUri.LastIndexOf('#') + 1));

            // Create a session with OPC UA server.");
            exitCode = ExitCode.ErrorCreateSession;
            var endpointConfiguration = EndpointConfiguration.Create(config);
            var endpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);
            session = await Session.Create(config, endpoint, false, "LMOpcuaSession", 60000, new UserIdentity(new AnonymousIdentityToken()), null);

            // register keep alive handler
            session.KeepAliveInterval = 1000;
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


    }
}
