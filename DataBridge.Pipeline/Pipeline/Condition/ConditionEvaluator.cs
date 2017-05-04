namespace DataBridge
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using DataBridge.Common.Helper;
    using DataBridge.Extensions;

    public static class ConditionEvaluator
    {
        public static void Evaluate(IEnumerable<ParameterCondition> parameterConditions, CommandParameters parameters)
        {
            var tokens = parameters.ToDictionary();

            var condition = GetFirstMatchingCondition(parameterConditions, tokens) as ParameterCondition;
            if (condition != null)
            {
                foreach (var action in condition.Actions)
                {
                    ExecuteAction(action, tokens);
                }
            }
        }

        private static void ExecuteAction(ParameterAction action, IDictionary<string, object> tokens)
        {
            if (tokens == null)
            {
                tokens = new Dictionary<string, object>();
            }

            if (!string.IsNullOrEmpty(action.Token))
            {
                var value = action.Value;
                if (!string.IsNullOrEmpty(action.ValueToken))
                {
                    value = tokens.GetValue(action.ValueToken);
                }

                tokens.AddOrUpdate(action.Token, value);
            }
        }

        public static bool CheckMatchingConditions(Conditions conditions, IDictionary<string, object> tokens)
        {
            return CheckMatchingConditions(conditions, tokens, conditions.ConnectionOperator, conditions.FilterType);
        }

        public static bool CheckMatchingConditions(IEnumerable<Condition> conditions, IDictionary<string, object> tokens, ConnectionOperators connectionOperator, FilterTypes filterType)
        {
            if (conditions.IsNullOrEmpty())
            {
                return false;
            }

            bool isMatching = false;
            bool allMatching = true;

            switch (connectionOperator)
            {
                case ConnectionOperators.And:
                    allMatching = true;
                    break;

                case ConnectionOperators.Or:
                    allMatching = false;
                    break;
            }

            // Check all conditions
            foreach (var condition in conditions)
            {
                isMatching = IsConditionMatching(condition, tokens);

                // Check by operators
                switch (connectionOperator)
                {
                    case ConnectionOperators.And:
                        if (!isMatching)
                        {
                            // (early exit) when operator AND, the whole condition is FALSE, when one is FALSE
                            allMatching = false;
                        }

                        break;

                    case ConnectionOperators.Or:
                        if (isMatching)
                        {
                            // (early exit) when operator OR, the whole condition is TRUE, when one is TRUE
                            allMatching = true;
                        }

                        break;
                }
            }

            if (filterType == FilterTypes.Negative)
            {
                allMatching = !allMatching;
            }

            return allMatching;
        }

        public static Condition GetFirstMatchingCondition(IEnumerable<Condition> conditions, IDictionary<string, object> tokens)
        {
            if (conditions == null || !conditions.Any())
            {
                return null;
            }

            return GetMatchingConditions(conditions, tokens).FirstOrDefault();
        }

        public static IEnumerable<Condition> GetMatchingConditions(IEnumerable<Condition> conditions, IDictionary<string, object> tokens)
        {
            var matching = new List<Condition>();

            if (conditions == null)
            {
                return matching;
            }

            foreach (var condition in conditions)
            {
                if (IsConditionMatching(condition, tokens))
                {
                    matching.Add(condition);
                }
            }

            return matching;
        }

        public static bool IsConditionMatching(Condition condition, IDictionary<string, object> tokens)
        {
            if (condition == null)
            {
                return false;
            }

            if (tokens == null)
            {
                tokens = new Dictionary<string, object>();
            }

            var isMatching = false;
            object tokenValue = null;

            if (condition.Token != null)
            {
                object value = condition.Value;
                var valueString = string.Empty;

                if (!string.IsNullOrEmpty(condition.ValueToken))
                {
                    value = tokens.GetValue(condition.ValueToken);
                }

                if (value != null)
                {
                    valueString = value.ToString();
                }

                if (tokens.TryGetValue(condition.Token, out tokenValue))
                {
                    isMatching = IsOperatorMatching(condition.Operator, tokenValue, valueString);
                }
                else
                {
                    // If the Token does not exist, but it is checked, if the Token is empty, then return as TRUE
                    if (condition.Operator == ConditionOperators.IsNull)
                    {
                        isMatching = true;
                    }
                }
            }

            return isMatching;
        }

        private static bool IsOperatorMatching(ConditionOperators conditionOperator, object value, string valueToCompare)
        {
            decimal actValue;
            decimal refValue;

            string strValue = value.ToStringOrEmpty();
            valueToCompare = valueToCompare.ToStringOrEmpty();

            switch (conditionOperator)
            {
                case ConditionOperators.RegEx:
                    if (valueToCompare != null)
                    {
                        var regex = new Regex(valueToCompare);
                        if (regex.IsMatch(strValue))
                        {
                            return true;
                        }
                    }

                    break;

                case ConditionOperators.StartsWith:
                    if (strValue.StartsWith(valueToCompare))
                    {
                        return true;
                    }

                    break;

                case ConditionOperators.EndsWith:
                    if (strValue.EndsWith(valueToCompare))
                    {
                        return true;
                    }

                    break;

                case ConditionOperators.NotEquals:
                    if (!strValue.Equals(valueToCompare))
                    {
                        return true;
                    }

                    break;

                case ConditionOperators.Equals:
                    if (strValue.Equals(valueToCompare))
                    {
                        return true;
                    }

                    break;

                case ConditionOperators.IsNull:
                    if (string.IsNullOrEmpty(strValue))
                    {
                        return true;
                    }

                    break;

                case ConditionOperators.IsNotNull:
                    if (!string.IsNullOrEmpty(strValue))
                    {
                        return true;
                    }

                    break;

                case ConditionOperators.Contains:
                    if (strValue.Contains(valueToCompare))
                    {
                        return true;
                    }

                    break;

                case ConditionOperators.NotContains:
                    if (!strValue.Contains(valueToCompare))
                    {
                        return true;
                    }

                    break;

                case ConditionOperators.Lower:
                    if (decimal.TryParse(strValue, out actValue) && decimal.TryParse(valueToCompare, out refValue) && actValue < refValue)
                    {
                        return true;
                    }

                    break;

                case ConditionOperators.Greater:
                    if (decimal.TryParse(strValue, out actValue) && decimal.TryParse(valueToCompare, out refValue) && actValue > refValue)
                    {
                        return true;
                    }

                    break;

                case ConditionOperators.IsIn:

                    var splitValues = valueToCompare.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                    if (splitValues != null)
                    {
                        foreach (var c in splitValues)
                        {
                            if (strValue.Equals(c))
                            {
                                return true;
                            }
                        }
                    }

                    return false;

                case ConditionOperators.Like:
                    return FileUtil.MatchesWildcard(strValue, valueToCompare);

                default:
                    if (Equals(strValue, valueToCompare))
                    {
                        return true;
                    }

                    break;
            }

            return false;
        }
    }

    public enum ConnectionOperators
    {
        And = 0,
        Or = 1
    }

    public enum FilterTypes
    {
        Positve = 0,
        Negative = 1,
    }
}