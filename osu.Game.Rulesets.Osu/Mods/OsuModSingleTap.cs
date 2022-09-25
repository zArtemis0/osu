// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModSingleTap : InputBlockingMod
    {
        public override string Name => @"单指";
        public override string Acronym => @"SG";
        public override LocalisableString Description => @"你只能使用一个键位！";
        public override Type[] IncompatibleMods => base.IncompatibleMods.Concat(new[] { typeof(OsuModAlternate) }).ToArray();

        protected override bool CheckValidNewAction(OsuAction action) => LastAcceptedAction == null || LastAcceptedAction == action;
    }
}
