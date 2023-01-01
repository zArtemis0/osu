// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Database;
using osu.Game.Input.Bindings;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Framework.Input.Bindings;

namespace osu.Game.Input
{
    public partial class InvalidRulesetKeyBindingHandler : Component
    {
        [Resolved]
        private RealmAccess realm { get; set; }

        public Action<Notification> PostNotification { get; set; }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesetStore, RealmAccess realm)
        {
            foreach (var rulesetInfo in rulesetStore.AvailableRulesets)
            {
                var rulesetInstance = rulesetInfo.CreateInstance();

                foreach (int variant in rulesetInstance.AvailableVariants)
                {
                    var defaults = rulesetInstance.GetDefaultKeyBindings(variant).OrderBy(d => d.Action).ToList();

                    var bindings = realm.Run(r => r.All<RealmKeyBinding>()
                                                   .Where(b => b.RulesetName == rulesetInfo.ShortName && b.Variant == variant)
                                                   .Detach());

                    var bindingsWithoutUnassigned = bindings.Detach();
                    bindingsWithoutUnassigned.RemoveAll(b => b.KeyCombinationString == "0");

                    int keyBindingsUniqueCount = bindingsWithoutUnassigned.Select(b => b.KeyCombination).Distinct().Count();
                    string variantName = rulesetInstance.GetVariantName(variant).ToString();

                    if (bindingsWithoutUnassigned.Count == keyBindingsUniqueCount)
                        continue;

                    post(rulesetInfo.Name, variantName);

                    int i = 0;

                    foreach (var defaultKeyCombination in defaults.Select(d => d.KeyCombination))
                        setBindingsToDefault(bindings[i++], defaultKeyCombination);
                }
            }
        }

        private void setBindingsToDefault(RealmKeyBinding binding, KeyCombination defaultKeyCombination)
        {
            binding.KeyCombination = defaultKeyCombination;
            realm.WriteAsync(r => r.Find<RealmKeyBinding>(binding.ID).KeyCombinationString = binding.KeyCombinationString);
        }

        private void post(string rulesetName, string variantName)
        {
            variantName = string.IsNullOrEmpty(variantName) ? "" : variantName + " ";

            Schedule(() => PostNotification?.Invoke(new SimpleErrorNotification
            {
                Icon = FontAwesome.Solid.ExclamationCircle,
                Text = $"Key bindings for {rulesetName} {variantName}is set to default.",
            }));
        }
    }
}