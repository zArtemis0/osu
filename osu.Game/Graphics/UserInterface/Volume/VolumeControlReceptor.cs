﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Game.Input.Bindings;
using System;

namespace osu.Game.Graphics.UserInterface.Volume
{
    internal class VolumeControlReceptor : Container, IKeyBindingHandler<GlobalAction>
    {
        public Func<GlobalAction, bool> ActionRequested;

        public bool OnPressed(GlobalAction action) => ActionRequested?.Invoke(action) ?? false;

        public bool OnReleased(GlobalAction action) => false;
    }
}
