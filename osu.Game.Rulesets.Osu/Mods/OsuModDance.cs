﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModDance : ModDance
    {
        public override string Name => "Cusordance";
        public override string Acronym => "CD";
        public override string Description => "观看 lazer!dance";
        public override double ScoreMultiplier => 1;

        public override Score CreateReplayScore(IBeatmap beatmap, IReadOnlyList<Mod> mods) => new Score
        {
            ScoreInfo = new ScoreInfo { User = new APIUser { Username = $"lazer!dance{ENDCHAR}", IsBot = true } },
            Replay = new OsuDanceAutoGenerator(beatmap, mods).Generate()
        };
    }
}
