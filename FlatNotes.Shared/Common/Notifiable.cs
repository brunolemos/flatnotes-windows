﻿using System;
using System.ComponentModel;

namespace FlatNotes.Common
{
    public class Notifiable : Object, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void NotifyChanges()
        {
            NotifyPropertyChanged(String.Empty);
        }
    }
}
