﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public partial class TestSceneSpectatorList : OsuTestScene
    {
        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        private TestGameplaySpectatorList spectatorList;

        private readonly BindableInt maxSpectators = new BindableInt();

        public TestSceneSpectatorList()
        {
            Child = spectatorList = new TestGameplaySpectatorList();

            AddSliderStep("set max spectators", 1, 15, 8, v => maxSpectators.Value = v);
        }

        [Test]
        public void TestManySpectators()
        {
            createSpectatorList();

            AddStep("Add peppy", () =>
            {
                createSpectator(new APIUser { Username = "peppy" });
            });

            AddStep("Add spectator", createRandomSpectator);

            AddStep("Remove peppy", () =>
            {
                removeSpectator(new APIUser { Username = "peppy" });
            });

            AddStep("Invert Visibility Setting", () =>
            {
                config.SetValue(OsuSetting.DisplaySpectatorList, !config.Get<bool>(OsuSetting.DisplaySpectatorList));
            });
        }

        private void createSpectatorList()
        {
            AddStep("create spectator list", () =>
            {
                Child = spectatorList = new TestGameplaySpectatorList
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Margin = new MarginPadding { Top = 50 },
                    Scale = new Vector2(1.5f),
                };
                spectatorList.MaxNames.BindTo(maxSpectators);
            });
        }

        private void createSpectator(APIUser user) => spectatorList.Add(user);

        private void createRandomSpectator()
        {
            APIUser user = new APIUser
            {
                Username = RNG.NextDouble(1_000.0, 100_000_000_000.0).ToString(CultureInfo.CurrentCulture),
            };

            spectatorList.Add(user);
        }

        private void removeSpectator(APIUser user) => spectatorList.Remove(user);

        private partial class TestGameplaySpectatorList : GameplaySpectatorList
        {
            public float Spacing => Flow.Spacing.Y;
        }
    }
}
