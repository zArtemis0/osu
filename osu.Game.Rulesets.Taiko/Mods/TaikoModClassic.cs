// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Graphics;
using osu.Framework.Extensions.EnumExtensions;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModClassic : ModClassic, IApplicableToDrawableRuleset<TaikoHitObject>, IApplicableToDrawableHitObject, IUpdatableByPlayfield
    {
        /// <summary>
        /// The maximum aspect ratio with classic mod enabled.
        /// </summary>
        private const float classic_max_aspect_ratio = 2.0f / 1.0f;

        /// <summary>
        /// The classic hidden aspect ratio. Note that time rate is also stretched to this from the default aspect ratio.
        /// </summary>
        private const float classic_hidden_aspect_ratio = 4.0f / 3.0f;

        private LegacyMods enabledMods = LegacyMods.None;

        private DrawableTaikoRuleset? drawableTaikoRuleset;

        private double classicMaxTimeRange;

        public void EnableLegacyMods(LegacyMods legacyMods)
        {
            enabledMods = enabledMods | legacyMods;
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<TaikoHitObject> drawableRuleset)
        {
            drawableTaikoRuleset = (DrawableTaikoRuleset)drawableRuleset;
            drawableTaikoRuleset.AdjustmentMethod.Value = AspectRatioAdjustmentMethod.None;

            var playfield = (TaikoPlayfield)drawableRuleset.Playfield;
            playfield.ClassicHitTargetPosition.Value = true;

            classicMaxTimeRange = DrawableTaikoRuleset.AspectRatioToTimeRange(classic_max_aspect_ratio);
        }

        public void Update(Playfield playfield)
        {
            Debug.Assert(drawableTaikoRuleset != null);

            // Classic taiko scrolls at a constant 100px per 1000ms. More notes become visible as the playfield is lengthened.
            const float scroll_rate = 10;

            // Since the time range will depend on a positional value, it is referenced to the x480 pixel space.
            float ratio = drawableTaikoRuleset.DrawHeight / 480;

            double timeRange = (playfield.HitObjectContainer.DrawWidth / ratio) * scroll_rate;

            if (enabledMods.HasFlagFast(LegacyMods.HardRock))
            {
                // For hardrock, the playfield time range is locked to classicMaxTimeRange, but the display aspect ratio
                // is kept at a minimum of classic_max_aspect_ratio. If the display aspect ratio is less than that, the
                // play field will be extended beyond the display, and time range will effectively be cut short.
                drawableTaikoRuleset.AdjustmentMethod.Value = AspectRatioAdjustmentMethod.MaintainMinimum;
                drawableTaikoRuleset.AspectRatioLimit.Value = classic_max_aspect_ratio;
                timeRange = classicMaxTimeRange;
            }
            else if (enabledMods.HasFlagFast(LegacyMods.Hidden))
            {
                // Hidden aspect adjustment is overriden by hardrock in the case of hdhr, hence these are applied only
                // if hardrock is not enabled.
                drawableTaikoRuleset.AdjustmentMethod.Value = AspectRatioAdjustmentMethod.Trim;
                drawableTaikoRuleset.AspectRatioLimit.Value = classic_hidden_aspect_ratio;
            }

            drawableTaikoRuleset.TimeRange.Value = timeRange;
        }

        void IApplicableToDrawableHitObject.ApplyToDrawableHitObject(DrawableHitObject hitObject)
        {
            switch (hitObject)
            {
                case DrawableDrumRoll:
                case DrawableDrumRollTick:
                case DrawableHit:
                    hitObject.ApplyCustomUpdateState += (o, state) =>
                    {
                        Debug.Assert(drawableTaikoRuleset != null);

                        if (enabledMods == LegacyMods.None)
                        {
                            if (drawableTaikoRuleset.TimeRange.Value > classicMaxTimeRange)
                            {
                                o.Alpha = 0;

                                double preempt = drawableTaikoRuleset.TimeRange.Value / drawableTaikoRuleset.ControlPointAt(o.HitObject.StartTime).Multiplier;
                                double fadeInEnd = o.HitObject.StartTime - preempt * classicMaxTimeRange / drawableTaikoRuleset.TimeRange.Value;
                                double fadeInStart = fadeInEnd - 2000 / drawableTaikoRuleset.ControlPointAt(o.HitObject.StartTime).Multiplier;

                                using (o.BeginAbsoluteSequence(fadeInStart))
                                {
                                    o.FadeIn(fadeInEnd - fadeInStart);
                                }
                            }
                        }
                        else if (enabledMods.HasFlagFast(LegacyMods.Hidden | LegacyMods.HardRock))
                        {
                            // Decrease the initial alpha of the hitobject for hdhr
                            o.Alpha = 0.25f;
                        }
                    };
                    break;
            }
        }
    }
}
