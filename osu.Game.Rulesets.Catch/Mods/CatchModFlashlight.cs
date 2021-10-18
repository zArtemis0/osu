﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModFlashlight : ModFlashlight<CatchHitObject>
    {
        public override double ScoreMultiplier => 1.12;

        private const float default_flashlight_size = 350;

        private const int max_combo_offset = 200;

        private CatchFlashlight flashlight;

        public override Flashlight CreateFlashlight() => flashlight = new CatchFlashlight(playfield);

        private CatchPlayfield playfield;

        public override void ApplyToDrawableRuleset(DrawableRuleset<CatchHitObject> drawableRuleset)
        {
            playfield = (CatchPlayfield)drawableRuleset.Playfield;
            base.ApplyToDrawableRuleset(drawableRuleset);

            flashlight.ComboOffset = ComboOffset.Value;
        }

        [SettingSource("Combo offset", "Combo to start at for changing flashlight radius")]
        public BindableNumber<int> ComboOffset { get; } = new BindableInt
        {
            MinValue = 0,
            MaxValue = max_combo_offset,
            Precision = 1,
        };

        private class CatchFlashlight : Flashlight
        {
            private readonly CatchPlayfield playfield;

            public int ComboOffset { private get; set; }

            public CatchFlashlight(CatchPlayfield playfield)
            {
                this.playfield = playfield;
                FlashlightSize = new Vector2(0, getSizeFor(0));
            }

            protected override void Update()
            {
                base.Update();

                FlashlightPosition = playfield.CatcherArea.ToSpaceOfOtherDrawable(playfield.Catcher.DrawPosition, this);
            }

            private float getSizeFor(int combo)
            {
                int effectiveCombo = combo + ComboOffset;

                if (effectiveCombo > 200)
                    return default_flashlight_size * 0.8f;
                else if (effectiveCombo > 100)
                    return default_flashlight_size * 0.9f;
                else
                    return default_flashlight_size;
            }

            protected override void OnComboChange(ValueChangedEvent<int> e)
            {
                this.TransformTo(nameof(FlashlightSize), new Vector2(0, getSizeFor(e.NewValue)), FLASHLIGHT_FADE_DURATION);
            }

            protected override string FragmentShader => "CircularFlashlight";
        }
    }
}
