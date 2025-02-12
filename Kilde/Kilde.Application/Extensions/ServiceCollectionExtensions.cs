using Kilde.Application.ApplicationServices;
using Kilde.Domain.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kilde.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureApplicationServices(this IServiceCollection services)
        {
            services.ConfigureDomainServices();

            services.AddTransient<ISourceService, SourceService>();
            services.AddTransient<IArticleService, ArticleService>();

            return services;
        }
    }
}
