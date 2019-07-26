﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Framework.Bindables;
using System.Collections.Generic;
using osu.Game.Rulesets;
using osuTK;
using osu.Game.Rulesets.UI;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;
using System;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Allocation;
using osu.Game.Graphics;

namespace osu.Game.Overlays.BeatmapSet
{
    public class LeaderboardModSelector : Container
    {
        public readonly Bindable<IEnumerable<Mod>> SelectedMods = new Bindable<IEnumerable<Mod>>();

        private RulesetInfo ruleset = new RulesetInfo();
        private readonly FillFlowContainer<ModButton> modsContainer;

        public LeaderboardModSelector()
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            Child = modsContainer = new FillFlowContainer<ModButton>
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Full,
                Spacing = new Vector2(4),
            };
        }

        public void ResetRuleset(RulesetInfo ruleset)
        {
            SelectedMods.Value = new List<Mod>();

            if (ruleset is null)
            {
                this.ruleset = null;
                modsContainer.Clear();
                return;
            }

            if (this.ruleset?.Equals(ruleset) ?? false)
            {
                deselectAll();
                return;
            }

            this.ruleset = ruleset;

            modsContainer.Clear();

            modsContainer.Add(new NoModButton());

            foreach (var mod in ruleset.CreateInstance().GetAllMods())
                if (mod.Ranked)
                    modsContainer.Add(new ModButton(mod));

            foreach (var mod in modsContainer)
                mod.OnSelectionChanged += selectionChanged;
        }

        private void selectionChanged(Mod mod, bool selected)
        {
            var mods = SelectedMods.Value.ToList();

            if (selected)
                mods.Add(mod);
            else
                mods.Remove(mod);

            SelectedMods.Value = mods;
        }

        private void deselectAll() => modsContainer.ForEach(mod => mod.Selected.Value = false);

        private class ModButton : Container
        {
            private const float mod_scale = 0.4f;
            private const int duration = 200;

            public readonly BindableBool Selected = new BindableBool();
            public Action<Mod, bool> OnSelectionChanged;

            protected readonly ModIcon ModIcon;
            private readonly Mod mod;

            public ModButton(Mod mod)
            {
                this.mod = mod;

                AutoSizeAxes = Axes.Both;
                Children = new Drawable[]
                {
                    ModIcon = new ModIcon(mod)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Scale = new Vector2(mod_scale),
                    },
                    new HoverClickSounds()
                };

                Selected.BindValueChanged(_ => updateState(), true);
            }

            protected override bool OnClick(ClickEvent e)
            {
                Selected.Value = !Selected.Value;
                OnSelectionChanged?.Invoke(mod, Selected.Value);

                return base.OnClick(e);
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateState();
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);
                updateState();
            }

            private void updateState() => ModIcon.FadeColour(Selected.Value || IsHovered ? Color4.White : Color4.Gray, duration, Easing.OutQuint);
        }

        private class NoModButton : ModButton
        {
            public NoModButton()
                : base(new NoMod())
            {
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colors)
            {
                ModIcon.IconColour = colors.Yellow;
            }
        }

        private class NoMod : Mod
        {
            public override string Name => "NoMod";

            public override string Acronym => "NM";

            public override double ScoreMultiplier => 1;

            public override IconUsage Icon => FontAwesome.Solid.Ban;

            public override ModType Type => ModType.Custom;
        }
    }
}
