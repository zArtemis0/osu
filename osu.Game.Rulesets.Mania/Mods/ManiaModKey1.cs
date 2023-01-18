﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModKey1 : ManiaKeyMod
    {
        public override int KeyCount => 1;
        public override string Name => "One Key";
        public override string Acronym => "1K";
        public override IconUsage? Icon => OsuIcon.ModKey1;
        public override LocalisableString Description => @"Play with one key.";
    }
}
