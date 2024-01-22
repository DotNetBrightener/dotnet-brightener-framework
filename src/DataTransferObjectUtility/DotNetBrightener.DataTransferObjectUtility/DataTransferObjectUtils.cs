using DotNetBrightener.DataTransferObjectUtility.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace DotNetBrightener.DataTransferObjectUtility;

public class AuditProperty
{
    public string PropertyName { get; set; }

    public object OldValue { get; set; }

    public object NewValue { get; set; }
}

public class AuditTrail<T>
{
    public string Identifier { get; set; }

    public string TypeName { get; set; } = typeof(T).FullName;

    public List<AuditProperty> AuditProperties { get; set; } = new List<AuditProperty>();
}

public static class DataTransferObjectUtils
{
    /// <summary>
    ///     Updates the given <see cref="entityObject"/> by the values provided in the <seealso cref="dataTransferObject"/>
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the <see cref="entityObject"/>
    /// </typeparam>
    /// <param name="entityObject">
    ///     The object to be updated
    /// </param>
    /// <param name="updateExpression">
    ///     The expression to describe the data to apply updates to <see cref="entityObject"/>, with accesses to the <seealso cref="entityObject"/>
    /// </param>
    /// <param name="ignoreProperties">
    ///     The properties that should not be updated by this method
    /// </param>
    /// <returns>
    ///     The <see cref="entityObject"/> itself
    /// </returns>
    public static T UpdateEntityFromDtoExpression<T>(T               entityObject,
                                                     Func<T, object> updateExpression,
                                                     params string[] ignoreProperties) where T : class =>
        UpdateEntityFromDtoExpression(entityObject, updateExpression, out _, ignoreProperties);

    /// <summary>
    ///     Updates the given <see cref="entityObject"/> by the values provided in the <seealso cref="dataTransferObject"/>
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the <see cref="entityObject"/>
    /// </typeparam>
    /// <param name="entityObject">
    ///     The object to be updated
    /// </param>
    /// <param name="updateExpression">
    ///     The expression to describe the data to apply updates to <see cref="entityObject"/>, with accesses to the <seealso cref="entityObject"/>
    /// </param>
    /// <param name="ignoreProperties">
    ///     The properties that should not be updated by this method
    /// </param>
    /// <returns>
    ///     The <see cref="entityObject"/> itself
    /// </returns>
    public static T UpdateEntityFromDtoExpression<T>(T                   entityObject,
                                                     Func<T, object>     updateExpression,
                                                     out    AuditTrail<T> auditTrail,
                                                     params string[]     ignoreProperties) where T : class
    {
        auditTrail = new AuditTrail<T>();

        Type entityType = typeof(T);

        var dataTransferObject = updateExpression(entityObject);
        var jObject            = JObject.FromObject(dataTransferObject);
        var propertiesFromDto  = jObject.Properties();

        foreach (var propertyInfo in propertiesFromDto)
        {
            if (ignoreProperties.Contains(propertyInfo.Name))
                continue;

            if (!TryPickPropAndValue(entityType, propertyInfo, out var destinationProp, out var value))
                continue;

            var oldValue = destinationProp.GetValue(entityObject);

            auditTrail.AuditProperties.Add(new AuditProperty
            {
                PropertyName = propertyInfo.Name,
                OldValue     = oldValue,
                NewValue     = value
            });

            destinationProp.SetValue(entityObject, value);
        }

        return entityObject;
    }

    /// <summary>
    ///     Updates the given <see cref="entityObject"/> by the values provided in the <seealso cref="dataTransferObject"/>
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the <see cref="entityObject"/>
    /// </typeparam>
    /// <param name="entityObject">
    ///     The object to be updated
    /// </param>
    /// <param name="dataTransferObject">
    ///     The object contains the data to apply updates to <see cref="entityObject"/>
    /// </param>
    /// <param name="ignoreProperties">
    ///     The properties that should not be updated by this method
    /// </param>
    /// <returns>
    ///     The <see cref="entityObject"/> itself
    /// </returns>
    public static T UpdateEntityFromDto<T>(T               entityObject,
                                           object          dataTransferObject,
                                           out AuditTrail<T> auditTrail,
                                           params string[] ignoreProperties) where T : class
    {
        auditTrail = new AuditTrail<T>();

        Type entityType = typeof(T);

        var jobject           = JObject.FromObject(dataTransferObject);
        var propertiesFromDto = jobject.Properties();

        foreach (var propertyInfo in propertiesFromDto)
        {
            if (ignoreProperties.Contains(propertyInfo.Name))
                continue;

            var csConventionName = propertyInfo.Name[0].ToString().ToUpper() + propertyInfo.Name.Substring(1);

            if (ignoreProperties.Contains(csConventionName))
                continue;

            if (!TryPickPropAndValue(entityType, propertyInfo, out var destinationProp, out var value))
                continue;

            var oldValue = destinationProp.GetValue(entityObject);

            auditTrail.AuditProperties.Add(new AuditProperty
            {
                PropertyName = propertyInfo.Name,
                OldValue     = oldValue,
                NewValue     = value
            });

            destinationProp.SetValue(entityObject, value);
        }

        return entityObject;
    }

