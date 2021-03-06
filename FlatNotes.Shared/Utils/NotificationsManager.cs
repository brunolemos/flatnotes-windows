﻿using FlatNotes.Models;
using NotificationsExtensions;
using NotificationsExtensions.TileContent;
using NotificationsExtensions.Tiles;
using NotificationsExtensions.Toasts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;

namespace FlatNotes.Utils
{
    //NotificationsExtensions: https://msdn.microsoft.com/en-us/library/windows/apps/xaml/dn642158.aspx
    //Tile Template Catalog: https://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh761491.aspx
    public static class NotificationsManager
    {
        public static void UpdateDefaultTile(bool transparentTile = false)
        {
            var tileSubFolder = transparentTile ? "Transparent" :  "Solid";
            var tileSquare71Content = TileContentFactory.CreateTileSquare71x71Image();
            ITileNotificationContent biggerTile;

            tileSquare71Content.Image.Src = String.Format("ms-appx:///Assets/Tiles/{0}/Square71x71Logo.png", tileSubFolder);

            var tileSquare150Content = TileContentFactory.CreateTileSquare150x150Image();
            tileSquare150Content.Image.Src = String.Format("ms-appx:///Assets/Tiles/{0}/Square150x150Logo.png", tileSubFolder);
            tileSquare150Content.Square71x71Content = tileSquare71Content;

            var tileWideContent = TileContentFactory.CreateTileWide310x150Image();
            tileWideContent.Image.Src = String.Format("ms-appx:///Assets/Tiles/{0}/Wide310x150Logo.png", tileSubFolder);
            tileWideContent.Square150x150Content = tileSquare150Content;

#if WINDOWS_PHONE_APP
            biggerTile = tileWideContent;
#else
            var tileSquare310Content = TileContentFactory.CreateTileSquare310x310Image();
            tileSquare310Content.Image.Src = String.Format("ms-appx:///Assets/Tiles/{0}/Square310x310Logo.png", tileSubFolder);
            tileSquare310Content.Wide310x150Content = tileWideContent;

            biggerTile = tileSquare310Content;
#endif

            TileUpdateManager.CreateTileUpdaterForApplication().Update(biggerTile.CreateNotification());
        }

        public static async Task<bool> CreateOrUpdateNoteTile(Note note, bool transparentTile = false)
        {
            if (note == null || note.IsEmpty()) return false;

            //update content
            if (SecondaryTile.Exists(note.ID)) return await UpdateNoteTileIfExists(note, transparentTile);

            //create and update (and suspend if is 8.1)
            var success = await CreateNoteTile(note, transparentTile);
            success &= await UpdateNoteTileIfExists(note, transparentTile);

            return success;
        }

        private static async Task<bool> CreateNoteTile(Note note, bool transparentTile = false)
        {
            var tile = new SecondaryTile(note.ID, App.Name, GenerateNavigationArgumentFromNote(note), 
                new Uri("ms-appx:///Assets/Tiles/Transparent/Logo.png"), TileSize.Wide310x150);

            tile.VisualElements.ForegroundText = ForegroundText.Light;
            tile.VisualElements.BackgroundColor = transparentTile ? Colors.Transparent : note.Color.DarkColor.Color;

            tile.VisualElements.ShowNameOnSquare150x150Logo = false;
            tile.VisualElements.ShowNameOnWide310x150Logo = false;
            tile.VisualElements.ShowNameOnSquare310x310Logo = false;

            tile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Assets/Tiles/Transparent/Square71x71Logo.png");
            tile.VisualElements.Square150x150Logo = new Uri("ms-appx:///Assets/Tiles/Transparent/Square150x150Logo.png");
            tile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/Tiles/Transparent/Wide310x150Logo.png");
            tile.VisualElements.Square310x310Logo = new Uri("ms-appx:///Assets/Tiles/Transparent/Square310x310Logo.png");
            
            return await tile.RequestCreateAsync();
        }

