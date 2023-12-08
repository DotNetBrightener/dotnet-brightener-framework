using DotNetBrightener.WebApi.GenericCRUD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotNetBrightener.WebApi.GenericCRUD.Extensions;

public static class QueryableDeepFilterExtensions
{
    /// <summary>
    ///     Add addition query into initial one to order the result, and optionally pagination
    /// </summary>
    /// <param name="entitiesQuery">The initial query</param>
    /// <returns>
    ///     The new query with extra operations e.g ordering / pagination from <see cref="filterDictionary"/>
    /// </returns>
    public static IQueryable<TIn> AddOrderingAndPaginationQuery<TIn>(this IQueryable<TIn> entitiesQuery,
                                                                     Dictionary<string, string> filterDictionary,
                                                                     string entityIdColumnName,
                                                                     out int pageSize,
                                                                     out int pageIndex)
        where TIn : class
    {
        var paginationQuery = filterDictionary.ToQueryModel<BaseQueryModel>();

        pageSize = paginationQuery.PageSize;

        if (pageSize == 0)
        {
            pageSize = 20;
        }

        pageIndex = paginationQuery.PageIndex;

        var orderedEntitiesQuery = entitiesQuery.OrderBy(ExpressionExtensions
                                                            .BuildMemberAccessExpression<TIn>(entityIdColumnName));

        if (paginationQuery.OrderedColumns.Length > 0)
        {
            var sortIndex = 0;

            foreach (var orderByColumn in paginationQuery.OrderedColumns)
            {
                var actualColumnName = orderByColumn;

                if (orderByColumn.StartsWith("-"))
                {
                    actualColumnName = orderByColumn.Substring(1);
                    var orderByColumnExpr =
                        ExpressionExtensions.BuildMemberAccessExpression<TIn>(actualColumnName);

                    orderedEntitiesQuery = sortIndex == 0
                                               ? entitiesQuery.OrderByDescending(orderByColumnExpr)
                                               : orderedEntitiesQuery.ThenByDescending(orderByColumnExpr);
                }
                else
                {
                    var orderByColumnExpr =
                        ExpressionExtensions.BuildMemberAccessExpression<TIn>(actualColumnName);
                    orderedEntitiesQuery = sortIndex == 0
                                               ? entitiesQuery.OrderBy(orderByColumnExpr)
                                               : orderedEntitiesQuery.ThenBy(orderByColumnExpr);
                }

                sortIndex++;
            }
        }

        var itemsToSkip = pageIndex * pageSize;
        var itemsToTake = pageSize;

        return orderedEntitiesQuery.Skip(itemsToSkip)
                                   .Take(itemsToTake);
    }

    /// <summary>
    ///     Generates an <see cref="IQueryable{TIn}"/> for the given query, from the filter dictionary
    /// </summary>
    /// <typeparam name="TIn">The type associated with the query</typeparam>
    /// <param name="entitiesQuery">The initial query</param>
    /// <param name="filterDictionary">
    ///     The dictionary contains the filters to apply
    /// </param>
    /// <returns>
    ///     The new query with extra filters from <see cref="filterDictionary"/>
    /// </returns>
    public static async Task<IQueryable<TIn>> ApplyDeepFilters<TIn>(this IQueryable<TIn>       entitiesQuery,
                                                                    Dictionary<string, string> filterDictionary)
        where TIn : class
    {
        if (filterDictionary.Keys.Count == 0)
            return entitiesQuery;

        var predicateStatement = PredicateBuilder.True<TIn>();

        foreach (var filter in filterDictionary)
        {
            var          fieldName = filter.Key;
            var property  = ObtainPropertyPath<TIn>(fieldName);

            if (property == null)
                continue;

            var propertyUnderlingType = property.PropertyUnderlyingType;

            // only support single filter with string type
            if (propertyUnderlingType == typeof(string))
            {
                var predicateQuery = BuildStringPredicateQuery<TIn>(filter.Value, property);

                predicateStatement = predicateStatement.And(predicateQuery);

                continue;
            }
            
            var filterValues = filter.Value.Split(",",
                                                  StringSplitOptions.TrimEntries |
                                                  StringSplitOptions.RemoveEmptyEntries);

            if (propertyUnderlingType == typeof(int) ||
                propertyUnderlingType == typeof(long) ||
                propertyUnderlingType == typeof(float) ||
                propertyUnderlingType == typeof(double) ||
                propertyUnderlingType == typeof(decimal))
            {

                var predicateQuery = BuildNumericPredicateQuery<TIn>(filterValues, property);

                predicateStatement = predicateStatement.And(predicateQuery);

                continue;
            }

            if (propertyUnderlingType == typeof(DateTime) ||
                propertyUnderlingType == typeof(DateTimeOffset))
            {

                Expression<Func<TIn, bool>> predicateQuery = null;

                foreach (var value in filterValues)
                {
                    OperatorComparer? operation = GetComparisonOperations(value);

                    if (operation == null)
                        continue;

                    Expression<Func<TIn, bool>> subPredicateQuery      = null;
                    var                         filterValueSegments = value.Split("_");

                    var filterValueActualSegment = filterValueSegments[^1];

                    if (propertyUnderlingType == typeof(DateTime))
                    {
                        if (!DateTime.TryParse(filterValueActualSegment, out var filterValue))
                        {
                            throw new InvalidOperationException($"Date format cannot be recognized.");
                        }

                        subPredicateQuery = ExpressionExtensions.BuildPredicate<TIn>(filterValue,
                                                                                     operation!.Value,
                                                                                     property.Path);
                    }
                    else
                    {
                        if (!DateTimeOffset.TryParse(filterValueActualSegment, out var filterValue))
                        {
                            throw new InvalidOperationException($"Date format cannot be recognized.");
                        }

                        subPredicateQuery = ExpressionExtensions.BuildPredicate<TIn>(filterValue,
                                                                                     operation!.Value,
                                                                                     property.Path);

                    }

                    predicateQuery = predicateQuery != null ? predicateQuery.And(subPredicateQuery) : subPredicateQuery;
                }

                if (predicateQuery != null)
                {
                    predicateStatement = predicateStatement.And(predicateQuery);
                }
            }
        }

        return entitiesQuery.Where(predicateStatement);
    }

