using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KildeCronJobs.Common.Domain
{
    public class Kode24Article
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime Published { get; set; } = DateTime.Now;
        public string Section { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string Published_Url { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string FrontCropUrl { get; set; } = string.Empty;

        public Kode24Byline Byline { get; set; } = new Kode24Byline();
        public Kode24Reactions Reactions { get; set; } = new Kode24Reactions();
        public Kode24HighestRatedComment HighestRatedComment { get; set; } = new Kode24HighestRatedComment();
    }

    public class Kode24Byline
    {
        public string Name { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class Kode24Reactions
    {
        public List<int> ReactionsList { get; set; } = new List<int>();
        public int CommentsCount { get; set; }
        public int ReactionsCount { get; set; }
    }

    public class Kode24HighestRatedComment
    {
        public string PageIdentifier { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int Upvotes { get; set; }
        public int Downvotes { get; set; }
        public int Rating { get; set; }
        public Kode24User User { get; set; } = new Kode24User();
        public string BodySnippet { get; set; } = string.Empty;
    }

    public class Kode24User
    {
        public string Name { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
    }

    public class Kode24Root
    {
        public List<Kode24Article> LatestArticles { get; set; } = new List<Kode24Article>();
    }

    public class Kode24ImageObject
    {
        public string Url { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class Kode24Organization
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Kode24ImageObject Logo { get; set; } = new Kode24ImageObject();
    }

    public class Kode24Person
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class Kode24WebPage
    {
        public string Type { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
    }

    public class Kode24NewsArticle
    {
        public string Context { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Headline { get; set; } = string.Empty;
        public Kode24WebPage MainEntityOfPage { get; set; } = new Kode24WebPage();
        public DateTime DatePublished { get; set; } = DateTime.Now;
        public string Description { get; set; } = string.Empty;
        public DateTime DateModified { get; set; } = DateTime.Now;
        public Kode24Organization Publisher { get; set; } = new Kode24Organization();
        public List<Kode24Person> Author { get; set; } = new List<Kode24Person>();
        public List<string> Image { get; set; } = new List<string>();
        public string AlternativeHeadline { get; set; } = string.Empty;
        public string ArticleBody { get; set; } = string.Empty;
    }
}
