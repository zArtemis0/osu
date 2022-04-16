using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings.Sections.Mf
{
    public class DBusSettings : SettingsSubsection
    {
        private SettingsSlider<double, TimeSlider> dbusWaitOnlineSlider;
        protected override LocalisableString Header => "DBus";

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            SettingsCheckbox intergrationCheckbox;
            Children = new Drawable[]
            {
                intergrationCheckbox = new SettingsCheckbox
                {
                    LabelText = "D-Bus集成",
                    Current = config.GetBindable<bool>(MSetting.DBusIntegration)
                },
                new SettingsCheckbox
                {
                    LabelText = "允许通过D-Bus发送消息到游戏内",
                    Current = config.GetBindable<bool>(MSetting.DBusAllowPost)
                },
                new SettingsCheckbox
                {
                    LabelText = "总是使用avatarlogo作为mpris封面",
                    Current = config.GetBindable<bool>(MSetting.MprisUseAvatarlogoAsCover)
                },
                dbusWaitOnlineSlider = new SettingsSlider<double, TimeSlider>
                {
                    LabelText = "DBus初始化等待时间",
                    TooltipText = "延迟初始化一些DBus项目。过低或过高的值可能会导致一些莫名其妙的问题，例如Mpris或托盘不显示。",
                    Current = config.GetBindable<double>(MSetting.DBusWaitOnline)
                }
            };

            if (RuntimeInfo.OS != RuntimeInfo.Platform.Linux)
            {
                intergrationCheckbox.WarningText = "非Linux平台可能需要自行安装并启用DBus";
            }

            intergrationCheckbox.Current.BindValueChanged(v =>
            {
                intergrationCheckbox.WarningText = v.NewValue ? default : "需要重启";
            });

            dbusWaitOnlineSlider.Current.BindValueChanged(v =>
            {
                dbusWaitOnlineSlider.WarningText = v.NewValue == 3000d ? "真得有桌面需要拉这么高的值吗 O.O" : default;
            }, true);
        }
    }
}
