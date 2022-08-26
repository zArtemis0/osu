﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Input.StateChanges.Events;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu
{
    public class OsuInputManager : RulesetInputManager<OsuAction>
    {
        public IEnumerable<OsuAction> PressedActions => KeyBindingContainer.PressedActions;

        public bool AllowUserPresses
        {
            set => ((OsuKeyBindingContainer)KeyBindingContainer).AllowUserPresses = value;
        }

        /// <summary>
        /// Whether the user's cursor movement events should be accepted.
        /// Can be used to block only movement while still accepting button input.
        /// </summary>
        public bool AllowUserCursorMovement { get; set; } = true;

        private List<TouchSource> allTapTouchSources = Enum.GetValues(typeof(TouchSource)).Cast<TouchSource>().ToList();

        protected override KeyBindingContainer<OsuAction> CreateKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
            => new OsuKeyBindingContainer(ruleset, variant, unique);

        public OsuInputManager(RulesetInfo ruleset)
            : base(ruleset, 0, SimultaneousBindingMode.Unique)
        {
        }

        protected override bool Handle(UIEvent e)
        {
            if ((e is MouseMoveEvent || e is TouchMoveEvent) && !AllowUserCursorMovement) return false;

            return base.Handle(e);
        }

        private OsuAction getActionForTouchSource(TouchSource source)
        {
            int sourceIndex = allTapTouchSources.IndexOf(source);
            return sourceIndex % 2 == 0 ? OsuAction.LeftButton : OsuAction.RightButton;
        }

        protected override bool HandleMouseTouchStateChange(TouchStateChangeEvent e)
        {
            int tapTouchesLimit = 2;
            var source = e.Touch.Source;

            var cursorTouch = CurrentState.Touch.ActiveSources.Any() ? CurrentState.Touch.ActiveSources.First() : source;

            var activeTapTouches = AllowUserCursorMovement ? CurrentState.Touch.ActiveSources.Skip(1) : CurrentState.Touch.ActiveSources;
            var limitedActiveTapTouches = activeTapTouches.Take(tapTouchesLimit);

            bool isTapTouch = activeTapTouches.Contains(source);

            // Limits amount of simultaneous clicks
            if (isTapTouch && !limitedActiveTapTouches.Contains(source))
                return false;

            bool dragMode = limitedActiveTapTouches.Count() >= tapTouchesLimit;

            var newActiveTapActions = limitedActiveTapTouches.Select(s => getActionForTouchSource(s));
            var newInactiveTapActions = PressedActions.Where(a => !newActiveTapActions.Contains(a)).ToList();

            foreach (var action in newInactiveTapActions)
                KeyBindingContainer.TriggerReleased(action);

            if (isTapTouch)
            {
                var action = getActionForTouchSource(source);
                if (!PressedActions.Contains(action))
                    KeyBindingContainer.TriggerPressed(action);
            }

            if (dragMode)
            {
                e = new TouchStateChangeEvent(e.State, e.Input, e.Touch, false, e.LastPosition);
            }

            return AllowUserCursorMovement && source == cursorTouch && base.HandleMouseTouchStateChange(e);
        }

        private class OsuKeyBindingContainer : RulesetKeyBindingContainer
        {
            public bool AllowUserPresses = true;

            public OsuKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
                : base(ruleset, variant, unique)
            {
            }

            protected override bool Handle(UIEvent e)
            {
                if (!AllowUserPresses) return false;

                return base.Handle(e);
            }
        }
    }

    public enum OsuAction
    {
        [Description("Left button")]
        LeftButton,

        [Description("Right button")]
        RightButton
    }
}
