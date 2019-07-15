// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneUpdateableBeatmapSetCover : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(UpdateableBeatmapSetCover),
            typeof(BeatmapSetCover),
        };

        private readonly FillFlowContainer coversContainer;

        public TestSceneUpdateableBeatmapSetCover()
        {
            Child = coversContainer = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(10),
                Children = new[]
                {
                    new TestUpdateableBeatmapSetCover(BeatmapSetCoverType.Cover),
                    new TestUpdateableBeatmapSetCover(BeatmapSetCoverType.Card),
                    new TestUpdateableBeatmapSetCover(BeatmapSetCoverType.List),
                }
            };
        }

        [SetUp]
        public void SetUp()
        {
            foreach (var cover in coversContainer.Children.OfType<TestUpdateableBeatmapSetCover>())
                cover.BeatmapSet = null;
        }

        [Test]
        public void TestLocal()
        {
            var setInfo = new BeatmapSetInfo
            {
                OnlineInfo = new BeatmapSetOnlineInfo
                {
                    Covers = new BeatmapSetOnlineCovers
                    {
                        Cover = "https://assets.ppy.sh/beatmaps/241526/covers/cover@2x.jpg?1521086997",
                        Card = "https://assets.ppy.sh/beatmaps/241526/covers/card@2x.jpg?1521086997",
                        List = "https://assets.ppy.sh/beatmaps/241526/covers/list@2x.jpg?1521086997",
                    }
                }
            };

            foreach (var cover in coversContainer.Children.OfType<TestUpdateableBeatmapSetCover>())
            {
                var coverType = cover.CoverType.ToString().ToLower();
                AddStep($"set beatmap for {coverType}", () => cover.BeatmapSet = setInfo);
                AddUntilStep($"wait until {coverType} drawable is displayed", () => cover.DisplayedDrawable is BeatmapSetCover);
                AddAssert($"verify {coverType} drawable has correct beatmapset", () => (cover.DisplayedDrawable as BeatmapSetCover)?.BeatmapSet == setInfo);
                AddAssert($"verify {coverType} drawable has correct type", () => (cover.DisplayedDrawable as BeatmapSetCover)?.CoverType == cover.CoverType);
            }
        }

        private class TestUpdateableBeatmapSetCover : UpdateableBeatmapSetCover
        {
            public new BeatmapSetCoverType CoverType => base.CoverType;
            public new Drawable DisplayedDrawable => base.DisplayedDrawable;

            public TestUpdateableBeatmapSetCover(BeatmapSetCoverType coverType)
                : base(coverType)
            {
                switch (coverType)
                {
                    case BeatmapSetCoverType.Cover:
                        Size = new Vector2(400);
                        break;

                    case BeatmapSetCoverType.Card:
                        Size = new Vector2(400, 200);
                        break;

                    case BeatmapSetCoverType.List:
                        Size = new Vector2(600, 150);
                        break;
                }
            }
        }
    }
}
