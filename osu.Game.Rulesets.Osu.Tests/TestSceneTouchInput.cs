﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Play;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneTouchInput : TestSceneOsuPlayer
    {
        private OsuInputManager osuInputManager => Player.DrawableRuleset.ChildrenOfType<OsuInputManager>().First();

        private OsuTouchInputMapper touchInputMapper => osuInputManager.ChildrenOfType<OsuTouchInputMapper>().First();

        private Vector2 touchPosition => osuInputManager.ScreenSpaceDrawQuad.Centre;

        private void touch(TouchSource source) => InputManager.BeginTouch(new Touch(source, touchPosition));

        private void release(TouchSource source) => InputManager.EndTouch(new Touch(source, touchPosition));

        [Test]
        public void TestTouchInput()
        {
            // Cleanup
            AddStep("Release touches", () =>
            {
                foreach (TouchSource source in Enum.GetValues(typeof(TouchSource)))
                    release(source);
            });

            // Setup
            AddStep("Create key counter", () => osuInputManager.Add(new Container
            {
                Children = new Drawable[] { new OsuActionKeyCounter(OsuAction.LeftButton), new OsuActionKeyCounter(OsuAction.RightButton) { Margin = new MarginPadding { Left = 200 } } },
                Position = touchPosition - new Vector2(250, 0)
            }));

            // Cursor touch
            AddStep("Touch with cursor finger", () => touch(TouchSource.Touch1));

            AddAssert("The touch is a cursor touch", () => touchInputMapper.IsCursorTouch(TouchSource.Touch1));
            AddAssert("Allowing other touch", () => touchInputMapper.AllowingOtherTouch);

            // Left button touch
            AddStep("Touch with other finger", () => touch(TouchSource.Touch2));

            AddAssert("Pressed other finger key", () => osuInputManager.PressedActions.Contains(OsuAction.RightButton));
            AddAssert("The touch is a tap touch", () => touchInputMapper.IsTapTouch(TouchSource.Touch2));
            AddAssert("Check active tap touches", () => touchInputMapper.ActiveTapTouches.Count == 1);
            AddAssert("Allowing other touch", () => touchInputMapper.AllowingOtherTouch);

            // Right button touch
            AddStep("Touch with another finger (Doubletapping)...", () => touch(TouchSource.Touch3));

            AddAssert("The other touch is also a tap touch", () => touchInputMapper.IsTapTouch(TouchSource.Touch3));
            AddAssert("Both keys are pressed", () => osuInputManager.PressedActions.Count() == 2);
            AddAssert("Check active tap touches", () => touchInputMapper.ActiveTapTouches.Count == 2);
            AddAssert("Tap only key mapping", () => touchInputMapper.TapOnlyMapping);

            // Invalid touch
            AddStep("Touch with an invalid touch", () => touch(TouchSource.Touch4));

            AddAssert("Touch is blocked", () => !touchInputMapper.AllowingOtherTouch);
            AddAssert("Check active tap touches", () => touchInputMapper.ActiveTapTouches.Count == 2);
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap { HitObjects = new List<HitObject> { new HitCircle { StartTime = 99999 } } };

        public class OsuActionKeyCounter : KeyCounter, IKeyBindingHandler<OsuAction>
        {
            public OsuAction Action { get; }

            public OsuActionKeyCounter(OsuAction action)
                : base(action.ToString())
            {
                Action = action;
            }

            public bool OnPressed(KeyBindingPressEvent<OsuAction> e)
            {
                if (e.Action == Action)
                {
                    IsLit = true;
                    Increment();
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<OsuAction> e)
            {
                if (e.Action == Action) IsLit = false;
            }
        }
    }
}
