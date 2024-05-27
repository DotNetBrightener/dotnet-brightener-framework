#nullable enable
namespace DotNetBrightener.DataAccess.Models.Guards;

public static class Guards
{
    public static void AssertEntityRecoverable<T>() where T : class
    {
        if (!typeof(T).HasProperty<bool>(nameof(IAuditableEntity.IsDeleted)))
        {
            throw new
                NotSupportedException($"The entity type {typeof(T).Name} does not support soft-delete. Therefore, the deletion cannot be reverted");
        }
    }
}