using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace KildeCronJobs.Common.Domain
{
    public class VGArticleData
    {
        public string ArticleId { get; set; } = null!;
        public VGChanges Changes { get; set; } = null!;
        public string Link { get; set; } = null!;
        public string Brand { get; set; } = null!;
        public string HeadLine { get; set; } = null!;
    }
}
