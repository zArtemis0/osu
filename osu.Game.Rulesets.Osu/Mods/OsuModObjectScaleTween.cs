// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Mods
{
    /// <summary>
    /// Adjusts the size of hit objects during their fade in animation.
    /// </summary>
    public abstract class OsuModObjectScaleTween : ModWithVisibilityAdjustment, IHidesApproachCircles
    {
        public override ModType Type => ModType.Fun;

        public override double ScoreMultiplier => 1;

        [SettingSource("Starting Size", "The initial size multiplier applied to all objects.")]
        public abstract BindableNumber<float> StartScale { get; }

        [SettingSource("Scale Sliders", "Scale the slider bodies as well.")]
        public BindableBool ScaleSliders { get; } = new BindableBool(true);

        protected virtual float EndScale => 1;

        public override Type[] IncompatibleMods => new[] { typeof(IRequiresApproachCircles), typeof(OsuModSpinIn), typeof(OsuModObjectScaleTween) };

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state)
        {
        }

        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state) => applyCustomState(hitObject, state);

        private void applyCustomState(DrawableHitObject drawable, ArmedState state)
        {
            if (drawable is DrawableSpinner)
                return;

            var h = (OsuHitObject)drawable.HitObject;

            // apply grow effect
            if ((drawable is DrawableSlider && ScaleSliders.Value)
                || (drawable is DrawableSliderHead && !ScaleSliders.Value)
                || drawable is DrawableHitCircle)
            {
                using (drawable.BeginAbsoluteSequence(h.StartTime - h.TimePreempt))
                    drawable.ScaleTo(StartScale.Value).Then().ScaleTo(EndScale, h.TimePreempt, Easing.OutSine);
            }

            // remove approach circles
            switch (drawable)
            {
                case DrawableHitCircle circle:
                    // we don't want to see the approach circle
                    using (circle.BeginAbsoluteSequence(h.StartTime - h.TimePreempt))
                        circle.ApproachCircle.Hide();
                    break;
            }
        }
    }
}
