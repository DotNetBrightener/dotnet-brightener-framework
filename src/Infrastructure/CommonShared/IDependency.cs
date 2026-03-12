namespace WebApp.CommonShared;

/// <summary>
///     Marks the specified service as a Dependency, which could be used to automatically register itself and the implementation class into <see cref="IServiceCollection"/>
/// </summary>
public interface IDependency;

/// <summary>
///     Marks the specified service as a Dependency, which could be used to automatically register itself and the implementation class into <see cref="IServiceCollection"/>.
///     When the service is automatically registered, it will be registered as Singleton
/// </summary>
public interface ISingletonDependency: IDependency;

/// <summary>
///     Marks the specified service as a Dependency, which could be used to automatically register itself and the implementation class into <see cref="IServiceCollection"/>.
///     When the service is automatically registered, it will be registered as Transient
/// </summary>
public interface ITransientDependency: IDependency;