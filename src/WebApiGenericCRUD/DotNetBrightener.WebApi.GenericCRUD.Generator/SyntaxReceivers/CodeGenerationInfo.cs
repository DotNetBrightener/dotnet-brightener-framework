﻿namespace WebApi.GenericCRUD.Generator.SyntaxReceivers;

public class CodeGenerationInfo
{
    public string TargetEntity          { get; set; }
    public string TargetEntityNamespace { get; set; }

    public string ControllerAssembly     { get; set; }
    public string ControllerAssemblyPath { get; set; }
    public string ControllerNamespace    { get; set; }
    public string ControllerPath         { get; set; }

    public string DataServiceAssembly  { get; set; }
    public string DataServiceNamespace { get; set; }
    public string DataServicePath      { get; set; }
}