        public static async Task<bool> UpdateNoteTileIfExists(Note note, bool transparentTile = false)
        {
            //must exists
            if (!SecondaryTile.Exists(note.ID)) return false;

            //update background
            await UpdateNoteTileBackgroundColor(note, transparentTile);

            string contentWithoutTitle = note.GetContent(true, false, 150, 3);
            string contentWithTitle = note.GetContent(true, true, 200, 3);

            var tileSquare71Content = TileContentFactory.CreateTileSquare71x71Image();
            tileSquare71Content.Image.Src = "ms-appx:///Assets/Tiles/Transparent/Square71x71Logo.png";

            ITileNotificationContent biggerTile;
            ISquare150x150TileNotificationContent tileSquare150Content;

            if (note.Images != null && note.Images.Count > 0)
            {
                tileSquare150Content = TileContentFactory.CreateTileSquare150x150PeekImageAndText02();

                var _tileSquare150Content = tileSquare150Content as ITileSquare150x150PeekImageAndText02;
                _tileSquare150Content.TextHeading.Text = note.Title;
                _tileSquare150Content.TextBodyWrap.Text = contentWithoutTitle;
                _tileSquare150Content.Image.Src = note.Images[0].URL;
                _tileSquare150Content.Square71x71Content = tileSquare71Content;
                _tileSquare150Content.Branding = NotificationsExtensions.TileContent.TileBranding.Logo;
            }
            else
            {
                tileSquare150Content = TileContentFactory.CreateTileSquare150x150Text02();

                var _tileSquare150Content = tileSquare150Content as ITileSquare150x150Text02;
                _tileSquare150Content.TextHeading.Text = note.Title;
                _tileSquare150Content.TextBodyWrap.Text = contentWithoutTitle;
                _tileSquare150Content.Square71x71Content = tileSquare71Content;
                _tileSquare150Content.Branding = NotificationsExtensions.TileContent.TileBranding.Logo;
            }

            IWide310x150TileNotificationContent tileWide310x150Content;

            if (note.Images != null && note.Images.Count > 0)
            {
                tileWide310x150Content = TileContentFactory.CreateTileWide310x150PeekImage01();

                var _tileWide310x150Content = tileWide310x150Content as ITileWide310x150PeekImage01;
                _tileWide310x150Content.TextHeading.Text = note.Title;
                _tileWide310x150Content.TextBodyWrap.Text = contentWithoutTitle;
                _tileWide310x150Content.Image.Src = note.Images[0].URL;
                _tileWide310x150Content.Square150x150Content = tileSquare150Content;
                _tileWide310x150Content.Branding = NotificationsExtensions.TileContent.TileBranding.Logo;
            }
            else
            {
                tileWide310x150Content = TileContentFactory.CreateTileWide310x150Text09();

                var _tileWide310x150Content = tileWide310x150Content as ITileWide310x150Text09;
                _tileWide310x150Content.TextHeading.Text = note.Title;
                _tileWide310x150Content.TextBodyWrap.Text = contentWithoutTitle;
                _tileWide310x150Content.Square150x150Content = tileSquare150Content;
                _tileWide310x150Content.Branding = NotificationsExtensions.TileContent.TileBranding.Logo;
            }

#if WINDOWS_PHONE_APP
            biggerTile = tileWide310x150Content;
#else
            ISquare310x310TileNotificationContent tileSquare310Content;
            if (note.Images != null && note.Images.Count > 0)
            {
                tileSquare310Content = TileContentFactory.CreateTileSquare310x310ImageAndTextOverlay02();

                var _tileSquare310Content = tileSquare310Content as ITileSquare310x310ImageAndTextOverlay02;
                _tileSquare310Content.TextHeadingWrap.Text = note.Title;
                _tileSquare310Content.TextBodyWrap.Text = contentWithoutTitle;
                _tileSquare310Content.Image.Src = note.Images[0].URL;
                _tileSquare310Content.Wide310x150Content = tileWide310x150Content;
                _tileSquare310Content.Branding = NotificationsExtensions.TileContent.TileBranding.Logo;
            }
            else
            {
                tileSquare310Content = TileContentFactory.CreateTileSquare310x310ImageAndTextOverlay02();

                var _tileSquare310Content = tileSquare310Content as ITileSquare310x310ImageAndTextOverlay02;
                _tileSquare310Content.TextHeadingWrap.Text = note.Title;
                _tileSquare310Content.TextBodyWrap.Text = contentWithoutTitle;
                _tileSquare310Content.Image.Src = "";
                _tileSquare310Content.Wide310x150Content = tileWide310x150Content;
                _tileSquare310Content.Branding = NotificationsExtensions.TileContent.TileBranding.Logo;
            }

            biggerTile = tileSquare310Content;
#endif

            //create notification
            var notification = biggerTile.CreateNotification();
            notification.Tag = note.ID.Substring(0, 16);

            //update
            var tileUpdater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(note.ID);
            tileUpdater.Clear();
            //tileUpdater.EnableNotificationQueue(false); //crashing on w10mobile
            tileUpdater.Update(notification);

            return true;
        }

        public static async Task UpdateNoteTileBackgroundColor(Note note, bool transparentTile = false)
        {
            if (string.IsNullOrEmpty(note?.ID)) return;

            //must exists
            if (!SecondaryTile.Exists(note.ID)) return;

            var tile = new SecondaryTile(note.ID);
            if (tile == null) return;

            tile.VisualElements.BackgroundColor = transparentTile ? Colors.Transparent : note.Color.DarkColor.Color;
            await tile.UpdateAsync();
        }

