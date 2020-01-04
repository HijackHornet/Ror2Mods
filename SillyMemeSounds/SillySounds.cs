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
    using Hj;

    [BepInDependency("com.bepis.r2api")]
    [BepInDependency("com.hijackhornet.hjupdaterapi")]
    [BepInPlugin("com.hijackhornet.sillysounds", "Silly Sounds", "1.0.0")]
    public class SillySounds : BaseUnityPlugin
    {
        #region Constants

        private const string LOG = "[Silly Sounds] ";
        internal const uint OnDamageHit = 2600171160;//Done & tested

        internal const uint OnDeath = 2598090648;//Done & tested

        internal const uint OnExplosion = 2131585375;//Done

        internal const uint OnMoveItemPicked = 3567026048;//Done

        internal const uint OnHeavyDamageTaken = 1966329801;

        internal const uint OnHuntressSelectedInMenu = 334758607;

        internal const uint OnHuntressUltimateArrowShot = 948171580;

        internal const uint OnKeyPickUp = 1482489027;//Done

        internal const uint OnLoaderChargeCharging = 2079747858;

        internal const uint OnLoaderChargeRelease = 3259047430;

        internal const uint OnLobbyReady = 1091271883;

        internal const uint OnMultiplayerLastOneAlive = 2561528917;

        internal const uint OnRedItemPickUp = 1660580786;//Done

        internal const uint OnRunStart = 2053534051;//Done & tested

        internal const uint OnStageLeaving = 89815316;

        internal const uint OnVictory = 1840658428;//Done

        internal const uint OnWelcomeScreen = 3148788086;//Done & tested

        #endregion Constants

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
                Debug.LogError(LOG + "Soundbank fetching failed");
                Destroy(this);
            }
            SoundBanks.Add(soundbank);

            Run.onRunStartGlobal += Run_onRunStartGlobal;
            Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
            On.RoR2.UI.MainMenu.MainMenuController.Start += MainMenuController_Start;
            HjUpdaterAPI.RegisterForUpdate("SillySounds", MetadataHelper.GetMetadata(this).Version);
        }

        private void Run_onRunStartGlobal(Run obj)
        {
            On.EntityStates.GenericCharacterDeath.PlayDeathSound += GenericCharacterDeath_PlayDeathSound;
            RoR2.GlobalEventManager.onClientDamageNotified += OnHitDamageFromClient;
            RoR2.Run.onClientGameOverGlobal += Run_onClientGameOverGlobal;
            On.RoR2.UI.NotificationQueue.OnItemPickup += NotificationQueue_OnItemPickup;
            On.RoR2.EquipmentSlot.RpcOnClientEquipmentActivationRecieved += EquipmentSlot_RpcOnClientEquipmentActivationRecieved;
            PlaySound(OnRunStart, Camera.main.gameObject);
        }

        private void EquipmentSlot_RpcOnClientEquipmentActivationRecieved(On.RoR2.EquipmentSlot.orig_RpcOnClientEquipmentActivationRecieved orig, EquipmentSlot self)
        {
            orig(self);
            if (self.equipmentIndex.Equals(EquipmentIndex.BFG))
            {
                PlaySound(OnExplosion, Camera.main.gameObject);
            }
        }

        private void Run_onRunDestroyGlobal(Run obj)
        {
            On.EntityStates.GenericCharacterDeath.PlayDeathSound -= GenericCharacterDeath_PlayDeathSound;
            RoR2.GlobalEventManager.onClientDamageNotified -= OnHitDamageFromClient;
            RoR2.Run.onClientGameOverGlobal -= Run_onClientGameOverGlobal;
            On.RoR2.UI.NotificationQueue.OnItemPickup -= NotificationQueue_OnItemPickup;
        }

        private void NotificationQueue_OnItemPickup(On.RoR2.UI.NotificationQueue.orig_OnItemPickup orig, RoR2.UI.NotificationQueue self, CharacterMaster characterMaster, ItemIndex itemIndex)
        {
            Debug.Log(itemIndex);
            orig(self, characterMaster, itemIndex);
            PlayerCharacterMasterController pmc = characterMaster.GetComponent<PlayerCharacterMasterController>();
            if (pmc && pmc.networkUser.isLocalPlayer)
            {
                ItemDef item = ItemCatalog.GetItemDef(itemIndex);
                try
                {
                    if (item.tier.Equals(ItemTier.Tier3))
                    {
                        PlaySound(OnRedItemPickUp, Camera.main.gameObject);
                    }
                    else if (item.itemIndex.Equals(ItemIndex.Hoof) || item.itemIndex.Equals(ItemIndex.SprintBonus))
                    {
                        PlaySound(OnMoveItemPicked, Camera.main.gameObject);
                    }
                    else if (item.itemIndex.Equals(ItemIndex.TreasureCache))
                    {
                        PlaySound(OnKeyPickUp, Camera.main.gameObject);
                    }
                }
                catch
                {
                    Debug.LogWarning(LOG + "The item picked isnt referenced into the catalogue.");
                }
            }
        }

        private void GenericCharacterDeath_PlayDeathSound(On.EntityStates.GenericCharacterDeath.orig_PlayDeathSound orig, EntityStates.GenericCharacterDeath self)
        {
            if (self.outer.commonComponents.characterBody && self.outer.commonComponents.characterBody.master && self.outer.commonComponents.characterBody.master.GetComponent<PlayerCharacterMasterController>() && self.outer.commonComponents.characterBody.master.GetComponent<PlayerCharacterMasterController>().networkUser.isLocalPlayer)
            {
                PlaySound(OnDeath, Camera.main.gameObject);
            }
            else
            {
                orig(self);
            }
        }

        private void Run_onClientGameOverGlobal(Run run, RunReport runReport)
        {
            if ((runReport.gameResultType.Equals(GameResultType.Won)) || (runReport.gameResultType.Equals(GameResultType.Unknown)))
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

        #endregion Methods
    }
}