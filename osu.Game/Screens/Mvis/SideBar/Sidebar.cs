using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Mvis.SideBar.Header;
using osu.Game.Screens.Mvis.Skinning;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Mvis.SideBar
{
    public class Sidebar : VisibilityContainer
    {
        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        private readonly List<ISidebarContent> components = new List<ISidebarContent>();
        private readonly TabHeader header;
        private const float duration = 400;
        private HeaderTabItem prevTab;
        private SampleChannel popInSample;
        private SampleChannel popOutSample;
        private bool playPopoutSample;

        [CanBeNull]
        private Box sidebarBg;

        public bool IsHidden = true;
        public bool Hiding;
        public Bindable<Drawable> CurrentDisplay = new Bindable<Drawable>();

        private readonly Container<Drawable> contentContainer;
        protected override Container<Drawable> Content => contentContainer;

        public Sidebar()
        {
            Anchor = Anchor.BottomRight;
            Origin = Anchor.BottomRight;
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(0.3f, 1f);

            InternalChildren = new Drawable[]
            {
                header = new TabHeader
                {
                    Depth = float.MinValue
                },
                contentContainer = new Container
                {
                    Name = "Content",
                    RelativeSizeAxes = Axes.Both,
                    Masking = true
                },
                new Footer.Footer
                {
                    Depth = float.MinValue
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            AddInternal(new SkinnableComponent(
                "MSidebar-background",
                confineMode: ConfineMode.ScaleToFill,
                masking: true,
                defaultImplementation: _ => sidebarBg = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5,
                    Alpha = 0.5f,
                    Depth = float.MaxValue
                })
            {
                Name = "侧边栏背景",
                Depth = float.MaxValue,
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                ChildAnchor = Anchor.BottomRight,
                ChildOrigin = Anchor.BottomRight,
                RelativeSizeAxes = Axes.Both,
                CentreComponent = false,
                OverrideChildAnchor = true,
            });

            popInSample = audio.Samples.Get(@"UI/overlay-pop-in");
            popOutSample = audio.Samples.Get(@"UI/overlay-pop-out");

            colourProvider.HueColour.BindValueChanged(_ =>
            {
                sidebarBg?.FadeColour(colourProvider.Background5);
            }, true);
        }

        protected override void LoadComplete()
        {
            CurrentDisplay.BindValueChanged(onCurrentDisplayChanged);
            base.LoadComplete();
        }

        private void onCurrentDisplayChanged(ValueChangedEvent<Drawable> v)
        {
            if (!(v.NewValue is ISidebarContent)) return;

            var sc = (ISidebarContent)v.NewValue;
            prevTab?.MakeInActive();

            foreach (var t in header.Tabs)
            {
                if (t.Value == sc)
                {
                    t.MakeActive();
                    prevTab = t;
                    break;
                }
            }
        }

        protected override void UpdateAfterChildren()
        {
            contentContainer.Padding = new MarginPadding { Top = header.Height + header.DrawPosition.Y, Bottom = 50 };
            base.UpdateAfterChildren();
        }

        public void ShowComponent(Drawable d)
        {
            if (!(d is ISidebarContent))
                throw new InvalidOperationException($"{d}不是{typeof(ISidebarContent)}");

            var c = (ISidebarContent)d;
            if (!components.Contains(c))
                throw new InvalidOperationException($"组件不包含{c}");

            if (c.ResizeWidth < 0.3f || c.ResizeHeight < 0.3f)
                throw new InvalidOperationException("组件过小");

            Show();

            //如果要显示的是当前正在显示的内容，则中断
            if (CurrentDisplay.Value == d)
            {
                IsHidden = false;
                return;
            }

            var resizeDuration = IsHidden ? 0 : duration;

            CurrentDisplay.Value?.FadeOut(resizeDuration / 2, Easing.OutQuint);

            CurrentDisplay.Value = d;

            d.Delay(resizeDuration / 2).FadeIn(resizeDuration / 2);

            this.ResizeTo(new Vector2(c.ResizeWidth, c.ResizeHeight), resizeDuration, Easing.OutQuint);
            IsHidden = false;
        }

        private void addDrawableToList(Drawable d)
        {
            if (d is ISidebarContent s)
            {
                d.Alpha = 0;
                components.Add(s);
                contentContainer.Add(d);
                header.Tabs.Add(new HeaderTabItem(s)
                {
                    Action = () => ShowComponent(d)
                });
            }
        }

        public override void Add(Drawable drawable) => addDrawableToList(drawable);

        public override void Clear(bool disposeChildren)
        {
            header.Tabs.Clear(disposeChildren);
            contentContainer.Clear(disposeChildren);
        }

        public override bool Remove(Drawable drawable)
        {
            if (drawable is ISidebarContent sc)
            {
                foreach (var t in header.Tabs)
                {
                    if (t.Value == sc)
                    {
                        header.Tabs.Remove(t);
                        break;
                    }
                }
            }

            return base.Remove(drawable);
        }

        protected override void PopOut()
        {
            if (playPopoutSample)
                popOutSample?.Play();

            this.MoveToX(100, 600, Easing.OutQuint)
                .FadeOut(600 * 0.6f, Easing.OutExpo)
                .OnComplete(_ => IsHidden = true);
            contentContainer.FadeOut(WaveContainer.DISAPPEAR_DURATION, Easing.OutQuint);

            Hiding = true;
            playPopoutSample = true;
        }

        protected override void PopIn()
        {
            popInSample?.Play();

            this.MoveToX(0, 600, Easing.OutQuint)
                .FadeIn(600 * 0.6f, Easing.OutExpo);
            contentContainer.FadeIn(WaveContainer.APPEAR_DURATION, Easing.OutQuint);

            Hiding = false;
        }
    }
}
