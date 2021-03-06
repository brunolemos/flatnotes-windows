﻿using FlatNotes.Models;
using System;

namespace FlatNotes.Events
{
    public class NoteColorEventArgs : EventArgs
    {
        public Note Note { get; private set; }
        public NoteColor NoteColor { get; private set; }
        public bool Handled { get; set; }

        public NoteColorEventArgs(NoteColor noteColor)
        {
            NoteColor = noteColor;
            Handled = false;
        }

        public NoteColorEventArgs(Note note, NoteColor noteColor)
        {
            Note = note;
            NoteColor = noteColor;
            Handled = false;
        }
    }
}