        public static async void UpdateAllNoteTilesBackgroundColor(bool transparentTile = false)
        {
            var tiles = await SecondaryTile.FindAllAsync();
            if (tiles == null) return;

            foreach (var tile in tiles)
            {
                Note note = AppData.TryGetNoteById(tile.TileId);
                if (note == null) continue;

                //var tile = new SecondaryTile(t.TileId);
                tile.VisualElements.BackgroundColor = transparentTile ? Colors.Transparent : note.Color.DarkColor.Color;
                await tile.UpdateAsync();
            }
        }

        public static async void RemoveTileIfExists(string tileId)
        {
            //must exists
            if (!SecondaryTile.Exists(tileId)) return;

            var tile = new SecondaryTile(tileId);
            await tile.RequestDeleteAsync();
        }

        public static Note TryToGetNoteFromNavigationArgument(string navigationParameter)
        {
            if (String.IsNullOrEmpty(navigationParameter)) return null;

            string noteId;

            try
            {
                WwwFormUrlDecoder decoder = new WwwFormUrlDecoder(navigationParameter);
                Dictionary<string, string> queryParams = decoder.ToDictionary(x => x.Name, x => x.Value);

                if (!queryParams.ContainsKey("noteId")) return null;
                noteId = queryParams["noteId"];
            }
            catch (Exception)
            {
                return null;
            }

            var note = AppData.TryGetNoteById(noteId);
            if (note == null) RemoveTileIfExists(noteId);

            return note;
        }

        private static string GenerateNavigationArgumentFromNote(Note note)
        {
            return String.Format("?noteId={0}", note.ID);
        }

        public static bool TryCreateNoteReminder(Note note, DateTimeOffset? date)
        {
            RemoveScheduledToastsIfExists(note, date == null || !date.HasValue || date.Value == null || date.Value < DateTimeOffset.Now);

            if (note.IsEmpty() || date == null || !date.HasValue || date.Value == null || date.Value < DateTimeOffset.Now) return false;
            System.Diagnostics.Debug.WriteLine("Creating Reminder for noteId={0} date={1}", note.ID, date);

            var toastContent = new ToastContent()
            {
                Scenario = ToastScenario.Reminder,
                Launch = GenerateNavigationArgumentFromNote(note),
                Actions = new ToastActionsSnoozeAndDismiss(),
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                }
            };

            var title = note.Title;
            var text = note.GetContent(true, false, 200, 3);
            var imageURL = note.Images.Count > 0 ? note.Images.GetSelectedNoteImageOrLast().URL : null;

            if (!String.IsNullOrEmpty(title)) toastContent.Visual.BindingGeneric.Children.Add(new AdaptiveText() { Text = title });
            if (!String.IsNullOrEmpty(text)) toastContent.Visual.BindingGeneric.Children.Add(new AdaptiveText() { Text = text });
            if (!String.IsNullOrEmpty(imageURL)) toastContent.Visual.BindingGeneric.Children.Add(new AdaptiveImage() { Source = imageURL });
            
            try
            {
                note.Reminder.NoteId = note.ID;
                var toastId = Reminder.NoteIdToReminderID(note.ID);

                // Create the toast notification object.
                var toast = new ScheduledToastNotification(toastContent.GetXml(), date.Value);
                toast.NotificationMirroring = NotificationMirroring.Allowed;
                toast.Id = toastId;
                toast.Tag = toastId;
                toast.RemoteId = toastId;

                // Add to the schedule.
                ToastNotificationManager.CreateToastNotifier().AddToSchedule(toast);
                note.Reminder.IsActive = true;
                note.Reminder.Date = date;

                return true;
            } catch(Exception)
            {
                note.Reminder.IsActive = false;
                return false;
            }
        }

        public static void RemoveScheduledToastsIfExists(Note note, bool removeDateAndSetInactive = true)
        {
            note.Reminder.NoteId = note.ID;

            var toastNotifier = ToastNotificationManager.CreateToastNotifier();
            var allToastsScheduled = toastNotifier.GetScheduledToastNotifications().ToList();

            var toastId = Reminder.NoteIdToReminderID(note.ID);
            var toastsToRemove = allToastsScheduled.Where((t) => t.Id == toastId || t.Id == note.Reminder.ID).ToList();

            System.Diagnostics.Debug.WriteLine("Removing {0} scheduled toast notifications from a total of {1}", toastsToRemove.Count, allToastsScheduled.Count);
            foreach (var toast in toastsToRemove)
                toastNotifier.RemoveFromSchedule(toast);

            if (removeDateAndSetInactive)
            {
                note.Reminder.IsActive = false;
                note.Reminder.Date = null;
            }
        }
    }
}
