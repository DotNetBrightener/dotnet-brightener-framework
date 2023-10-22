using System.ComponentModel.DataAnnotations;

namespace DotNetBrightener.DataAccess.EF.Tests;

public class TestEntity
{
    [Key]
    public long Id { get; set; }


    [MaxLength(512)]
    public string Value { get; set; }
}