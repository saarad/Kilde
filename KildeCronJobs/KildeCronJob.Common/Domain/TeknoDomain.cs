using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KildeCronJobs.Common.Domain
{
    public class TeknoListItem
    {
        public string Type { get; set; } = string.Empty;
        public int Position { get; set; } = 0;
        public string Url { get; set; } = string.Empty;
    }

    public class TeknoCollectionPage
    {
        public string Context { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class TeknoItemListElement
    {
        public string Type { get; set; } = string.Empty;
        public int Position { get; set; } = 0;
        public string Url { get; set; } = string.Empty;
    }

    public class TeknoMainEntityOfPage
    {
        public string Id { get; set; } = string.Empty;
    }

    public class TeknoRoot
    {
        public List<TeknoCollectionPage> CollectionPages { get; set; } = new List<TeknoCollectionPage>();
        public List<TeknoItemListElement> ItemListElement { get; set; } = new List<TeknoItemListElement>();
        public int NumberOfItems { get; set; } = 0;
        public TeknoMainEntityOfPage MainEntityOfPage { get; set; } = new TeknoMainEntityOfPage();
    }


    public class TeknoPerson
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class TeknoImageObject
    {
        public int Height { get; set; } = 0;
        public string Url { get; set; } = string.Empty;
        public int Width { get; set; } = 0;
    }

    public class TeknoPublisher
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public TeknoImageObject Logo { get; set; } = new TeknoImageObject();
    }

    public class TeknoWebPage
    {
        public string Context { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
    }

    public class TeknoNewsArticle
    {
        public List<TeknoPerson> Author { get; set; } = new List<TeknoPerson>();
        public DateTime DateModified { get; set; } = DateTime.Now;
        public DateTime DatePublished { get; set; } = DateTime.Now;
        public string Description { get; set; } = string.Empty;
        public string Headline { get; set; } = string.Empty;
        public List<TeknoImageObject> Image { get; set; } = new List<TeknoImageObject>();
        public bool IsAccessibleForFree { get; set; } = false;
        public TeknoMainEntityOfPage MainEntityOfPage { get; set; } = new TeknoMainEntityOfPage();
        public string Name { get; set; } = string.Empty;
        public TeknoPublisher Publisher { get; set; } = new TeknoPublisher();
        public string Url { get; set; } = string.Empty;
    }
}
