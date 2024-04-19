using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.StaticFiles;

namespace UVASON.Microservices;

/// <summary>
/// Wraps access to sharepoint data in sql
/// </summary>
public class GraphClient
{
    private GraphClientConfig? config;
    private GraphServiceClient client;
    private FileExtensionContentTypeProvider fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();

    /// <summary>
    /// Creates a new Graph Client
    /// </summary>
    /// <param name="configuration"></param>
    public GraphClient(IConfiguration configuration)
    {
        config = configuration.GetSection(GraphClientConfig.Key).Get<GraphClientConfig>();
        if (config is null)
            throw new Exception("Graph client config cannot be loaded.");

        var tokenAcquirerFactory = TokenAcquirerFactory.GetDefaultInstance();
        tokenAcquirerFactory.Services.AddMicrosoftGraph();
        var serviceProvider = tokenAcquirerFactory.Build();

        client = serviceProvider.GetRequiredService<GraphServiceClient>();


    }

    /// <summary>
    /// Makes an API Call to share point via graph to see if the file already exists based on filename, path, size, and sha1 hash.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="academicYear"></param>
    /// <returns></returns>

    public async Task<bool> IsInSharepointFolder(DataBaseFile file, int academicYear)
    {
        var data = await client.Drives.GetAsync(r => r.Options.WithAppOnly());
        return true;
        /*

        https://learn.microsoft.com/en-us/graph/api/driveitem-search?view=graph-rest-1.0&tabs=http

        /groups/{group-id}/drive/root/search(q='{search-text}')
https://learn.microsoft.com/en-us/graph/api/resources/driveitem?view=graph-rest-1.0
        */
    }

    /// <summary>
    /// Adds a File to an existing Folder
    /// </summary>
    /// <param name="file"></param>
    /// <param name="year"></param>
    /// <returns></returns>

    public async Task AddFileToSharepointFolder(DataBaseFile file, int year, byte[] content)
    {
        string Foldername = FolderNameFromYear(year);
        var data = await client.Drives.GetAsync(r => r.Options.WithAppOnly());

        string? mime;
        if (!fileExtensionContentTypeProvider.TryGetContentType(file.FileName, out mime))
        {
            ///if  unknown file type just upload as binary and figure out later
            mime = "application/octet-stream";
        }


        //see https://learn.microsoft.com/en-us/graph/sdks/large-file-upload?tabs=csharp
        //https://learn.microsoft.com/en-us/sharepoint/dev/sp-add-ins/working-with-folders-and-files-with-rest#working-with-files-by-using-rest
        /*PUT https://graph.microsoft.com/v1.0/me/drive/root:/FolderA/FileB.txt:/content
    Content-Type: text/plain

    The contents of the file goes here.*/
    }

    /// <summary>
    /// Add a subfolder to the main folder
    /// </summary>
    /// <param name="foldername"></param>
    /// <returns></returns>
    public async Task AddFolderToSharePoint(string foldername)
    {
        //var data = await client.Drives[config.driveId].GetAsync(r => r.Options.WithAppOnly())
        /* https://learn.microsoft.com/en-us/graph/api/driveitem-post-children?view=graph-rest-1.0&tabs=http
        POST https://graph.microsoft.com/v1.0/me/drive/root/children
Content-Type: application/json

{
  "name": "New Folder",
  "folder": { },
  "@microsoft.graph.conflictBehavior": "rename"
}*/

    }
    /// <summary>
    /// Make a subfolder name
    /// </summary>
    /// <param name="year"></param>
    /// <returns></returns>

    public string FolderNameFromYear(int year)
    {
        return $"{config!.sharepointRootFolder}/{year} - {year + 1}";
    }

    /// <summary>
    /// Stuff we are getting from the config file.
    /// </summary>
    private class GraphClientConfig
    {
        public static readonly string Key = "graph";
        public string driveId { get; set; } = "";
        public string sharepointRootFolder { get; set; } = "";
    }
}
