using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kilde.Application.Extensions;
using E24Function;

[assembly: FunctionsStartup(typeof(Startup))]
namespace E24Function
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.ConfigureApplicationServices();
        }
    }
}
