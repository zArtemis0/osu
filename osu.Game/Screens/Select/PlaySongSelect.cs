﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;
using osuTK.Input;

namespace osu.Game.Screens.Select
{
    public class PlaySongSelect : SongSelect
    {
        private bool removeAutoModOnResume;
        private OsuScreen player;

        public override bool AllowExternalScreenChange => true;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BeatmapOptions.AddButton(@"Edit", @"beatmap", FontAwesome.fa_pencil, colours.Yellow, () =>
            {
                ValidForResume = false;
                Edit();
            }, Key.Number3);
        }

        public override void OnResuming(IScreen last)
        {
            player = null;

            if (removeAutoModOnResume)
            {
                Type autoType = Ruleset.Value.CreateInstance().GetAutoplayMod().GetType();
                ModSelect.DeselectTypes(new[] { autoType }, true);
                removeAutoModOnResume = false;
            }

            base.OnResuming(last);
        }

        protected override bool OnStart()
        {
            if (player != null) return false;

            // Ctrl+Enter should start map with autoplay enabled.
            if (GetContainingInputManager().CurrentState?.Keyboard.ControlPressed == true)
            {
                Mod auto = Ruleset.Value.CreateInstance().GetAutoplayMod();
                Type autoType = auto.GetType();

                IEnumerable<Mod> mods = SelectedMods.Value;
                if (mods.All(m => m.GetType() != autoType))
                {
                    SelectedMods.Value = mods.Append(auto);
                    removeAutoModOnResume = true;
                }
            }

            Beatmap.Value.Track.Looping = false;

            SampleConfirm?.Play();

            LoadComponentAsync(player = new PlayerLoader(() => new Player()), l =>
            {
                if (this.IsCurrentScreen()) this.Push(player);
            });

            return true;
        }
    }
}
