// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.UI;

namespace osu.Game.Rulesets.Osu.Mods
{
    /// <summary>
    /// Mod that randomises the positions of the <see cref="HitObject"/>s
    /// </summary>
    public class OsuModRandom : ModRandom, IApplicableToBeatmap
    {
        public override string Description => "It never gets boring!";

        private static readonly float playfield_diagonal = OsuPlayfield.BASE_SIZE.LengthFast;

        private Random rng;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            if (!(beatmap is OsuBeatmap osuBeatmap))
                return;

            Seed.Value ??= RNG.Next();

            rng = new Random((int)Seed.Value);

            var positionModifier = new OsuHitObjectPositionModifier(osuBeatmap.HitObjects);

            float rateOfChangeMultiplier = 0;

            foreach (var positionInfo in positionModifier.HitObjectPositions)
            {
                // rateOfChangeMultiplier only changes every 5 iterations in a combo
                // to prevent shaky-line-shaped streams
                if (positionInfo.HitObject.IndexInCurrentCombo % 5 == 0)
                    rateOfChangeMultiplier = (float)rng.NextDouble() * 2 - 1;

                if (positionInfo == positionModifier.HitObjectPositions.First())
                {
                    positionInfo.Distance = (float)rng.NextDouble() * OsuPlayfield.BASE_SIZE.Y / 2;
                    positionInfo.RelativeAngle = (float)(rng.NextDouble() * 2 * Math.PI - Math.PI);
                }
                else
                {
                    positionInfo.RelativeAngle = (float)(rateOfChangeMultiplier * 2 * Math.PI * Math.Min(1f, positionInfo.Distance / (playfield_diagonal * 0.5f)));
                }
            }

            positionModifier.ApplyModifications();
        }
    }
}
