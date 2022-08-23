﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Mods
{
    public interface IApplicableToDisable : IApplicableMod
    {
        Bindable<bool> ReplayLoaded
        {
            get;
        }

        bool IsDisable
        {
            get;
        }

        void OnToggle();
    }
}
