﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Framework.Graphics;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using System.Threading;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Overlays.Comments
{
    public class CommentsContainer : CompositeDrawable
    {
        private const int avatar_size = 50;

        private CommentableType? type;
        private long? id;

        public readonly Bindable<CommentsSortCriteria> Sort = new Bindable<CommentsSortCriteria>();
        public readonly BindableBool ShowDeleted = new BindableBool();

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        private GetCommentsRequest request;
        private CancellationTokenSource loadCancellation;
        private int currentPage;

        private readonly Box background;
        private readonly Box placeholderBackground;
        private readonly FillFlowContainer content;
        private readonly DeletedCommentsPlaceholder deletedCommentsPlaceholder;
        private readonly CommentsShowMoreButton moreButton;
        private readonly Container noCommentsPlaceholder;
        private readonly GlobalResponseContainer responseContainer;
        private readonly Container header;
        private readonly UpdateableAvatar avatar;

        public CommentsContainer()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            AddRangeInternal(new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        header = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding { Horizontal = 50, Vertical = 10 },
                            Alpha = 0,
                            Children = new Drawable[]
                            {
                                new GridContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    ColumnDimensions = new[]
                                    {
                                        new Dimension(GridSizeMode.AutoSize),
                                        new Dimension(),
                                    },
                                    RowDimensions = new[]
                                    {
                                        new Dimension(GridSizeMode.AutoSize)
                                    },
                                    Content = new[]
                                    {
                                        new Drawable[]
                                        {
                                            new Container
                                            {
                                                Size = new Vector2(avatar_size),
                                                Masking = true,
                                                CornerRadius = avatar_size / 2f,
                                                Children = new Drawable[]
                                                {
                                                    new Box
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        Colour = OsuColour.Gray(0.2f)
                                                    },
                                                    avatar = new UpdateableAvatar
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                    },
                                                }
                                            },
                                            new Container
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Padding = new MarginPadding { Left = 10 },
                                                Child = responseContainer = new GlobalResponseContainer(),
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new CommentsHeader
                        {
                            Sort = { BindTarget = Sort },
                            ShowDeleted = { BindTarget = ShowDeleted }
                        },
                        noCommentsPlaceholder = new Container
                        {
                            Height = 80,
                            RelativeSizeAxes = Axes.X,
                            Alpha = 0,
                            Children = new Drawable[]
                            {
                                placeholderBackground = new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                new SpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Margin = new MarginPadding { Left = 50 },
                                    Text = @"No comments yet."
                                }
                            }
                        },
                        content = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = OsuColour.Gray(0.2f)
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        deletedCommentsPlaceholder = new DeletedCommentsPlaceholder
                                        {
                                            ShowDeleted = { BindTarget = ShowDeleted }
                                        },
                                        new Container
                                        {
                                            AutoSizeAxes = Axes.Y,
                                            RelativeSizeAxes = Axes.X,
                                            Child = moreButton = new CommentsShowMoreButton
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Margin = new MarginPadding(5),
                                                Action = getComments,
                                                IsLoading = true,
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = colours.Gray2;
            placeholderBackground.Colour = colours.Gray3;

            avatar.User = api.LocalUser.Value;
        }

        protected override void LoadComplete()
        {
            Sort.BindValueChanged(_ =>
            {
                if (!type.HasValue || !id.HasValue)
                    return;

                ShowComments(type.Value, id.Value, false);
            });
            base.LoadComplete();
        }

        public void ShowComments(CommentableType type, long id, bool hideHeader = true)
        {
            this.type = type;
            this.id = id;
            clearComments(hideHeader);
            getComments();
        }

        private void getComments()
        {
            moreButton.IsLoading = true;
            moreButton.Show();
            request?.Cancel();
            loadCancellation?.Cancel();

            if (!type.HasValue || !id.HasValue)
                return;

            request = new GetCommentsRequest(type.Value, id.Value, Sort.Value, currentPage++);
            request.Success += onSuccess;
            api.Queue(request);
        }

        private void clearComments(bool hideHeader)
        {
            if (hideHeader)
                header.Hide();

            currentPage = 1;
            deletedCommentsPlaceholder.DeletedCount.Value = 0;
            noCommentsPlaceholder.Hide();
            content.Clear();
        }

        private void onSuccess(CommentBundle response)
        {
            if (!response.Comments.Any())
            {
                noCommentsPlaceholder.Show();
                moreButton.IsLoading = false;
                moreButton.Hide();
                return;
            }

            loadCancellation = new CancellationTokenSource();

            FillFlowContainer page = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
            };

            foreach (var c in response.Comments)
            {
                if (c.IsTopLevel)
                    page.Add(new DrawableComment(c)
                    {
                        ShowDeleted = { BindTarget = ShowDeleted },
                        Sort = { BindTarget = Sort }
                    });
            }

            LoadComponentAsync(page, loaded =>
            {
                content.Add(loaded);

                deletedCommentsPlaceholder.DeletedCount.Value += response.Comments.Count(c => c.IsDeleted && c.IsTopLevel);

                if (response.HasMore)
                {
                    int loadedTopLevelComments = 0;
                    content.Children.OfType<FillFlowContainer>().ForEach(p => loadedTopLevelComments += p.Children.OfType<DrawableComment>().Count());

                    moreButton.Current.Value = response.TopLevelCount - loadedTopLevelComments;
                    moreButton.IsLoading = false;
                }
                else
                {
                    moreButton.Hide();
                }

                responseContainer.SetParameters(type.Value, id.Value);
                header.Show();

            }, loadCancellation.Token);
        }

        protected override void Dispose(bool isDisposing)
        {
            request?.Cancel();
            loadCancellation?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
