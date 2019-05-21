// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Input.Commands;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarSocialButton : ToolbarOverlayToggleButton
    {
        public ToolbarSocialButton()
        {
            Icon = FontAwesome.Solid.Users;
        }

        [BackgroundDependencyLoader(true)]
        private void load(SocialOverlay chat, ToggleOnlineOverlayCommand<SocialOverlay> toggleOverlayCommand)
        {
            StateContainer = chat;
            Command = toggleOverlayCommand;
        }
    }
}
