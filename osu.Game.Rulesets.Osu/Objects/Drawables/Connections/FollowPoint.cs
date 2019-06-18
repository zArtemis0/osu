﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.MathUtils;
using osu.Game.Skinning;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Connections
{
    public class FollowPoint : Container
    {
        private const float width = 8;

        public override bool RemoveWhenNotAlive => false;

        public FollowPoint()
        {
            Origin = Anchor.Centre;

            Child = new SkinnableDrawable("Play/osu/followpoint", _ => new Container
            {
                Masking = true,
                AutoSizeAxes = Axes.Both,
                CornerRadius = width / 2,
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Colour = Color4.White.Opacity(0.2f),
                    Radius = 4,
                },
                Child = new Box
                {
                    Size = new Vector2(width),
                    Blending = BlendingMode.Additive,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Alpha = 0.5f,
                }
            }, restrictSize: false);
        }

        public void Fail()
        {
            this.FadeOut(FailAnimation.FAIL_DURATION / 2, Easing.OutQuad);
            this.ScaleTo(Scale * 0.5f, FailAnimation.FAIL_DURATION);
            this.MoveToOffset(new Vector2(RNG.NextSingle(-100, 100), 400), FailAnimation.FAIL_DURATION);
        }
    }
}
