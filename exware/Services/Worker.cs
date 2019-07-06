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
        private readonly OPCUAClient _opc;
        private readonly Logger _logger;
        public Worker(OPCUAClient opc, Logger logger)
        {
            _opc = opc;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.UseConsole();
            _logger.LogInfo(this, "Fire up opc connection.");
            _opc.Run();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _opc
            return Task.CompletedTask;
        }


    }
}
