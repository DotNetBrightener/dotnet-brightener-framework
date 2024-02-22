using DotNetBrightener.GenericCRUD.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotNetBrightener.Framework.Exceptions;

namespace DotNetBrightener.GenericCRUD.Extensions;

public static partial class QueryableDeepFilterExtensions
{
    /// <summary>
    ///     Add addition query into initial one to order the result, and optionally pagination
    /// </summary>
    /// <param name="entitiesQuery">The initial query</param>
    /// <returns>
    ///     The new query with extra operations e.g ordering / pagination from <see cref="filterDictionary"/>
    /// </returns>
    public static IQueryable<TIn> AddOrderingAndPaginationQuery<TIn>(this IQueryable<TIn>       entitiesQuery,
                                                                     Dictionary<string, string> filterDictionary,
                                                                     string                     defaultSortPropName,
                                                                     out int                    pageSize,
                                                                     out int                    pageIndex,
                                                                     Func<IQueryable<TIn>, IEnumerable<TIn>>
                                                                         postProcessing = null)
        where TIn : class
    {
        var paginationQuery = filterDictionary.ToQueryModel<BaseQueryModel>();

        pageSize  = paginationQuery.PageSize;
        pageIndex = paginationQuery.PageIndex;

        var orderedEntitiesQuery = entitiesQuery.OrderBy(defaultSortPropName.ToMemberAccessExpression<TIn>());

        if (paginationQuery.OrderedColumns.Count > 0)
        {
            var sortInitialized = false;

            foreach (var orderByColumn in paginationQuery.OrderedColumns)
            {
                var actualColumnName = orderByColumn.TrimStart('-');
                try
                {

                    var orderByColumnExpr = actualColumnName.ToMemberAccessExpression<TIn>();

                    if (orderByColumn.StartsWith("-"))
                    {
                        orderedEntitiesQuery = !sortInitialized 
                                                   ? entitiesQuery.OrderByDescending(orderByColumnExpr)
                                                   : orderedEntitiesQuery.ThenByDescending(orderByColumnExpr);
                    }
                    else
                    {
                        orderedEntitiesQuery = !sortInitialized 
                                                   ? entitiesQuery.OrderBy(orderByColumnExpr)
                                                   : orderedEntitiesQuery.ThenBy(orderByColumnExpr);
                    }

                    if (!sortInitialized)
                        sortInitialized = true;
                }
                catch (UnknownPropertyException)
                {
                    continue;
                }
            }
        }

        var itemsToSkip = pageIndex * pageSize;
        var itemsToTake = pageSize;

        var finalDataSetQuery = orderedEntitiesQuery.Skip(itemsToSkip)
                                                    .Take(itemsToTake);

        if (postProcessing != null)
        {
            finalDataSetQuery = postProcessing(finalDataSetQuery).AsQueryable();
        }

        return finalDataSetQuery;
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
    public static IQueryable<TIn> ApplyDeepFilters<TIn>(this IQueryable<TIn>       entitiesQuery,
                                                        Dictionary<string, string> filterDictionary)
        where TIn : class
    {
        if (filterDictionary.Keys.Count == 0)
            return entitiesQuery;

        Expression<Func<TIn, bool>> predicateStatement = null;

        foreach (var filter in filterDictionary)
        {
            var fieldName = filter.Key;
            var property  = ObtainPropertyPath<TIn>(fieldName);

            if (property == null)
                continue;

            var propertyUnderlingType = property.PropertyUnderlyingType;

            if (propertyUnderlingType == typeof(string))
            {
                var predicateQuery = BuildStringPredicateQuery<TIn>(filter.Value, property);

                if (predicateQuery is null)
                    continue;

                predicateStatement =
                    predicateStatement != null ? predicateStatement.And(predicateQuery) : predicateQuery;

                continue;
            }

            if (propertyUnderlingType == typeof(bool))
            {
                var predicateQuery = BuildBooleanPredicateQuery<TIn>(filter.Value, property);

                if (predicateQuery is null)
                    continue;

                predicateStatement =
                    predicateStatement != null ? predicateStatement.And(predicateQuery) : predicateQuery;

                continue;
            }

            if (propertyUnderlingType == typeof(Guid))
            {
                var predicateQuery = BuildGuidPredicateQuery<TIn>(filter.Value, property);

                if (predicateQuery is null)
                    continue;

                predicateStatement =
                    predicateStatement != null ? predicateStatement.And(predicateQuery) : predicateQuery;

                continue;
            }

            if (propertyUnderlingType == typeof(int) ||
                propertyUnderlingType == typeof(long) ||
                propertyUnderlingType == typeof(float) ||
                propertyUnderlingType == typeof(double) ||
                propertyUnderlingType == typeof(decimal))
            {

                var predicateQuery = BuildNumericPredicateQuery<TIn>(filter.Value, property);

                if (predicateQuery is null)
                    continue;

                predicateStatement =
                    predicateStatement != null ? predicateStatement.And(predicateQuery) : predicateQuery;

                continue;
            }

            var filterValues = filter.Value.Split(",",
                                                  StringSplitOptions.TrimEntries |
                                                  StringSplitOptions.RemoveEmptyEntries);

            if (propertyUnderlingType == typeof(DateTime) ||
                propertyUnderlingType == typeof(DateTimeOffset))
            {
                var predicateQuery = BuildDateTimePredicateQuery<TIn>(filterValues, propertyUnderlingType, property);

                if (predicateQuery != null)
                {
                    predicateStatement =
                        predicateStatement != null ? predicateStatement.And(predicateQuery) : predicateQuery;
                }
            }
        }

        return predicateStatement == null ? entitiesQuery : entitiesQuery.Where(predicateStatement);
    }

    private static PropertyPathInfo ObtainPropertyPath<TIn>(string fieldAccessPath) where TIn : class
    {
        return ObtainPropertyPath(typeof(TIn), fieldAccessPath);
    }

    private static PropertyPathInfo ObtainPropertyPath(Type fromType, string fieldPath)
    {
        var nestedFields = fieldPath.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        PropertyInfo property              = null;
        var          outputFieldPaths      = new List<string>();
        Type         detectingType         = fromType;
        Type         nullableDetectingType = null;
        var          isNullable            = false;

        for (var i = 0; i < nestedFields.Length; i++)
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
                isNullable            = true;
                nullableDetectingType = propertyUnderlingType;
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
            Path                           = string.Join(".", outputFieldPaths),
            DeclaredType                   = fromType,
            DestinationProperty            = property,
            PropertyUnderlyingType         = detectingType,
            NullablePropertyUnderlyingType = nullableDetectingType,
            IsNullable                     = isNullable
        };

        return obtainPropertyPath;
    }

    private static PropertyInfo GetPropertyFromType(Type fromType, ref string fieldName)
    {
        var property = fromType.GetProperty(fieldName);

        if (property == null)
        {
            fieldName = fieldName.First().ToString().ToUpper() + fieldName.Substring(1);
            property  = fromType.GetProperty(fieldName);
        }

        if (property == null)
        {
            var s = fieldName;
            property = fromType.GetProperties()
                               .FirstOrDefault(_ => _.Name.Equals(s, StringComparison.OrdinalIgnoreCase));

            if (property != null)
            {
                fieldName = property.Name;
            }
        }

        return property;
    }
}

public class PropertyPathInfo
{
    public string Path { get; set; }

    public Type DeclaredType { get; set; }

    public Type PropertyUnderlyingType { get; set; }

    public Type NullablePropertyUnderlyingType { get; set; }

    public PropertyInfo DestinationProperty { get; set; }

    public bool IsNullable { get; set; }
}