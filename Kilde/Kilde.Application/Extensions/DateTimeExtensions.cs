using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kilde.Application.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToKildeFormat(this DateTime date)
        {
            return date.ToUniversalTime().ToString("u").Replace(" ", "T");
        }
    }
}
