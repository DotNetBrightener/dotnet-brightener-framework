namespace VampireCoder.SharedUtils;

public class UuidV7
{
    public static Guid New()
    {
        return Ulid.NewUlid().ToGuid();
    }
}