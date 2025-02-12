using Kilde.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Sanity.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kilde.Domain.Extensions
{
    public static class ServiceCollectionExtensions
    {
        internal static IServiceCollection ConfigureDomainServices(this IServiceCollection services)
        {
            services.ConfigureSanity();
            services.AddTransient<ISourceReadRepository, SourceReadRepository>();
            services.AddTransient<ISourceWriteRepository, SourceWriteRepository>();
            services.AddTransient<IArticleReadRepository, ArticleReadRepository>();
            services.AddTransient<IArticleWriteRepository, ArticleWriteRepository>();
            return services;
        }

        private static IServiceCollection ConfigureSanity(this IServiceCollection services)
        {
            services.AddScoped(x => new KildeReadClient());
            services.AddScoped(x => new KildeWriteClient());
            return services;
        }
    }
}
