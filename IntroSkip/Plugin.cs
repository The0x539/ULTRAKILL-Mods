using System.Reflection;

using UnityEngine;
using UnityEngine.SceneManagement;

using BepInEx;
using BepInEx.Configuration;

namespace IntroSkip;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public sealed class Plugin : BaseUnityPlugin {
    private readonly ConfigEntry<string> targetScene;
    private readonly IntroViolenceScreen introViolenceScreen;
    private readonly GameObject loadingScreen;

    public Plugin() {
        this.introViolenceScreen = FindObjectOfType<IntroViolenceScreen>();
        var fieldInfo = typeof(IntroViolenceScreen).GetField("loadingScreen", BindingFlags.Instance | BindingFlags.NonPublic);
        this.loadingScreen = (GameObject)fieldInfo.GetValue(this.introViolenceScreen);
        this.targetScene = this.Config.Bind("IntroSkip", "TargetScene", "Main Menu");

        SceneManager.activeSceneChanged += this.OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene current, Scene next) {
        if (next.name == "Intro") {
            this.StartCoroutine(this.LoadTargetScene());
        }
    }

    private System.Collections.IEnumerator LoadTargetScene() {
        this.introViolenceScreen.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.01f); // I don't know what this line actually accomplishes, but the experience is jankier if you take it out
        yield return new WaitForFixedUpdate();
        yield return new WaitForEndOfFrame();

        this.loadingScreen.SetActive(true);
        yield return new WaitForFixedUpdate();
        yield return new WaitForEndOfFrame();

        var targetScene = this.targetScene.Value;
        // if the user is loading straight into a map, they're probably developing a mod,
        // so they probably don't care if there's a little stutter when you first click on an act in the main menu,
        // and would rather the game load ASAP
        if (targetScene == "Main Menu") {
            MapLoader.Instance.EnsureCommonIsLoaded();
        }
        SceneManager.LoadScene(targetScene);
    }
}
