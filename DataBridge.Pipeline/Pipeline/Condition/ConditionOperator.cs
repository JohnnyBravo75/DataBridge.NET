namespace DataBridge
{
    /// <summary>
    /// Operator für Conditions
    /// </summary>
    public enum ConditionOperators
    {
        /// <summary>
        /// Left operand must be equal to the right one.
        /// </summary>
        Equals = 0,

        // Zusammenfassung:
        // Left operand must be smaller than or equal to the right one.
        // IsLessThanOrEqualTo = 1,

        /// <summary>
        /// Left operand must NOT be equal to the right one.
        /// </summary>
        NotEquals = 1,

        /// <summary>
        /// Left operand must be smaller than the right one.
        /// </summary>
        Lower = 2,

        // Zusammenfassung:
        // Left operand must be different from the right one. NotEquals = 3,

        // Zusammenfassung:
        // Left operand must be larger than the right one.
        //IsGreaterThanOrEqualTo = 4,

        /// <summary>
        /// Left operand must be larger than or equal to the right one.
        /// </summary>
        Greater = 5,

        /// <summary>
        /// Left operand must start with the right one.
        /// </summary>
        StartsWith = 6,

        /// <summary>
        /// Left operand must end with the right one.
        /// </summary>
        EndsWith = 7,

        /// <summary>
        /// Left operand must contain the right one.
        /// </summary>
        Contains = 8,

        /// <summary>
        /// Left operand must not contain the right one.
        /// </summary>
        NotContains = 9,

        /// <summary>
        /// Operand is null or empty.
        /// </summary>
        IsNull = 12,

        /// <summary>
        /// Operand is not null.
        /// </summary>
        IsNotNull = 13,

        /// <summary>
        /// Operand matches to a regular expression.
        /// </summary>
        RegEx = 16,

        /// <summary>
        /// ThOperand matches to a in
        /// </summary>
        IsIn = 18,

        /// <summary>
        /// Operand matches to wildcards
        /// </summary>
        Like = 19
    }
}