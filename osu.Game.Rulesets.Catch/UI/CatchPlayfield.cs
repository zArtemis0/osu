﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawable;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatchPlayfield : ScrollingPlayfield
    {
        public const float BASE_WIDTH = 512;

        protected override Container<Drawable> Content => content;
        private readonly Container<Drawable> content;

        private readonly CatcherArea catcherArea;

        public CatchPlayfield(BeatmapDifficulty difficulty)
            : base(ScrollingDirection.Down, BASE_WIDTH)
        {
            Container explodingFruitContainer;

            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;

            ScaledContent.Anchor = Anchor.BottomLeft;
            ScaledContent.Origin = Anchor.BottomLeft;

            ScaledContent.AddRange(new Drawable[]
            {
                content = new Container<Drawable>
                {
                    RelativeSizeAxes = Axes.Both,
                },
                explodingFruitContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                },
                catcherArea = new CatcherArea(difficulty)
                {
                    ExplodingFruitTarget = explodingFruitContainer,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.TopLeft,
                }
            });
        }

        public bool CheckIfWeCanCatch(CatchHitObject obj) => catcherArea.AttemptCatch(obj);

        public override void Add(DrawableHitObject h)
        {
            h.Depth = (float)h.HitObject.StartTime;

            base.Add(h);

            var fruit = (DrawableCatchHitObject)h;
            fruit.CheckPosition = CheckIfWeCanCatch;
        }

        public override void OnJudgement(DrawableHitObject judgedObject, Judgement judgement) => catcherArea.OnJudgement((DrawableCatchHitObject)judgedObject, judgement);
    }
}
