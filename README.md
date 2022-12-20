some kind of racecar hud for ultrakill

inspired by that trav guy's review of the game.
i realized i personally wanted something like this, but there wasn't anything even close to this, so i just figured out how to do it

# screenshots
give me some good ones and i'll add them here

oh yeah, i did make [a video](https://www.youtube.com/watch?v=Gyw0B9_qMYc).
enjoy the bad gameplay.

# installation
1. install [BepInEx](https://github.com/BepInEx/BepInEx)
2. create an `RcHud` directory in `ULTRAKILL/BepInEx/plugins/`
3. put `RcHud.dll` in the directory
4. enjoy

# configuration
edit `ULTRAKILL/BepInEx/config/RcHud.cfg`

# special thanks
documentation authors.
i didn't get any direct help making this, so i wouldn't have been able to make this without the docs for the stuff it uses, which were mostly pretty good, if sometimes hard to find.

props to visual studio for letting you look at what's in a dll using the object browser and letting you navigate to decompiled code from the code window, but not letting you go to decompiled code from the object browser.
what's up with that? it's been [an open issue](https://github.com/dotnet/roslyn/issues/36120) [for years](https://developercommunity.visualstudio.com/content/problem/589562/support-go-to-definition-in-object-browser-for-imp.html), without so much as a "we can't do this for Stupid Reasons" response.

oh, looks like there's [a pr](https://github.com/dotnet/roslyn/pull/66026) as of a couple days ago. neat.
wish that was a thing when i was still trying to figure out how the relevant parts of the game work.