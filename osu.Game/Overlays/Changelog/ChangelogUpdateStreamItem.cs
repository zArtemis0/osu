﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Changelog
{
    public class ChangelogUpdateStreamItem : OverlayStreamItem<APIUpdateStream>
    {
        public ChangelogUpdateStreamItem(APIUpdateStream stream)
            : base(stream)
        {
            if (stream.IsFeatured)
                Width *= 2;
        }

        protected override string MainText => Value.DisplayName;

        protected override string AdditionalText => Value.LatestBuild.DisplayVersion;

        protected override string InfoText => Value.LatestBuild.Users > 0 ? $"{"user".ToQuantity(Value.LatestBuild.Users, "N0")} online" : null;

        protected override Colour4 GetBarColour(OsuColour colours) => Value.Colour;
    }
}
