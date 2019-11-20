namespace SillyMemeSounds
{
    using BepInEx;
    using R2API.AssetPlus;
    using R2API.Utils;
    using RoR2;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;

    [BepInDependency("com.bepis.r2api")]

    [BepInPlugin("com.hijackhornet.SillyMemeSounds", "Silly Meme Sounds", "1.0")]

    public class SillyMemeSounds : BaseUnityPlugin
    {
        #region Constants

        internal const uint OnDamageHit = 2600171160;//Done

        internal const uint OnDeath = 2598090648;//Done

        internal const uint OnDiosRevive = 4072839998;//Done

        internal const uint OnExplosion = 2131585375;//Done

        internal const uint OnMoveItemPicked = 3567026048;//Done

        internal const uint OnHeavyDamageTaken = 1966329801;

        internal const uint OnHuntressSelectedInMenu = 334758607;

        internal const uint OnHuntressUltimateArrowShot = 948171580;

        internal const uint OnKeyPickUp = 1482489027;

        internal const uint OnLoaderChargeCharging = 2079747858;

        internal const uint OnLoaderChargeRelease = 3259047430;

        internal const uint OnLobbyReady = 1091271883;

        internal const uint OnMultiplayerLastOneAlive = 2561528917;

        internal const uint OnRedItemPickUp = 1660580786;//Done

        internal const uint OnRunStart = 2053534051;//Done

        internal const uint OnStageLeaving = 89815316;

        internal const uint OnVictory = 1840658428;//Done

        internal const uint OnWelcomeScreen = 3148788086;//Done

        #endregion

        #region Fields

        private CharacterBody characterBody;

        #endregion

        #region Methods

        public void Awake()
        {
            var soundbank = LoadFile("SoundBank.bnk");
            if (soundbank != null)
            {
                SoundBanks.Add(soundbank);
            }
            else
            {
                Debug.LogError("[SillyMemeSounds] Soundbank fetching failed");
                Destroy(this);
            }
            SoundBanks.Add(soundbank);

            Run.onRunStartGlobal += Run_onRunStartGlobal;
            Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
            On.RoR2.UI.MainMenu.MainMenuController.Start += MainMenuController_Start;
        }

        private void Run_onRunStartGlobal(Run obj)
        {
            On.RoR2.PlayerCharacterMasterController.Start += PlayerCharacterMasterController_Start;
            On.RoR2.PlayerCharacterMasterController.OnDisable += PlayerCharacterMasterController_OnDisable;
            On.EntityStates.GenericCharacterDeath.PlayDeathSound += GenericCharacterDeath_PlayDeathSound;
            RoR2.GlobalEventManager.onClientDamageNotified += OnHitDamageFromClient;
            RoR2.Run.onClientGameOverGlobal += Run_onClientGameOverGlobal;
            On.RoR2.CharacterMaster.PlayExtraLifeSFX += CharacterMaster_PlayExtraLifeSFX;
            On.RoR2.EquipmentSlot.CmdExecuteIfReady += OnPreonFire;
            On.RoR2.GenericPickupController.SyncPickupIndex += GenericPickupController_SyncPickupIndex;
            PlaySound(OnRunStart, Camera.main.gameObject);
        }

        private void GenericPickupController_SyncPickupIndex(On.RoR2.GenericPickupController.orig_SyncPickupIndex orig, GenericPickupController self, PickupIndex newPickupIndex)
        {
            orig(self,newPickupIndex);
            if (newPickupIndex.isValid)
            {
                PickupDef pickupDef = PickupCatalog.GetPickupDef(newPickupIndex);
                if (pickupDef.baseColor == ColorCatalog.GetColor(ColorCatalog.ColorIndex.Tier3Item))
                {
                    PlaySound(OnRedItemPickUp, this.characterBody.gameObject);
                }
                else if(pickupDef.itemIndex.Equals(ItemIndex.Hoof)|| pickupDef.itemIndex.Equals(ItemIndex.SprintOutOfCombat)|| pickupDef.itemIndex.Equals(ItemIndex.SprintBonus))
                {
                    PlaySound(OnMoveItemPicked, this.characterBody.gameObject);
                }
            }
        }

        private void Run_onRunDestroyGlobal(Run obj)
        {
            On.RoR2.PlayerCharacterMasterController.Start += PlayerCharacterMasterController_Start;
            On.RoR2.PlayerCharacterMasterController.OnDisable -= PlayerCharacterMasterController_OnDisable;
            On.EntityStates.GenericCharacterDeath.PlayDeathSound -= GenericCharacterDeath_PlayDeathSound;
            RoR2.GlobalEventManager.onClientDamageNotified -= OnHitDamageFromClient;
            RoR2.Run.onClientGameOverGlobal -= Run_onClientGameOverGlobal;
            On.RoR2.EquipmentSlot.CmdExecuteIfReady -= OnPreonFire;
            On.RoR2.CharacterMaster.PlayExtraLifeSFX -= CharacterMaster_PlayExtraLifeSFX;
        }

        private void OnPreonFire(On.RoR2.EquipmentSlot.orig_CmdExecuteIfReady orig, EquipmentSlot self)
        {
            orig(self);
            if (this.characterBody) { 
                PlaySound(OnExplosion, this.characterBody.gameObject);
            }
                
        }

        private void GenericCharacterDeath_PlayDeathSound(On.EntityStates.GenericCharacterDeath.orig_PlayDeathSound orig, EntityStates.GenericCharacterDeath self)
        {

            if (self.outer.networkIdentity.isLocalPlayer)
            {
                PlaySound(OnDeath, Camera.main.gameObject);
            }
            else
            {
                orig(self);
            }
        }

        private void PlayerCharacterMasterController_Start(On.RoR2.PlayerCharacterMasterController.orig_Start orig, PlayerCharacterMasterController self)
        {
            orig(self);
            if (self.isLocalPlayer)
            {
                this.characterBody = self.GetPropertyValue<CharacterBody>("body");
            }
        }

        private void PlayerCharacterMasterController_OnDisable(On.RoR2.PlayerCharacterMasterController.orig_OnDisable orig, PlayerCharacterMasterController self)
        {
            orig(self);
            this.characterBody = null;
        }

        private void CharacterMaster_PlayExtraLifeSFX(On.RoR2.CharacterMaster.orig_PlayExtraLifeSFX orig, CharacterMaster self)
        {
            GameObject bodyInstanceObject = self.GetPropertyValue<GameObject>("bodyInstanceObject");
            if (bodyInstanceObject)
            {
                PlaySound(OnDiosRevive, bodyInstanceObject);
            }
        }

        private void Run_onClientGameOverGlobal(Run run, RunReport runReport)
        {
            if (runReport.gameResultType.ToString() == "Won")
            {
                PlaySound(OnVictory, Camera.main.gameObject);
            }
        }

        private void OnHitDamageFromClient(DamageDealtMessage damageDealtMessage)
        {
            PlayerCharacterMasterController pmc;
            try
            {
                pmc = damageDealtMessage.attacker.GetComponent<CharacterBody>().master.GetComponent<PlayerCharacterMasterController>();
            }
            catch { return; }
            if (pmc)
            {
                if (pmc.networkUser.isLocalPlayer)
                {
                    PlaySound(OnDamageHit, damageDealtMessage.victim);
                }
                else
                {
#if DEBUG
                    Debug.Log("The attacker isnt the local player");
#endif
                }
            }
            else
            {
#if DEBUG
                Debug.Log("Damage origine isnt a player");
#endif
            }
        }

        private void MainMenuController_Start(On.RoR2.UI.MainMenu.MainMenuController.orig_Start orig, RoR2.UI.MainMenu.MainMenuController self)
        {
            orig(self);
            PlaySound(OnWelcomeScreen, Camera.main.gameObject);
            On.RoR2.UI.MainMenu.MainMenuController.Start -= MainMenuController_Start;
        }

        private void PlaySound(uint eventId, GameObject objectDestination)
        {

            AkSoundEngine.PostEvent(eventId, objectDestination);
        }

        internal byte[] LoadFile(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            resourceName = assembly.GetManifestResourceNames()
                .First(str => str.EndsWith(resourceName));
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                return reader.ReadBytes(Convert.ToInt32(stream.Length.ToString()));
            }
        }

        #endregion
    }
}
