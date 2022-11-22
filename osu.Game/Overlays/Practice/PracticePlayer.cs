// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Rulesets;
using osu.Game.Screens.Play;

namespace osu.Game.Overlays.Practice
{
    [Cached]
    public class PracticePlayer : Player
    {
        public PracticeOverlay PracticeOverlay = null!;

        public Ruleset CurrentRuleset = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            var rulesetInfo = Ruleset.Value ?? Beatmap.Value.BeatmapInfo.Ruleset;

            CurrentRuleset = rulesetInfo.CreateInstance();

            AddInternal(PracticeOverlay = new PracticeOverlay());
        }

        protected override void Update()
        {
            base.Update();

            if (PracticeOverlay.IsPresent is not true) return;

            GameplayClockContainer.Stop();
            GameplayClockContainer.Hide();
        }
    }
}
