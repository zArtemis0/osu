﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Mods;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Tests.Mods;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneModSelectOverlay : OsuManualInputManagerTestScene
    {
        protected override bool UseFreshStoragePerRun => true;

        private RulesetStore rulesetStore = null!;

        private TestModSelectOverlay modSelectOverlay = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(rulesetStore = new RealmRulesetStore(Realm));
            Dependencies.Cache(Realm);
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("clear contents", Clear);
            AddStep("reset ruleset", () => Ruleset.Value = rulesetStore.GetRuleset(0));
            AddStep("reset mods", () => SelectedMods.SetDefault());
            AddStep("set up presets", () =>
            {
                Realm.Write(r =>
                {
                    r.RemoveAll<ModPreset>();
                    r.Add(new ModPreset
                    {
                        Name = "AR0",
                        Description = "Too... many... circles...",
                        Ruleset = r.Find<RulesetInfo>(OsuRuleset.SHORT_NAME),
                        Mods = new[]
                        {
                            new OsuModDifficultyAdjust
                            {
                                ApproachRate = { Value = 0 }
                            }
                        }
                    });
                });
            });
        }

        private void createScreen()
        {
            AddStep("create screen", () => Child = modSelectOverlay = new TestModSelectOverlay
            {
                RelativeSizeAxes = Axes.Both,
                State = { Value = Visibility.Visible },
                SelectedMods = { BindTarget = SelectedMods }
            });
            waitForColumnLoad();
        }

        [Test]
        public void TestStateChange()
        {
            createScreen();
            AddStep("toggle state", () => modSelectOverlay.ToggleVisibility());
        }

        /// <summary>
        /// This is emulating a failure which can happen when entering and exiting the editor while on a convert ruleset with convert-specific mods applied.
        /// It involves the decoupled bindables in song select and also leasing, which makes it a bit fiddly to reproduce here.
        /// </summary>
        [Test]
        public void TestButtonStatesAfterLeasedRulesetChange()
        {
            LeasedBindable<IReadOnlyList<Mod>> lease = null!;

            createScreen();

            changeRuleset(1);

            AddStep("select mod", () => SelectedMods.Value = new[] { new TaikoModSwap() });
            AddAssert("swap mod button is active", () => modSelectOverlay.ChildrenOfType<ModPanel>().Single(panel => panel.Mod is TaikoModSwap).Active.Value, () => Is.True);

            // This stops OsuGameBase from deselecting incompatible mods on ruleset change.
            // Similar to what happens when entering the editor (due to lease logic).
            AddStep("Take out lease", () => lease = SelectedMods.BeginLease(true));

            // This is done on leaving song select.
            AddStep("Unbind from selected mods", () => modSelectOverlay.SelectedMods.UnbindFrom(SelectedMods));

            changeRuleset(0);

            // Lease will mean that OsuGameBase doesn't reset selected mods, even though they don't match compatibility.
            // but this is done via EditorLoader.
            AddStep("Clear mods", () => lease.Value = Array.Empty<Mod>());

            AddAssert("game selected mods empty", () => SelectedMods.Value.Count, () => Is.EqualTo(0));
            AddAssert("mod select selected mods unchanged (not bound)", () => modSelectOverlay.SelectedMods.Value.Count, () => Is.EqualTo(1));

            // but the button won't exist as it's not in available mods.
            AddAssert("swap mod button not present", () => modSelectOverlay.ChildrenOfType<ModPanel>().Any(panel => panel.Mod is TaikoModSwap), () => Is.False);

            AddStep("Return lease", () => lease.Return());
            changeRuleset(1);

            AddStep("Rebind to selected mods", () => modSelectOverlay.SelectedMods.UnbindFrom(SelectedMods));

            AddAssert("selected mods not empty", () => SelectedMods.Value.Count, () => Is.EqualTo(1));
            AddAssert("swap mod button is still active", () => modSelectOverlay.ChildrenOfType<ModPanel>().Single(panel => panel.Mod is TaikoModSwap).Active.Value, () => Is.True);
        }

        [Test]
        public void TestPreexistingSelection()
        {
            AddStep("set mods", () => SelectedMods.Value = new Mod[] { new OsuModAlternate(), new OsuModDaycore() });
            createScreen();
            AddUntilStep("two panels active", () => modSelectOverlay.ChildrenOfType<ModPanel>().Count(panel => panel.Active.Value) == 2);
            AddAssert("mod multiplier correct", () =>
            {
                double multiplier = SelectedMods.Value.Aggregate(1d, (m, mod) => m * mod.ScoreMultiplier);
                return Precision.AlmostEquals(multiplier, modSelectOverlay.ChildrenOfType<DifficultyMultiplierDisplay>().Single().Current.Value);
            });
            assertCustomisationToggleState(disabled: false, active: false);
            AddAssert("setting items created", () => modSelectOverlay.ChildrenOfType<ISettingsItem>().Any());
        }

        [Test]
        public void TestExternalSelection()
        {
            createScreen();
            AddStep("set mods", () => SelectedMods.Value = new Mod[] { new OsuModAlternate(), new OsuModDaycore() });
            AddUntilStep("two panels active", () => modSelectOverlay.ChildrenOfType<ModPanel>().Count(panel => panel.Active.Value) == 2);
            AddAssert("mod multiplier correct", () =>
            {
                double multiplier = SelectedMods.Value.Aggregate(1d, (m, mod) => m * mod.ScoreMultiplier);
                return Precision.AlmostEquals(multiplier, modSelectOverlay.ChildrenOfType<DifficultyMultiplierDisplay>().Single().Current.Value);
            });
            assertCustomisationToggleState(disabled: false, active: false);
            AddAssert("setting items created", () => modSelectOverlay.ChildrenOfType<ISettingsItem>().Any());
        }

        [Test]
        public void TestRulesetChange()
        {
            createScreen();
            changeRuleset(0);
            changeRuleset(1);
            changeRuleset(2);
            changeRuleset(3);
        }

        [Test]
        public void TestIncompatibilityToggling()
        {
            createScreen();
            changeRuleset(0);

            AddStep("activate DT", () => getPanelForMod(typeof(OsuModDoubleTime)).TriggerClick());
            AddAssert("DT active", () => SelectedMods.Value.Single().GetType() == typeof(OsuModDoubleTime));
            AddAssert("DT panel active", () => getPanelForMod(typeof(OsuModDoubleTime)).Active.Value);

            AddStep("activate NC", () => getPanelForMod(typeof(OsuModNightcore)).TriggerClick());
            AddAssert("only NC active", () => SelectedMods.Value.Single().GetType() == typeof(OsuModNightcore));
            AddAssert("DT panel not active", () => !getPanelForMod(typeof(OsuModDoubleTime)).Active.Value);
            AddAssert("NC panel active", () => getPanelForMod(typeof(OsuModNightcore)).Active.Value);

            AddStep("activate HR", () => getPanelForMod(typeof(OsuModHardRock)).TriggerClick());
            AddAssert("NC+HR active", () => SelectedMods.Value.Any(mod => mod.GetType() == typeof(OsuModNightcore))
                                            && SelectedMods.Value.Any(mod => mod.GetType() == typeof(OsuModHardRock)));
            AddAssert("NC panel active", () => getPanelForMod(typeof(OsuModNightcore)).Active.Value);
            AddAssert("HR panel active", () => getPanelForMod(typeof(OsuModHardRock)).Active.Value);

            AddStep("activate MR", () => getPanelForMod(typeof(OsuModMirror)).TriggerClick());
            AddAssert("NC+MR active", () => SelectedMods.Value.Any(mod => mod.GetType() == typeof(OsuModNightcore))
                                            && SelectedMods.Value.Any(mod => mod.GetType() == typeof(OsuModMirror)));
            AddAssert("NC panel active", () => getPanelForMod(typeof(OsuModNightcore)).Active.Value);
            AddAssert("HR panel not active", () => !getPanelForMod(typeof(OsuModHardRock)).Active.Value);
            AddAssert("MR panel active", () => getPanelForMod(typeof(OsuModMirror)).Active.Value);
        }

        [Test]
        public void TestDimmedState()
        {
            createScreen();
            changeRuleset(0);

            AddUntilStep("any column dimmed", () => this.ChildrenOfType<ModColumn>().Any(column => !column.Active.Value));

            ModSelectColumn lastColumn = null!;

            AddAssert("last column dimmed", () => !this.ChildrenOfType<ModColumn>().Last().Active.Value);
            AddStep("request scroll to last column", () =>
            {
                var lastDimContainer = this.ChildrenOfType<ModSelectOverlay.ColumnDimContainer>().Last();
                lastColumn = lastDimContainer.Column;
                lastDimContainer.RequestScroll?.Invoke(lastDimContainer);
            });
            AddUntilStep("column undimmed", () => lastColumn.Active.Value);

            AddStep("click panel", () =>
            {
                InputManager.MoveMouseTo(lastColumn.ChildrenOfType<ModPanel>().First());
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("panel selected", () => lastColumn.ChildrenOfType<ModPanel>().First().Active.Value);
        }

        [Test]
        public void TestCustomisationToggleState()
        {
            createScreen();
            assertCustomisationToggleState(disabled: true, active: false);

            AddStep("select customisable mod", () => SelectedMods.Value = new[] { new OsuModDoubleTime() });
            assertCustomisationToggleState(disabled: false, active: false);

            AddStep("select mod requiring configuration externally", () => SelectedMods.Value = new[] { new OsuModDifficultyAdjust() });
            assertCustomisationToggleState(disabled: false, active: false);

            AddStep("reset mods", () => SelectedMods.SetDefault());
            AddStep("select difficulty adjust via panel", () => getPanelForMod(typeof(OsuModDifficultyAdjust)).TriggerClick());
            assertCustomisationToggleState(disabled: false, active: true);

            AddStep("dismiss mod customisation via toggle", () =>
            {
                InputManager.MoveMouseTo(modSelectOverlay.CustomisationButton);
                InputManager.Click(MouseButton.Left);
            });
            assertCustomisationToggleState(disabled: false, active: false);

            AddStep("reset mods", () => SelectedMods.SetDefault());
            AddStep("select difficulty adjust via panel", () => getPanelForMod(typeof(OsuModDifficultyAdjust)).TriggerClick());
            assertCustomisationToggleState(disabled: false, active: true);

            AddStep("dismiss mod customisation via keyboard", () => InputManager.Key(Key.Escape));
            assertCustomisationToggleState(disabled: false, active: false);

            AddStep("append another mod not requiring config", () => SelectedMods.Value = SelectedMods.Value.Append(new OsuModFlashlight()).ToArray());
            assertCustomisationToggleState(disabled: false, active: false);

            AddStep("select mod without configuration", () => SelectedMods.Value = new[] { new OsuModAutoplay() });
            assertCustomisationToggleState(disabled: true, active: false);

            AddStep("select difficulty adjust via panel", () => getPanelForMod(typeof(OsuModDifficultyAdjust)).TriggerClick());
            assertCustomisationToggleState(disabled: false, active: true);

            AddStep("select mod without configuration", () => SelectedMods.Value = new[] { new OsuModAutoplay() });
            assertCustomisationToggleState(disabled: true, active: false); // config was dismissed without explicit user action.

            AddStep("select mod preset with mod requiring configuration", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<ModPresetPanel>().First());
                InputManager.Click(MouseButton.Left);
            });
            assertCustomisationToggleState(disabled: false, active: false);
        }

        [Test]
        public void TestDismissCustomisationViaDimmedArea()
        {
            createScreen();
            assertCustomisationToggleState(disabled: true, active: false);

            AddStep("select difficulty adjust via panel", () => getPanelForMod(typeof(OsuModDifficultyAdjust)).TriggerClick());
            assertCustomisationToggleState(disabled: false, active: true);

            AddStep("move mouse to settings area", () => InputManager.MoveMouseTo(this.ChildrenOfType<ModSettingsArea>().Single()));
            AddStep("move mouse to dimmed area", () =>
            {
                InputManager.MoveMouseTo(new Vector2(
                    modSelectOverlay.ScreenSpaceDrawQuad.TopLeft.X,
                    (modSelectOverlay.ScreenSpaceDrawQuad.TopLeft.Y + modSelectOverlay.ScreenSpaceDrawQuad.BottomLeft.Y) / 2));
            });
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            assertCustomisationToggleState(disabled: false, active: false);

            AddStep("move mouse to first mod panel", () => InputManager.MoveMouseTo(modSelectOverlay.ChildrenOfType<ModPanel>().First()));
            AddAssert("first mod panel is hovered", () => modSelectOverlay.ChildrenOfType<ModPanel>().First().IsHovered);
        }

        /// <summary>
        /// Ensure that two mod overlays are not cross polluting via central settings instances.
        /// </summary>
        [Test]
        public void TestSettingsNotCrossPolluting()
        {
            Bindable<IReadOnlyList<Mod>> selectedMods2 = null!;
            ModSelectOverlay modSelectOverlay2 = null!;

            createScreen();
            AddStep("select difficulty adjust via panel", () => getPanelForMod(typeof(OsuModDifficultyAdjust)).TriggerClick());

            AddStep("set setting", () => modSelectOverlay.ChildrenOfType<RoundedSliderBar<float>>().First().Current.Value = 8);

            AddAssert("ensure setting is propagated", () => SelectedMods.Value.OfType<OsuModDifficultyAdjust>().Single().CircleSize.Value == 8);

            AddStep("create second bindable", () => selectedMods2 = new Bindable<IReadOnlyList<Mod>>(new Mod[] { new OsuModDifficultyAdjust() }));

            AddStep("create second overlay", () =>
            {
                Add(modSelectOverlay2 = new UserModSelectOverlay().With(d =>
                {
                    d.Origin = Anchor.TopCentre;
                    d.Anchor = Anchor.TopCentre;
                    d.SelectedMods.BindTarget = selectedMods2;
                }));
            });

            AddStep("show", () => modSelectOverlay2.Show());

            AddAssert("ensure first is unchanged", () => SelectedMods.Value.OfType<OsuModDifficultyAdjust>().Single().CircleSize.Value == 8);
            AddAssert("ensure second is default", () => selectedMods2.Value.OfType<OsuModDifficultyAdjust>().Single().CircleSize.Value == null);
        }

        [Test]
        public void TestSettingsResetOnDeselection()
        {
            var osuModDoubleTime = new OsuModDoubleTime { SpeedChange = { Value = 1.2 } };

            createScreen();
            changeRuleset(0);

            AddStep("set dt mod with custom rate", () => { SelectedMods.Value = new[] { osuModDoubleTime }; });

            AddAssert("selected mod matches", () => (SelectedMods.Value.Single() as OsuModDoubleTime)?.SpeedChange.Value == 1.2);

            AddStep("deselect", () => getPanelForMod(typeof(OsuModDoubleTime)).TriggerClick());
            AddAssert("selected mods empty", () => SelectedMods.Value.Count == 0);

            AddStep("reselect", () => getPanelForMod(typeof(OsuModDoubleTime)).TriggerClick());
            AddAssert("selected mod has default value", () => (SelectedMods.Value.Single() as OsuModDoubleTime)?.SpeedChange.IsDefault == true);
        }

        [Test]
        public void TestAnimationFlushOnClose()
        {
            createScreen();
            changeRuleset(0);

            AddStep("Select all fun mods", () =>
            {
                modSelectOverlay.ChildrenOfType<ModColumn>()
                                .Single(c => c.ModType == ModType.DifficultyIncrease)
                                .SelectAll();
            });

            AddUntilStep("many mods selected", () => SelectedMods.Value.Count >= 5);

            AddStep("trigger deselect and close overlay", () =>
            {
                modSelectOverlay.ChildrenOfType<ModColumn>()
                                .Single(c => c.ModType == ModType.DifficultyIncrease)
                                .DeselectAll();

                modSelectOverlay.Hide();
            });

            AddAssert("all mods deselected", () => SelectedMods.Value.Count == 0);
        }

        [Test]
        public void TestCommonModsMaintainedOnRulesetChange()
        {
            createScreen();
            changeRuleset(0);

            AddStep("select relax mod", () => SelectedMods.Value = new[] { Ruleset.Value.CreateInstance().CreateMod<ModRelax>() });

            changeRuleset(0);
            AddAssert("ensure mod still selected", () => SelectedMods.Value.SingleOrDefault() is OsuModRelax);

            changeRuleset(2);
            AddAssert("catch variant selected", () => SelectedMods.Value.SingleOrDefault() is CatchModRelax);

            changeRuleset(3);
            AddAssert("no mod selected", () => SelectedMods.Value.Count == 0);
        }

        [Test]
        public void TestUncommonModsDiscardedOnRulesetChange()
        {
            createScreen();
            changeRuleset(0);

            AddStep("select single tap mod", () => SelectedMods.Value = new[] { new OsuModSingleTap() });

            changeRuleset(0);
            AddAssert("ensure mod still selected", () => SelectedMods.Value.SingleOrDefault() is OsuModSingleTap);

            changeRuleset(3);
            AddAssert("no mod selected", () => SelectedMods.Value.Count == 0);
        }

        [Test]
        public void TestExternallySetCustomizedMod()
        {
            createScreen();
            changeRuleset(0);

            AddStep("set customized mod externally", () => SelectedMods.Value = new[] { new OsuModDoubleTime { SpeedChange = { Value = 1.01 } } });

            AddAssert("ensure button is selected and customized accordingly", () =>
            {
                var button = getPanelForMod(SelectedMods.Value.Single().GetType());
                return ((OsuModDoubleTime)button.Mod).SpeedChange.Value == 1.01;
            });
        }

        [Test]
        public void TestSettingsAreRetainedOnReload()
        {
            createScreen();
            changeRuleset(0);

            AddStep("set customized mod externally", () => SelectedMods.Value = new[] { new OsuModDoubleTime { SpeedChange = { Value = 1.01 } } });
            AddAssert("setting remains", () => (SelectedMods.Value.SingleOrDefault() as OsuModDoubleTime)?.SpeedChange.Value == 1.01);

            createScreen();
            AddAssert("setting remains", () => (SelectedMods.Value.SingleOrDefault() as OsuModDoubleTime)?.SpeedChange.Value == 1.01);
        }

        [Test]
        public void TestExternallySetModIsReplacedByOverlayInstance()
        {
            Mod external = new OsuModDoubleTime();
            Mod overlayButtonMod = null!;

            createScreen();
            changeRuleset(0);

            AddStep("set mod externally", () => { SelectedMods.Value = new[] { external }; });

            AddAssert("ensure button is selected", () =>
            {
                var button = getPanelForMod(SelectedMods.Value.Single().GetType());
                overlayButtonMod = button.Mod;
                return button.Active.Value;
            });

            // Right now, when an external change occurs, the ModSelectOverlay will replace the global instance with its own
            AddAssert("mod instance doesn't match", () => external != overlayButtonMod);

            AddAssert("one mod present in global selected", () => SelectedMods.Value.Count == 1);
            AddAssert("globally selected matches button's mod instance", () => SelectedMods.Value.Any(mod => ReferenceEquals(mod, overlayButtonMod)));
            AddAssert("globally selected doesn't contain original external change", () => !SelectedMods.Value.Any(mod => ReferenceEquals(mod, external)));
        }

        [Test]
        public void TestChangeIsValidChangesButtonVisibility()
        {
            createScreen();
            changeRuleset(0);

            AddAssert("double time visible", () => modSelectOverlay.ChildrenOfType<ModPanel>().Where(panel => panel.Mod is OsuModDoubleTime).Any(panel => !panel.Filtered.Value));

            AddStep("make double time invalid", () => modSelectOverlay.IsValidMod = m => !(m is OsuModDoubleTime));
            AddUntilStep("double time not visible", () => modSelectOverlay.ChildrenOfType<ModPanel>().Where(panel => panel.Mod is OsuModDoubleTime).All(panel => panel.Filtered.Value));
            AddAssert("nightcore still visible", () => modSelectOverlay.ChildrenOfType<ModPanel>().Where(panel => panel.Mod is OsuModNightcore).Any(panel => !panel.Filtered.Value));

            AddStep("make double time valid again", () => modSelectOverlay.IsValidMod = _ => true);
            AddUntilStep("double time visible", () => modSelectOverlay.ChildrenOfType<ModPanel>().Where(panel => panel.Mod is OsuModDoubleTime).Any(panel => !panel.Filtered.Value));
            AddAssert("nightcore still visible", () => modSelectOverlay.ChildrenOfType<ModPanel>().Where(b => b.Mod is OsuModNightcore).Any(panel => !panel.Filtered.Value));
        }

        [Test]
        public void TestChangeIsValidPreservesSelection()
        {
            createScreen();
            changeRuleset(0);

            AddStep("select DT + HD", () => SelectedMods.Value = new Mod[] { new OsuModDoubleTime(), new OsuModHidden() });
            AddAssert("DT + HD selected", () => modSelectOverlay.ChildrenOfType<ModPanel>().Count(panel => panel.Active.Value) == 2);

            AddStep("make NF invalid", () => modSelectOverlay.IsValidMod = m => !(m is ModNoFail));
            AddAssert("DT + HD still selected", () => modSelectOverlay.ChildrenOfType<ModPanel>().Count(panel => panel.Active.Value) == 2);
        }

        [Test]
        public void TestUnimplementedModIsUnselectable()
        {
            var testRuleset = new TestUnimplementedModOsuRuleset();

            createScreen();

            AddStep("set ruleset", () => Ruleset.Value = testRuleset.RulesetInfo);
            waitForColumnLoad();

            AddAssert("unimplemented mod panel is filtered", () => getPanelForMod(typeof(TestUnimplementedMod)).Filtered.Value);
        }

        [Test]
        public void TestDeselectAllViaKey()
        {
            createScreen();
            changeRuleset(0);

            AddStep("select DT + HD", () => SelectedMods.Value = new Mod[] { new OsuModDoubleTime(), new OsuModHidden() });
            AddAssert("DT + HD selected", () => modSelectOverlay.ChildrenOfType<ModPanel>().Count(panel => panel.Active.Value) == 2);

            AddStep("press backspace", () => InputManager.Key(Key.BackSpace));
            AddUntilStep("all mods deselected", () => !SelectedMods.Value.Any());
        }

        [Test]
        public void TestDeselectAllViaButton()
        {
            createScreen();
            changeRuleset(0);

            AddAssert("deselect all button disabled", () => !this.ChildrenOfType<DeselectAllModsButton>().Single().Enabled.Value);

            AddStep("select DT + HD", () => SelectedMods.Value = new Mod[] { new OsuModDoubleTime(), new OsuModHidden() });
            AddAssert("DT + HD selected", () => modSelectOverlay.ChildrenOfType<ModPanel>().Count(panel => panel.Active.Value) == 2);
            AddAssert("deselect all button enabled", () => this.ChildrenOfType<DeselectAllModsButton>().Single().Enabled.Value);

            AddStep("click deselect all button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<DeselectAllModsButton>().Single());
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("all mods deselected", () => !SelectedMods.Value.Any());
            AddAssert("deselect all button disabled", () => !this.ChildrenOfType<DeselectAllModsButton>().Single().Enabled.Value);
        }

        [Test]
        public void TestCloseViaBackButton()
        {
            createScreen();
            changeRuleset(0);

            AddStep("select difficulty adjust via panel", () => getPanelForMod(typeof(OsuModDifficultyAdjust)).TriggerClick());
            assertCustomisationToggleState(disabled: false, active: true);
            AddAssert("back button disabled", () => !modSelectOverlay.BackButton.Enabled.Value);

            AddStep("dismiss customisation area", () => InputManager.Key(Key.Escape));
            AddStep("click back button", () =>
            {
                InputManager.MoveMouseTo(modSelectOverlay.BackButton);
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("mod select hidden", () => modSelectOverlay.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestColumnHiding()
        {
            AddStep("create screen", () => Child = modSelectOverlay = new TestModSelectOverlay
            {
                RelativeSizeAxes = Axes.Both,
                State = { Value = Visibility.Visible },
                SelectedMods = { BindTarget = SelectedMods },
                IsValidMod = mod => mod.Type == ModType.DifficultyIncrease || mod.Type == ModType.Conversion
            });
            waitForColumnLoad();
            changeRuleset(0);

            AddAssert("two columns visible", () => this.ChildrenOfType<ModColumn>().Count(col => col.IsPresent) == 2);

            AddStep("unset filter", () => modSelectOverlay.IsValidMod = _ => true);
            AddAssert("all columns visible", () => this.ChildrenOfType<ModColumn>().All(col => col.IsPresent));

            AddStep("filter out everything", () => modSelectOverlay.IsValidMod = _ => false);
            AddAssert("no columns visible", () => this.ChildrenOfType<ModColumn>().All(col => !col.IsPresent));

            AddStep("hide", () => modSelectOverlay.Hide());
            AddStep("set filter for 3 columns", () => modSelectOverlay.IsValidMod = mod => mod.Type == ModType.DifficultyReduction
                                                                                           || mod.Type == ModType.Automation
                                                                                           || mod.Type == ModType.Conversion);

            AddStep("show", () => modSelectOverlay.Show());
            AddUntilStep("3 columns visible", () => this.ChildrenOfType<ModColumn>().Count(col => col.IsPresent) == 3);
        }

        [Test]
        public void TestColumnHidingOnRulesetChange()
        {
            createScreen();

            changeRuleset(0);
            AddAssert("5 columns visible", () => this.ChildrenOfType<ModColumn>().Count(col => col.IsPresent) == 5);

            AddStep("change to ruleset without all mod types", () => Ruleset.Value = TestCustomisableModRuleset.CreateTestRulesetInfo());
            AddUntilStep("1 column visible", () => this.ChildrenOfType<ModColumn>().Count(col => col.IsPresent) == 1);

            changeRuleset(0);
            AddAssert("5 columns visible", () => this.ChildrenOfType<ModColumn>().Count(col => col.IsPresent) == 5);
        }

        private void waitForColumnLoad() => AddUntilStep("all column content loaded",
            () => modSelectOverlay.ChildrenOfType<ModColumn>().Any() && modSelectOverlay.ChildrenOfType<ModColumn>().All(column => column.IsLoaded && column.ItemsLoaded));

        private void changeRuleset(int id)
        {
            AddStep($"set ruleset to {id}", () => Ruleset.Value = rulesetStore.GetRuleset(id));
            waitForColumnLoad();
        }

        private void assertCustomisationToggleState(bool disabled, bool active)
        {
            AddAssert($"customisation toggle is {(disabled ? "" : "not ")}disabled", () => modSelectOverlay.CustomisationButton.AsNonNull().Active.Disabled == disabled);
            AddAssert($"customisation toggle is {(active ? "" : "not ")}active", () => modSelectOverlay.CustomisationButton.AsNonNull().Active.Value == active);
        }

        private ModPanel getPanelForMod(Type modType)
            => modSelectOverlay.ChildrenOfType<ModPanel>().Single(panel => panel.Mod.GetType() == modType);

        private partial class TestModSelectOverlay : UserModSelectOverlay
        {
            protected override bool ShowPresets => true;

            public new ShearedButton BackButton => base.BackButton;
            public new ShearedToggleButton? CustomisationButton => base.CustomisationButton;
        }

        private class TestUnimplementedMod : Mod
        {
            public override string Name => "Unimplemented mod";
            public override string Acronym => "UM";
            public override LocalisableString Description => "A mod that is not implemented.";
            public override double ScoreMultiplier => 1;
            public override ModType Type => ModType.Conversion;
        }

        private class TestUnimplementedModOsuRuleset : OsuRuleset
        {
            public override string ShortName => "unimplemented";

            public override IEnumerable<Mod> GetModsFor(ModType type)
            {
                if (type == ModType.Conversion) return base.GetModsFor(type).Concat(new[] { new TestUnimplementedMod() });

                return base.GetModsFor(type);
            }
        }
    }
}
