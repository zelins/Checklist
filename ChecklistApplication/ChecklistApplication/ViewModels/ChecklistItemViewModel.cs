using ChecklistApplication.Models;
using Prism.Mvvm;
using System.Collections.Generic;
using System.ComponentModel;


namespace ChecklistApplication.ViewModels
{
    internal sealed class RowViewModel : BindableBase, IEditableObject
    {
        #region Internal Data
        private Row data = new Row();
        private Row clone;
        #endregion

        #region IEditableObject Members
        private bool editing = false;
        public void BeginEdit()
        {
            if (!editing)
            {
                clone = data;
                editing = true;
            }
        }

        public void CancelEdit()
        {
            if (editing)
            {
                data = clone;
                OnPropertyChanged(string.Empty);              
                editing = false;
            }
        }

        public void EndEdit()
        {
            if (editing)
            {
                clone = new Row();
                editing = false;
            }
        }
        #endregion

        #region Public Data
        public uint ID
        {
            get { return data.ID; }
            set
            {
                if (data.ID != value)
                {
                    data.ID = value;
                    OnPropertyChanged(nameof(ID));
                }
            }
        }

        public string Description
        {
            get { return data.Description ?? string.Empty; }
            set
            {
                if (data.Description != value)
                {
                    data.Description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        public ChecklistState State
        {
            get { return data.State; }
            set
            {
                if (data.State != value)
                {
                    data.State = value;
                    OnPropertyChanged(nameof(State));
                }
            }
        }

        public static IEnumerable<string> States => System.Enum.GetNames(typeof(ChecklistState)); 

        public RowViewModel(Row data)
        {
            this.data = data;
        }

        public RowViewModel() : this(new Row()) { }
        #endregion
    }
}
