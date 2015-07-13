﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FlatNotes.Controls
{
    public sealed partial class NotesControl : UserControl
    {
        public event EventHandler<ItemClickEventArgs> ItemClick;
        public const double ITEM_MIN_WIDTH = 150;

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(Object), typeof(NotesControl), new PropertyMetadata(-1));
        public Object ItemsSource { get { return (Object)GetValue(ItemsSourceProperty); } set { SetValue(ItemsSourceProperty, value); } }

        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(int), typeof(NotesControl), new PropertyMetadata(-1));
        public int Columns { get { return (int)GetValue(ColumnsProperty); } set { SetValue(ColumnsProperty, value); } }

        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register("ItemWidth", typeof(double), typeof(NotesControl), new PropertyMetadata(ITEM_MIN_WIDTH));
        public double ItemWidth { get { return (double)GetValue(ItemWidthProperty); } set { SetValue(ItemWidthProperty, value); } }

        public static readonly DependencyProperty ItemStretchProperty = DependencyProperty.Register("ItemStretch", typeof(bool), typeof(NotesControl), new PropertyMetadata(false));
        public bool ItemStretch { get { return (bool)GetValue(ItemStretchProperty); } set { SetValue(ItemStretchProperty, value); } }

        public static readonly DependencyProperty AllowSingleColumnProperty = DependencyProperty.Register("AllowSingleColumn", typeof(bool), typeof(NotesControl), new PropertyMetadata(false));
        public bool AllowSingleColumn { get { return (bool)GetValue(AllowSingleColumnProperty); } set { SetValue(AllowSingleColumnProperty, value); } }

        public static readonly DependencyProperty BiggerItemHeightProperty = DependencyProperty.Register("BiggerItemHeight", typeof(double), typeof(NotesControl), new PropertyMetadata(0));
        public double BiggerItemHeight { get { return (double)GetValue(BiggerItemHeightProperty); } private set { SetValue(BiggerItemHeightProperty, value); } }

        public NotesControl()
        {
            this.InitializeComponent();
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var handler = ItemClick;
            if (handler != null) handler(sender, e);
        }

        private void NotePreview_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            ShowNoteFlyout(sender as FrameworkElement);
        }

        private void NotePreview_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            ShowNoteFlyout(sender as FrameworkElement);
        }

        private void ShowNoteFlyout(FrameworkElement element)
        {
            if (element == null) return;
            Flyout.ShowAttachedFlyout(element);
        }
    }
}
