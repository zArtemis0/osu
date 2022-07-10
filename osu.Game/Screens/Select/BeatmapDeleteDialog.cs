﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Screens.Select
{
    public class BeatmapDeleteDialog : PopupDialog
    {
        private BeatmapManager manager;

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmapManager)
        {
            manager = beatmapManager;
        }

        public BeatmapDeleteDialog(BeatmapSetInfo beatmap)
        {
            BodyText = $@"{beatmap.Metadata.Artist} - {beatmap.Metadata.Title}";

            Icon = FontAwesome.Regular.TrashAlt;
            HeaderText = @"是否删除";
            Buttons = new PopupDialogButton[]
            {
                new PopupDialogDangerousButton
                {
                    Text = @"是的。完全，删掉他。",
                    Action = () => manager?.Delete(beatmap),
                },
                new PopupDialogCancelButton
                {
                    Text = @"不是...是我点错了><!",
                },
            };
        }
    }
}
