﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Types;
using System;
using osu.Game.Rulesets.Osu.UI;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace osu.Game.Rulesets.Osu.Beatmaps
{
    public class OsuBeatmapConverter : BeatmapConverter<OsuHitObject>
    {
        public OsuBeatmapConverter(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        protected override IEnumerable<Type> ValidConversionTypes { get; } = new[] { typeof(IHasPosition) };

        protected override IEnumerable<OsuHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap)
        {
            var positionData = original as IHasPosition;
            var comboData = original as IHasCombo;

            OsuHitObject converted = original switch
            {
                IHasCurve curveData => new Slider
                {
                    StartTime = original.StartTime,
                    Samples = original.Samples,
                    Path = curveData.Path,
                    NodeSamples = curveData.NodeSamples,
                    RepeatCount = curveData.RepeatCount,
                    Position = positionData?.Position ?? Vector2.Zero,
                    NewCombo = comboData?.NewCombo ?? false,
                    ComboOffset = comboData?.ComboOffset ?? 0,
                    LegacyLastTickOffset = (original as IHasLegacyLastTickOffset)?.LegacyLastTickOffset,
                    // prior to v8, speed multipliers don't adjust for how many ticks are generated over the same distance.
                    // this results in more (or less) ticks being generated in <v8 maps for the same time duration.
                    TickDistanceMultiplier = beatmap.BeatmapInfo.BeatmapVersion < 8 ? 1f / beatmap.ControlPointInfo.DifficultyPointAt(original.StartTime).SpeedMultiplier : 1
                },
                IHasEndTime endTimeData => new Spinner
                {
                    StartTime = original.StartTime,
                    Samples = original.Samples,
                    EndTime = endTimeData.EndTime,
                    Position = positionData?.Position ?? OsuPlayfield.BASE_SIZE / 2,
                    NewCombo = comboData?.NewCombo ?? false,
                    ComboOffset = comboData?.ComboOffset ?? 0,
                },
                _ => new HitCircle
                {
                    StartTime = original.StartTime,
                    Samples = original.Samples,
                    Position = positionData?.Position ?? Vector2.Zero,
                    NewCombo = comboData?.NewCombo ?? false,
                    ComboOffset = comboData?.ComboOffset ?? 0,
                },
            };

            return converted.Yield();
        }

        protected override Beatmap<OsuHitObject> CreateBeatmap() => new OsuBeatmap();
    }
}
