﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using OpenTK;
using OpenTK.Graphics;
using System.Collections.Generic;
using System.Text;

namespace osu.Game.Overlays.Chat
{
    public class ChatLine : Container
    {
        public readonly Message Message;

        private static readonly Color4[] username_colours = {
            OsuColour.FromHex("588c7e"),
            OsuColour.FromHex("b2a367"),
            OsuColour.FromHex("c98f65"),
            OsuColour.FromHex("bc5151"),
            OsuColour.FromHex("5c8bd6"),
            OsuColour.FromHex("7f6ab7"),
            OsuColour.FromHex("a368ad"),
            OsuColour.FromHex("aa6880"),

            OsuColour.FromHex("6fad9b"),
            OsuColour.FromHex("f2e394"),
            OsuColour.FromHex("f2ae72"),
            OsuColour.FromHex("f98f8a"),
            OsuColour.FromHex("7daef4"),
            OsuColour.FromHex("a691f2"),
            OsuColour.FromHex("c894d3"),
            OsuColour.FromHex("d895b0"),

            OsuColour.FromHex("53c4a1"),
            OsuColour.FromHex("eace5c"),
            OsuColour.FromHex("ea8c47"),
            OsuColour.FromHex("fc4f4f"),
            OsuColour.FromHex("3d94ea"),
            OsuColour.FromHex("7760ea"),
            OsuColour.FromHex("af52c6"),
            OsuColour.FromHex("e25696"),

            OsuColour.FromHex("677c66"),
            OsuColour.FromHex("9b8732"),
            OsuColour.FromHex("8c5129"),
            OsuColour.FromHex("8c3030"),
            OsuColour.FromHex("1f5d91"),
            OsuColour.FromHex("4335a5"),
            OsuColour.FromHex("812a96"),
            OsuColour.FromHex("992861"),
        };

        private Color4 getUsernameColour(Message message)
        {
            if (!string.IsNullOrEmpty(message.Sender?.Colour))
                return OsuColour.FromHex(message.Sender.Colour);

            //todo: use User instead of Message when user_id is correctly populated.
            return username_colours[message.UserId % username_colours.Length];
        }

        public const float LEFT_PADDING = message_padding + padding * 2;

        private const float padding = 15;
        private const float message_padding = 200;
        private const float text_size = 20;

        public ChatLine(Message message)
        {
            Message = message;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding { Left = padding, Right = padding };

            TextFlowContainer textContainer;

            Children = new Drawable[]
            {
                new Container
                {
                    Size = new Vector2(message_padding, text_size),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = @"Exo2.0-SemiBold",
                            Text = $@"{Message.Timestamp.LocalDateTime:HH:mm:ss}",
                            FixedWidth = true,
                            TextSize = text_size * 0.75f,
                            Alpha = 0.4f,
                        },
                        new OsuSpriteText
                        {
                            Font = @"Exo2.0-BoldItalic",
                            Text = $@"{Message.Sender.Username}:",
                            Colour = getUsernameColour(Message),
                            TextSize = text_size,
                            Origin = Anchor.TopRight,
                            Anchor = Anchor.TopRight,
                        }
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Left = message_padding + padding },
                    Children = new Drawable[]
                    {
                        textContainer = new TextFlowContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                        }
                    }
                }
            };

            string toParse = message.Content;
            List<SplitMarker> markers = new List<SplitMarker>();
            markers.AddRange(parseSplitMarkers(ref toParse, "**", SplitTypes.Bold));
            markers.AddRange(parseSplitMarkers(ref toParse, "*", SplitTypes.Italic));
            markers.AddRange(parseSplitMarkers(ref toParse, "_", SplitTypes.Italic));

            // Add a sentinel marker for the end of the string such that the entire string is rendered
            // without requiring code duplication.
            markers.Add(new SplitMarker { Index = toParse.Length, Length = 0, Types = SplitTypes.None });

            // Sort markers from earliest to latest
            markers.Sort((a, b) => a.Index.CompareTo(b.Index));

            // Cut up string into parts according to all found markers
            int lastIndex = 0;
            SplitTypes splitTypes = SplitTypes.None;
            foreach (var marker in markers)
            {
                // We do not need to add empty strings if we have 2 consecutive markers
                if (lastIndex < marker.Index)
                {
                    string font = "Exo2.0-" + ((splitTypes & SplitTypes.Bold) > 0 ? "Bold" : "Regular") + ((splitTypes & SplitTypes.Italic) > 0 ? "Italic" : string.Empty);
                    textContainer.AddText(message.Content.Substring(lastIndex, marker.Index - lastIndex), spriteText =>
                    {
                        spriteText.TextSize = text_size;
                        spriteText.Font = font;
                    });
                }

                lastIndex = marker.Index + marker.Length;

                // Flip those types which the marker denotes.
                splitTypes ^= marker.Types;
            }
        }

        private enum SplitTypes
        {
            None = 0,
            Italic = 1 << 0,
            Bold = 1 << 1,
        }

        private struct SplitMarker
        {
            public int Index;
            public int Length;
            public SplitTypes Types;
        }

        private static List<SplitMarker> parseSplitMarkers(ref string toParse, string delimiter, SplitTypes types)
        {
            List<SplitMarker> escapeMarkers = new List<SplitMarker>();
            List<SplitMarker> delimiterMarkers = new List<SplitMarker>();

            // The output string will contain toParse with all successfully parsed
            // delimiters replaced by spaces.
            StringBuilder outputString = new StringBuilder(toParse);

            // For each char in toParse...
            for (int i = 0; i < toParse.Length; i++)
            {
                // ...check whether delimiter is matched char-by-char.
                for (int j = 0; j + i < toParse.Length && j < delimiter.Length; j++)
                {
                    if (toParse[j + i] != delimiter[j])
                        break;
                    else if (j == delimiter.Length - 1)
                    {
                        // Were we escaped? In this case put a marker skipping the escape character
                        if (i > 0 && toParse[i - 1] == '\\')
                            escapeMarkers.Add(new SplitMarker { Index = i - 1, Types = SplitTypes.None, Length = 1 });
                        else
                        {
                            delimiterMarkers.Add(new SplitMarker { Index = i, Types = types, Length = delimiter.Length });

                            // Replace parsed delimiter with spaces such that future delimiters which may be substrings
                            // do not parse a second time. One specific usecase are ** and * for markdown.
                            for (int k = i; k < i + delimiter.Length; ++k)
                                outputString[k] = ' ';

                            // Make sure we advance beyond the end of the discovered delimiter
                            i += delimiter.Length - 1;
                        }
                    }
                }
            }

            // Disregard trailing marker if we have an odd amount
            if (delimiterMarkers.Count % 2 == 1)
                delimiterMarkers.RemoveAt(delimiterMarkers.Count - 1);

            toParse = outputString.ToString();

            // Return a single list containing all markers
            escapeMarkers.AddRange(delimiterMarkers);
            return escapeMarkers;
        }
    }
}
