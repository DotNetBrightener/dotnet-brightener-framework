using DotNetBrightener.Mapper.Mapping;
using DotNetBrightener.Mapper.Tests.TestModels;

namespace DotNetBrightener.Mapper.Tests.UnitTests.Core.Target;

public class ComplexTypeTargetsTests
{
    [Fact]
    public void TargetConstructor_ShouldMapSimpleNestedType_WhenUsingChildrenParameter()
    {
        // Arrange
        var company = new CompanyEntity
        {
            Id = 1,
            Name = "Acme Corp",
            Industry = "Technology",
            HeadquartersAddress = new AddressEntity
            {
                Street = "123 Main St",
                City = "San Francisco",
                State = "CA",
                ZipCode = "94105",
                Country = "USA"
            }
        };

        // Act
        var dto = new CompanyTarget(company);

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(1);
        dto.Name.ShouldBe("Acme Corp");
        dto.Industry.ShouldBe("Technology");

        // Child target should be automatically mapped
        dto.HeadquartersAddress.ShouldNotBeNull();
        dto.HeadquartersAddress.Street.ShouldBe("123 Main St");
        dto.HeadquartersAddress.City.ShouldBe("San Francisco");
        dto.HeadquartersAddress.State.ShouldBe("CA");
        dto.HeadquartersAddress.ZipCode.ShouldBe("94105");
        dto.HeadquartersAddress.Country.ShouldBe("USA");
    }

    [Fact]
    public void TargetConstructor_ShouldMapMultipleDifferentNestedTypes_WhenUsingChildrenParameter()
    {
        // Arrange
        var employee = new StaffMember
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PasswordHash = "secrethash123",
            HireDate = new DateTime(2020, 1, 15),
            Salary = 75000m,
            Company = new CompanyEntity
            {
                Id = 10,
                Name = "Tech Solutions",
                Industry = "Software",
                HeadquartersAddress = new AddressEntity
                {
                    Street = "456 Tech Blvd",
                    City = "Austin",
                    State = "TX",
                    ZipCode = "78701",
                    Country = "USA"
                }
            },
            HomeAddress = new AddressEntity
            {
                Street = "789 Residential Ave",
                City = "Round Rock",
                State = "TX",
                ZipCode = "78664",
                Country = "USA"
            }
        };

        // Act
        var dto = new StaffMemberTarget(employee);

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(1);
        dto.FirstName.ShouldBe("John");
        dto.LastName.ShouldBe("Doe");
        dto.Email.ShouldBe("john.doe@example.com");
        dto.HireDate.ShouldBe(new DateTime(2020, 1, 15));

        // Excluded properties should not exist
        var dtoType = dto.GetType();
        dtoType.GetProperty("PasswordHash").ShouldBeNull("PasswordHash should be excluded");
        dtoType.GetProperty("Salary").ShouldBeNull("Salary should be excluded");

        // Company child target should be mapped
        dto.Company.ShouldNotBeNull();
        dto.Company.Id.ShouldBe(10);
        dto.Company.Name.ShouldBe("Tech Solutions");
        dto.Company.Industry.ShouldBe("Software");

        // Nested child target (Company -> Address)
        dto.Company.HeadquartersAddress.ShouldNotBeNull();
        dto.Company.HeadquartersAddress.Street.ShouldBe("456 Tech Blvd");
        dto.Company.HeadquartersAddress.City.ShouldBe("Austin");

