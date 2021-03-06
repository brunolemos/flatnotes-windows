using FlatNotes.Common;
using FlatNotes.Controls;
using FlatNotes.Converters;
using FlatNotes.Models;
using FlatNotes.Utils;
using FlatNotes.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Controls;

namespace FlatNotes.ViewModels
{
    public class NoteEditViewModel : ViewModelBase
    {
        public static NoteEditViewModel Instance { get { if (instance == null) instance = new NoteEditViewModel(); return instance; } }
        private static NoteEditViewModel instance = null;

        public event EventHandler ReorderModeEnabled;
        public event EventHandler ReorderModeDisabled;

        public RelayCommand OpenImagePickerCommand { get; private set; }
        public RelayCommand ToggleChecklistCommand { get; private set; }
        public RelayCommand CreateNoteReminderCommand { get; private set; }
        public RelayCommand RemoveNoteReminderCommand { get; private set; }
        public RelayCommand PinCommand { get; private set; }
        public RelayCommand UnpinCommand { get; private set; }
        public RelayCommand ArchiveNoteCommand { get; private set; }
        public RelayCommand RestoreNoteCommand { get; private set; }
        public RelayCommand DeleteNoteCommand { get; private set; }
        public RelayCommand DeleteNoteImageCommand { get; private set; }

        private NoteEditViewModel()
        {
            OpenImagePickerCommand = new RelayCommand(_OpenImagePicker);
            ToggleChecklistCommand = new RelayCommand(ToggleChecklist);
            CreateNoteReminderCommand = new RelayCommand(CreateNoteReminder);
            RemoveNoteReminderCommand = new RelayCommand(RemoveNoteReminder);
            PinCommand = new RelayCommand(Pin);
            UnpinCommand = new RelayCommand(Unpin);
            ArchiveNoteCommand = new RelayCommand(ArchiveNote, CanArchiveNote);
            RestoreNoteCommand = new RelayCommand(RestoreNote);
            DeleteNoteCommand = new RelayCommand(DeleteNote);
            DeleteNoteImageCommand = new RelayCommand(DeleteNoteImage);

            PropertyChanged += OnPropertyChanged;

            AppData.NotesSaved += OnNotesSaved;
        }

        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Note")
            {
                CurrentNoteBeingEdited = Note;
                if (Note == null) return;

                IsNewNote = AppData.LocalDB.Find<Note>(Note.ID) == null;
                Note.NotifyPropertyChanged("IsPinned");
                Note.NotifyPropertyChanged("CanPin");

                NotifyPropertyChanged("ReminderFormatedString");
                NotifyPropertyChanged("ArchivedAtFormatedString");
                NotifyPropertyChanged("UpdatedAtFormatedString");
                NotifyPropertyChanged("CreatedAtFormatedString");
                ArchiveNoteCommand.RaiseCanExecuteChanged();
            }
        }

        private async void OnNotesSaved(object sender, EventArgs e)
        {
            NotifyPropertyChanged("Note");
            if (Note == null) return;

            Note.Changed = false;

            await NotificationsManager.UpdateNoteTileIfExists(Note, AppSettings.Instance.TransparentNoteTile);
        }

        public static Note CurrentNoteBeingEdited { get; set; }
        private static FriendlyTimeConverter friendlyTimeConverter = new FriendlyTimeConverter();

        public Note Note {
            get {
                if (note == null) Note = new Note();
                return note;
            }
            set {
                if(note != null) note.PropertyChanged -= OnNotePropertyChanged;
                if (value != null)
                {
                    note = value;
                    note.PropertyChanged += OnNotePropertyChanged;
                }

                NotifyPropertyChanged("Note");
            }
        }
        private static Note note;

        public bool IsNewNote { get { return isNewNote; } set { isNewNote = value; NotifyPropertyChanged("IsNewNote"); } }
        private bool isNewNote;

        public NoteImage TempNoteImage { get { return tempNoteImage; } set { tempNoteImage = value; } }
        private static NoteImage tempNoteImage = null;

