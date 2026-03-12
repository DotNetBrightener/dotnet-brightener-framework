using Shouldly;
using Xunit;

public class PredicateBuilderTests
{
    [Fact]
    public void TestRepositoryUpdate_ShouldBeAbleToUpdateWithoutRetrievingData()
    {
        // Example usage:

        var entitiesCollection = new List<Entity>
        {
            new Entity
            {
                TargetProp = "value1"
            },
            new Entity
            {
                TargetProp = "value2"
            },
            new Entity
            {
                TargetProp = "value3"
            }
            // omitted elements
        };

        var arrayOfValues = new List<string>
        {
            "value1",
            "value2"
            // omitted values
        };


        var predicate = PredicateBuilder.BuildInOperatorExpression<Entity, string>("TargetProp", arrayOfValues);

        var filteredCollection = entitiesCollection.Where(predicate.Compile());

        // Test the filtered collection
        foreach (var entity in filteredCollection)
        {
            Console.WriteLine(entity.TargetProp);
        }
    }
}

public class FilterParserTests
{

    public enum OperatorComparer
    {
        Equal,
        NotEqual,
        GreaterThan,
        LessThan,
        In
    }

    public static class FilterParser
    {
        public static (OperatorComparer, List<string>) ParseFilterCommand(string inputString)
        {
            // Splitting the input string by '(' and ')'
            var parts = inputString.Split([
                                              '(', ')'
                                          ],
                                          StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
            {
                throw new ArgumentException("Invalid input string format");
            }

            var operatorString = parts[0].Trim(); // Fetching the operator string
            var valuesString   = parts[1];        // Fetching the values part

            // Mapping operator string to OperatorComparer enum
            OperatorComparer actualOperator;

            switch (operatorString.ToLower())
            {
                case "in":
                    actualOperator = OperatorComparer.In;

                    break;
                // Add other cases for more operators if needed
                default:
                    throw new ArgumentException("Invalid operator");
            }

            // Extracting filter values
            var filterValues = valuesString
                              .Split([
                                         ','
                                     ],
                                     StringSplitOptions.RemoveEmptyEntries)
                              .Select(value => value.Trim())
                              .ToList();

            return (actualOperator, filterValues);
        }
    }

    [Fact]
    public void TestRepositoryUpdate_ShouldBeAbleToUpdateWithoutRetrievingData()
    {

        // Example usage:

        var inputString      = "in(value1,value2,value3)";
        var expectedOperator = OperatorComparer.In;
        var expectedFilterValues = new List<string>
        {
            "value1",
            "value2",
            "value3"
        };

        var (actualOperator, filterValues) = FilterParser.ParseFilterCommand(inputString);

        Console.WriteLine($"Actual Operator: {actualOperator}");
        Console.WriteLine("Actual Filter Values:");
        actualOperator.ShouldBe(expectedOperator);

        foreach (var value in filterValues)
        {
            Console.WriteLine(value);
            expectedFilterValues.ShouldContain(value);
        }
    }
}