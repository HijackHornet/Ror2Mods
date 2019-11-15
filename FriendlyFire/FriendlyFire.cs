namespace FriendlyFire
{
    using BepInEx;
    using RoR2;
    using UnityEngine;
    using UnityEngine.Networking;

    [BepInPlugin("com.hijackhornet.friendlyfire", "Friendly Fire", "1.0")]

    public class FriendlyFire : BaseUnityPlugin
    {
        public void Awake()
        {
            Run.onRunStartGlobal += Run_onRunStartGlobal;
            Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
        }

        private void Run_onRunStartGlobal(Run obj)
        {
            if (NetworkServer.active)
                On.RoR2.CharacterBody.Start += CharacterBody_Start;
        }

        private void Run_onRunDestroyGlobal(Run obj)
        {
            if (NetworkServer.active)
                On.RoR2.CharacterBody.Start -= CharacterBody_Start;
        }

        private void CharacterBody_Start(On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);
            if (self.master && self.master.GetComponent<PlayerCharacterMasterController>() /*&& self.master.GetComponent<PlayerCharacterMasterController>().networkUser.isLocalPlayer*/)
            {
                Debug.Log("Found an ally, lets make it an ennemy !");
                self.teamComponent.teamIndex = TeamIndex.None;
            }
        }
    }
}
