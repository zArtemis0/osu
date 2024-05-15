// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public partial class HitMarker : CompositeDrawable
    {
        public HitMarker(OsuAction? action)
        {
            var (colour, length, hasBorder) = getConfig(action);

            if (hasBorder)
            {
                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(3, length),
                        Colour = Colour4.Black.Opacity(0.5F)
                    },
                    new Box
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(3, length),
                        Rotation = 90,
                        Colour = Colour4.Black.Opacity(0.5F)
                    }
                };
            }

            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(1, length),
                    Colour = colour
                },
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(1, length),
                    Rotation = 90,
                    Colour = colour
                }
            });
        }

        private (Colour4 colour, float length, bool hasBorder) getConfig(OsuAction? action)
        {
            switch (action)
            {
                case OsuAction.LeftButton:
                    return (Colour4.Orange, 20, true);

                case OsuAction.RightButton:
                    return (Colour4.LightGreen, 20, true);

                default:
                    return (Colour4.Gray.Opacity(0.5F), 8, false);
            }
        }
    }
}
