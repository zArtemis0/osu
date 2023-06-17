// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModSlide : ModWithVisibilityAdjustment
    {
        public override string Name => "Slide";
        public override string Acronym => "SL";
        public override ModType Type => ModType.Fun;
        public override LocalisableString Description => "Wheeeeee.";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModTransform), typeof(OsuModWiggle), typeof(OsuModSpinIn), typeof(OsuModMagnetised), typeof(OsuModRepel), typeof(OsuModFreezeFrame) };

        [SettingSource("Slide Speed", "How fast the objects slide in.")]
        public BindableNumber<double> SlideFactor { get; } = new BindableDouble(1)
        {
            MinValue = 0.5,
            MaxValue = 1.5,
            Precision = 0.01,
        };

        [SettingSource("Slide Direction", "Change where the circles slide in from.", 1)]
        public Bindable<SlideDirectionEnum> SlideDirection { get; } = new Bindable<SlideDirectionEnum>(SlideDirectionEnum.FromPrevious);

        protected override void ApplyIncreasedVisibilityState(DrawableHitObject hitObject, ArmedState state) => applySlideIn(hitObject, state);
        protected override void ApplyNormalVisibilityState(DrawableHitObject hitObject, ArmedState state) => applySlideIn(hitObject, state);

        private int counter = -1;

        private readonly List<Vector2> movementVectors = new List<Vector2>();
        private readonly List<Vector2> originalPositions = new List<Vector2>();
        private readonly List<double> animationDurations = new List<double>();

        public override void ApplyToBeatmap(IBeatmap beatmap)
        {
            movementVectors.Clear();
            originalPositions.Clear();

            var osuHitObjects = beatmap.HitObjects
                                       .OfType<OsuHitObject>().ToList()
                                       .Where(h => h is HitCircle or Slider)
                                       .ToList();

            // For each pair of consecutive hit objects in the beatmap...
            for (int i = 0; i < osuHitObjects.Count - 1; i++)
            {
                var currentHitObject = osuHitObjects[i];
                var nextHitObject = osuHitObjects[i + 1];

                Vector2 effectiveStartPosition = currentHitObject.Position;

                if (currentHitObject is Slider currentSlider)
                {
                    effectiveStartPosition = currentSlider.TailCircle.Position;
                }

                Vector2 effectiveEndPosition = nextHitObject.Position;

                if (nextHitObject is Slider nextSlider)
                {
                    effectiveEndPosition = nextSlider.HeadCircle.Position;
                }

                Vector2 movementVector = effectiveEndPosition - effectiveStartPosition;

                switch (SlideDirection.Value)
                {
                    case SlideDirectionEnum.FromPrevious:
                        movementVectors.Add(movementVector);
                        originalPositions.Add(effectiveStartPosition);
                        break;

                    case SlideDirectionEnum.TowardsPrevious:
                        movementVectors.Add(-movementVector);
                        originalPositions.Add(effectiveStartPosition + 2 * movementVector);
                        break;

                    case SlideDirectionEnum.Random:
                        // Actual logic is in ApplySlideIn to make randomness "per play" rather than "per map load"
                        movementVectors.Add(movementVector);
                        originalPositions.Add(effectiveEndPosition);
                        break;
                }

                // Calculate the animation durations
                double timeDiff = nextHitObject.StartTime - currentHitObject.GetEndTime();
                double animationDuration = ((-SlideFactor.Value) + 2.5) * timeDiff;

                animationDurations.Add(animationDuration);
            }

            base.ApplyToBeatmap(beatmap);
        }

        private void applySlideIn(DrawableHitObject drawable, ArmedState state)
        {
            // Here you will be applying the transformation to each hit object based on its corresponding movement vector.
            if (drawable.HitObject is not OsuHitObject osuHitObject) return;

            switch (drawable)
            {
                case DrawableSliderHead:
                case DrawableSliderTail:
                case DrawableSliderTick:
                case DrawableSliderRepeat:
                case DrawableSpinner:
                case DrawableSpinnerTick:
                    return;

                default:
                    if (!drawable.Judged)
                    {
                        if (osuHitObject is Slider or HitCircle) counter++;

                        if (counter == 0) return;

                        Vector2 originalPosition = originalPositions[counter - 1];
                        Vector2 movementVector = movementVectors[counter - 1];

                        if (SlideDirection.Value == SlideDirectionEnum.Random)
                        {
                            float length = movementVector.Length;
                            Random random = new Random();
                            Vector2 directionVector = new Vector2(2 * random.NextSingle() - 1, 2 * random.NextSingle() - 1).Normalized();
                            movementVector = directionVector * length;
                            originalPosition = originalPosition - movementVector;
                        }

                        drawable.MoveTo(originalPosition);

                        drawable.MoveTo(originalPosition + movementVector, Math.Min(animationDurations[counter - 1], 0.8 * osuHitObject.TimePreempt), Easing.OutQuad);
                    }

                    break;
            }
        }

        public enum SlideDirectionEnum
        {
            [LocalisableDescription(typeof(ModCustomizationSettingsStrings), nameof(ModCustomizationSettingsStrings.FromPrevious))]
            FromPrevious,

            [LocalisableDescription(typeof(ModCustomizationSettingsStrings), nameof(ModCustomizationSettingsStrings.TowardsPrevious))]
            TowardsPrevious,

            [LocalisableDescription(typeof(ModCustomizationSettingsStrings), nameof(ModCustomizationSettingsStrings.RandomDirection))]
            Random
        }
    }
}
