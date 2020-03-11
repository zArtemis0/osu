﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Overlays;
using osu.Game.Overlays.Home;
using osu.Game.Overlays.Home.Friends;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneHomeOverlay : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(HomeOverlay),
            typeof(HomeOverlayHeader),
            typeof(FriendsLayout),
        };

        protected override bool UseOnlineAPI => true;

        private readonly HomeOverlay overlay;

        public TestSceneHomeOverlay()
        {
            Add(overlay = new HomeOverlay());
        }

        [Test]
        public void TestShow()
        {
            AddStep("Show", overlay.Show);
        }

        [Test]
        public void TestHide()
        {
            AddStep("Hide", overlay.Hide);
        }
    }
}
