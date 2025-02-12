using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kilde.Application.Extensions;
using TeknoFunction;

[assembly: FunctionsStartup(typeof(Startup))]
namespace TeknoFunction
{
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.ConfigureApplicationServices();
        }
    }
}
