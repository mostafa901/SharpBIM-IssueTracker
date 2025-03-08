using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpBIM.UIContexts.Abstracts.Interfaces;

namespace SharpBIM.GitTracker.Core.GitHttp.Models
{
    public class CommentModel : IModel
    {
        public string url { get; set; }
        public string html_url { get; set; }
        public string issue_url { get; set; }
        public long Id { get; set; }
        public string node_id { get; set; }
        public Account user { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string author_association { get; set; }
        public string body { get; set; }
        public Reactions reactions { get; set; }
        public object performed_via_github_app { get; set; }
        public string Title { get; set; }
    }
}