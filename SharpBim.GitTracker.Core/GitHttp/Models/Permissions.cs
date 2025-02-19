public class Permissions
{
    public bool admin { get; set; }
    public bool maintain { get; set; }
    public bool push { get; set; }
    public bool triage { get; set; }
    public bool pull { get; set; }

    public string issues { get; set; }
    public string metadata { get; set; }
}