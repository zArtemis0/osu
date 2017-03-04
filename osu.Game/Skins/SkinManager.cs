﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Platform;
using osu.Game.Configuration;

namespace osu.Game.Skins
{
    public class SkinManager
    {
        public static SkinInfo DEFAULT_SKIN = new SkinInfo {
            Name = @"default",
        };

        private Bindable<SkinInfo> bindable;

        private List<SkinInfo> skins;
        private List<Skin> skinContents;

        private SkinInfo selected;
        private Skin selectedContents;

        public SkinManager()
        {
            skins = new List<SkinInfo>();
            skinContents = new List<Skin>();
        }

        public List<KeyValuePair<string, SkinInfo>> UpdateItems() {
            var list = new List<KeyValuePair<string, SkinInfo>>();
            foreach (SkinInfo skin in skins)
            {
                list.Add(new KeyValuePair<string, SkinInfo>(skin.Name, skin));
            }
            return list;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            bindable = config.GetBindable<SkinInfo>(OsuConfig.Skin);
            bindable.ValueChanged += ChangedSkin;
            skins.Add(DEFAULT_SKIN);
            // TODO load skins
        }

        private Skin GetSkinContents(SkinInfo skin) => skinContents.Find((x) => (x.info.Path == skin.Path));

        private void ChangedSkin(object sender, EventArgs e) {
            foreach (SkinInfo skin in skins) {
                if (skin.Path == bindable.Value.Path)
                    selected = skin;
            }
            selectedContents = GetSkinContents(selected);
        }
    }
}
