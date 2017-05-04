using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using DataBridge.Extensions;

namespace DataBridge.Models
{
    public class FieldDefinitionList : ItemObservableCollection<FieldDefinition>, INotifyPropertyChanged, IDisposable
    {
        // ***********************Fields***********************

        private bool canRemoveField = false;
        private bool canAddField = false;

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public new event PropertyChangedEventHandler PropertyChanged;

        // ***********************Properties***********************

        /// <summary>
        /// Gets or sets a value indicating whether it is allowed to remove a Field.
        /// </summary>
        /// <value>
        /// <c>true</c> if it is allowed to remove a Field; otherwise, <c>false</c>.
        /// </value>
        public bool CanRemoveField
        {
            get
            {
                return this.canRemoveField;
            }

            set
            {
                if (this.canRemoveField != value)
                {
                    this.canRemoveField = value;
                    this.RaisePropertyChanged("CanRemoveField");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether it is allowed to add a Field.
        /// </summary>
        /// <value>
        /// <c>true</c> if it is allowed to add a Field; otherwise, <c>false</c>.
        /// </value>
        public bool CanAddField
        {
            get
            {
                return this.canAddField;
            }

            set
            {
                if (this.canAddField != value)
                {
                    this.canAddField = value;
                    this.RaisePropertyChanged("CanAddField");
                }
            }
        }

        /// <summary>
        /// Gets the count how many table field are not empty.
        /// </summary>
        /// <value>
        /// The table field not empty count.
        /// </value>
        public int TableFieldNotEmptyCount
        {
            get
            {
                return this.Count(x => !string.IsNullOrEmpty(x.TableField.Name));
            }
        }

        /// <summary>
        /// Gets the count how many data source field are not empty count.
        /// </summary>
        /// <value>
        /// The data source field not empty count.
        /// </value>
        public int DataSourceFieldNotEmptyCount
        {
            get
            {
                return this.Count(x => !string.IsNullOrEmpty(x.DataSourceField.Name));
            }
        }

        public bool IsLengthVisible { get; set; }

        // ***********************Functions***********************

        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <exception cref="System.Exception">Invalid property name ' + propertyName + ' in  + this.ToString()</exception>
        public void RaisePropertyChanged(string propertyName)
        {
            if (!this.HasProperty(propertyName))
            {
                if (Debugger.IsAttached)
                {
                    throw new Exception("Invalid property name '" + propertyName + "' in " + this.ToString());
                }
            }

            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public override void Dispose()
        {
            foreach (var item in this)
            {
                if (item is IDisposable)
                {
                    (item as IDisposable).Dispose();
                }
            }

            this.Clear();

            base.Dispose();
        }

        public FieldDefinition GetActiveFieldbyIndex(int index)
        {
            int activeCount = -1;
            foreach (var item in this.Items)
            {
                if (item.IsActive)
                {
                    activeCount++;
                }

                if (activeCount == index)
                {
                    return item;
                }
            }

            return null;
        }

        public string GetSourceField(string tableField)
        {
            return this.First(x => x.TableField.Name == tableField).DataSourceField.Name;
        }
    }
}