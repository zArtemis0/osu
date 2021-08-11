using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Mvis.BottomBar;
using osu.Game.Screens.Mvis.Plugins.Internal.BottomBar.Buttons;
using osu.Game.Screens.Mvis.Plugins.Types;
using osuTK;

namespace osu.Game.Screens.Mvis.Plugins.Internal.BottomBar
{
    internal class LegacyBottomBar : MvisPlugin, IFunctionBarProvider
    {
        protected override Drawable CreateContent()
        {
            throw new NotImplementedException();
        }

        protected override bool OnContentLoaded(Drawable content) => true;

        protected override bool PostInit() => true;

        public override int Version => 0;

        private readonly FillFlowContainer<BottomBarButton> leftContent;
        private readonly FillFlowContainer<BottomBarButton> centreContent;
        private readonly FillFlowContainer<BottomBarButton> rightContent;
        private readonly FillFlowContainer<BottomBarButton> pluginEntriesFillFlow;

        private readonly SongProgressBar progressBar;
        private readonly Container contentContainer;

        [Resolved]
        private MvisScreen mvisScreen { get; set; }

        public LegacyBottomBar()
        {
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                contentContainer = new Container
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    AutoSizeDuration = 300,
                    AutoSizeEasing = Easing.OutQuint,
                    Children = new Drawable[]
                    {
                        leftContent = new FillFlowContainer<BottomBarButton>
                        {
                            Name = "Left Container",
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            AutoSizeAxes = Axes.Both,
                            Spacing = new Vector2(5),
                            Margin = new MarginPadding { Left = 5, Bottom = 10 }
                        },
                        centreContent = new FillFlowContainer<BottomBarButton>
                        {
                            Name = "Centre Container",
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            AutoSizeAxes = Axes.Both,
                            Spacing = new Vector2(5),
                            Margin = new MarginPadding { Bottom = 10 }
                        },
                        rightContent = new FillFlowContainer<BottomBarButton>
                        {
                            Name = "Right Container",
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                            AutoSizeAxes = Axes.Both,
                            Spacing = new Vector2(5),
                            Margin = new MarginPadding { Right = 5, Bottom = 10 }
                        },
                        pluginEntriesFillFlow = new FillFlowContainer<BottomBarButton>
                        {
                            Name = "Plugin Entries Container",
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            AutoSizeAxes = Axes.Both,
                            AutoSizeDuration = 300,
                            AutoSizeEasing = Easing.OutQuint,
                            Spacing = new Vector2(5),
                            Margin = new MarginPadding { Bottom = 35 + 10 }
                        },
                    }
                },
                progressBar = new SongProgressBar()
            };
        }

        [BackgroundDependencyLoader]
        private void load(MvisScreen mvisScreen)
        {
            mvisScreen.OnIdle += Hide;
            mvisScreen.OnResumeFromIdle += Show;

            progressBar.OnSeek = mvisScreen.SeekTo;
        }

        protected override void Update()
        {
            progressBar.CurrentTime = mvisScreen.CurrentTrack.CurrentTime;
            progressBar.EndTime = mvisScreen.CurrentTrack.Length;
            base.Update();
        }

        public float GetSafeAreaPadding() => contentContainer.Height - contentContainer.Y;

        public override void Show()
        {
            contentContainer.MoveToY(0, 300, Easing.OutQuint);
            progressBar.MoveToY(0, 300, Easing.OutQuint);
        }

        public override void Hide()
        {
            contentContainer.MoveToY(40, 300, Easing.OutQuint);
            progressBar.MoveToY(4f, 300, Easing.OutQuint);
        }

        public bool OkForHide() => IsHovered;

        private void checkForPluginControls(IFunctionProvider provider)
        {
            if (provider is IPluginFunctionProvider pluginFunctionProvider)
                pluginButtons.Add(pluginFunctionProvider);
        }

        public bool AddFunctionControl(IFunctionProvider provider)
        {
            checkForPluginControls(provider);

            var button = provider is IToggleableFunctionProvider
                ? new BottomBarSwitchButton((IToggleableFunctionProvider)provider)
                : new BottomBarButton(provider);

            switch (provider.Type)
            {
                case FunctionType.Audio:
                    centreContent.Add(button);
                    break;

                case FunctionType.Base:
                    leftContent.Add(button);
                    break;

                case FunctionType.Misc:
                    rightContent.Add(button);
                    break;

                case FunctionType.Plugin:
                    pluginEntriesFillFlow.Add(button);
                    break;

                case FunctionType.ProgressDisplay:
                    button.Dispose();
                    centreContent.Add(new SongProgressButton(provider));
                    break;

                default:
                    throw new InvalidOperationException("???");
            }

            return true;
        }

        public bool AddFunctionControls(List<IFunctionProvider> providers)
        {
            foreach (var provider in providers)
            {
                AddFunctionControl(provider);
            }

            return true;
        }

        public bool SetFunctionControls(List<IFunctionProvider> providers)
        {
            leftContent.Clear();
            centreContent.Clear();
            rightContent.Clear();
            pluginEntriesFillFlow.Clear();

            return AddFunctionControls(providers);
        }

        private readonly List<IPluginFunctionProvider> pluginButtons = new List<IPluginFunctionProvider>();

        public void Remove(IFunctionProvider provider)
        {
            BottomBarButton target;

            switch (provider.Type)
            {
                case FunctionType.ProgressDisplay:
                case FunctionType.Audio:
                    target = centreContent.FirstOrDefault(b => b.Provider == provider);
                    break;

                case FunctionType.Base:
                    target = leftContent.FirstOrDefault(b => b.Provider == provider);
                    break;

                case FunctionType.Misc:
                    target = rightContent.FirstOrDefault(b => b.Provider == provider);
                    break;

                case FunctionType.Plugin:
                    target = pluginEntriesFillFlow.FirstOrDefault(b => b.Provider == provider);
                    break;

                default:
                    throw new InvalidOperationException("???");
            }

            if (target != null)
            {
                target.Expire();

                if (provider is IPluginFunctionProvider pluginFunctionProvider)
                    pluginButtons.Remove(pluginFunctionProvider);
            }
            else
                throw new ButtonNotFoundException(provider);
        }

        public void ShowFunctionControlTemporary() => pluginEntriesFillFlow.FadeIn(500, Easing.OutQuint).Then().Delay(2000).FadeOut(500, Easing.OutQuint);

        public List<IPluginFunctionProvider> GetAllPluginFunctionButton() => pluginButtons;

        public class ButtonNotFoundException : Exception
        {
            public ButtonNotFoundException(IFunctionProvider provider)
                : base($"无法找到与{provider.ToString()}对应的按钮")
            {
            }
        }
    }
}
