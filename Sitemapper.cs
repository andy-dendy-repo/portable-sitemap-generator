using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitemapGenerator.Sitemap
{
    public class Sitemapper
    {
        public SitemapDocument Document;
        public string BaseUrl { get; set; }
        public string Domain { get; set; }
        public string SavePath { get; set; }
        public bool Exclude { get; set; }
        private ILoader _loader = new Loader();
        public delegate void Info();
        public Info Notify;
        public Sitemapper(string domain, string baseulr)
        {
            Domain = domain;
            BaseUrl = baseulr.Contains("http://") || baseulr.Contains("https://") ? baseulr : ("https://"+baseulr);
            SavePath = DateTime.Now.ToString("yyyymmddhhmmss")+".xml";
            Exclude = true;
            Document = new SitemapDocument();
        }
        public async Task GenerateSitemap()
        {
            List<string> new_urls = new List<string>();
            List<string> visited = new List<string>();
            Document.Urls = visited;
            new_urls.Add(BaseUrl);
            do
            {
                List<string> hrefs=new List<string>();
                foreach (var url in new_urls)
                {
                    string text =await _loader.Get(url);
                    if (string.IsNullOrEmpty(text)) continue;
                    visited.Add(url);
                    Notify?.Invoke();
                    List<string> meta=Parser.GetAHrefs(text).Distinct().ToList();
                    Parser.Normalize(Domain,url,ref meta);
                    if (Exclude)
                        meta = meta.Select(u => u.Contains('?') ? u.Split('?')[0] : u).ToList();
                    hrefs.AddRange(meta);
                    hrefs = hrefs.Distinct().ToList();
                }
                new_urls = hrefs.Except(visited).ToList();
            }
            while (new_urls.Count != 0);
            Document.Save(SavePath);
        }
    }
}
