// ReSharper disable once CheckNamespace
namespace DotNetBrightener;

public interface IDependency
{
        
}

public interface ISingletonDependency: IDependency
{
        
}

public interface ITransientDependency: IDependency {}