    private static Expression<Func<TIn, bool>> BuildNumericPredicateQuery<TIn>(string[]         filterValues,
                                                                               PropertyPathInfo property)
        where TIn : class
    {
        Expression<Func<TIn, bool>> predicateQuery = null;

        foreach (var value in filterValues)
        {
            OperatorComparer? operation = GetComparisonOperations(value);

            if (operation == null)
                continue;

            var filterValueSegments      = value.Split("_");
            var filterValueActualSegment = filterValueSegments[^1];
            var filterValue              = Regex.Replace(filterValueActualSegment, "[^0-9.]", "");
            var targetTypedFilterValue   = Convert.ChangeType(filterValue, property.PropertyUnderlyingType);

            if (targetTypedFilterValue == null)
                continue;

            var subQuery = ExpressionExtensions.BuildPredicate<TIn>(targetTypedFilterValue,
                                                                    operation!.Value,
                                                                    property.Path);

            predicateQuery = predicateQuery != null ? predicateQuery.And(subQuery) : subQuery;
        }

        return predicateQuery;
    }

    private static Expression<Func<TIn, bool>> BuildStringPredicateQuery<TIn>(string           filterValue,
                                                                              PropertyPathInfo property)
        where TIn : class
    {
        Expression<Func<TIn, bool>> subQuery         = PredicateBuilder.True<TIn>();
        var                         filterWholeValue = filterValue;

        var filterWithOperation = filterWholeValue.Split("_",
                                                         StringSplitOptions.RemoveEmptyEntries |
                                                         StringSplitOptions.TrimEntries);

        OperatorComparer operation = OperatorComparer.Equals;

        if (filterWithOperation.Length == 2)
        {
            filterWholeValue = filterWithOperation[1];

            operation = filterWithOperation[0].ToCompareOperator(false) ?? OperatorComparer.Equals;
        }

        var escapedFilterValue = filterWholeValue.Replace("*", "");

        if (filterWholeValue.StartsWith("*") &&
            filterWholeValue.EndsWith("*"))
        {
            operation = OperatorComparer.Contains;
        }

        else if (filterWholeValue.EndsWith("*"))
        {
            operation = OperatorComparer.StartsWith;
        }

        else if (filterWholeValue.StartsWith("*"))
        {
            operation = OperatorComparer.EndsWith;
        }

        var predicateQuery =
            ExpressionExtensions.BuildPredicate<TIn>(escapedFilterValue,
                                                     operation,
                                                     property.Path);
        subQuery = subQuery.And(predicateQuery);

        return subQuery;
    }

    private static OperatorComparer? GetComparisonOperations(string filterValue)
    {
        var segments = filterValue.Split('_');

        if (segments.Length == 1)
        {
            return OperatorComparer.Equals;
        }

        var comparisonOperations = segments[0].ToCompareOperator(false);

        return comparisonOperations;
    }

    private static PropertyPathInfo ObtainPropertyPath<TIn>(string fieldAccessPath) where TIn : class
    {
        return ObtainPropertyPath(typeof(TIn), fieldAccessPath);
    }

    private static PropertyPathInfo ObtainPropertyPath(Type fromType, string fieldPath)
    {
        var nestedFields = fieldPath.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        PropertyInfo property         = null;
        var          outputFieldPaths = new List<string>();
        Type         detectingType    = fromType;

        for (var i = 0; i < nestedFields.Length;i++)
        {
            var fieldName = nestedFields[i];

            property = GetPropertyFromType(detectingType, ref fieldName);

            if (property == null)
            {
                return null;
            }

            var propertyUnderlingType = property.PropertyType;

            if (propertyUnderlingType.FullName!.Contains("System.Nullable`1"))
            {
                propertyUnderlingType = propertyUnderlingType.GetGenericArguments()
                                                             .FirstOrDefault();

                if (propertyUnderlingType == null)
                    return null;
            }

            if (i < nestedFields.Length - 1 &&
                (
                    propertyUnderlingType.IsPrimitive ||
                    propertyUnderlingType == typeof(string)
                )
               )
            {
                return null;
            }

            detectingType = propertyUnderlingType;
            outputFieldPaths.Add(fieldName);
        }

        if (property == null)
        {
            return null;
        }

        var obtainPropertyPath = new PropertyPathInfo
        {
            Path                   = string.Join(".", outputFieldPaths),
            DeclaredType           = fromType,
            DestinationProperty    = property,
            PropertyUnderlyingType = detectingType
        };

        return obtainPropertyPath;
    }

    private static PropertyInfo GetPropertyFromType(Type fromType, ref string fieldName)
    {
        PropertyInfo property;
        property = fromType.GetProperty(fieldName);

        if (property == null)
        {
            fieldName = fieldName.First().ToString().ToUpper() + fieldName.Substring(1);
            property  = fromType.GetProperty(fieldName);
        }

        return property;
    }
}

public class PropertyPathInfo
{
    public string Path { get; set; }

    public Type DeclaredType { get; set; }

    public Type PropertyUnderlyingType { get; set; }

    public PropertyInfo DestinationProperty { get; set; }
}