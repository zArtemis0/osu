﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Framework.Platform;
using osu.Game.Beatmaps.IO;

namespace osu.Game.Skins
{
    public class Skin
    {
        public SkinInfo Info { get; private set; }
        public TextureStore Textures { get; private set; }

        private ArchiveReader skinsStore;
        private Storage storage;

        public Skin(SkinInfo info)
        {
            Info = info;
        }

        public void UpdateSkin() {
            Textures = new TextureStore();
            if (Info.Name != SkinManager.DefaultSkin.Name)
            {
                Textures.AddStore(new RawTextureLoaderStore(skinsStore));
            }
            // TODO update audio component
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host) {
            storage = host.Storage;
            skinsStore = ArchiveReader.GetReader(storage, Info.Path);
            UpdateSkin();
        }
    }

    public class SkinInfo
    {
        public string Name;
        public string Path;
    }
}
