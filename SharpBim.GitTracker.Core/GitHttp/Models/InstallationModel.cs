public class InstallationModel
{
    public int id { get; set; }
    public string client_id { get; set; }
    public Account account { get; set; }
    public string repository_selection { get; set; }
    public string access_tokens_url { get; set; }
    public string repositories_url { get; set; }
    public string html_url { get; set; }
    public int app_id { get; set; }
    public string app_slug { get; set; }
    public int target_id { get; set; }
    public string target_type { get; set; }
    public Permissions permissions { get; set; }
    public object[] events { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public object single_file_name { get; set; }
    public bool has_multiple_single_files { get; set; }
    public object[] single_file_paths { get; set; }
    public object suspended_by { get; set; }
    public object suspended_at { get; set; }
}
