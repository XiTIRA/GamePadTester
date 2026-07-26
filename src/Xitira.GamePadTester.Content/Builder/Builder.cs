using Microsoft.Xna.Framework.Content.Pipeline;
using MonoGame.Framework.Content.Pipeline.Builder;
using LiteDB;


List<ContentBuilderParams> contentParams = new();

var workingDirectory = @"/big/Source/Xitira.GamePadTester/src/Xitira.GamePadTester.Content/"; 
var sourceFolder = "Assets";
var mode = ContentBuilderMode.Builder;

contentParams.Add(new ContentBuilderParams()
{
    Mode = mode,
    WorkingDirectory = workingDirectory, 
    SourceDirectory = sourceFolder,
    OutputDirectory = "ContentVK", 
    Platform = TargetPlatform.DesktopVK
});

contentParams.Add(new ContentBuilderParams()
{
    Mode = mode,
    WorkingDirectory = workingDirectory, 
    SourceDirectory = sourceFolder,
    OutputDirectory = "ContentGL", 
    Platform = TargetPlatform.DesktopGL
});

contentParams.Add(new ContentBuilderParams()
{
    Mode = mode,
    WorkingDirectory = workingDirectory, 
    SourceDirectory = sourceFolder,
    OutputDirectory = "ContentAND", 
    Platform = TargetPlatform.Android,
});

contentParams.Add(new ContentBuilderParams()
{
    Mode = mode,
    WorkingDirectory = workingDirectory, 
    SourceDirectory = sourceFolder,
    OutputDirectory = "ContentDX", 
    Platform = TargetPlatform.Windows,
});

foreach (var contentParam in contentParams)
{
    var builder = new Builder();
    builder.Run(contentParam);
    
    var files = Directory.GetFiles(contentParam.OutputDirectory, "*.*", SearchOption.AllDirectories)
        .Where(file => !file.EndsWith(".db", StringComparison.OrdinalIgnoreCase))
        .ToList();
    
        if (File.Exists($"{contentParam.OutputDirectory}/Content.db"))
            File.Delete($"{contentParam.OutputDirectory}/Content.db");
    
    {
        using var litedb = new LiteDatabase($"{contentParam.OutputDirectory}/Content.db");
        var storage = litedb.GetStorage<string>("asset","assetChunks");
        
        foreach (var file in files)
        {
            var assetName = Path.ChangeExtension(file, null)
                .Replace($"{contentParam.OutputDirectory}/Content", string.Empty).Replace("\\", "/").TrimStart('/');

            storage.Upload(assetName, file);
        }
    }
}

return 0;


public class Builder : ContentBuilder
{
    public override IContentCollection GetContentCollection()
    {
        var contentCollection = new ContentCollection();
        contentCollection.Include<WildcardRule>("*.fx");
        contentCollection.Include<WildcardRule>("*.png");
        contentCollection.IncludeCopy<WildcardRule>("*.fnt");
        contentCollection.Include<WildcardRule>("*.spritefont");
        contentCollection.IncludeCopy<WildcardRule>("*.ttf");

        // By default, no content will be imported from the Assets folder using the default importer for their file type.
        // Please define your content collection rules here.

        /* Examples

        // Import all content in the Assets folder using the default importer for their file type.

        // Only copy content from the assets folder rather than build it with the pipeline.
        contentCollection.IncludeCopy<WildcardRule>("*.json");

        // Exclude assets that match the pattern., only required overriding a default import behaviour.
        contentCollection.Exclude<WildcardRule>("Font/*.txt");

        // Include a specific asset with processor parameters.
        contentCollection.Include("Models/character.glb", new FbxImporter(),
            new MeshAnimatedModelProcessor()
            {
                Scale = 100.0f
            }
        );
        */

        return contentCollection;
    }
}