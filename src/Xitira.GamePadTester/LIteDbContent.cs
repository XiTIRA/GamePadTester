using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using LiteDB;
using Microsoft.Xna.Framework.Content;

namespace Xitira.GamePadTester;

public class LiteDbContent : ContentManager
{
    private MemoryStream _memoryStream = new();
    private ILiteStorage<string> _store;
    private ILiteDatabase _db;
    
    public LiteDbContent(System.IServiceProvider serviceProvider, string rootDirectory) 
        : base(serviceProvider, rootDirectory)
    {
        var names  = LauncherAssembly.GetManifestResourceNames();
        var content = names.First(x  => x.EndsWith(".db"));
        
        using var resourceStream = LauncherAssembly.GetManifestResourceStream(content);
        if (resourceStream == null)
        {
            throw new FileNotFoundException($"Could not find resource '{content}'");
        }
        resourceStream.CopyTo(_memoryStream);
        _memoryStream.Position = 0;
        
        _db = new LiteDatabase(_memoryStream);
        _store = _db.GetStorage<string>("asset","assetChunks");
    }

    public Stream LoadRaw(string assetName)
    {
        return OpenStream(assetName);
    }

    protected override Stream OpenStream(string assetName)
    {
        var stream = new MemoryStream();
        _store.Download(assetName, stream);
        stream.Position = 0;
        return stream;
    }

    public static Assembly LauncherAssembly;
}