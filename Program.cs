
using System.Text;

using Microsoft.Extensions.Configuration;
namespace UVASON.Microservices;

public static class EntryPoint
{
    /// <summary>
    /// Where we do the thing.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static async Task Main(string[] args)
    {
        //load Configuration files
        IConfiguration config = GetConfiguration();
        // don't continue if things are bad.
        if (config == null)
            throw new Exception("unable to load config");


        //Initialize both data streams
        SqlClient Sql = new SqlClient(config);
        GraphClient Graph = new GraphClient(config);


        //Get Data from sql
        List<DataBaseFile> files = await Sql.GetRecentlyChangedCVs();
        //Place to keep files we need to upload
        List<(DataBaseFile file, int year)> toUpload = new List<(DataBaseFile, int)>();

        //look at all files we cared about.
        foreach (var file in files)
        {
            //confirm files are valid enough
            if (file.EndYear > file.StartYear)
                // Loop through all years
                for (int academicYear = file.StartYear; academicYear <= file.EndYear; academicYear++)
                {
                    //Look to see if it already exists
                    if (!await Graph.IsInSharepointFolder(file, academicYear))
                    {
                        //add to upload list.
                        toUpload.Add((file, academicYear));
                    }
                }


        }

        //now that we have found the stuff to upload, do the upload.
        foreach (var upload in toUpload)
        {
            await Graph.AddFileToSharepointFolder(upload.file, upload.year, await Sql.getFileContent(upload.file.guid));
        }
    }

    /// <summary>
    /// Read the config files and returns config.
    /// </summary>
    /// <returns></returns>
    public static IConfiguration GetConfiguration()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);
        return configBuilder.Build();
    }
}


