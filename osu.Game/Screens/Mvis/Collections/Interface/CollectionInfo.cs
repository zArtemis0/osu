using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Mvis.Skinning;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Mvis.Collections.Interface
{
    public class CollectionInfo : CompositeDrawable
    {
        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        private Container flashBox;
        private OsuSpriteText collectionName;
        private OsuSpriteText collectionBeatmapCount;
        private readonly Bindable<BeatmapCollection> collection = new Bindable<BeatmapCollection>();
        private readonly List<BeatmapSetInfo> beatmapSets = new List<BeatmapSetInfo>();

        [CanBeNull]
        private BeatmapCover cover;

        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        private BeatmapList beatmapList;
        private readonly BindableBool isCurrentCollection = new BindableBool();

        public CollectionInfo()
        {
            RelativeSizeAxes = Axes.Both;
            Masking = true;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new SkinnableComponent(
                    "MSidebar-Collection-background",
                    confineMode: ConfineMode.ScaleToFill,
                    defaultImplementation: _ => bgBox = new Box
                    {
                        Colour = colourProvider.Background3,
                        RelativeSizeAxes = Axes.Both
                    })
                {
                    Name = "收藏夹背景",
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    ChildAnchor = Anchor.BottomRight,
                    ChildOrigin = Anchor.BottomRight,
                    RelativeSizeAxes = Axes.Both,
                    CentreComponent = false,
                    OverrideChildAnchor = true,
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                Name = "标题容器",
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                AutoSizeDuration = 300,
                                AutoSizeEasing = Easing.OutQuint,
                                Masking = true,
                                Children = new Drawable[]
                                {
                                    new SkinnableComponent(
                                        "transparent",
                                        confineMode: ConfineMode.ScaleToFill,
                                        masking: true,
                                        defaultImplementation: _ => cover = createCover())
                                    {
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                        ChildAnchor = Anchor.TopRight,
                                        ChildOrigin = Anchor.TopRight,
                                        RelativeSizeAxes = Axes.Both,
                                        CentreComponent = false,
                                        OverrideChildAnchor = true,
                                    },
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(12),
                                        Padding = new MarginPadding { Horizontal = 35, Vertical = 25 },
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Children = new Drawable[]
                                        {
                                            collectionName = new OsuSpriteText
                                            {
                                                Font = OsuFont.GetFont(size: 50),
                                                RelativeSizeAxes = Axes.X,
                                                Text = "未选择收藏夹",
                                                Truncate = true,
                                            },
                                            collectionBeatmapCount = new OsuSpriteText
                                            {
                                                Font = OsuFont.GetFont(size: 38),
                                                RelativeSizeAxes = Axes.X,
                                                Text = "请先选择一个收藏夹!"
                                            }
                                        }
                                    },
                                    flashBox = new Container
                                    {
                                        AutoSizeAxes = Axes.Y,
                                        RelativeSizeAxes = Axes.X,
                                        Y = -1,
                                        Children = new Drawable[]
                                        {
                                            new Box
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Height = 14, //8 + 5 + 1, 8是基础高度，5是额外高度，1是避免一些奇怪的渲染问题
                                                Alpha = 0.6f
                                            },
                                            new Box
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Height = 9 // 8 + 1
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    listContainer = new Container
                                    {
                                        RelativeSizeAxes = Axes.Both
                                    },
                                    loadingSpinner = new LoadingSpinner(true)
                                    {
                                        Size = new Vector2(50)
                                    }
                                }
                            },
                        }
                    }
                },
            };
        }

        private BeatmapCover createCover()
        {
            var c = new BeatmapCover(beatmaps.GetWorkingBeatmap(beatmapSets.FirstOrDefault()?.Beatmaps.First()))
            {
                BackgroundBox = false,
                TimeBeforeWrapperLoad = 0,
                Colour = ColourInfo.GradientVertical(
                    Colour4.LightGray,
                    Colour4.LightGray.Opacity(0)
                )
            };
            return c;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            colourProvider.HueColour.BindValueChanged(_ =>
            {
                bgBox?.FadeColour(colourProvider.Background3);

                flashBox.Colour = isCurrentCollection.Value ? colourProvider.Highlight1 : colourProvider.Light1;
            }, true);
            collection.BindValueChanged(OnCollectionChanged);
        }

        private void OnCollectionChanged(ValueChangedEvent<BeatmapCollection> v)
        {
            var c = v.NewValue;

            if (c == null)
            {
                clearInfo();
                return;
            }

            beatmapSets.Clear();

            //From CollectionHelper.cs
            foreach (var item in c.Beatmaps)
            {
                //获取当前BeatmapSet
                var currentSet = item.BeatmapSet;

                //进行比对，如果beatmapList中不存在，则添加。
                if (!beatmapSets.Contains(currentSet))
                    beatmapSets.Add(currentSet);
            }

            collectionName.Text = c.Name.Value;
            collectionBeatmapCount.Text = $"{beatmapSets.Count}首歌曲";

            cover?.UpdateBackground(beatmaps.GetWorkingBeatmap(beatmapSets.FirstOrDefault()?.Beatmaps.First()));
            flashBox.FlashColour(Colour4.White, 1000, Easing.OutQuint);

            refreshBeatmapSetList();
        }

        private CancellationTokenSource refreshTaskCancellationToken;
        private Container listContainer;
        private LoadingSpinner loadingSpinner;

        [CanBeNull]
        private Box bgBox;

        private void refreshBeatmapSetList()
        {
            Task refreshTask;
            refreshTaskCancellationToken?.Cancel();
            refreshTaskCancellationToken = new CancellationTokenSource();

            beatmapList?.FadeOut(250).Then().Expire();
            loadingSpinner.Show();

            Task.Run(async () =>
            {
                refreshTask = Task.Run(() =>
                {
                    LoadComponentAsync(new BeatmapList(beatmapSets), newList =>
                    {
                        newList.IsCurrent.BindTo(isCurrentCollection);
                        beatmapList = newList;

                        listContainer.Add(newList);
                        newList.Show();
                        loadingSpinner.Hide();
                    }, refreshTaskCancellationToken.Token);
                }, refreshTaskCancellationToken.Token);

                await refreshTask;
            });
        }

        public void UpdateCollection(BeatmapCollection collection, bool isCurrent)
        {
            flashBox.FadeColour(isCurrent
                ? colourProvider.Highlight1
                : colourProvider.Light2, 300, Easing.OutQuint);

            if (collection != this.collection.Value && beatmapList != null)
            {
                beatmapList.IsCurrent.UnbindAll();
                beatmapList.IsCurrent.Value = false;
            }

            //设置当前选择是否为正在播放的收藏夹
            isCurrentCollection.Value = isCurrent;

            //将当前收藏夹设为collection
            this.collection.Value = collection;
        }

        private void clearInfo()
        {
            cover?.UpdateBackground(null);

            beatmapSets.Clear();
            beatmapList.ClearList();
            collectionName.Text = "未选择收藏夹";
            collectionBeatmapCount.Text = "请先选择一个收藏夹!";

            flashBox.FadeColour(colourProvider.Light2);
        }
    }
}
