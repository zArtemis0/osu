﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Modes.UI;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects;
using osu.Framework.MathUtils;

namespace osu.Game.Modes.Taiko.UI
{
    public class TaikoPlayfield : Playfield
    {
        protected override Container<Drawable> Content => this;

        private const float left_area_size = 0.25f;
        private const float hit_target_offset = 0.1f;
        private const float playfield_height = 106;

        private HitTarget hitTarget;
        private Container explosionRingContainer;
        private Container judgementContainer;

        public TaikoPlayfield()
        {
            RelativeSizeAxes = Axes.Both;

            // Right area under notes
            AddInternal(new Container()
            {
                RelativeSizeAxes = Axes.X,
                RelativePositionAxes = Axes.X,
                Position = new Vector2(left_area_size, 0),
                Size = new Vector2(1f - left_area_size, playfield_height),

                BorderColour = new Color4(17, 17, 17, 255),
                BorderThickness = 2,

                Depth = 1,

                Children = new Drawable[]
                {
                    // Background
                    new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = new Color4(0, 0, 0, 127)
                    },
                    // Hit target
                    new Container()
                    {
                        RelativeSizeAxes = Axes.Both,
                        RelativePositionAxes = Axes.Both,

                        Position = new Vector2(hit_target_offset, 0),

                        Children = new Drawable[]
                        {
                            explosionRingContainer = new Container()
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.Centre,

                                RelativeSizeAxes = Axes.Y,
                                Size = new Vector2(74.2f, 0.7f),
                            },
                            hitTarget = new HitTarget()
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.Centre
                            }
                        }
                    },
                }
            });

            // Notes
            HitObjects.Anchor = Anchor.TopLeft;
            HitObjects.Origin = Anchor.TopLeft;

            HitObjects.RelativePositionAxes = Axes.X;
            HitObjects.RelativeSizeAxes = Axes.X;
            HitObjects.Position = new Vector2(left_area_size + hit_target_offset * (1f - left_area_size), 0);
            HitObjects.Size = new Vector2(1f - left_area_size - hit_target_offset * (1f - left_area_size), playfield_height);

            // Bar lines
            AddInternal(new Container()
            {
                RelativePositionAxes = HitObjects.RelativePositionAxes,
                RelativeSizeAxes = HitObjects.RelativeSizeAxes,
                Position = HitObjects.Position,
                Size = HitObjects.Size
            });

            // Left area above notes
            AddInternal(new Container()
            {
                RelativeSizeAxes = Axes.X,
                Size = new Vector2(left_area_size, 106),

                Masking = true,

                BorderColour = Color4.Black,
                BorderThickness = 1,

                Children = new Drawable[]
                {
                    // Background
                    new Box()
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = new Color4(17, 17, 17, 255)
                    },
                    new InputDrum()
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,

                        RelativePositionAxes = Axes.X,
                        Position = new Vector2(0.10f, 0)
                    }
                }
            });
        }

        public override void Add(DrawableHitObject h)
        {
            h.Depth = (float)h.HitObject.StartTime;

            h.OnJudgement += onJudgement;

            base.Add(h);
        }

        private void onJudgement(DrawableHitObject h, JudgementInfo j)
        {
            TaikoJudgementInfo tj = j as TaikoJudgementInfo;
            DrawableTaikoHitObject dth = h as DrawableTaikoHitObject;

            ExplodingRing ring = null;

            if (tj.Score == TaikoScoreResult.Great)
            {
                hitTarget.Flash(dth.ExplodeColour);
                ring = new ExplodingRing(dth.ExplodeColour, true);
            }
            else if (tj.Score == TaikoScoreResult.Good)
                ring = new ExplodingRing(dth.ExplodeColour, false);

            if (ring != null)
                explosionRingContainer.Add(ring);
        }

        class ExplodingRing : CircularContainer
        {
            private const float offset_min = -0.5f;
            private const float offset_max = 0.5f;

            public ExplodingRing(Color4 fillColour, bool fill)
            {
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;

                RelativeSizeAxes = Axes.Both;
                RelativePositionAxes = Axes.Both;

                BorderColour = Color4.White;
                BorderThickness = 1;

                Alpha = 0.5f;

                Children = new[]
                {
                    new Box()
                    {
                        RelativeSizeAxes = Axes.Both,

                        Alpha = fill ? 0.5f : 0,
                        Colour = fillColour,

                        AlwaysPresent = true
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                ScaleTo(5f, 500);
                FadeOut(500);

                Expire();
            }
        }
    }
}