// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModSingleTap : InputBlockingMod
    {
        public override string Name => "Single Tap";
        public override string Acronym => "ST";
        public override string Description => @"Alternate tapping!";
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay) };
        public override bool checkCorrectAction(OsuAction action)
        {
            if(base.checkCorrectAction(action))
                return true;

            if (lastActionPressed == null)
            {
                lastActionPressed = action;
                return true;
            }
            else if (lastActionPressed == action)
            {
                return true;
            }

            ruleset.Cursor.FlashColour(Colour4.Red, flash_duration, Easing.OutQuint);
            return false;
        }
    }
}
