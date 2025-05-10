namespace GorillaShirts.Models
{
    public interface IStandNavigationInfo
    {
        (string name, string author, string description, string type, string source, string note) GetNavigationInfo();
    }
}
