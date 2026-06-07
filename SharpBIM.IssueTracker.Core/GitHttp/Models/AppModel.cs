using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBIM.GitTracker.Core.GitHttp.Models
{
    public class AppModel
    {
        public int id { get; set; }
        public string client_id { get; set; }
        public string slug { get; set; }
        public string node_id { get; set; }
        public Account owner { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string external_url { get; set; }
        public string html_url { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public Permissions permissions { get; set; }
        public object[] events { get; set; }
        public int installations_count { get; set; }
    }
}