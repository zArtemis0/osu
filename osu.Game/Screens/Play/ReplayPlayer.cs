﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Play
{
    public class ReplayPlayer : Player, IKeyBindingHandler<GlobalAction>
    {
        private readonly Func<IBeatmap, IReadOnlyList<Mod>, Score> createScore;

        // Disallow replays from failing. (see https://github.com/ppy/osu/issues/6108)
        protected override bool CheckModsAllowFailure() => false;

        public ReplayPlayer(Score score, PlayerConfiguration configuration = null)
            : this((_, _) => score, configuration)
        {
        }

        public ReplayPlayer(Func<IBeatmap, IReadOnlyList<Mod>, Score> createScore, PlayerConfiguration configuration = null)
            : base(configuration)
        {
            this.createScore = createScore;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            LoadComponentAsync(new LLinGameplayLeaderboard(Score.ScoreInfo.User)
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft
            }, HUDOverlay.Add);
        }

        protected override void PrepareReplay()
        {
            DrawableRuleset?.SetReplayScore(Score);
        }

        protected override Score CreateScore(IBeatmap beatmap) => createScore(beatmap, Mods.Value);

        // Don't re-import replay scores as they're already present in the database.
        protected override Task ImportScore(Score score, bool bypassChesk = false)
        {
            //目前的DanceMod仍然需要手动导入
            var danceMod = (ModDance)Mods.Value.FirstOrDefault(m => m is ModDance);

            if (danceMod?.SaveScore.Value ?? false)
            {
                if (!score.ScoreInfo.User.Username.EndsWith(danceMod.ENDCHAR, StringComparison.Ordinal)) return Task.CompletedTask;

                score.ScoreInfo.User.Username = score.ScoreInfo.User.Username.Replace(danceMod.ENDCHAR, danceMod.ENDCHARREPLACE);
                base.ImportScore(score, bypassChesk);
            }

            return Task.CompletedTask;
        }

        protected override ResultsScreen CreateResults(ScoreInfo score) => new SoloResultsScreen(score, false);

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            const double keyboard_seek_amount = 5000;

            switch (e.Action)
            {
                case GlobalAction.SeekReplayBackward:
                    keyboardSeek(-1);
                    return true;

                case GlobalAction.SeekReplayForward:
                    keyboardSeek(1);
                    return true;

                case GlobalAction.TogglePauseReplay:
                    if (GameplayClockContainer.IsPaused.Value)
                        GameplayClockContainer.Start();
                    else
                        GameplayClockContainer.Stop();
                    return true;
            }

            return false;

            void keyboardSeek(int direction)
            {
                double target = Math.Clamp(GameplayClockContainer.CurrentTime + direction * keyboard_seek_amount, 0, GameplayState.Beatmap.HitObjects.Last().GetEndTime());

                Seek(target);
            }
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }
    }
}
