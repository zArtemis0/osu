﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Objects.Drawables.Pieces
{
    public class Pulp : PoolableDrawable
    {
        public Pulp()
        {
            RelativePositionAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Blending = BlendingParameters.Additive;
            Colour = Color4.White.Opacity(0.9f);

            Masking = true;
        }

        protected override void FreeAfterUse()
        {
            AccentColour.UnbindAll();
            base.FreeAfterUse();
        }

        public readonly Bindable<Color4> AccentColour = new Bindable<Color4>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddInternal(new Box { RelativeSizeAxes = Axes.Both });

            AccentColour.BindValueChanged(updateAccentColour, true);
        }

        private void updateAccentColour(ValueChangedEvent<Color4> colour)
        {
            CornerRadius = DrawWidth / 2;
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Radius = DrawWidth / 2,
                Colour = colour.NewValue.Darken(0.2f).Opacity(0.75f)
            };
        }
    }
}
