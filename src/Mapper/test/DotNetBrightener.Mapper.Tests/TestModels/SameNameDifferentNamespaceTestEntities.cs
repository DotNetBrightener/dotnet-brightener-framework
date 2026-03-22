// Test entities for GitHub issue #249
// Generator issue when DTOs with the same name are under different namespaces      

namespace DotNetBrightener.Mapper.Tests.TestModels.SameName.A
{
    public class Employee
    {
        public decimal Salary     { get; set; }
        public string  Department { get; set; } = string.Empty;
    }

    [MappingTarget<Employee>()]
    public partial class EmployeeDto {}
}

namespace DotNetBrightener.Mapper.Tests.TestModels.SameName.B
{
    public class Employee
    {
        public decimal Salary { get; set; }
        public string  Role   { get; set; } = string.Empty;
    }

    [MappingTarget<Employee>()]
    public partial class EmployeeDto {}
}
