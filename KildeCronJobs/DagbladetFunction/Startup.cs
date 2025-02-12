using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kilde.Application.Extensions;
using DagbladetFunction;

[assembly: FunctionsStartup(typeof(Startup))]
namespace DagbladetFunction
{
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.ConfigureApplicationServices();
        }
    }
}
