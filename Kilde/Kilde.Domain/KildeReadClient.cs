using Sanity.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kilde.Domain
{
    internal class KildeReadClient
    {
        public SanityDataContext Context { get; set; }
        public KildeReadClient()
        {
            var options = new SanityOptions //read only token
            {
                ProjectId = "0h4v7s5n",
                Dataset = "production",
                Token = "",
                UseCdn = false
            };

            Context = new SanityDataContext(options);
        }
    }
}
