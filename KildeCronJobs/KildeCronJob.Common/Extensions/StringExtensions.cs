using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KildeCronJobs.Common.Extensions
{
    public static class StringExtensions
    {
        public static string? SchibstedIdentifier(this string link)
        {
            //finding identifier
            var indexOfLastSlash = link.LastIndexOf("/");
            var linkWithoutTitle = link.Substring(0, indexOfLastSlash);
            var indexOfIdentifier = linkWithoutTitle.LastIndexOf("/");
            var identifier = linkWithoutTitle.Substring(indexOfIdentifier + 1);

            return identifier;
        }

        public static string DagbladetIdentifier(this string link)
        {
            //finding identifier
            var indexOfLastSlash = link.LastIndexOf("/");
            var identifier = link.Substring(indexOfLastSlash + 1);

            return identifier;
        }
    }
}
