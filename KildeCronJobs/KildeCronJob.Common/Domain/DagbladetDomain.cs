using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KildeCronJobs.Common.Domain
{
    public class DagbladetImageObject
    {
        public string Url { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class DagbladetPerson
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class DagbladetOrganization
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DagbladetImageObject Logo { get; set; } = new DagbladetImageObject();
    }

    public class DagbladetWebPage
    {
        public string Type { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
    }

    public class DagbladetNewsArticle
    {
        public string Context { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DagbladetWebPage MainEntityOfPage { get; set; } = new DagbladetWebPage();
        public string InLanguage { get; set; } = string.Empty;
        public DateTime DatePublished { get; set; }
        public DateTime DateModified { get; set; }
        public DagbladetOrganization Publisher { get; set; } = new DagbladetOrganization();
        public List<DagbladetPerson> Author { get; set; } = new List<DagbladetPerson>();
        public List<DagbladetImageObject> Image { get; set; } = new List<DagbladetImageObject>();
        public bool IsAccessibleForFree { get; set; }
        public string Headline { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ArticleBody { get; set; } = string.Empty;
    }
}
