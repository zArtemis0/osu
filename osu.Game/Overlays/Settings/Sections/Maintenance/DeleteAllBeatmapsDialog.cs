﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Overlays.Settings.Sections.Maintenance
{
    public class DeleteAllBeatmapsDialog : PopupDialog
    {
        public DeleteAllBeatmapsDialog(Action deleteAction)
        {
            BodyText = "Everything?";

            Icon = FontAwesome.Regular.TrashAlt;
            HeaderText = @"Confirm deletion of";

            PopupDialogOkButton popupDialogOkButton;
            Buttons = new PopupDialogButton[]
            {
                popupDialogOkButton=new PopupDialogOkButton
                {
                    Text = @"Yes. Go for it."
                },
                new PopupDialogCancelButton
                {
                    Text = @"No! Abort mission!",
                },
            };

            popupDialogOkButton.Clicked += deleteAction;
        }
    }
}
