using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LMOpcuaConnector.Model;
using LMLogger.Model;

namespace exware.Services
{
    public class Worker : IHostedService
    {
        #warning da sistemare i nomi e le cartelle
        private readonly Data.EventHandlerLinker _projectWorker;
        private readonly OPCUAClient _opc;
        private readonly OPCUATagEventHandler _tagEventHandler;


        private readonly Logger _logger;
        public Worker(OPCUAClient opc, Logger logger, OPCUATagEventHandler tagEventHandler, Data.EventHandlerLinker projectWorker)
        {
            _opc = opc;
            _logger = logger;
            _tagEventHandler = tagEventHandler;
            _projectWorker = projectWorker;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //configurazione del servizio logger
            _logger.UseConsole().SetPath(@"C:\ProgramData\exware").UseTxtFile();

            #region events binding
            //gestione necessaria per poter valutare ogni singolo tag change e chiamare le singole istruzioni
            _opc.OnTagChange += _tagEventHandler.TagUpdate;

            //evento gestione delle singole occorrenze
            _tagEventHandler.OnPezzoConcluso +=_projectWorker.ContaPezziHandler;
            _tagEventHandler.OnContapezziChange += _projectWorker.ContapezziHasChanged;
            #endregion

            _logger.LogInfo(this, "Fire up opc connection.");
            _opc.Run();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInfo(this, "Closins opcua session.");
            _opc.CloseSession();
            return Task.CompletedTask;
        }


    }
}
