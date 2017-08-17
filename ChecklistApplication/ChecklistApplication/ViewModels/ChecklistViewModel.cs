using ChecklistApplication.Extensions;
using ChecklistApplication.Models;
using CsvHelper;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;
using System.Windows.Input;

namespace ChecklistApplication.ViewModels
{
    internal sealed class ChecklistViewModel : BindableBase
    {
        public ChecklistViewModel()
        {
            Config = new CsvHelper.Configuration.CsvConfiguration
            {
                BufferSize = 8192,
                Delimiter = ";",
            };

            OpenFilter = "csv files (*.csv)|*.csv|text files (*.txt)|*.txt|text and csv files (*.txt; *.csv)|*.txt; *.csv";
            SaveFilter = "csv files (*.csv)|*.csv|text files (*.txt)|*.txt";

            addCommand = new DelegateCommand(ExecuteAdd, CanAdd).
                ObservesProperty(() => IsAsyncExecuting);

            deleteCommand = new DelegateCommand(ExecuteDelete, CanDelete).
                ObservesProperty(() => IsAsyncExecuting);

            openCommand = new DelegateCommand(ExecuteOpen, CanOpen).
                ObservesProperty(() => IsAsyncExecuting);

            saveCommand = new DelegateCommand(ExecuteSave, CanSave).
                ObservesProperty(() => CurrentFile).
                ObservesProperty(() => IsAsyncExecuting);

            newCommand = new DelegateCommand(ExecuteNew, CanNew).
                ObservesProperty(() => IsAsyncExecuting);

            closeCommand = new DelegateCommand(ExecuteClose, CanClose).
                ObservesProperty(() => CurrentFile).
                ObservesProperty(() => IsAsyncExecuting);

            saveAsCommand = new DelegateCommand(ExecuteSaveAs, CanSaveAs).
                ObservesProperty(() => IsAsyncExecuting);

            cvs.Filter += Filtering;

            cvs.Source = checklist;

            PropertyChanged += WhenPropertyChanged;

            Checklist.CurrentChanged += (s, e) => deleteCommand.RaiseCanExecuteChanged();

            if (!(App.Current is App))      // design mode
            {
                checklist.Add(new RowViewModel
                {
                    ID = 1,
                    Description = "first element",
                    State = ChecklistState.Empty
                });
                checklist.Add(new RowViewModel
                {
                    ID = 2,
                    Description = "second element",
                    State = ChecklistState.InProgress
                });
                checklist.Add(new RowViewModel
                {
                    ID = 3,
                    Description = "third element",
                    State = ChecklistState.Opened
                });
                checklist.Add(new RowViewModel
                {
                    ID = 4,
                    Description = "fourth element",
                    State = ChecklistState.Completed
                });

                Checklist.MoveCurrentToFirst();
            }
        }

        #region Data

        private ObservableCollection<RowViewModel> checklist = new ObservableCollection<RowViewModel>();

        private CollectionViewSource cvs = new CollectionViewSource();
        private string currentFile;

        public ICollectionView Checklist => cvs.View;

        private string OpenFilter { get; }

        private string SaveFilter { get; }

        public string CurrentFile
        {
            get { return currentFile ?? string.Empty; }
            set
            {
                if (currentFile != value)
                {
                    currentFile = value;
                    OnPropertyChanged(nameof(CurrentFile));
                }
            }
        }

        private CsvHelper.Configuration.CsvConfiguration Config { get; set; }

        #endregion Data

        #region Logic

        private string filter;

        public string Filter
        {
            get { return filter ?? string.Empty; }
            set
            {
                if (filter != value)
                {
                    filter = value;
                    OnPropertyChanged(nameof(Filter));
                }
            }
        }

        private int progress = 0;

        public int Progress
        {
            get { return progress; }
            set
            {
                SetProperty(ref progress, value);
            }
        }

        private RowViewModel SelectedItem => Checklist.CurrentItem as RowViewModel;

        private void Filtering(object sender, FilterEventArgs e)
        {
            bool result = false;
            var item = e.Item as RowViewModel;
            if (item != null)
            {
                result = item.Description.Contains(Filter);
            }
            e.Accepted = result;
        }

