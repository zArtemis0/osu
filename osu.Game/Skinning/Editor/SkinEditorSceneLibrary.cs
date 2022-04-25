// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Skinning.Editor
{
    public class SkinEditorSceneLibrary : CompositeDrawable
    {
        public const float BUTTON_HEIGHT = 40;

        private const float padding = 10;

        [Resolved(canBeNull: true)]
        private IPerformFromScreenRunner performer { get; set; }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

        [Resolved]
        private Bindable<IReadOnlyList<Mod>> mods { get; set; }

        public SkinEditorSceneLibrary()
        {
            Height = BUTTON_HEIGHT + padding * 2;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider overlayColourProvider)
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = overlayColourProvider.Background6,
                },
                new OsuScrollContainer(Direction.Horizontal)
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            Name = "Scene library",
                            AutoSizeAxes = Axes.X,
                            RelativeSizeAxes = Axes.Y,
                            Spacing = new Vector2(padding),
                            Padding = new MarginPadding(padding),
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = "可用界面",
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Margin = new MarginPadding(10),
                                },
                                new SceneButton
                                {
                                    Text = "歌曲选择",
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Action = () => performer?.PerformFromScreen(screen =>
                                    {
                                        if (screen is SongSelect)
                                            return;

                                        screen.Push(new PlaySongSelect());
                                    }, new[] { typeof(SongSelect) })
                                },
                                new SceneButton
                                {
                                    Text = "游戏内",
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Action = () => performer?.PerformFromScreen(screen =>
                                    {
                                        if (screen is Player)
                                            return;

                                        var replayGeneratingMod = ruleset.Value.CreateInstance().GetAutoplayMod();

                                        if (!ModUtils.CheckCompatibleSet(mods.Value.Append(replayGeneratingMod), out var invalid))
                                            mods.Value = mods.Value.Except(invalid).ToArray();

                                        if (replayGeneratingMod != null)
                                            screen.Push(new PlayerLoader(() => new ReplayPlayer((beatmap, mods) => replayGeneratingMod.CreateScoreFromReplayData(beatmap, mods))));
                                    }, new[] { typeof(Player), typeof(SongSelect) })
                                },
                            }
                        },
                    }
                }
            };
        }

        public class SceneButton : OsuButton
        {
            public SceneButton()
            {
                Width = 100;
                Height = BUTTON_HEIGHT;
            }

            [BackgroundDependencyLoader(true)]
            private void load([CanBeNull] OverlayColourProvider overlayColourProvider, OsuColour colours)
            {
                BackgroundColour = overlayColourProvider?.Background3 ?? colours.Blue3;
                Content.CornerRadius = 5;
            }
        }
    }
}
