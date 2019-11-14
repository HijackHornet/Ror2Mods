namespace QuickRestart
{
    using BepInEx;
    using BepInEx.Configuration;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Networking;
    using UnityEngine.SceneManagement;

    [BepInDependency("com.bepis.r2api")]

    [BepInPlugin("com.hijackhornet.quickrestart", "Quick Restart", "1.0.2")]

    public class QuickRestart : BaseUnityPlugin
    {
        public static ConfigEntry<KeyboardShortcut> QuickRestartConfigWrapperRestart { get; set; }

        public static ConfigEntry<KeyboardShortcut> QuickRestartConfigWrapperBack { get; set; }

        private bool isInChatBox = false;

        public void Awake()
        {
            QuickRestartConfigWrapperRestart = Config.Bind<KeyboardShortcut>(
                "Shortcut",
                "Quick_restart_key",
                new KeyboardShortcut(KeyCode.V),
                "Type the key you want to use as a shortcut to restart a run"
                );
            QuickRestartConfigWrapperBack = Config.Bind<KeyboardShortcut>(
               "Shortcut",
                "Quick_return_to_character_selection_key",
                 new KeyboardShortcut(KeyCode.B),
                "Type the key you want to use as a shortcut to get everyone back to the character selection menu"
            );
            On.RoR2.UI.ChatBox.FocusInputField += (orig, self) => { orig(self); isInChatBox = true; };
            On.RoR2.UI.ChatBox.UnfocusInputField += (orig, self) => { orig(self); isInChatBox = false; };
        }

        public void Update()
        {
            if (QuickRestartConfigWrapperRestart.Value.IsDown() && RoR2.NetworkSession.instance && NetworkServer.active && !this.isInChatBox && (SceneManager.GetActiveScene().name != "lobby"))
            {
                RestartRun();
            }
            else if (QuickRestartConfigWrapperBack.Value.IsDown() && RoR2.NetworkSession.instance && NetworkServer.active && !this.isInChatBox && (SceneManager.GetActiveScene().name != "lobby"))
            {
                GoBackToCharacterSelection();
            }
        }

        internal IEnumerator Restart()
        {
            while (!RoR2.PreGameController.instance)
            {
                yield return new WaitForSeconds(.1f);
            }
            RoR2.PreGameController.instance.StartLaunch();
        }

        private void RestartRun()
        {
            RoR2.NetworkSession.instance.EndRun();
            StartCoroutine(Restart());
        }

        private void GoBackToCharacterSelection()
        {
            RoR2.NetworkSession.instance.EndRun();
        }
    }
}