    /// <summary>
    ///     Updates the given <see cref="entityObject"/> by the values provided in the <seealso cref="dataTransferObject"/>
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the <see cref="entityObject"/>
    /// </typeparam>
    /// <param name="entityObject">
    ///     The object to be updated
    /// </param>
    /// <param name="dataTransferObject">
    ///     The object contains the data to apply updates to <see cref="entityObject"/>
    /// </param>
    /// <param name="ignoreProperties">
    ///     The properties that should not be updated by this method
    /// </param>
    /// <returns>
    ///     The <see cref="entityObject"/> itself
    /// </returns>
    public static T UpdateEntityFromDto<T>(T               entityObject,
                                           object          dataTransferObject,
                                           params string[] ignoreProperties) where T : class
        => UpdateEntityFromDto(entityObject, dataTransferObject, out _, ignoreProperties);

    /// <summary>
    ///     Generates the member init expression for <typeparamref name="T"/> type from the given <seealso cref="dataTransferObject"/>
    /// </summary>
    /// <typeparam name="T">Type of the object to build the expression</typeparam>
    /// <param name="dataTransferObject">
    ///     The data transfer object used to build the expression
    /// </param>
    /// <returns>
    ///     The expression to initialize a new object of type <typeparamref name="T"/>
    /// </returns>
    public static Expression<Func<T, T>> BuildMemberInitExpressionFromDto<T>(object          dataTransferObject,
                                                                             params string[] ignoreProperties)
        where T : class
    {
        Type entityType           = typeof(T);
        var  newExpression        = Expression.New(entityType.GetConstructor(new Type[0])!);
        var  memberAssignmentList = new List<MemberAssignment>();

        var jobject = JObject.FromObject(dataTransferObject);

        var propertiesFromDto = jobject.Properties();

        foreach (var propertyInfo in propertiesFromDto)
        {
            if (ignoreProperties.Contains(propertyInfo.Name))
                continue;

            var csConventionName = propertyInfo.Name[0].ToString().ToUpper() + propertyInfo.Name.Substring(1);

            if (ignoreProperties.Contains(csConventionName))
                continue;

            if (!TryPickPropAndValue(entityType, propertyInfo, out var propertyOnEntity, out var valueToUpdate))
                continue;

            memberAssignmentList.Add(Expression.Bind(
                                                     propertyOnEntity,
                                                     Expression.Constant(valueToUpdate,
                                                                         propertyOnEntity.PropertyType)
                                                    )
                                    );
        }

        var memberInitExpression =
            Expression
               .Lambda<Func<T, T>>(Expression.MemberInit(newExpression, memberAssignmentList),
                                   Expression.Parameter(entityType));

        return memberInitExpression;
    }

