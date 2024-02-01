namespace DotNetBrightener.gRPC.Generator.SyntaxReceivers;

internal class CodeGenerationInfo
{
    public string TargetEntity          { get; set; }
    public string TargetEntityNamespace { get; set; }

    public string GrpcAssembly     { get; set; }
    public string GrpcAssemblyPath { get; set; }
    public string GrpcNamespace    { get; set; }
    public string GrpcPackageName  { get; set; }
    public string ProtoFilePath    { get; set; }
    public string ServiceFilePath  { get; set; }

    public string DataServiceAssembly  { get; set; }
    public string DataServiceNamespace { get; set; }
    public string DataServicePath      { get; set; }
}