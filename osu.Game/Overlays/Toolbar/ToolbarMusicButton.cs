﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Overlays.Music;

namespace osu.Game.Overlays.Toolbar
{
    class ToolbarMusicButton : ToolbarOverlayToggleButton
    {
        public ToolbarMusicButton()
        {
            Icon = FontAwesome.fa_music;
        }

        [BackgroundDependencyLoader]
        private void load(MusicController music)
        {
            StateContainer = music;
            Action = music.ToggleVisibility;
        }
    }
}