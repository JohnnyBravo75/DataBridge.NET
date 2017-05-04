namespace DataBridge.GUI.Model
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using DataBridge.Common;

    public class DefaultDisplayObject : ModelBase
    {
        // ************************************Fields**********************************************

        private ObservableCollection<DefaultDisplayObject> children = new ObservableCollection<DefaultDisplayObject>();

        private string name = "";

        private string title = "";
        private string image;
        private object value;
        private bool isSelected;
        private bool isExpanded;

        // ************************************Properties**********************************************

        public ObservableCollection<DefaultDisplayObject> Children
        {
            get
            {
                return this.children;
            }

            set
            {
                if (this.children != value)
                {
                    this.children = value;
                    this.RaisePropertyChanged("Children");
                }
            }
        }

        public bool HasDummyChild { get; set; }

        public bool IsExpanded
        {
            get
            {
                return this.isExpanded;
            }
            set
            {
                if (this.isExpanded != value)
                {
                    this.isExpanded = value;
                    this.RaisePropertyChanged("IsExpanded");
                }
            }
        }

        public bool IsSelected
        {
            get
            {
                return this.isSelected;
            }
            set
            {
                if (this.isSelected != value)
                {
                    this.isSelected = value;
                    this.RaisePropertyChanged("IsSelected");
                }
            }
        }

        public DefaultDisplayObject Parent { get; set; }

        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (this.name != value)
                {
                    this.name = value;
                    this.RaisePropertyChanged("Name");
                }
            }
        }

        public object Value
        {
            get { return this.value; }
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    this.RaisePropertyChanged("Value");
                }
            }
        }

        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                if (this.title != value)
                {
                    this.title = value;
                    this.RaisePropertyChanged("Title");
                }
            }
        }

        public string Image
        {
            get
            {
                return this.image;
            }
            set
            {
                this.image = value;
                this.RaisePropertyChanged("Image");
            }
        }

        public Dictionary<string, object> ExtendedProperties { get; set; }

        public string Type { get; set; }

        public string SubType { get; set; }
    }
}