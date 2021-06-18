using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using DotNetBrightener.Core.DataAccess.NameWriters;
using LinqToDB.Metadata;
using LinqToDB.SqlQuery;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = LinqToDB.Mapping.TableAttribute;

namespace DotNetBrightener.Core.DataAccess.Providers
{
    internal class MetadataReader : IMetadataReader
    {
        private readonly DataAccessConfiguration _dataAccessConfiguration;

        #region Singleton Pattern
        private static readonly object LockObject = new object();

        private static MetadataReader _singleInstance;

        public static MetadataReader RetrieveInstance(IServiceProvider serviceProvider)
        {
            if (_singleInstance != null)
                return _singleInstance;

            lock (LockObject)
            {
                if (_singleInstance != null)
                    return _singleInstance;

                return _singleInstance ??= serviceProvider.TryGetService<MetadataReader>();
            }
        }
        #endregion

        public MetadataReader(DataAccessConfiguration dataAccessConfiguration)
        {
            _dataAccessConfiguration = dataAccessConfiguration;
            if (_dataAccessConfiguration.UsingNameRewriter == null)
            {
                _dataAccessConfiguration.UsingNameRewriter = new NoneRewriter();
            }
        }

        protected static ConcurrentDictionary<(Type, MemberInfo), Attribute> Types => new ConcurrentDictionary<(Type, MemberInfo), Attribute>();

        public T[] GetAttributes<T>(Type type, bool inherit = true) where T : Attribute
        {
            return GetAttributes<T>(type, typeof(TableAttribute));
        }

        public T[] GetAttributes<T>(Type type, MemberInfo memberInfo, bool inherit = true) where T : Attribute
        {
            return GetAttributes<T>(type, typeof(ColumnAttribute), memberInfo);
        }

        public MemberInfo[] GetDynamicColumns(Type type)
        {
            return Array.Empty<MemberInfo>();
        }

        protected T[] GetAttributes<T>(Type type, Type attributeType, MemberInfo memberInfo = null)
            where T : Attribute
        {
            if (typeof(T) == attributeType && GetAttribute<T>(type, memberInfo) is T attr)
            {
                return new[] { attr };
            }

            return Array.Empty<T>();
        }

        protected T GetAttribute<T>(Type type, MemberInfo memberInfo) where T : Attribute
        {
            Attribute RetrieveAttribute((Type, MemberInfo) t)
            {
                if (typeof(T) == typeof(TableAttribute))
                {
                    var tableAttributeValue = type.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.TableAttribute>();

                    if (tableAttributeValue != null)
                    {
                        return new TableAttribute(tableAttributeValue.Name)
                        {
                            Schema = tableAttributeValue.Schema
                        };
                    }
                    
                    return new TableAttribute(_dataAccessConfiguration.UsingNameRewriter.RewriteName(type.Name));
                }

                if (typeof(T) != typeof(ColumnAttribute)) 
                    return null;

                if (memberInfo.HasAttribute<NotMappedAttribute>())
                    return null;

                var sqlDataType = new SqlDataType((memberInfo as PropertyInfo)?.PropertyType ?? typeof(string));
                if (sqlDataType.Type.DataType == null)
                    return null;

                var columnName = _dataAccessConfiguration.UsingNameRewriter.RewriteName(memberInfo.Name);

                var columnAttributeValue = memberInfo.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>();
                if (columnAttributeValue != null)
                {
                    columnName = columnAttributeValue.Name;
                }

                var canBeNull = false;
                if (memberInfo is PropertyInfo property)
                {
                    // if the set value is virtual, usually because it is navigation property from Entity Framework,
                    // we won't map this property to Linq2DB
                    var retrieveValueMethod = property.GetSetMethod();

                    if (retrieveValueMethod == null || retrieveValueMethod.IsVirtual)
                    {
                        return null;
                    }

                    canBeNull = property.PropertyType == typeof(string) ||
                                property.PropertyType.IsNullable();
                }

                var isPrimaryKey = memberInfo.HasAttribute<KeyAttribute>();
                var isIdentity   = isPrimaryKey;

                var databaseGeneratedAttribute = memberInfo.GetCustomAttribute<DatabaseGeneratedAttribute>();
                if (databaseGeneratedAttribute != null &&
                    databaseGeneratedAttribute.DatabaseGeneratedOption == DatabaseGeneratedOption.None)
                {
                    isIdentity = false;
                }

                var maxlength          = 0;
                var maxlengthAttribute = memberInfo.GetCustomAttribute<MaxLengthAttribute>();

                if (maxlengthAttribute != null)
                {
                    maxlength = maxlengthAttribute.Length;
                }

                var columnAttr = new ColumnAttribute
                {
                    Name         = columnName,
                    IsPrimaryKey = isPrimaryKey,
                    IsColumn     = true,
                    CanBeNull    = canBeNull && !isPrimaryKey,
                    Length       = maxlength,
                    IsIdentity   = isIdentity,
                    DataType     = sqlDataType.Type.DataType
                };

                return columnAttr;
            }

            var attribute = Types.GetOrAdd((type, memberInfo), RetrieveAttribute);

            return (T)attribute;
        }
    }
}