    /// <summary>
    ///     Generates the expression to initialize data transfer object from the given properties of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">
    ///     The type of entity
    /// </typeparam>
    /// <param name="propertiesList">
    ///     The list of properties belong to <typeparamref name="T"/> type, that is used to generate the data transfer object
    /// </param>
    /// <returns>
    ///     The expression to initialize a dynamic data transfer object from type <typeparamref name="T"/>
    /// </returns>
    public static Expression<Func<T, object>> BuildDtoSelectorExpressionFromEntity<T>(
        IEnumerable<string> propertiesList) where T : class
    {
        var properties = propertiesList != null
                             ? propertiesList as string[] ?? propertiesList.ToArray()
                             : Array.Empty<string>();

        if (properties.Length == 0)
        {
            throw new InvalidOperationException("Properties must be provided");
        }

        var sourceQuery = Expression.Parameter(typeof(T), "o");

        List<DynamicProperty>                        mainPropertiesBinding = new();
        Dictionary<string, NestedPropTypeDefinition> nestedProps           = new();

        foreach (var propName in properties)
        {
            PropertyInfo prop;

            if (propName.Contains('.'))
            {
                var props   = propName.Split('.', 2);
                var navProp = props.First();

                if (!nestedProps.TryGetValue(navProp, out var nestedPropType))
                {
                    prop = GetProperty<T>(navProp);

                    if (prop == null)
                        continue;

                    nestedPropType = new NestedPropTypeDefinition
                    {
                        Name         = navProp,
                        PropertyType = prop.PropertyType
                    };
                    nestedProps.Add(navProp, nestedPropType);
                }

                nestedPropType.ChildProps.Add(props.Last());

                continue;
            }

            prop = GetProperty<T>(propName);

            if (prop == null ||
                prop.HasAttribute<JsonIgnoreAttribute>() ||
                prop.HasAttribute<System.Text.Json.Serialization.JsonIgnoreAttribute>())
                continue;

            mainPropertiesBinding.Add(new DynamicProperty(prop.Name, prop.PropertyType));
        }


        List<DynamicProperty> nestedPropertiesBinding = new();

        foreach (var prop in nestedProps.Values)
        {
            prop.FreezeDynamicBinding(sourceQuery);

            if (prop.ReturnDynamicType is not null)
            {
                nestedPropertiesBinding.Add(new DynamicProperty(prop.Name, prop.ReturnDynamicType));
            }
        }

        var returningProperties = mainPropertiesBinding.Concat(nestedPropertiesBinding).ToList();
        var resultType          = DynamicClassFactory.CreateType(returningProperties, false);
        var bindings            = new List<MemberAssignment>();

        foreach (var p in mainPropertiesBinding)
        {
            MemberAssignment bindingAssignment = Expression.Bind(resultType.GetProperty(p.Name)!,
                                                                 Expression.Property(sourceQuery, p.Name));
            bindings.Add(bindingAssignment);
        }

        foreach (var prop in nestedProps.Values)
        {
            var        resultProp           = resultType.GetProperty(prop.Name);
            Expression assignmentExpression = prop.RetrieveAssignmentExpression();

            MemberAssignment bindingAssignment = Expression.Bind(resultProp!, assignmentExpression);

            bindings.Add(bindingAssignment);
        }


        var result            = Expression.MemberInit(Expression.New(resultType), bindings);
        var mainPropSelectors = Expression.Lambda<Func<T, dynamic>>(result, sourceQuery);

        return mainPropSelectors;
    }

    private static bool TryPickPropAndValue(Type             entityType,
                                            JProperty        propertyFromDto,
                                            out PropertyInfo propertyOnEntity,
                                            out object       valueToUpdate)
    {
        valueToUpdate    = null;
        propertyOnEntity = GetProperty(entityType, propertyFromDto.Name);

        // not converting some properties that should not be put back to the entity
        if (propertyOnEntity == null ||
            propertyOnEntity.HasAttribute<NotMappedAttribute>())
            return false;

        if (propertyOnEntity.HasAttribute<KeyAttribute>())
        {
            var keyPropAttr = propertyOnEntity.GetCustomAttribute<DatabaseGeneratedAttribute>();

            if (keyPropAttr == null ||
                keyPropAttr.DatabaseGeneratedOption != DatabaseGeneratedOption.None)
                return false;
        }

        if (!propertyOnEntity.CanWrite)
            return false;

        if (propertyOnEntity.GetGetMethod()?.IsVirtual == true)
        {
            return false;
        }

        valueToUpdate = propertyFromDto.Value.ToObject(propertyOnEntity.PropertyType);

        if (valueToUpdate != null)
        {
            if (propertyOnEntity.PropertyType == typeof(DateTime) &&
                valueToUpdate is DateTime dateTimeValue &&
                dateTimeValue == DateTime.MinValue)
            {
                valueToUpdate = new DateTime(1970, 1, 1);
            }

            else if (propertyOnEntity.PropertyType == typeof(DateTimeOffset) &&
                     valueToUpdate is DateTimeOffset dateTimeOffsetValue &&
                     dateTimeOffsetValue == DateTimeOffset.MinValue)
            {
                valueToUpdate = new DateTimeOffset(new DateTime(1970, 1, 1), TimeSpan.Zero);
            }
        }

        return true;
    }

    internal static PropertyInfo GetProperty<T>(string propName) where T : class
    {
        return GetProperty(typeof(T), propName);
    }

    internal static PropertyInfo GetProperty(Type type, string propName)
    {
        PropertyInfo prop = type.GetProperty(propName);

        if (prop == null)
        {
            var csConventionName = propName[0].ToString().ToUpper() + propName.Substring(1);
            prop = type.GetProperty(csConventionName);
        }

        return prop;
    }


    /// <summary>
    ///     Defines the type of nested property picked from an entity type
    /// </summary>
    private class NestedPropTypeDefinition
    {
        private readonly Type _propertyType;

        public string Name { get; init; }