        public string ReminderFormatedString { get { return Note.Reminder.FormatedString; } }
        public string ArchivedAtFormatedString { get { return string.Format(LocalizedResources.ArchivedAtFormatString, friendlyTimeConverter.Convert(Note.ArchivedAt)); } }
        public string UpdatedAtFormatedString { get { return string.Format(LocalizedResources.UpdatedAtFormatString, friendlyTimeConverter.Convert(Note.UpdatedAt)); } }
        public string CreatedAtFormatedString { get { return string.Format(LocalizedResources.CreatedAtFormatString, friendlyTimeConverter.Convert(Note.CreatedAt)); } }

        public ListViewReorderMode ReorderMode
        {
            get { return reorderMode; }
            set
            {
                if (reorderMode == value) return;

                reorderMode = value;
                NotifyPropertyChanged("ReorderMode");

                var handler = value == ListViewReorderMode.Enabled ? ReorderModeEnabled : ReorderModeDisabled;
                if (handler != null) handler(this, EventArgs.Empty);
            }
        }
        public ListViewReorderMode reorderMode = ListViewReorderMode.Disabled;

        #region COMMANDS_ACTIONS


        private void _OpenImagePicker()
        {
            App.TelemetryClient.TrackEvent("OpenImagePicker_NoteEditViewModel");
            OpenImagePicker();
        }

        public async void OpenImagePicker()
        {
            NotesPage.DisablePopupLightDismiss();

            FileOpenPicker picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            //image
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");

#if WINDOWS_PHONE_APP
            await Task.Delay(0);
            //open
            picker.PickMultipleFilesAndContinue();
#elif WINDOWS_UWP
            var files = await picker.PickMultipleFilesAsync();
            HandleImageFromFilePicker(files);
#endif
        }

        public void HandleImageFromFilePicker(StorageFile file)
        {
            HandleImageFromFilePicker(new List<StorageFile>() { file });
        }

        public async void HandleImageFromFilePicker(IReadOnlyList<StorageFile> files)
        {
            NotesPage.EnablePopupLightDismiss();

            if (files == null || files.Count <= 0) return;
            bool error = false;

            try
            {
                //delete old images
                //await AppData.RemoveNoteImages(Note.Images);

                //clear image list
                //Note.Images.Clear();

                //add new images
                foreach (var file in files)
                {
                    if (file == null) continue;
                    Debug.WriteLine("Picked photo: " + file.Path);

                    NoteImage noteImage = new NoteImage();

                    StorageFile savedImage = await AppSettings.Instance.SaveImage(file, Note.ID, noteImage.ID);

                    var imageProperties = await savedImage.Properties.GetImagePropertiesAsync();
                    noteImage.NoteId = Note.ID;
                    noteImage.URL = savedImage.Path;
                    noteImage.Width = imageProperties.Width;
                    noteImage.Height = imageProperties.Height;

                    Note.Images.Add(noteImage);
                    break;
                }

                //Note.NotifyPropertyChanged("Images");
            }
            catch (Exception e)
            {
                error = true;

                var exceptionProperties = new Dictionary<string, string>() { { "Details", "Failed to save images" } };

                var exceptionMetrics = new Dictionary<string, double>();
                exceptionMetrics.Add("Image count", Note.Images.Count);
                for (int i = 0; i < Note.Images.Count; i++)
                {
                    exceptionMetrics.Add(string.Format("Image[{0}] Width", i), Note.Images[i].Width);
                    exceptionMetrics.Add(string.Format("Image[{0}] Height", i), Note.Images[i].Height);
                }

                App.TelemetryClient.TrackException(e, exceptionProperties, exceptionMetrics);
            }

            if (error)
            {
                //await(new MessageDialog("Failed to save image. Try again.", "Sorry")).ShowAsync();
                return;
            }

            // update note with new image so we dont lose it if the user closes the app
            if (!IsNewNote && !Note.IsEmpty())
            {
                try
                {
                    bool success = await AppData.CreateOrUpdateNote(Note);
                    if (success) IsNewNote = false;
                } catch(Exception e) { }
            }
        }

