// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public class PlaylistsReadyButton : ReadyButton
    {
        [Resolved(typeof(Room), nameof(Room.EndDate))]
        private Bindable<DateTimeOffset?> endDate { get; set; }

        [Resolved(typeof(Room), nameof(Room.MaxAttempts))]
        private Bindable<int?> maxAttempts { get; set; }

        [Resolved(typeof(Room), nameof(Room.UserScore))]
        private Bindable<PlaylistAggregateScore> userScore { get; set; }

        [Resolved]
        private IBindable<WorkingBeatmap> gameBeatmap { get; set; }

        public PlaylistsReadyButton()
        {
            Text = "开始";
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BackgroundColour = colours.Green;
            Triangles.ColourDark = colours.Green;
            Triangles.ColourLight = colours.GreenLight;
        }

        private bool hasRemainingAttempts = true;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            userScore.BindValueChanged(aggregate =>
            {
                if (maxAttempts.Value == null)
                    return;

                int remaining = maxAttempts.Value.Value - aggregate.NewValue.PlaylistItemAttempts.Sum(a => a.Attempts);

                hasRemainingAttempts = remaining > 0;
            });
        }

        protected override void Update()
        {
            base.Update();

            Enabled.Value = hasRemainingAttempts && enoughTimeLeft;
        }

        public override LocalisableString TooltipText
        {
            get
            {
                if (Enabled.Value)
                    return string.Empty;

                if (!enoughTimeLeft)
                    return "时间已用完!";

                if (!hasRemainingAttempts)
                    return "尝试次数已用尽!";

                return base.TooltipText;
            }
        }

        private bool enoughTimeLeft =>
            // This should probably consider the length of the currently selected item, rather than a constant 30 seconds.
            endDate.Value != null && DateTimeOffset.UtcNow.AddSeconds(30).AddMilliseconds(gameBeatmap.Value.Track.Length) < endDate.Value;
    }
}