        public Type PropertyType
        {
            get => _propertyType;
            init
            {
                _propertyType              = value;
                _sourceGenericTypeArgument = _propertyType;

                if (!_propertyType.IsGenericType)
                {
                    return;
                }

                var genericTypeParam = _propertyType.GetGenericArguments();
                var enumerableType   = typeof(IEnumerable<>).MakeGenericType(genericTypeParam);

                if (enumerableType.IsAssignableFrom(_propertyType))
                {
                    _enumerableGenericType     = enumerableType;
                    _sourceGenericTypeArgument = genericTypeParam.First();
                }
            }
        }

        public HashSet<string> ChildProps { get; } = new HashSet<string>();

        public Type ReturnDynamicType { get; private set; }

        /// <summary>
        ///     Represents the type of the generic collection
        /// </summary>
        private Type _sourceGenericTypeArgument;

        /// <summary>
        ///     The type of enumerable generic, eg <seealso cref="IEnumerable{T}"/>
        /// </summary>
        private Type _enumerableGenericType;

        private Type _destinationResultTypeArgument;

        private readonly List<MemberAssignment> _memberAssignments = new List<MemberAssignment>();
        private          ParameterExpression    _childSourceParameter;
        private          MemberInitExpression   _memberInitExpression;
        private          Expression             _assignmentExpression;

        private bool IsEnumerableGenericType()
        {
            return _enumerableGenericType != null;
        }

        public void FreezeDynamicBinding(ParameterExpression sourceQuery)
        {
            List<DynamicProperty> arrayPropBindings = new();

            foreach (var childPropName in ChildProps)
            {
                var childProp = GetProperty(_sourceGenericTypeArgument, childPropName);

                if (childProp == null ||
                    childProp.HasAttribute<JsonIgnoreAttribute>() ||
                    childProp.HasAttribute<System.Text.Json.Serialization.JsonIgnoreAttribute>())
                    continue;

                arrayPropBindings.Add(new DynamicProperty(childProp.Name, childProp.PropertyType));
            }

            _destinationResultTypeArgument = DynamicClassFactory.CreateType(arrayPropBindings, false);

            if (IsEnumerableGenericType())
            {
                _childSourceParameter = Expression.Parameter(_sourceGenericTypeArgument, "c");

                foreach (var p in arrayPropBindings)
                {
                    MemberAssignment assignment = Expression.Bind(_destinationResultTypeArgument.GetProperty(p.Name)!,
                                                                  Expression.Property(_childSourceParameter, p.Name));
                    _memberAssignments.Add(assignment);
                }

                _memberInitExpression = Expression.MemberInit(Expression.New(_destinationResultTypeArgument),
                                                              _memberAssignments);

                ReturnDynamicType = typeof(IEnumerable<>).MakeGenericType(_destinationResultTypeArgument);
                var parentAccessProperty = Expression.Property(sourceQuery, Name);

                var delegateType = typeof(Func<,>).MakeGenericType(_sourceGenericTypeArgument,
                                                                   _destinationResultTypeArgument);

                var lambda = EnumerableMethodHelpers.LambdaMethod.MakeGenericMethod(delegateType);

                dynamic childSelector = lambda.Invoke(null,
                                                      new object[]
                                                      {
                                                          _memberInitExpression,
                                                          new[]
                                                          {
                                                              _childSourceParameter
                                                          }
                                                      });
                var selectMethod =
                    EnumerableMethodHelpers.NestedSelectMethod.MakeGenericMethod(_sourceGenericTypeArgument,
                                                                                 _destinationResultTypeArgument);
                _assignmentExpression = Expression.Call(null,
                                                        selectMethod,
                                                        parentAccessProperty,
                                                        childSelector);

                return;
            }

            foreach (var p in arrayPropBindings)
            {
                MemberAssignment assignment = Expression.Bind(_destinationResultTypeArgument.GetProperty(p.Name)!,
                                                              Expression.Property(Expression.Property(sourceQuery,
                                                                                                      Name),
                                                                                  p.Name));
                _memberAssignments.Add(assignment);
            }

            _memberInitExpression = Expression.MemberInit(Expression.New(_destinationResultTypeArgument),
                                                          _memberAssignments);

            _assignmentExpression = _memberInitExpression;

            ReturnDynamicType = _destinationResultTypeArgument;
        }

        public Expression RetrieveAssignmentExpression()
        {
            return _assignmentExpression;
        }
    }

    internal static bool HasAttribute<TAttribute>(this MemberInfo type) where TAttribute : Attribute
    {
        return type?.GetCustomAttribute<TAttribute>() != null;
    }
}