        private void ToggleChecklist()
        {
            App.TelemetryClient.TrackEvent("ToggleChecklist_NoteEditViewModel");
            Note.ToggleChecklist();
        }

        private void CreateNoteReminder()
        {
            App.TelemetryClient.TrackEvent("CreateNoteReminder_EditViewModel");

            if (!IsNewNote && !Note.IsEmpty() && Note.Reminder.Date.HasValue)
                NotificationsManager.TryCreateNoteReminder(Note, Note.Reminder.Date);

            Note.NotifyPropertyChanged("Reminder");
        }

        private void RemoveNoteReminder()
        {
            App.TelemetryClient.TrackEvent("RemoveNoteReminder_EditViewModel");

            NotificationsManager.RemoveScheduledToastsIfExists(Note);
            Note.NotifyPropertyChanged("Reminder");
        }

        private async void Pin()
        {
            App.TelemetryClient.TrackEvent("Pin_EditViewModel");

            if (Note.IsEmpty()) return;
            if (IsNewNote) await AppData.CreateOrUpdateNote(Note);

            await NotificationsManager.CreateOrUpdateNoteTile(Note, AppSettings.Instance.TransparentNoteTile);
            Note.NotifyPropertyChanged("IsPinned");
            Note.NotifyPropertyChanged("CanPin");
        }

        private async void Unpin()
        {
            App.TelemetryClient.TrackEvent("Unpin_NoteEditViewModel");

            NotificationsManager.RemoveTileIfExists(Note.ID);
            Note.NotifyPropertyChanged("IsPinned");

            await Task.Delay(0500);
            Note.NotifyPropertyChanged("IsPinned");
            Note.NotifyPropertyChanged("CanPin");
        }

        private void ArchiveNote()
        {
            App.TelemetryClient.TrackEvent("Archive_NoteEditViewModel");
            AppData.ArchiveNote(Note);
            note = null;

            CloseNote();
        }

        private bool CanArchiveNote()
        {
            if (Note == null || Note.IsArchived) return false;

            return !IsNewNote;
        }

        private void RestoreNote()
        {
            App.TelemetryClient.TrackEvent("Restore_NoteEditViewModel");
            AppData.RestoreNote(Note);
            note = null;

            CloseNote();
        }

        private async void DeleteNote()
        {
            App.TelemetryClient.TrackEvent("Delete_NoteEditViewModel");

            await AppData.RemoveNote(Note);

            note = null;
            CloseNote();
        }

        public async void DeleteNoteImage()
        {
            App.TelemetryClient.TrackEvent("DeleteNoteImage_NoteEditViewModel");
            if (TempNoteImage == null) return;

            bool success = await AppData.RemoveNoteImage(TempNoteImage);
            if (!success) return;

            Note.Images.Remove(TempNoteImage);
            TempNoteImage = null;
        }

        #endregion

        void OnNotePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Reminder":
                    NotifyPropertyChanged("ReminderFormatedString");
                    break;

                case "ArchivedAt":
                    NotifyPropertyChanged("ArchivedAtFormatedString");
                    break;

                case "UpdatedAt":
                    NotifyPropertyChanged("UpdatedAtFormatedString");
                    break;

                case "CreatedAt":
                    NotifyPropertyChanged("CreatedAtFormatedString");
                    break;

                case "IsNewNote":
                    ArchiveNoteCommand.RaiseCanExecuteChanged();
                    break;
            }

            if (!(e.PropertyName == "IsChecklist"
                || e.PropertyName == "Title" || e.PropertyName == "Text"
                || e.PropertyName == "Checklist" || e.PropertyName == "Images"
                || e.PropertyName == "Color" || e.PropertyName == "UpdatedAt"
                || e.PropertyName == "Reminder"))
                return;

            Note.Changed = true;

            //Debug.WriteLine("Note_PropertyChanged " + e.PropertyName);
            if (e.PropertyName == "UpdatedAt") return;
            Note.Touch();
        }
    }
}