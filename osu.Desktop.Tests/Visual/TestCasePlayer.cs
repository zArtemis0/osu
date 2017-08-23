﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Desktop.Tests.Beatmaps;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Play;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Desktop.Tests.Visual
{
    internal class TestCasePlayer : OsuTestCase
    {
        protected Player Player;
        protected RulesetStore Rulesets;

        public override string Description => @"Showing everything to play the game.";

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            Rulesets = rulesets;
        }

        protected virtual Beatmap TestBeatmap()
        {
            var objects = new List<HitObject>();

            int time = 1500;
            for (int i = 0; i < 50; i++)
            {
                objects.Add(new HitCircle
                {
                    StartTime = time,
                    Position = new Vector2(i % 4 == 0 || i % 4 == 2 ? 0 : OsuPlayfield.BASE_SIZE.X,
                    i % 4 < 2 ? 0 : OsuPlayfield.BASE_SIZE.Y),
                    NewCombo = i % 4 == 0
                });

                time += 500;
            }

            return new Beatmap
            {
                HitObjects = objects,
                BeatmapInfo = new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty(),
                    Ruleset = Rulesets.Query<RulesetInfo>().First(),
                    Metadata = new BeatmapMetadata
                    {
                        Artist = @"Unknown",
                        Title = @"Sample Beatmap",
                        Author = @"peppy",
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            WorkingBeatmap beatmap = new TestWorkingBeatmap(TestBeatmap());

            Add(new Box
            {
                RelativeSizeAxes = Framework.Graphics.Axes.Both,
                Colour = Color4.Black,
            });

            Add(Player = CreatePlayer(beatmap));
        }

        protected virtual Player CreatePlayer(WorkingBeatmap beatmap)
        {
            return new Player
            {
                InitialBeatmap = beatmap
            };
        }
    }
}
