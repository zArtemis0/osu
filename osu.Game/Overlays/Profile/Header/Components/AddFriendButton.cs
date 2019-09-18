// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class AddFriendButton : ProfileHeaderButton
    {
        public override string TooltipText => "friends";

        private readonly OsuSpriteText followerText;

        public AddFriendButton()
        {
            Child = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Direction = FillDirection.Horizontal,
                Padding = new MarginPadding { Right = 10 },
                Children = new Drawable[]
                {
                    new SpriteIcon
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Icon = FontAwesome.Solid.User,
                        FillMode = FillMode.Fit,
                        Size = new Vector2(50, 14)
                    },
                    followerText = new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = OsuFont.GetFont(weight: FontWeight.Bold)
                    }
                }
            };
        }

        protected override void UpdateUser(User user) => followerText.Text = user?.FollowerCount.ToString("#,##0");
    }
}