        // HomeAddress child target should be mapped
        dto.HomeAddress.ShouldNotBeNull();
        dto.HomeAddress.Street.ShouldBe("789 Residential Ave");
        dto.HomeAddress.City.ShouldBe("Round Rock");
        dto.HomeAddress.State.ShouldBe("TX");
    }

    [Fact]
    public void TargetConstructor_ShouldMapDeeplyNestedTypes_WhenUsingChildrenParameter()
    {
        // Arrange
        var department = new DepartmentEntity
        {
            Id = 5,
            Name = "Engineering",
            EmployeeCount = 50,
            Company = new CompanyEntity
            {
                Id = 20,
                Name = "Innovate Inc",
                Industry = "Innovation",
                HeadquartersAddress = new AddressEntity
                {
                    Street = "100 Innovation Way",
                    City = "Seattle",
                    State = "WA",
                    ZipCode = "98101",
                    Country = "USA"
                }
            },
            Manager = new StaffMember
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@innovate.com",
                PasswordHash = "anothersecret",
                HireDate = new DateTime(2018, 3, 1),
                Salary = 120000m,
                Company = new CompanyEntity
                {
                    Id = 20,
                    Name = "Innovate Inc",
                    Industry = "Innovation",
                    HeadquartersAddress = new AddressEntity
                    {
                        Street = "100 Innovation Way",
                        City = "Seattle",
                        State = "WA",
                        ZipCode = "98101",
                        Country = "USA"
                    }
                },
                HomeAddress = new AddressEntity
                {
                    Street = "555 Manager Lane",
                    City = "Bellevue",
                    State = "WA",
                    ZipCode = "98004",
                    Country = "USA"
                }
            }
        };

        // Act
        var dto = new DepartmentTarget(department);

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(5);
        dto.Name.ShouldBe("Engineering");
        dto.EmployeeCount.ShouldBe(50);

        // Company child target
        dto.Company.ShouldNotBeNull();
        dto.Company.Id.ShouldBe(20);
        dto.Company.Name.ShouldBe("Innovate Inc");
        dto.Company.HeadquartersAddress.ShouldNotBeNull();
        dto.Company.HeadquartersAddress.City.ShouldBe("Seattle");

        // Manager (Employee) child target
        dto.Manager.ShouldNotBeNull();
        dto.Manager.Id.ShouldBe(2);
        dto.Manager.FirstName.ShouldBe("Jane");
        dto.Manager.LastName.ShouldBe("Smith");
        dto.Manager.Email.ShouldBe("jane.smith@innovate.com");

        // Manager's Company child target
        dto.Manager.Company.ShouldNotBeNull();
        dto.Manager.Company.Name.ShouldBe("Innovate Inc");
        dto.Manager.Company.HeadquartersAddress.City.ShouldBe("Seattle");

        // Manager's HomeAddress child target
        dto.Manager.HomeAddress.ShouldNotBeNull();
        dto.Manager.HomeAddress.Street.ShouldBe("555 Manager Lane");
        dto.Manager.HomeAddress.City.ShouldBe("Bellevue");
    }

    [Fact]
    public void ToSource_ShouldMapBackToSourceType_WithSimpleNestedType()
    {
        // Arrange
        var originalCompany = new CompanyEntity
        {
            Id = 1,
            Name = "Test Corp",
            Industry = "Testing",
            HeadquartersAddress = new AddressEntity
            {
                Street = "111 Test St",
                City = "Test City",
                State = "TC",
                ZipCode = "12345",
                Country = "Testland"
            }
        };

        var dto = new CompanyTarget(originalCompany);

        // Act
        var mapped = dto.ToSource();

        // Assert
        mapped.ShouldNotBeNull();
        mapped.Id.ShouldBe(1);
        mapped.Name.ShouldBe("Test Corp");
        mapped.Industry.ShouldBe("Testing");

        mapped.HeadquartersAddress.ShouldNotBeNull();
        mapped.HeadquartersAddress.Street.ShouldBe("111 Test St");
        mapped.HeadquartersAddress.City.ShouldBe("Test City");
        mapped.HeadquartersAddress.State.ShouldBe("TC");
        mapped.HeadquartersAddress.ZipCode.ShouldBe("12345");
        mapped.HeadquartersAddress.Country.ShouldBe("Testland");
    }

    [Fact]
    public void ToSource_ShouldMapBackToSourceType_WithMultipleNestedTypes()
    {
        // Arrange
        var originalEmployee = new StaffMember
        {
            Id = 3,
            FirstName = "Alice",
            LastName = "Johnson",
            Email = "alice@example.com",
            PasswordHash = "willbemissing",
            HireDate = new DateTime(2019, 6, 1),
            Salary = 85000m,
            Company = new CompanyEntity
            {
                Id = 30,
                Name = "Example Co",
                Industry = "Examples",
                HeadquartersAddress = new AddressEntity
                {
                    Street = "200 Example Rd",
                    City = "Example City",
                    State = "EX",
                    ZipCode = "55555",
                    Country = "Exampleland"
                }
            },
            HomeAddress = new AddressEntity
            {
                Street = "300 Home St",
                City = "Home Town",
                State = "HT",
                ZipCode = "66666",
                Country = "Homeland"
            }
        };

        var dto = new StaffMemberTarget(originalEmployee);

        // Act
        var mapped = dto.ToSource();

        // Assert
        mapped.ShouldNotBeNull();
        mapped.Id.ShouldBe(3);
        mapped.FirstName.ShouldBe("Alice");
        mapped.LastName.ShouldBe("Johnson");
        mapped.Email.ShouldBe("alice@example.com");
        mapped.HireDate.ShouldBe(new DateTime(2019, 6, 1));

        // Excluded properties should have default values
        mapped.PasswordHash.ShouldBeEmpty();
        mapped.Salary.ShouldBe(0m);

        // Company should be mapped back
        mapped.Company.ShouldNotBeNull();
        mapped.Company.Id.ShouldBe(30);
        mapped.Company.Name.ShouldBe("Example Co");
        mapped.Company.Industry.ShouldBe("Examples");
        mapped.Company.HeadquartersAddress.ShouldNotBeNull();
        mapped.Company.HeadquartersAddress.Street.ShouldBe("200 Example Rd");
        mapped.Company.HeadquartersAddress.City.ShouldBe("Example City");

        // HomeAddress should be mapped back
        mapped.HomeAddress.ShouldNotBeNull();
        mapped.HomeAddress.Street.ShouldBe("300 Home St");
        mapped.HomeAddress.City.ShouldBe("Home Town");
    }

    [Fact]
    public void Projection_ShouldWork_WithChildTargets()
    {
        // Arrange
        var companies = new[]
        {
            new CompanyEntity
            {
                Id = 1,
                Name = "Company A",
                Industry = "Industry A",
                HeadquartersAddress = new AddressEntity { City = "City A", State = "CA" }
            },
            new CompanyEntity
            {
                Id = 2,
                Name = "Company B",
                Industry = "Industry B",
                HeadquartersAddress = new AddressEntity { City = "City B", State = "NY" }
            }
        }.AsQueryable();

        // Act
        var dtos = companies.Select(CompanyTarget.Projection).ToList();

        // Assert
        dtos.Count().ShouldBe(2);

        dtos[0].Id.ShouldBe(1);
        dtos[0].Name.ShouldBe("Company A");
        dtos[0].HeadquartersAddress.ShouldNotBeNull();
        dtos[0].HeadquartersAddress.City.ShouldBe("City A");
        dtos[0].HeadquartersAddress.State.ShouldBe("CA");

        dtos[1].Id.ShouldBe(2);
        dtos[1].Name.ShouldBe("Company B");
        dtos[1].HeadquartersAddress.ShouldNotBeNull();
        dtos[1].HeadquartersAddress.City.ShouldBe("City B");
        dtos[1].HeadquartersAddress.State.ShouldBe("NY");
    }

    [Fact]
    public void ChildTarget_TypeProperty_ShouldBeCorrectType()
    {
        // Arrange & Act
        var dto = new CompanyTarget(new CompanyEntity
        {
            HeadquartersAddress = new AddressEntity { City = "Test" }
        });

        // Assert
        dto.HeadquartersAddress.ShouldBeOfType<AddressTarget>();
        dto.HeadquartersAddress.ShouldNotBeAssignableTo<AddressEntity>();
    }

    [Fact]
    public void ToTarget_ShouldMapNestedTargets_WhenUsingExtensionMethod()
    {
        // Arrange
        var staffMember = new StaffMember
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PasswordHash = "secrethash123",
            HireDate = new DateTime(2020, 1, 15),
            Salary = 75000m,
            Company = new CompanyEntity
            {
                Id = 10,
                Name = "Tech Solutions",
                Industry = "Software",
                HeadquartersAddress = new AddressEntity
                {
                    Street = "456 Tech Blvd",
                    City = "Austin",
                    State = "TX",
                    ZipCode = "78701",
                    Country = "USA"
                }
            },
            HomeAddress = new AddressEntity
            {
                Street = "789 Residential Ave",
                City = "Round Rock",
                State = "TX",
                ZipCode = "78664",
                Country = "USA"
            }
        };

        // Act
        var dto = staffMember.ToTarget<StaffMemberTarget>();

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(1);
        dto.FirstName.ShouldBe("John");
        dto.LastName.ShouldBe("Doe");
        dto.Email.ShouldBe("john.doe@example.com");

        // Verify nested Company target
        dto.Company.ShouldNotBeNull();
        dto.Company.ShouldBeOfType<CompanyTarget>();
        dto.Company.Id.ShouldBe(10);
        dto.Company.Name.ShouldBe("Tech Solutions");
        dto.Company.Industry.ShouldBe("Software");

        // Verify nested Address within Company
        dto.Company.HeadquartersAddress.ShouldNotBeNull();
        dto.Company.HeadquartersAddress.ShouldBeOfType<AddressTarget>();
        dto.Company.HeadquartersAddress.Street.ShouldBe("456 Tech Blvd");
        dto.Company.HeadquartersAddress.City.ShouldBe("Austin");

        // Verify HomeAddress target
        dto.HomeAddress.ShouldNotBeNull();
        dto.HomeAddress.ShouldBeOfType<AddressTarget>();
        dto.HomeAddress.Street.ShouldBe("789 Residential Ave");
        dto.HomeAddress.City.ShouldBe("Round Rock");
    }

    [Fact]
    public void SelectTargets_ShouldMapNestedTargets_WhenProjectingCollection()
    {
        // Arrange
        var staffMembers = new[]
        {
            new StaffMember
            {
                Id = 1,
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice@example.com",
                Company = new CompanyEntity
                {
                    Id = 100,
                    Name = "Company A",
                    Industry = "Tech",
                    HeadquartersAddress = new AddressEntity { City = "New York", State = "NY" }
                },
                HomeAddress = new AddressEntity { City = "Brooklyn", State = "NY" }
            },
            new StaffMember
            {
                Id = 2,
                FirstName = "Bob",
                LastName = "Johnson",
                Email = "bob@example.com",
                Company = new CompanyEntity
                {
                    Id = 200,
                    Name = "Company B",
                    Industry = "Finance",
                    HeadquartersAddress = new AddressEntity { City = "Chicago", State = "IL" }
                },
                HomeAddress = new AddressEntity { City = "Evanston", State = "IL" }
            }
        }.AsQueryable();

        // Act
        var dtos = staffMembers.SelectTargets<StaffMemberTarget>().ToList();

        // Assert
        dtos.Count().ShouldBe(2);

        // First staff member
        dtos[0].FirstName.ShouldBe("Alice");
        dtos[0].Company.ShouldBeOfType<CompanyTarget>();
        dtos[0].Company.Name.ShouldBe("Company A");
        dtos[0].Company.HeadquartersAddress.ShouldBeOfType<AddressTarget>();
        dtos[0].Company.HeadquartersAddress.City.ShouldBe("New York");
        dtos[0].HomeAddress.ShouldBeOfType<AddressTarget>();
        dtos[0].HomeAddress.City.ShouldBe("Brooklyn");

        // Second staff member
        dtos[1].FirstName.ShouldBe("Bob");
        dtos[1].Company.ShouldBeOfType<CompanyTarget>();
        dtos[1].Company.Name.ShouldBe("Company B");
        dtos[1].Company.HeadquartersAddress.ShouldBeOfType<AddressTarget>();
        dtos[1].Company.HeadquartersAddress.City.ShouldBe("Chicago");
        dtos[1].HomeAddress.ShouldBeOfType<AddressTarget>();
        dtos[1].HomeAddress.City.ShouldBe("Evanston");
    }

    [Fact]
    public void ToTarget_WithTypedParameters_ShouldMapNestedTargets()
    {
        // Arrange
        var company = new CompanyEntity
        {
            Id = 1,
            Name = "Acme Corp",
            Industry = "Technology",
            HeadquartersAddress = new AddressEntity
            {
                Street = "123 Main St",
                City = "San Francisco",
                State = "CA",
                ZipCode = "94105",
                Country = "USA"
            }
        };

        // Act - Using typed ToTarget for better performance
        var dto = company.ToTarget<CompanyEntity, CompanyTarget>();

        // Assert
        dto.ShouldNotBeNull();
        dto.Id.ShouldBe(1);
        dto.Name.ShouldBe("Acme Corp");
        dto.Industry.ShouldBe("Technology");

        // Child target should be automatically mapped
        dto.HeadquartersAddress.ShouldNotBeNull();
        dto.HeadquartersAddress.ShouldBeOfType<AddressTarget>();
        dto.HeadquartersAddress.Street.ShouldBe("123 Main St");
        dto.HeadquartersAddress.City.ShouldBe("San Francisco");
        dto.HeadquartersAddress.State.ShouldBe("CA");
        dto.HeadquartersAddress.ZipCode.ShouldBe("94105");
        dto.HeadquartersAddress.Country.ShouldBe("USA");
    }

    [Fact]
    public void SelectTargets_WithTypedParameters_ShouldMapNestedTargets()
    {
        // Arrange
        var companies = new[]
        {
            new CompanyEntity
            {
                Id = 1,
                Name = "Company A",
                Industry = "Industry A",
                HeadquartersAddress = new AddressEntity { City = "City A", State = "CA" }
            },
            new CompanyEntity
            {
                Id = 2,
                Name = "Company B",
                Industry = "Industry B",
                HeadquartersAddress = new AddressEntity { City = "City B", State = "NY" }
            }
        }.AsQueryable();

        // Act - Using typed SelectTargets for better performance
        var dtos = companies.SelectTargets<CompanyEntity, CompanyTarget>().ToList();

        // Assert
        dtos.Count().ShouldBe(2);

        dtos[0].Id.ShouldBe(1);
        dtos[0].Name.ShouldBe("Company A");
        dtos[0].HeadquartersAddress.ShouldNotBeNull();
        dtos[0].HeadquartersAddress.ShouldBeOfType<AddressTarget>();
        dtos[0].HeadquartersAddress.City.ShouldBe("City A");
        dtos[0].HeadquartersAddress.State.ShouldBe("CA");

        dtos[1].Id.ShouldBe(2);
        dtos[1].Name.ShouldBe("Company B");
        dtos[1].HeadquartersAddress.ShouldNotBeNull();
        dtos[1].HeadquartersAddress.ShouldBeOfType<AddressTarget>();
        dtos[1].HeadquartersAddress.City.ShouldBe("City B");
        dtos[1].HeadquartersAddress.State.ShouldBe("NY");
    }
}
