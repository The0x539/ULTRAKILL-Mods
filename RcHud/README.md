# RcHud
A "racehar HUD" mod of sorts.
Adds some UI elements to the game's existing crosshair HUD, allowing you to completely disable the game's normal HUD without giving up any information.

Inspired by [That Trav Guy's review](https://youtu.be/PBFHe5rgYOU?t=176).
It made me realize I could personally go for such a thing, so I just got to work and figured out how to make it happen.

# Screenshots
I don't have any, but contributions are welcome.
I do have [a video](https://www.youtube.com/watch?v=Gyw0B9_qMYc) from early in development, though. Enjoy the bad gameplay.
There are a handful of other videos showcasing it on the channel, too.

# Installation
- Install [BepInEx](https://github.com/BepInEx/BepInEx) (5, not 6)
- Create a directory named `RcHud` inside `steamapps/common/ULTRAKILL/BepInEx/plugins`
- Put `RcHud.dll` in the directory you just created

The mod is also available [on ThunderStore](https://thunderstore.io/c/ultrakill/p/The0x539/RcHud/), but *please* don't subject yourself to Overwolf.

# Configuration
This mod respects the game's "crosshair HUD fade" setting. The icons will also switch positions to match left-handed mode.

In addition, you can edit `ULTRAKILL/BepInEx/config/RcHud.cfg` to configure:
- The way colors work on the stamina meter ([new in 1.2.0!](https://www.youtube.com/watch?v=kEcWEBIaO6g))
- Whether to use the tweaked overheal meter (new in 1.2.0!)
- How long the icons remain visible before fading
- How long the railcannon meter remains visible (once fully charged) before fading
- Individual position and scaling for the gun and arm icons
- Which situations cause the icons to become visible:
  - Punching
  - Switching gun/arm
  - Combat music
  - Presence of a boss health bar
