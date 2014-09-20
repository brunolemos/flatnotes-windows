﻿using System;
using System.Collections.Generic;
using System.Text;

using Keep.Commands;
using Keep.Models;
using Keep.Utils;

namespace Keep.ViewModels
{
    public class MainViewModel
    {
        public DeleteNoteCommand DeleteNoteCommand { get { return deleteNoteCommand; } }
        private DeleteNoteCommand deleteNoteCommand = new DeleteNoteCommand();

        public SendFeedbackCommand SendFeedbackCommand { get { return sendFeedbackCommand; } }
        private SendFeedbackCommand sendFeedbackCommand = new SendFeedbackCommand();

        public Double CellWidth { get; set; }
        public Double BiggerCellHeight { get; set; }

        public Notes Notes { get { return notes; } }
        private Notes notes = AppSettings.Instance.LoggedUser.Notes;

        public UserPreferences UserPreferences { get { return userPreferences; } set { userPreferences = value; } }
        private UserPreferences userPreferences = AppSettings.Instance.LoggedUser.Preferences;
    }
}
