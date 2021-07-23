<a name='assembly'></a>
# DotNetBrightener.DataTransferObjectUtility

## Contents

- [DataTransferObjectUtils](#T-DotNetBrightener-DataTransferObjectUtility-DataTransferObjectUtils 'DotNetBrightener.DataTransferObjectUtility.DataTransferObjectUtils')
  - [BuildDtoSelectorExpressionFromEntity\`\`1(propertiesList)](#M-DotNetBrightener-DataTransferObjectUtility-DataTransferObjectUtils-BuildDtoSelectorExpressionFromEntity``1-System-Collections-Generic-IEnumerable{System-String}- 'DotNetBrightener.DataTransferObjectUtility.DataTransferObjectUtils.BuildDtoSelectorExpressionFromEntity``1(System.Collections.Generic.IEnumerable{System.String})')
  - [BuildMemberInitExpressionFromDto\`\`1(dataTransferObject)](#M-DotNetBrightener-DataTransferObjectUtility-DataTransferObjectUtils-BuildMemberInitExpressionFromDto``1-System-Object- 'DotNetBrightener.DataTransferObjectUtility.DataTransferObjectUtils.BuildMemberInitExpressionFromDto``1(System.Object)')
- [EnumerableMethodHelpers](#T-DotNetBrightener-DataTransferObjectUtility-Internal-EnumerableMethodHelpers 'DotNetBrightener.DataTransferObjectUtility.Internal.EnumerableMethodHelpers')
  - [LambdaMethod](#F-DotNetBrightener-DataTransferObjectUtility-Internal-EnumerableMethodHelpers-LambdaMethod 'DotNetBrightener.DataTransferObjectUtility.Internal.EnumerableMethodHelpers.LambdaMethod')
  - [NestedSelectMethod](#F-DotNetBrightener-DataTransferObjectUtility-Internal-EnumerableMethodHelpers-NestedSelectMethod 'DotNetBrightener.DataTransferObjectUtility.Internal.EnumerableMethodHelpers.NestedSelectMethod')
- [NestedPropTypeDefinition](#T-DotNetBrightener-DataTransferObjectUtility-DataTransferObjectUtils-NestedPropTypeDefinition 'DotNetBrightener.DataTransferObjectUtility.DataTransferObjectUtils.NestedPropTypeDefinition')
  - [_enumerableGenericType](#F-DotNetBrightener-DataTransferObjectUtility-DataTransferObjectUtils-NestedPropTypeDefinition-_enumerableGenericType 'DotNetBrightener.DataTransferObjectUtility.DataTransferObjectUtils.NestedPropTypeDefinition._enumerableGenericType')
  - [_sourceGenericTypeArgument](#F-DotNetBrightener-DataTransferObjectUtility-DataTransferObjectUtils-NestedPropTypeDefinition-_sourceGenericTypeArgument 'DotNetBrightener.DataTransferObjectUtility.DataTransferObjectUtils.NestedPropTypeDefinition._sourceGenericTypeArgument')

<a name='T-DotNetBrightener-DataTransferObjectUtility-DataTransferObjectUtils'></a>
## DataTransferObjectUtils `type`

##### Namespace

DotNetBrightener.DataTransferObjectUtility

<a name='M-DotNetBrightener-DataTransferObjectUtility-DataTransferObjectUtils-BuildDtoSelectorExpressionFromEntity``1-System-Collections-Generic-IEnumerable{System-String}-'></a>
### BuildDtoSelectorExpressionFromEntity\`\`1(propertiesList) `method`

##### Summary

Generates the expression to initialize data transfer object from the given properties of type `T`

##### Returns

The expression to initialize a dynamic data transfer object from type `T`

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| propertiesList | [System.Collections.Generic.IEnumerable{System.String}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Collections.Generic.IEnumerable 'System.Collections.Generic.IEnumerable{System.String}') | The list of properties belong to `T` type, that is used to generate the data transfer object |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | The type of entity |

<a name='M-DotNetBrightener-DataTransferObjectUtility-DataTransferObjectUtils-BuildMemberInitExpressionFromDto``1-System-Object-'></a>
### BuildMemberInitExpressionFromDto\`\`1(dataTransferObject) `method`

##### Summary

Generates the member init expression for `T` type from the given

##### Returns

The expression to initialize a new object of type `T`

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| dataTransferObject | [System.Object](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Object 'System.Object') | The data transfer object used to build the expression |

##### Generic Types

| Name | Description |
| ---- | ----------- |
| T | Type of the object to build the expression |

<a name='T-DotNetBrightener-DataTransferObjectUtility-Internal-EnumerableMethodHelpers'></a>
## EnumerableMethodHelpers `type`

##### Namespace

DotNetBrightener.DataTransferObjectUtility.Internal

<a name='F-DotNetBrightener-DataTransferObjectUtility-Internal-EnumerableMethodHelpers-LambdaMethod'></a>
### LambdaMethod `constants`

##### Summary

Retrieves the generic [Lambda\`\`1](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Linq.Expressions.Expression.Lambda``1 'System.Linq.Expressions.Expression.Lambda``1(System.Linq.Expressions.Expression,System.Collections.Generic.IEnumerable{System.Linq.Expressions.ParameterExpression})')

<a name='F-DotNetBrightener-DataTransferObjectUtility-Internal-EnumerableMethodHelpers-NestedSelectMethod'></a>
### NestedSelectMethod `constants`

##### Summary

Retrieves the generic [Select\`\`2](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Linq.Enumerable.Select``2 'System.Linq.Enumerable.Select``2(System.Collections.Generic.IEnumerable{``0},System.Func{``0,``1})') method;

<a name='T-DotNetBrightener-DataTransferObjectUtility-DataTransferObjectUtils-NestedPropTypeDefinition'></a>
## NestedPropTypeDefinition `type`

##### Namespace

DotNetBrightener.DataTransferObjectUtility.DataTransferObjectUtils

##### Summary

Defines the type of nested property picked from an entity type

<a name='F-DotNetBrightener-DataTransferObjectUtility-DataTransferObjectUtils-NestedPropTypeDefinition-_enumerableGenericType'></a>
### _enumerableGenericType `constants`

##### Summary

The type of enumerable generic, eg

<a name='F-DotNetBrightener-DataTransferObjectUtility-DataTransferObjectUtils-NestedPropTypeDefinition-_sourceGenericTypeArgument'></a>
### _sourceGenericTypeArgument `constants`

##### Summary

Represents the type of the generic collection
