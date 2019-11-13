namespace QuickRestart
{
    using BepInEx;
    using BepInEx.Configuration;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Networking;
    using UnityEngine.SceneManagement;

    [BepInDependency("com.bepis.r2api")]

    [BepInPlugin("com.hijackhornet.quickrestart", "Quick Restart", "1.0")]

    public class QuickRestart : BaseUnityPlugin
    {
        public static ConfigWrapper<string> QuickRestartConfigWrapperRestart { get; set; }

        public static ConfigWrapper<string> QuickRestartConfigWrapperBack { get; set; }

        private bool isInChatBox = false;

        private KeyCode shortcut;

        private KeyCode shortcut2;

        public void Awake()
        {
            QuickRestartConfigWrapperRestart = Config.Wrap<string>(
                "Shortcut",
                "Quick_restart_key",
                "Type the key you want to use as a shortcut to restart a run",
                "V"
                );
            QuickRestartConfigWrapperBack = Config.Wrap<string>(
               "Shortcut",
                "Quick_return_to_character_selection_key",
                "Type the key you want to use as a shortcut to get everyone back to the character selection menu",
                "B"
            );
            this.shortcut = (KeyCode)System.Enum.Parse(typeof(KeyCode), QuickRestartConfigWrapperRestart.Value);
            this.shortcut2 = (KeyCode)System.Enum.Parse(typeof(KeyCode), QuickRestartConfigWrapperBack.Value);
            On.RoR2.UI.ChatBox.FocusInputField += (orig, self) => { orig(self); isInChatBox = true; };
            On.RoR2.UI.ChatBox.UnfocusInputField += (orig, self) => { orig(self); isInChatBox = false; };
        }

        public void Update()
        {
            if (Input.GetKeyUp(shortcut) && RoR2.NetworkSession.instance && NetworkServer.active && !this.isInChatBox && (SceneManager.GetActiveScene().name != "lobby"))
            {
                RestartRun();
            }
            else if (Input.GetKeyUp(shortcut2) && RoR2.NetworkSession.instance && NetworkServer.active && !this.isInChatBox && (SceneManager.GetActiveScene().name != "lobby"))
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
