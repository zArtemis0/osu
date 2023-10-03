// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Skinning
{
    public partial class LegacyRankDisplay : CompositeDrawable, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; } = null!;

        [Resolved]
        private ISkinSource source { get; set; } = null!;

        private readonly Sprite rank;

        public LegacyRankDisplay()
        {
            AutoSizeAxes = Axes.Both;

            AddInternal(rank = new Sprite());
        }

        protected override void LoadComplete()
        {
            scoreProcessor.Rank.BindValueChanged(v => rank.Texture = source.GetTexture($"ranking-{v.NewValue}-small"), true);
        }
    }
}