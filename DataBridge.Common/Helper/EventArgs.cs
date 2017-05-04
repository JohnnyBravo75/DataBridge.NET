namespace DataBridge.Helper
{
    using System;

    /// <summary>
    /// Generic class for Eventargs. No need for individual typed Eventargs.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EventArgs<T> : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventArgs{T}" /> class.
        /// </summary>
        public EventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventArgs{T}" /> class.
        /// </summary>
        /// <param name="result">The result.</param>
        public EventArgs(T result)
        {
            this.Result = result;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventArgs{T}" /> class.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="result">The result.</param>
        public EventArgs(Exception error, T result = default(T))
        {
            this.Error = error;
            this.Result = result;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventArgs{T}" /> class.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="result">The result.</param>
        /// <param name="userstate">The userstate.</param>
        public EventArgs(Exception error, T result, object userstate)
        {
            this.Error = error;
            this.Result = result;
            this.UserState = userstate;
        }

        /// <summary>
        /// Gets or sets eine Fehlernachricht
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// Gets the ResultType
        /// </summary>
        public virtual Type ResultType
        {
            get
            {
                return this.Result.GetType();
            }
        }

        /// <summary>
        /// Gets or sets ein Resultobjekt
        /// </summary>
        public virtual T Result { get; set; }

        /// <summary>
        /// Gets or sets UserState Objekt.
        /// </summary>
        public virtual object UserState { get; set; }
    }
}