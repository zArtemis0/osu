﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Configuration
{
    public enum ScoreMeterType
    {
        [Description("不显示")]
        None,

        [Description("误差 (左侧)")]
        HitErrorLeft,

        [Description("误差 (右侧)")]
        HitErrorRight,

        [Description("误差 (左右侧)")]
        HitErrorBoth,

        [Description("误差 (底部)")]
        HitErrorBottom,

        [Description("误差 (左侧)")]
        ColourLeft,

        [Description("颜色 (右侧)")]
        ColourRight,

        [Description("颜色 (左右侧)")]
        ColourBoth,

        [Description("颜色 (底部)")]
        ColourBottom,
    }
}
