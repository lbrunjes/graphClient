using System.Data;
using System.Data.SqlClient;
using System.Runtime.Intrinsics.Arm;
using Microsoft.Extensions.Configuration;

namespace UVASON.Microservices;
/// <summary>
/// Wraps Access Sql
/// </summary>
public class SqlClient : IDisposable
{
    private SqlConnection dbConn;
    private string? SqlQueryList = "";
    private string? SqlQueryFileContent = "";


    /// <summary>
    /// Create a new SqlClient
    /// </summary>
    /// <param name="configuration"></param>
    public SqlClient(IConfiguration configuration)
    {
        dbConn = new SqlConnection(configuration.GetConnectionString("mssql"));
        SqlQueryList = configuration!.GetValue<string>("sqlQueryList");
        SqlQueryFileContent = configuration!.GetValue<string>("sqlQueryFileContent");

        if (String.IsNullOrEmpty(SqlQueryList) || String.IsNullOrEmpty(SqlQueryFileContent))
            throw new Exception("missing sql query data");
        dbConn.Open();
    }
    /// <summary>
    /// Get all the metadata and the sha1 hash of the file from sql.
    /// </summary>
    /// <param name="dbConn"></param>
    /// <returns></returns>
    public async Task<List<DataBaseFile>> GetRecentlyChangedCVs()
    {
        List<DataBaseFile> results = new List<DataBaseFile>();

        SqlCommand getFiles = new SqlCommand(SqlQueryList, dbConn);

        SqlDataReader dr = await getFiles.ExecuteReaderAsync();
        while (dr.Read())
        {
            results.Add(new DataBaseFile
            {
                Hash = (byte[])dr["sha1"],
                FileName = $"{dr["file"]}.{dr["extension"]}",
                StartYear = (int)dr["startYear"],
                EndYear = (int)dr["endYear"],
                Size = (int)dr["size"],
                guid = (Guid)dr["guid"]
            });



        }

        return results;
    }

    /// <summary>
    /// Get The file contents from the sql database
    /// </summary>
    /// <param name="dbConn"></param>
    /// <param name="guid"></param>
    /// <returns></returns>
    public async Task<Byte[]> getFileContent(Guid guid)
    {
        SqlCommand getFiles = new SqlCommand(SqlQueryFileContent, dbConn);
        getFiles.Parameters.AddWithValue("@guid", guid);

        SqlDataReader dr = await getFiles.ExecuteReaderAsync();
        if (dr.Read())
        {
            return (byte[])dr[0];

        }

        return new byte[0];
    }

    public void Dispose()
    {
        dbConn.Close();
    }
}
