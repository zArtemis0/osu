﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Game.Audio;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Objects.Legacy.Mania
{
    /// <summary>
    /// A HitObjectParser to parse legacy osu!mania Beatmaps.
    /// </summary>
    public class ConvertHitObjectParser : Legacy.ConvertHitObjectParser
    {
        public ConvertHitObjectParser(double offset, int formatVersion)
            : base(offset, formatVersion)
        {
            // LayeredHitSounds is disabled in mania
            AllowsLayeredHitSounds = false;
        }

        protected override HitObject CreateHit(Vector2 position, bool newCombo, int comboOffset)
        {
            return new ConvertHit
            {
                X = position.X
            };
        }

        protected override HitObject CreateSlider(Vector2 position, bool newCombo, int comboOffset, PathControlPoint[] controlPoints, double? length, int repeatCount,
                                                  List<IList<HitSampleInfo>> nodeSamples)
        {
            return new ConvertSlider
            {
                X = position.X,
                Path = new SliderPath(controlPoints, length),
                NodeSamples = nodeSamples,
                RepeatCount = repeatCount
            };
        }

        protected override HitObject CreateSpinner(Vector2 position, bool newCombo, int comboOffset, double endTime)
        {
            return new ConvertSpinner
            {
                X = position.X,
                EndTime = endTime
            };
        }

        protected override HitObject CreateHold(Vector2 position, bool newCombo, int comboOffset, double endTime)
        {
            return new ConvertHold
            {
                X = position.X,
                EndTime = endTime
            };
        }
    }
}
