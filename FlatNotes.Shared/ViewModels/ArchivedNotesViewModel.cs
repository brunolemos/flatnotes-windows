using FlatNotes.Models;
using FlatNotes.Utils;

namespace FlatNotes.ViewModels
{
    public class ArchivedNotesViewModel : ViewModelBase
    {
        public static ArchivedNotesViewModel Instance { get { if (instance == null) instance = new ArchivedNotesViewModel(); return instance; } }
        private static ArchivedNotesViewModel instance = null;

        public Notes Notes { get { return notes; } private set { notes = value; NotifyPropertyChanged("Notes"); } }
        public Notes notes = AppData.ArchivedNotes;

        public int Columns { get { return -1; } }// AppSettings.Instance.Columns; } internal set { AppSettings.Instance.Columns = value; } }

#region COMMANDS_ACTIONS

        private ArchivedNotesViewModel()
        {
            AppData.ArchivedNotesChanged += (s, e) => NotifyPropertyChanged("Notes");
            //AppSettings.Instance.ColumnsChanged += (s, e) => NotifyPropertyChanged("Columns");
        }

#endregion
    }
}