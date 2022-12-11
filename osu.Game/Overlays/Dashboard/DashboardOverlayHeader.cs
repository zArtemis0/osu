// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.ComponentModel;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Overlays.Dashboard
{
    public partial class DashboardOverlayHeader : TabControlOverlayHeader<DashboardOverlayTabs>
    {
        protected override OverlayTitle CreateTitle() => new DashboardTitle();

        private partial class DashboardTitle : OverlayTitle
        {
            public DashboardTitle()
            {
                Title = PageTitleStrings.MainHomeControllerIndex;
                Description = NamedOverlayComponentStrings.DashboardDescription;
                IconTexture = "Icons/Hexacons/social";
            }
        }
    }

    public enum DashboardOverlayTabs
    {
        [LocalisableDescription(typeof(FriendsStrings), nameof(FriendsStrings.TitleCompact))]
        Friends,

        [Description("正在游玩")]
        CurrentlyPlaying
    }
}
