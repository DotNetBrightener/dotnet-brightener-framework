namespace DotNetBrightener.gRPC.Generator.Models;

internal abstract class FileContentModel
{
    public string FileContent { get; set; }

    public string FilePath { get; set; }

    public string FileName { get; set; }

    public string ClassName { get; set; }
}

internal class ProtoFileModel : FileContentModel
{
}

internal class ServiceFileModel : FileContentModel
{
}


internal class MessageFileModel : FileContentModel
{
}

internal class ServiceImplFileModel : FileContentModel
{
}
