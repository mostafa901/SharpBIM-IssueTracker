using SharpBIM.UIContexts.Abstracts.Interfaces;

namespace SharpBIM.GitTracker.Core.GitHttp.Models;

public class GitLabel : IModel
{
    public long Id { get; set; }
    public string node_id { get; set; }
    public string url { get; set; }
    public string name { get; set; }

    /// <summary>
    /// The hexadecimal color code for the label, without the leading #.
    /// </summary>
    public string color { get; set; }

    public bool _default { get; set; }
    public string description { get; set; }
    public string Title { get; set; }
}