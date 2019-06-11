// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using EnsureThat;
using Microsoft.Health.Fhir.Core.Features.Search;
using Microsoft.Health.Fhir.Core.Features.Search.Expressions;
using Microsoft.Health.Fhir.TableStorage.Features.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Health.Fhir.TableStorage.Features.Search
{
    internal class ExpressionQueryBuilder : IExpressionVisitorWithInitialContext<ExpressionQueryBuilder.Context, object>
    {
        private readonly StringBuilder _tableQuery;

        private static readonly Dictionary<FieldName, string> FieldNameMapping = new Dictionary<FieldName, string>
        {
            { FieldName.DateTimeEnd, SearchValueConstants.DateTimeEndName },
            { FieldName.DateTimeStart, SearchValueConstants.DateTimeStartName },
            { FieldName.Number, SearchValueConstants.NumberName },
            { FieldName.ParamName, SearchValueConstants.ParamName },
            { FieldName.Quantity, SearchValueConstants.QuantityName },
            { FieldName.QuantityCode, SearchValueConstants.CodeName },
            { FieldName.QuantitySystem, SearchValueConstants.SystemName },
            { FieldName.ReferenceBaseUri, SearchValueConstants.ReferenceBaseUriName },
            { FieldName.ReferenceResourceId, SearchValueConstants.ReferenceResourceIdName },
            { FieldName.ReferenceResourceType, SearchValueConstants.ReferenceResourceTypeName },
            { FieldName.String, SearchValueConstants.NormalizedStringName },
            { FieldName.TokenCode, SearchValueConstants.CodeName },
            { FieldName.TokenSystem, SearchValueConstants.SystemName },
            { FieldName.TokenText, SearchValueConstants.TextName },
            { FieldName.Uri, SearchValueConstants.UriName },
        };

        public ExpressionQueryBuilder(StringBuilder tableQuery)
        {
            _tableQuery = tableQuery;
        }

        Context IExpressionVisitorWithInitialContext<Context, object>.InitialContext => default;

        public object VisitSearchParameter(SearchParameterExpression expression, Context context)
        {
            if (expression.Parameter.Name == SearchParameterNames.ResourceType)
            {
                expression.Expression.AcceptVisitor(this, context.WithNameOverride("PartitionKey", isPartitionKey: true));
            }
            else if (expression.Parameter.Name == SearchParameterNames.Id)
            {
                expression.Expression.AcceptVisitor(this, context.WithNameOverride("RowKey"));
            }
            else if (expression.Parameter.Name == SearchParameterNames.LastUpdated)
            {
                expression.Expression.AcceptVisitor(this, context.WithNameOverride("LastModified"));
            }
            else
            {
                AppendSubquery(expression.Parameter.Name, expression.Expression, context);
            }

            return null;
        }

        public object VisitBinary(BinaryExpression expression, Context context)
        {
            VisitBinary(GetFieldName(expression, context), expression.BinaryOperator, expression.Value, context);

            return null;
        }

        private void VisitBinary(string fieldName, BinaryOperator op, object value, Context context)
        {
            string generateFilterCondition;

            switch ((value?.GetType() ?? typeof(string)).Name)
            {
                case nameof(DateTimeOffset):
                    generateFilterCondition = TableQuery.GenerateFilterConditionForDate(
                        fieldName ?? $"{context.FieldNameOverride}",
                        GetMappedValue(op),
                        (DateTimeOffset)value);
                    break;
                case nameof(Double):
                    generateFilterCondition = TableQuery.GenerateFilterConditionForDouble(
                        fieldName ?? $"{context.FieldNameOverride}",
                        GetMappedValue(op),
                        (double)value);
                    break;
                default:
                    generateFilterCondition = TableQuery.GenerateFilterCondition(
                        fieldName ?? $"{context.FieldNameOverride}",
                        GetMappedValue(op),
                        value?.ToString());
                    break;
            }

            AddFilter(context, generateFilterCondition);
        }

        private static string GetMappedValue(BinaryOperator expressionBinaryOperator)
        {
            switch (expressionBinaryOperator)
            {
                case BinaryOperator.Equal:
                    return QueryComparisons.Equal;
                case BinaryOperator.GreaterThan:
                    return QueryComparisons.GreaterThan;
                case BinaryOperator.LessThan:
                    return QueryComparisons.LessThan;
                case BinaryOperator.NotEqual:
                    return QueryComparisons.NotEqual;
                case BinaryOperator.GreaterThanOrEqual:
                    return QueryComparisons.GreaterThanOrEqual;
                case BinaryOperator.LessThanOrEqual:
                    return QueryComparisons.LessThanOrEqual;
                default:
                    throw new ArgumentOutOfRangeException(nameof(expressionBinaryOperator));
            }
        }

        public object VisitChained(ChainedExpression expression, Context context)
        {
            throw new SearchOperationNotSupportedException("ChainedExpression is not supported.");
        }

        public object VisitMissingField(MissingFieldExpression expression, Context context)
        {
            throw new System.NotImplementedException();
        }

        public object VisitMissingSearchParameter(MissingSearchParameterExpression expression, Context context)
        {
            if (expression.Parameter.Name != SearchParameterNames.ResourceType)
            {
                AppendSubquery(expression.Parameter.Name, null, negate: expression.IsMissing, context: context);
            }

            return null;
        }

        public object VisitMultiary(MultiaryExpression expression, Context context)
        {
            var newContext = context.WithMultiaryOperation(expression.MultiaryOperation);

            for (var index = 0; index < expression.Expressions.Count; index++)
            {
                var e = expression.Expressions[index];
                e.AcceptVisitor(this, newContext);

                if (index < expression.Expressions.Count - 1)
                {
                    _tableQuery.Append($" {newContext.MultiOperation} ");
                }
            }

            return context;
        }

        public object VisitString(StringExpression expression, Context context)
        {
            string fieldName = GetFieldName(expression, context);

            string value = expression.IgnoreCase ? expression.Value.ToUpperInvariant() : expression.Value;

            if (!expression.IgnoreCase && expression.StringOperator != StringOperator.Equals)
            {
                throw new NotImplementedException();
            }

            if (context.IsHistory && context.IsPartitionKey)
            {
                var partitionBuilder = new StringBuilder();
                partitionBuilder.Append("(");
                partitionBuilder.Append(TableQuery.GenerateFilterCondition(fieldName, QueryComparisons.Equal, value));
                partitionBuilder.Append(" or ");
                partitionBuilder.Append(TableQuery.GenerateFilterCondition(fieldName, QueryComparisons.Equal, $"{value}_History"));
                partitionBuilder.Append(")");
                AddFilter(context, partitionBuilder.ToString());
            }
            else
            {
                switch (expression.StringOperator)
                {
                    case StringOperator.Equals:
                    case StringOperator.StartsWith:
                        AddFilter(context, TableQuery.GenerateFilterCondition(fieldName, QueryComparisons.Equal, value));

                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return null;
        }

        private void AddFilter(Context context, string filter)
        {
            Debug.WriteLine(context);

            _tableQuery.Append("(");
            _tableQuery.Append(filter);
            _tableQuery.Append(")");
        }

        public object VisitCompartment(CompartmentSearchExpression expression, Context context)
        {
            throw new SearchOperationNotSupportedException("Compartment search is not supported.");
        }

        private string GetFieldName(IFieldExpression fieldExpression, Context state)
        {
            if (state.FieldNameOverride != null)
            {
                return state.FieldNameOverride;
            }

            string fieldNameInString = GetMappedValue(FieldNameMapping, fieldExpression.FieldName);

            if (fieldExpression.ComponentIndex == null)
            {
                return $"{state.FieldNamePrefix}{fieldNameInString}";
            }

            return $"s_{fieldNameInString}_{fieldExpression.ComponentIndex.Value}";
        }

        private static string GetMappedValue<T>(Dictionary<T, string> mapping, T key)
        {
            if (mapping.TryGetValue(key, out string value))
            {
                return value;
            }

            string message = string.Format("UnhandledEnumValue: {0}, {1}", typeof(T).Name, key);

            Debug.Fail(message);

            throw new InvalidOperationException(message);
        }

        private void AppendSubquery(string parameterName, Expression expression, Context context, bool negate = false)
        {
            EnsureArg.IsNotNull(parameterName, nameof(parameterName));

            Trace.WriteLine($"{parameterName}:{negate}");

            if (negate)
            {
                _tableQuery.Append(" not ");
            }

            var currentContext = context;

            string variable = parameterName.Replace("-", string.Empty, StringComparison.Ordinal);

            _tableQuery.Append("(");
            var maxFields = 3;

            for (int i = 0; i < maxFields; i++)
            {
                string namePrefix = context.FieldNameOverride ?? $"s_{variable}{i}_";

                if (expression is IFieldExpression fieldExpression)
                {
                    string fieldName = GetMappedValue(FieldNameMapping, fieldExpression.FieldName);
                    currentContext = context.WithNameOverride($"{namePrefix}{fieldName}");
                }
                else
                {
                    currentContext = context.WithNamePrefix(namePrefix);
                }

                if (expression != null)
                {
                    _tableQuery.Append("(");
                    expression.AcceptVisitor(this, currentContext);
                    _tableQuery.Append(")");
                }
                else
                {
                    // :missing will end up here
                    throw new NotSupportedException("This query is not supported");
                }

                if (i < maxFields - 1)
                {
                    _tableQuery.Append(" or ");
                }
            }

            _tableQuery.Append(")");
        }

        /// <summary>
        /// Context that is passed through the visit.
        /// </summary>
        internal struct Context
        {
            public string FieldNameOverride { get; set; }

            public string FieldNamePrefix { get; set; }

            public string MultiOperation { get; set; }

            public bool IsHistory { get; set; }

            public bool IsPartitionKey { get; set; }

            public Context WithNameOverride(string name, bool? isPartitionKey = null)
            {
                return new Context
                {
                    FieldNameOverride = name,
                    MultiOperation = MultiOperation,
                    IsHistory = IsHistory,
                    IsPartitionKey = isPartitionKey ?? IsPartitionKey,
                };
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1308", Justification = "Required by syntax")]
            public Context WithMultiaryOperation(MultiaryOperator multiaryOperator)
            {
                return new Context
                {
                    FieldNamePrefix = FieldNamePrefix,
                    FieldNameOverride = FieldNameOverride,
                    IsHistory = IsHistory,
                    IsPartitionKey = IsPartitionKey,
                    MultiOperation = multiaryOperator.ToString().ToLowerInvariant(),
                };
            }

            public Context WithNamePrefix(string namePrefix)
            {
                return new Context
                {
                    FieldNamePrefix = namePrefix,
                    IsHistory = IsHistory,
                    IsPartitionKey = IsPartitionKey,
                    MultiOperation = MultiOperation,
                };
            }
        }
    }
}
