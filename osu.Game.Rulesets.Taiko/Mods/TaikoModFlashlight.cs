﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Layout;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModFlashlight : ModFlashlight<TaikoHitObject>
    {
        public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.12 : 1;

        public override BindableFloat SizeMultiplier { get; } = new BindableFloat(1)
        {
            MinValue = 0.5f,
            MaxValue = 1.5f,
            Precision = 0.1f
        };

        public override BindableBool ComboBasedSize { get; } = new BindableBool(true);

        public override float DefaultFlashlightSize => 200;

        protected override Flashlight CreateFlashlight() => new TaikoFlashlight(this, playfield);

        private TaikoPlayfield playfield = null!;

        public override void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            playfield = (TaikoPlayfield)drawableRuleset.Playfield;
            base.ApplyToDrawableRuleset(drawableRuleset);
        }

        private class TaikoFlashlight : Flashlight
        {
            private readonly LayoutValue flashlightProperties = new LayoutValue(Invalidation.RequiredParentSizeToFit | Invalidation.DrawInfo);
            private readonly TaikoPlayfield taikoPlayfield;

            private float sizeMultiplier;
            private bool comboBasedSize;

            public TaikoFlashlight(TaikoModFlashlight modFlashlight, TaikoPlayfield taikoPlayfield)
                : base(modFlashlight)
            {
                this.taikoPlayfield = taikoPlayfield;

                FlashlightSize = adjustSize(GetSize());
                FlashlightSmoothness = 1.4f;

                modFlashlight.SizeMultiplier.BindValueChanged(_ => updateSize(modFlashlight), true);
                modFlashlight.ComboBasedSize.BindValueChanged(_ => updateSize(modFlashlight), true);

                AddLayout(flashlightProperties);
            }

            private void updateSize(TaikoModFlashlight modFlashlight)
            {
                sizeMultiplier = modFlashlight.SizeMultiplier.Value;
                comboBasedSize = modFlashlight.ComboBasedSize.Value;
                FlashlightSize = adjustSize(GetSize());
            }

            private Vector2 adjustSize(float size)
            {
                // Preserve flashlight size through the playfield's aspect adjustment.
                return new Vector2(0, size * taikoPlayfield.DrawHeight / TaikoPlayfield.DEFAULT_HEIGHT);
            }

            protected override void UpdateFlashlightSize(float size)
            {
                this.TransformTo(nameof(FlashlightSize), adjustSize(size), FLASHLIGHT_FADE_DURATION);
            }

            protected override string FragmentShader => "CircularFlashlight";

            protected override void Update()
            {
                base.Update();

                if (FlashlightSize.Y < 5)
                {
                    UpdateFlashlightSize(GetSize());
                }

                if (!flashlightProperties.IsValid)
                {
                    FlashlightPosition = ToLocalSpace(taikoPlayfield.HitTarget.ScreenSpaceDrawQuad.Centre);

                    ClearTransforms(targetMember: nameof(FlashlightSize));
                    FlashlightSize = adjustSize(Combo.Value);

                    flashlightProperties.Validate();
                }
            }
        }
    }
}
