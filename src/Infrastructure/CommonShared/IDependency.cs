namespace WebApp.CommonShared;

public interface IDependency
{
        
}

public interface ISingletonDependency: IDependency
{
        
}

public interface ITransientDependency: IDependency {}


public interface IActionFilterProvider
{

}