        private void WhenPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Filter):
                    Checklist.Refresh();
                    break;
            }
        }

        #region Add

        private readonly DelegateCommand addCommand;
        public ICommand AddCommand => addCommand;

        private void ExecuteAdd()
        {
            checklist.Add(new RowViewModel());
        }

        private bool CanAdd()
        {
            return !IsAsyncExecuting;
        }

        #endregion Add

        #region Delete

        private readonly DelegateCommand deleteCommand;
        public ICommand DeleteCommand => deleteCommand;

        private bool CanDelete()
        {
            return SelectedItem != null && !isAsyncExecuting;
        }

        private void ExecuteDelete()
        {
            checklist.Remove(SelectedItem);
        }

        #endregion Delete

        #region OpenFile

        private readonly DelegateCommand openCommand;
        public ICommand OpenCommand => openCommand;

        private async void ExecuteOpen()
        {
            OpenFileDialog dialog = new OpenFileDialog { Filter = OpenFilter };
            if (dialog.ShowDialog() == true)
            {
                StartAsyncOperation();          // start
                checklist.Clear();
                CurrentFile = dialog.FileName;

                FileStream fs = null;
                StreamReader sr = null;
                CsvReader cr = null;
                try
                {
                    fs = new FileStream(CurrentFile, FileMode.Open);
                    sr = new StreamReader(fs);
                    cr = new CsvReader(sr, Config);

                    while (await cr.ReadAsync())
                    {
                        Progress = (int)(fs.Position * 100L / fs.Length);
                        var item = new RowViewModel
                        {
                            ID = await cr.GetFieldAsync<uint>("ID"),
                            Description = await cr.GetFieldAsync<string>("Description"),
                            State = await cr.GetFieldAsync<ChecklistState>("State")
                        };
                        checklist.Add(item);
                    }
                }
                catch (CsvHelperException ex)
                {
                    checklist.Clear();
                    CurrentFile = null;
                    System.Windows.MessageBox.Show(ex.Message, "Error",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
                catch
                {
                    checklist.Clear();
                    CurrentFile = null;
                    throw;
                }
                finally
                {
                    cr?.Dispose();
                    sr?.Dispose();
                    fs?.Dispose();
                    EndAsyncOperation();        // end
                }
            }
        }

        private bool CanOpen()
        {
            return !IsAsyncExecuting;
        }

        #endregion OpenFile

        #region Save

        private readonly DelegateCommand saveCommand;
        public ICommand SaveCommand => saveCommand;

        private async void ExecuteSave()
        {
            StartAsyncOperation();      // start
            StreamWriter sw = null;
            CsvWriter cw = null;

            try
            {
                sw = new StreamWriter(CurrentFile);
                cw = new CsvWriter(sw, Config);

                await cw.WriteHeaderAsync<RowViewModel>();

                for (int i = 0; i < checklist.Count; i++)
                {
                    Progress = i * 100 / checklist.Count;
                    await cw.WriteRecordAsync(checklist[i]);
                }
            }
            finally
            {
                cw?.Dispose();
                sw?.Dispose();
                EndAsyncOperation();    // end
            }
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(CurrentFile) && !IsAsyncExecuting;
        }

        #endregion Save

        #region Save As

        private readonly DelegateCommand saveAsCommand;
        public ICommand SaveAsCommand => saveAsCommand;

        private bool CanSaveAs()
        {
            return !IsAsyncExecuting;
        }

        private async void ExecuteSaveAs()
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = SaveFilter
            };
            if (dialog.ShowDialog() == true)
            {
                StartAsyncOperation();          // start
                StreamWriter sw = null;
                CsvWriter cw = null;
                try
                {
                    sw = new StreamWriter(dialog.FileName);
                    cw = new CsvWriter(sw, Config);

                    await cw.WriteHeaderAsync<RowViewModel>();
                    for (int i = 0; i < checklist.Count; i++)
                    {
                        Progress = i * 100 / checklist.Count;
                        await cw.WriteRecordAsync(checklist[i]);
                    }
                    CurrentFile = dialog.FileName;
                }
                finally
                {
                    sw?.Dispose();
                    cw?.Dispose();
                    EndAsyncOperation();        // end
                }
            }
        }

        #endregion Save As

        #region New File

        private readonly DelegateCommand newCommand;
        public ICommand NewCommand => newCommand;

        private void ExecuteNew()
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = SaveFilter
            };
            if (dialog.ShowDialog() == true)
            {
                checklist.Clear();
                CurrentFile = dialog.FileName;

                File.Create(CurrentFile).Dispose();
            }
        }

        private bool CanNew()
        {
            return !IsAsyncExecuting;
        }

        #endregion New File

        #region Close File

        private readonly DelegateCommand closeCommand;
        public ICommand CloseCommand => closeCommand;

        private void ExecuteClose()
        {
            checklist.Clear();
            CurrentFile = null;
        }

        private bool CanClose()
        {
            return !string.IsNullOrWhiteSpace(CurrentFile) && !IsAsyncExecuting;
        }

        #endregion Close File

        #endregion Logic

        #region Simple Syncronization

        private bool isAsyncExecuting = false;

        public bool IsAsyncExecuting
        {
            get { return isAsyncExecuting; }
            private set
            {
                SetProperty(ref isAsyncExecuting, value);
            }
        }

        private void StartAsyncOperation()
        {
            IsAsyncExecuting = true;
        }

        private void EndAsyncOperation()
        {
            IsAsyncExecuting = false;
        }

        #endregion Simple Syncronization
    }
}