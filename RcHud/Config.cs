﻿using BepInEx.Configuration;

namespace RcHud;

#nullable disable

public static class Config {
    private static ConfigEntry<bool>
        refreshFistOnPunch,
        refreshFistOnSwitch,
        refreshGunOnSwitch,
        refreshOnMusic,
        refreshOnBossBar;

    public static bool RefreshFistOnPunch => refreshFistOnPunch.Value;
    public static bool RefreshFistOnSwitch => refreshFistOnSwitch.Value;
    public static bool RefreshGunOnSwitch => refreshGunOnSwitch.Value;
    public static bool RefreshIconsOnBattleMusic => refreshOnMusic.Value;
    public static bool RefreshIconsOnBossHealthBar => refreshOnBossBar.Value;

    private static ConfigEntry<float>
        iconFade,
        wheelFade,
        fistScale,
        gunScale,
        fistOffset,
        gunOffset;

    public static float IconFadeTime => iconFade.Value;
    public static float WheelFadeTime => wheelFade.Value;

    public static float FistIconScale => fistScale.Value;
    public static float GunIconScale => gunScale.Value;

    // the game's UI is scaled for 720p but my default values were set for 1080p before the addition of aim assist support
    public static float FistIconOffset => fistOffset.Value * 2 / 3;
    public static float GunIconOffset => gunOffset.Value * 2 / 3;

    public static void Init(ConfigFile cfg) {
        refreshFistOnPunch = cfg.Bind("Refresh", "Punch", true);
        refreshFistOnSwitch = cfg.Bind("Refresh", "FistSwitch", true);
        refreshGunOnSwitch = cfg.Bind("Refresh", "GunSwitch", true);
        refreshOnMusic = cfg.Bind("Refresh", "BattleMusic", true);
        refreshOnBossBar = cfg.Bind("Refresh", "BossHealthBar", true);

        iconFade = cfg.Bind("FadeTime", "WeaponIcons", 4.0f);
        wheelFade = cfg.Bind("FadeTime", "RailcannonMeter", 5.0f);

        fistScale = cfg.Bind("IconScale", "Fist", 1.25f);
        gunScale = cfg.Bind("IconScale", "Gun", 0.1f);

        fistOffset = cfg.Bind("IconOffset", "Fist", 50f);
        gunOffset = cfg.Bind("IconOffset", "Gun", 45f);
    }
}
