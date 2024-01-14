namespace CRUDApiDemo;

public class TestEntity
{
    public string?  Description  { get; set; }
    public long     DisplayOrder { get; set; }
    public Guid     Identifier   { get; set; }
    public DateTime DateCreated  { get; set; }
    public bool     IsActive     { get; set; }
}

public class TestData
{
    public static readonly List<TestEntity> TestEntities = new List<TestEntity>
    {
        new TestEntity
        {
            Description  = "A fascinating tale of adventure and discovery.",
            DisplayOrder = 1,
            Identifier   = new Guid("11111111-1111-1111-1111-111111111111"),
            DateCreated  = new DateTime(2023, 5, 15, 10, 30, 0),
            IsActive     = true
        },
        new TestEntity
        {
            Description  = "Exploring the depths of creativity and imagination.",
            DisplayOrder = 2,
            Identifier   = new Guid("22222222-2222-2222-2222-222222222222"),
            DateCreated  = new DateTime(2022, 10, 8, 14, 45, 0),
            IsActive     = true
        },
        new TestEntity
        {
            Description  = "Embracing the joy of simplicity and tranquility.",
            DisplayOrder = 3,
            Identifier   = new Guid("33333333-3333-3333-3333-333333333333"),
            DateCreated  = new DateTime(2024, 3, 21, 9, 0, 0),
            IsActive     = true
        },
        new TestEntity
        {
            Description  = "Capturing moments of beauty in everyday life.",
            DisplayOrder = 4,
            Identifier   = new Guid("44444444-4444-4444-4444-444444444444"),
            DateCreated  = new DateTime(2023, 7, 12, 16, 20, 0),
            IsActive     = true
        },
        new TestEntity
        {
            Description  = "Unraveling the mysteries of the universe.",
            DisplayOrder = 5,
            Identifier   = new Guid("55555555-5555-5555-5555-555555555555"),
            DateCreated  = new DateTime(2022, 12, 30, 11, 55, 0),
            IsActive     = true
        },
        new TestEntity
        {
            Description  = "Celebrating diversity and the richness of cultures.",
            DisplayOrder = 6,
            Identifier   = new Guid("66666666-6666-6666-6666-666666666666"),
            DateCreated  = new DateTime(2024, 1, 5, 8, 15, 0),
            IsActive     = true
        },
        new TestEntity
        {
            Description  = "Embracing change and new beginnings.",
            DisplayOrder = 7,
            Identifier   = new Guid("77777777-7777-7777-7777-777777777777"),
            DateCreated  = new DateTime(2023, 8, 18, 17, 0, 0),
            IsActive     = true
        },
        new TestEntity
        {
            Description  = "Cherishing the bonds that connect us all.",
            DisplayOrder = 8,
            Identifier   = new Guid("88888888-8888-8888-8888-888888888888"),
            DateCreated  = new DateTime(2022, 9, 27, 13, 40, 0),
            IsActive     = true
        },
        new TestEntity
        {
            Description  = "Embarking on a journey of self-discovery and growth.",
            DisplayOrder = 9,
            Identifier   = new Guid("99999999-9999-9999-9999-999999999999"),
            DateCreated  = new DateTime(2024, 6, 7, 9, 50, 0),
            IsActive     = false
        },
        new TestEntity
        {
            Description  = "Finding beauty in the ordinary and extraordinary.",
            DisplayOrder = 10,
            Identifier   = new Guid("00000000-0000-0000-0000-000000000000"),
            DateCreated  = new DateTime(2023, 2, 14, 12, 0, 0),
            IsActive     = false
        }
    };
}