﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Commands;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarMusicButton : ToolbarOverlayToggleButton
    {
        public ToolbarMusicButton()
        {
            Icon = FontAwesome.Solid.Music;
        }

        [BackgroundDependencyLoader(true)]
        private void load(MusicController music, ToggleOverlayCommand<MusicController> toggleOverlayCommand)
        {
            StateContainer = music;
            Command = toggleOverlayCommand;
        }
    }
}
