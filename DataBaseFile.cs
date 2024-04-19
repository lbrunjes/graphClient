namespace UVASON.Microservices;
/// <summary>
/// A file record from the sql database. Does not include file contents
/// </summary>
public class DataBaseFile
{
    /// <summary>
    /// Academic Year The item was uploaded to cover
    /// </summary>
    /// <value></value>
    public int StartYear { get; set; } = DateTime.Now.Year - 2000;
    /// <summary>
    /// initial covered Academic Year
    /// </summary>
    /// <value></value>
    public int EndYear { get; set; } = DateTime.Now.Year - 2000;
    /// <summary>
    /// Original FileName
    /// </summary>
    /// <value></value>
    public string FileName { get; set; } = "";

    /// <summary>
    /// How big it is.
    /// </summary>
    /// <value></value>
    public int Size { get; set; } = 0;
    /// <summary>
    /// FileContent hash to compare with azure.
    /// </summary>
    /// <value></value>
    public byte[] Hash { get; set; } = new byte[0];
    /// <summary>
    /// Friendly string  of the hash
    /// </summary>
    /// <value></value>
    public string HashString
    {
        get
        {
            return Convert.ToHexString(Hash);
        }
    }
    public Guid guid { get; set; } = new Guid();
}