﻿using System;
using System.Linq;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Keep.Common;
using Keep.Models;
using Keep.Utils;
using Keep.ViewModels;

namespace Keep.Views
{
    public sealed partial class ArchivedNotesPage : Page
    {
        public NavigationHelper NavigationHelper { get { return this.navigationHelper; } }
        private NavigationHelper navigationHelper;

        public ArchivedNotesViewModel viewModel { get { return (ArchivedNotesViewModel)DataContext; } }

        public ArchivedNotesPage()
        {
            this.InitializeComponent();

            //Navigation Helper
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            //this.Loaded += (s, e) => EnableReorderFeature();
            //this.Unloaded += (s, e) => DisableReorderFeature();
        }

        //partial void EnableReorderFeature();
        //partial void DisableReorderFeature();

        partial void EnableSwipeFeature(FrameworkElement element, FrameworkElement referenceFrame);
        partial void DisableSwipeFeature(FrameworkElement element);

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            GoogleAnalytics.EasyTracker.GetTracker().SendView("ArchivedNotesPage");

            //App.ChangeStatusBarColor(Colors.Black);
            App.RootFrame.Background = LayoutRoot.Background;
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }

        #region NavigationHelper registration

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }


        #endregion

        private async void OnNoteTapped(object sender, TappedRoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            if (viewModel.ReorderMode == ListViewReorderMode.Enabled) return;
#endif

            Note note1 = (sender as FrameworkElement).DataContext as Note;
            Note note = (e.OriginalSource as FrameworkElement).DataContext as Note;
            if (note == null) return;

            //it can be trimmed, so get the original
            Note originalNote = AppData.ArchivedNotes.Where<Note>(n => n.ID == note.ID).FirstOrDefault();
            if (originalNote == null)
            {
                GoogleAnalytics.EasyTracker.GetTracker().SendException(string.Format("Failed to load tapped archived note ({0})", note.GetContent()), false);
                return;
            }

            //this dispatcher fixes crash error (access violation on wp preview for developers)
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Frame.Navigate(typeof(NoteEditPage), originalNote);
            });
        }

        //swipe feature
        private void OnNoteLoaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            FrameworkElement referenceFrame = NotesListView;

            if (viewModel.ReorderMode != ListViewReorderMode.Enabled)
                EnableSwipeFeature(element, referenceFrame);

            enableSwipeEventHandlers[element] = (s, _e) => { EnableSwipeFeature(element, referenceFrame); };
            disableSwipeEventHandlers[element] = (s, _e) => { DisableSwipeFeature(element); };

            viewModel.ReorderModeDisabled += enableSwipeEventHandlers[element];
            viewModel.ReorderModeEnabled += disableSwipeEventHandlers[element];
        }

        private void OnNoteUnloaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;

            if (enableSwipeEventHandlers.ContainsKey(element)) viewModel.ReorderModeDisabled -= enableSwipeEventHandlers[element];
            if (disableSwipeEventHandlers.ContainsKey(element)) viewModel.ReorderModeEnabled -= disableSwipeEventHandlers[element];

            DisableSwipeFeature(element);
        }
    }
}