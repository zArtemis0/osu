﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModFadeIn : ManiaModPlayfieldCover
    {
        public override string Name => "渐入";
        public override string Acronym => "FI";
        public override string Description => @"上隐模式!";
        public override double ScoreMultiplier => 1;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ManiaModHidden)).ToArray();

        protected override CoverExpandDirection ExpandDirection => CoverExpandDirection.AlongScroll;
    }
}
