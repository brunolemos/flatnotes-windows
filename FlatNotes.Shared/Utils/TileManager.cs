﻿using FlatNotes.Models;
using NotificationsExtensions.TileContent;
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
    public static class TileManager
    {
        public static void UpdateDefaultTile(bool transparentTile = false)
        {
            App.TelemetryClient.TrackMetric("Transparent Tile", transparentTile ? 1 : 0);

            var tileSubFolder = transparentTile ? "Transparent" :  "Solid";
#if WINDOWS_UWP
#else
            var tileSquare71Content = TileContentFactory.CreateTileSquare71x71Image();
#endif
            ITileNotificationContent biggerTile;

#if WINDOWS_PHONE_APP
            tileSquare71Content.Image.Src = String.Format("ms-appx:///Assets/Tiles/{0}/Square71x71Logo.png", tileSubFolder);
#elif WINDOWS_APP
            tileSquare71Content.Image.Src = String.Format("ms-appx:///Assets/Tiles/{0}/Square70x70Logo.png", tileSubFolder);
#endif

            var tileSquare150Content = TileContentFactory.CreateTileSquare150x150Image();
            tileSquare150Content.Image.Src = String.Format("ms-appx:///Assets/Tiles/{0}/Logo.png", tileSubFolder);
#if WINDOWS_UWP
#else
            tileSquare150Content.Square71x71Content = tileSquare71Content;
#endif

            var tileWideContent = TileContentFactory.CreateTileWide310x150Image();
            tileWideContent.Image.Src = String.Format("ms-appx:///Assets/Tiles/{0}/WideLogo.png", tileSubFolder);
            tileWideContent.Square150x150Content = tileSquare150Content;

            biggerTile = tileWideContent;
//#if WINDOWS_PHONE_APP
//            biggerTile = tileWideContent;
//#else
//            var tileSquare310Content = TileContentFactory.CreateTileSquare310x310Image();
//            tileSquare310Content.Image.Src = String.Format("ms-appx:///Assets/Tiles/{0}/Square310x310Logo.png", tileSubFolder);
//            tileSquare310Content.Wide310x150Content = tileWideContent;

//            biggerTile = tileSquare310Content;
//#endif

            TileUpdateManager.CreateTileUpdaterForApplication().Update(biggerTile.CreateNotification());
        }

        public static async Task<bool> CreateOrUpdateNoteTile(Note note, bool transparentTile = false)
        {
            if (note == null || note.IsEmpty()) return false;

            //update content
            if (SecondaryTile.Exists(note.ID)) return await UpdateNoteTileIfExists(note, transparentTile);


#if WINDOWS_PHONE_APP
            //create (and suspend)
            return await CreateNoteTile(note, transparentTile);
#else
            //create and update
            var success = await CreateNoteTile(note);
            await UpdateNoteTileIfExists(note);

            return success;
#endif
        }

        private static async Task<bool> CreateNoteTile(Note note, bool transparentTile = false)
        {
            var tile = new SecondaryTile(note.ID, App.Name, GenerateNavigationArgumentFromNote(note), 
                new Uri("ms-appx:///Assets/Tiles/Transparent/Logo.png"), TileSize.Wide310x150);

            tile.VisualElements.ForegroundText = ForegroundText.Light;
            tile.VisualElements.BackgroundColor = transparentTile ? Colors.Transparent : new Color().FromHex(note.Color.DarkColor2);

            tile.VisualElements.ShowNameOnSquare150x150Logo = true;
            tile.VisualElements.ShowNameOnWide310x150Logo = true;
            tile.VisualElements.ShowNameOnSquare310x310Logo = true;

            tile.RoamingEnabled = true;

#if WINDOWS_UWP
#elif WINDOWS_PHONE_APP
            tile.VisualElements.Square30x30Logo = new Uri("ms-appx:///Assets/Tiles/Transparent/Square71x71Logo.png");
            tile.VisualElements.Square71x71Logo = new Uri("ms-appx:///Assets/Tiles/Transparent/Square71x71Logo.png");
#endif
            tile.VisualElements.Square150x150Logo = new Uri("ms-appx:///Assets/Tiles/Transparent/Logo.png");
            tile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/Tiles/Transparent/WideLogo.png");
            tile.VisualElements.Square310x310Logo = new Uri("ms-appx:///Assets/Tiles/Transparent/Square310x310Logo.png");
            
            return await tile.RequestCreateAsync();
        }

        public static async Task<bool> UpdateNoteTileIfExists(Note note, bool transparentTile = false)
        {
            //must exists
            if (!SecondaryTile.Exists(note.ID)) return false;

            //update background
            await UpdateNoteTileBackgroundColor(note, transparentTile);

            string contentWithoutTitle = note.GetContent(true, false, 4);

#if WINDOWS_UWP
            ISquare71x71TileNotificationContent tileSquare71Content = null;
#else
            var tileSquare71Content = TileContentFactory.CreateTileSquare71x71Image();
#endif

#if WINDOWS_PHONE_APP
            tileSquare71Content.Image.Src = "ms-appx:///Assets/Tiles/Transparent/Square71x71Logo.png";
#elif WINDOWS_APP
            tileSquare71Content.Image.Src = "ms-appx:///Assets/Tiles/Transparent/Square70x70Logo.png";
#endif

            ISquare150x150TileNotificationContent tileSquare150Content;

            if (note.Images.Count > 0)
            {
                tileSquare150Content = TileContentFactory.CreateTileSquare150x150PeekImageAndText02();

                var _tileSquare150Content = tileSquare150Content as ITileSquare150x150PeekImageAndText02;
                _tileSquare150Content.TextHeading.Text = note.Title;
                _tileSquare150Content.TextBodyWrap.Text = contentWithoutTitle;
                _tileSquare150Content.Image.Src = note.Images[0].URL;
                _tileSquare150Content.Square71x71Content = tileSquare71Content;
            }
            else
            {
                tileSquare150Content = TileContentFactory.CreateTileSquare150x150Text02();

                var _tileSquare150Content = tileSquare150Content as ITileSquare150x150Text02;
                _tileSquare150Content.TextHeading.Text = note.Title;
                _tileSquare150Content.TextBodyWrap.Text = contentWithoutTitle;
                _tileSquare150Content.Square71x71Content = tileSquare71Content;
            }


            IWide310x150TileNotificationContent tileWideContent;
            if (note.Images.Count > 0)
            {
                tileWideContent = TileContentFactory.CreateTileWide310x150PeekImage01();

                var _tileWideContent = tileWideContent as ITileWide310x150PeekImage01;
                _tileWideContent.TextHeading.Text = note.Title;
                _tileWideContent.TextBodyWrap.Text = contentWithoutTitle;
                _tileWideContent.Image.Src = note.Images[0].URL;
                _tileWideContent.Square150x150Content = tileSquare150Content;
            }
            else
            {
                tileWideContent = TileContentFactory.CreateTileWide310x150Text09();

                var _tileWideContent = tileWideContent as ITileWide310x150Text09;
                _tileWideContent.TextHeading.Text = note.Title;
                _tileWideContent.TextBodyWrap.Text = contentWithoutTitle;
                _tileWideContent.Square150x150Content = tileSquare150Content;
            }

            //create notification
            var notification = tileWideContent.CreateNotification();
            notification.Tag = note.ID.Substring(0, 16);

            //update
            var tileUpdater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(note.ID);
            tileUpdater.Clear();
            tileUpdater.EnableNotificationQueue(false);
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

            tile.VisualElements.BackgroundColor = transparentTile ? Colors.Transparent : new Color().FromHex(note.Color.DarkColor2);
            await tile.UpdateAsync();
        }

        public static async void UpdateAllNoteTilesBackgroundColor(bool transparentTile = false)
        {
            App.TelemetryClient.TrackMetric("Transparent Note Tile", transparentTile ? 1 : 0);

            var tiles = await SecondaryTile.FindAllAsync();
            if (tiles == null) return;

            foreach (var tile in tiles)
            {
                Note note = AppData.TryGetNoteById(tile.TileId);
                if (note == null) continue;

                //var tile = new SecondaryTile(t.TileId);
                tile.VisualElements.BackgroundColor = transparentTile ? Colors.Transparent : new Color().FromHex(note.Color.DarkColor2);
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

            return AppData.TryGetNoteById(noteId);
        }

        private static string GenerateNavigationArgumentFromNote(Note note)
        {
            return String.Format("?noteId={0}", note.ID);
        }
    }
}
