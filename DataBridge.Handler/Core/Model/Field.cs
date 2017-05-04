using DataBridge.PropertyChanged;

namespace DataBridge.Models
{
    using System;

    public class Field : NotifyPropertyChangedBase
    {
        private Type dataType = typeof(String);
        private int length = -1;
        private string name = string.Empty;
        private string formatMask = "";

        // ***********************Constructors***********************

        /// <summary>
        /// Initializes a new instance of the <see cref="Field" /> class.
        /// </summary>
        public Field()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Field" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public Field(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Field" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="length">The length.</param>
        public Field(string name, int length)
        {
            this.Name = name;
            this.Length = length;
        }

        public Field(string name, int length, Type dataType)
        {
            this.Name = name;
            this.Length = length;
            this.Datatype = dataType;
        }

        // ***********************Properties***********************

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get { return this.name; }

            set
            {
                if (this.name != value)
                {
                    this.name = value;
                    this.RaisePropertyChanged("Name");
                }
            }
        }

        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length
        {
            get { return this.length; }

            set
            {
                if (this.length != value)
                {
                    this.length = value;
                    this.RaisePropertyChanged("Length");
                }
            }
        }

        /// <summary>
        /// Gets or sets the datatype in string form (e.g "System.String")
        /// </summary>
        /// <value>
        /// The datatype.
        /// </value>
        public Type Datatype
        {
            get { return this.dataType; }

            set
            {
                if (this.dataType != value)
                {
                    this.dataType = value;
                    this.RaisePropertyChanged("Datatype");
                }
            }
        }

        /// <summary>
        /// Gets or sets the format mask.
        /// </summary>
        /// <value>
        /// The format mask.
        /// </value>
        public string FormatMask
        {
            get { return this.formatMask; }

            set
            {
                if (this.formatMask != value)
                {
                    this.formatMask = value;
                    this.RaisePropertyChanged("FormatMask");
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}