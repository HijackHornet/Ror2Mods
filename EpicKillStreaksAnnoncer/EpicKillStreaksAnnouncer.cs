namespace EpicKillStreaksAnnouncer
{
    using AssetPlus;
    using BepInEx;
    using BepInEx.Configuration;
    using RoR2;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;
    using UnityEngine.Networking;

    [BepInDependency("com.bepis.r2api")]
    [BepInDependency("com.mistername.AssetPlus")]

    [BepInPlugin("com.hijackhornet.epickillstreaksannouncer", "Epic KillStreaks Announcer", "1.1.0")]

    public class EpicKillStreaksAnnouncer : BaseUnityPlugin
    {
        internal const uint Headshot = 2632074263;

        internal const uint DoubleKill = 3650570114;

        internal const uint TripleKill = 3103184737;

        internal const uint MultiKill = 887867080;

        internal const uint Dominating = 305393359;

        internal const uint Rampage = 2756207098;

        internal const uint LudicrousKill = 2980856139;

        internal const uint GodLike = 1611658976;

        internal const uint MonsterKill = 2233416765;

        public static ConfigWrapper<int> ConfigWrapperHeadshot { get; set; }

        public static ConfigWrapper<int> ConfigWrapperDoubleKill { get; set; }

        public static ConfigWrapper<int> ConfigWrapperTripleKill { get; set; }

        public static ConfigWrapper<int> ConfigWrapperMultiKill { get; set; }

        public static ConfigWrapper<int> ConfigWrapperDominating { get; set; }

        public static ConfigWrapper<int> ConfigWrapperRampage { get; set; }

        public static ConfigWrapper<int> ConfigWrapperLudicrousKill { get; set; }

        public static ConfigWrapper<int> ConfigWrapperGodLike { get; set; }

        public static ConfigWrapper<int> ConfigWrapperMonsterKill { get; set; }

        public static ConfigWrapper<float> ConfigWrappertimeBeforeKillingSpreeEnd { get; set; }

        internal void Awake()
        {
            var soundbank = LoadFile("SillySoundsBank.bnk");
            if (soundbank != null)
            {
                SoundBanks.Add(soundbank);
            }
            else
            {
                UnityEngine.Debug.LogError("[EpicKillStreaksAnnoncer] Soundbank fetching failed");
                Destroy(this);
            }
            SoundBanks.Add(soundbank);
            ConfigWrapperHeadshot = Config.Wrap<int>(
                "Trigger Values",
                "HeadShot",
                "Choose at how many kills in a kill streak, this sound should trigger. -1 to desactivate.",
                -1
                );
            ConfigWrapperDoubleKill = Config.Wrap<int>(
                "Trigger Values",
                "DoubleKill",
                "Choose at how many kills in a kill streak, this sound should trigger. -1 to desactivate.",
                -1
                );
            ConfigWrapperTripleKill = Config.Wrap<int>(
                "Trigger Values",
                "TripleKill",
                "Choose at how many kills in a kill streak, this sound should trigger. -1 to desactivate.",
                3
                );
            ConfigWrapperMultiKill = Config.Wrap<int>(
                "Trigger Values",
                "MultiKill",
                "Choose at how many kills in a kill streak, this sound should trigger. -1 to desactivate.",
                5
                );
            ConfigWrapperDominating = Config.Wrap<int>(
                "Trigger Values",
                "Domination",
                "Choose at how many kills in a kill streak, this sound should trigger. -1 to desactivate.",
                8
                );
            ConfigWrapperRampage = Config.Wrap<int>(
                "Trigger Values",
                "Rampage",
                "Choose at how many kills in a kill streak, this sound should trigger. -1 to desactivate.",
                12
                );
            ConfigWrapperLudicrousKill = Config.Wrap<int>(
                "Trigger Values",
                "LudicrousKill",
                "Choose at how many kills in a kill streak, this sound should trigger. -1 to desactivate.",
                16
                );
            ConfigWrapperGodLike = Config.Wrap<int>(
                "Trigger Values",
                "GodLike",
                "Choose at how many kills in a kill streak, this sound should trigger. -1 to desactivate.",
                20
                );
            ConfigWrapperMonsterKill = Config.Wrap<int>(
                "Trigger Values",
                "MonsterKill",
                "Choose at how many kills in a kill streak, this sound should trigger. -1 to desactivate.",
                25
                );
            ConfigWrappertimeBeforeKillingSpreeEnd = Config.Wrap<float>(
                "Timer",
                "MaxTimeToEnd",
                "How much seconds are needed before the killing streak reset",
                3.0f
                );

            On.RoR2.CharacterBody.Start += (orig, self) =>
            {
                orig(self);
                if ((self.master != null) && (self.master.GetComponent<PlayerCharacterMasterController>() != null))
                {
                    if (NetworkServer.active)
                    {
                        self.gameObject.AddComponent<Announcer>().initSoundsList(Headshot, DoubleKill, TripleKill, MultiKill, Dominating, Rampage, LudicrousKill, GodLike, MonsterKill);
                    }
                    else if (self.master.GetComponent<PlayerCharacterMasterController>().networkUser.isLocalPlayer)
                    {
                        self.gameObject.AddComponent<Announcer>().initSoundsList(Headshot, DoubleKill, TripleKill, MultiKill, Dominating, Rampage, LudicrousKill, GodLike, MonsterKill);
                    }
                }
            };

            Run.onRunStartGlobal += Run_onRunStartGlobal;
            Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
        }

        private void Run_onRunStartGlobal(Run obj)
        {
            if (NetworkServer.active)
            {
                RoR2.GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            }
            else
            {
                RoR2.GlobalEventManager.onClientDamageNotified += GlobalEventManager_onClientDamageNotified;
            }
        }

        private void Run_onRunDestroyGlobal(Run obj)
        {
            if (NetworkServer.active)
            {
                RoR2.GlobalEventManager.onCharacterDeathGlobal -= GlobalEventManager_onCharacterDeathGlobal;
            }
            else
            {
                RoR2.GlobalEventManager.onClientDamageNotified -= GlobalEventManager_onClientDamageNotified;
            }
        }

        private void GlobalEventManager_onClientDamageNotified(DamageDealtMessage damageDealtMessage)
        {
            PlayerCharacterMasterController pmc = damageDealtMessage.attacker.GetComponent<CharacterBody>().master.GetComponent<PlayerCharacterMasterController>();
            if (pmc != null)
            {
                if (pmc.networkUser.isLocalPlayer)
                {
                    Announcer announcer = damageDealtMessage.attacker.GetComponent<Announcer>();
                    if (announcer != null)
                    {
                        if (damageDealtMessage.victim != null)
                        {
                            HealthComponent healthComp = damageDealtMessage.victim.GetComponent<HealthComponent>();
                            if (healthComp != null)
                            {
                                if (healthComp.combinedHealth <= damageDealtMessage.damage)
                                {
                                    Debug.Log("Monster killed");
                                    announcer.RegisterKill();
                                    Debug.Log("Kill registered");
                                }
                                else
                                {
                                    Debug.Log("Hit! Damage = " + damageDealtMessage.damage + " on " + healthComp.combinedHealth + "/" + healthComp.fullCombinedHealth);
                                    Debug.Log("------");
                                }
                            }
                            else
                            {
                                Debug.LogWarning("Victim has no healthComp !");
                            }

                        }
                        else
                        {
                            Debug.Log("Damage isnt toward a victim.");
                        }
                    }
                    else
                    {
                        Debug.LogError("Announcer not present on the localUser");
                        return;
                    }
                }
                else
                {
                    Debug.Log("The attacker isnt the local player");
                }
            }
            else
            {
                Debug.Log("Damage origine isnt a player");
            }
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            damageReport.attacker.gameObject.GetComponent<Announcer>().RegisterKill();
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
    }

    public class Announcer : NetworkBehaviour
    {
        private float timeBeforeKillingSpreeEnd = 3.0f;

        private float timeSinceLastKill = 0f;

        private int killSpreeCount = 0;

        private bool isLocal = false;

        private uint currentSoundId = 0;

        private uint currentSoundTypeId = 0;

        private uint sound1;

        private uint sound2;

        private uint sound3;

        private uint sound4;

        private uint sound5;

        private uint sound6;

        private uint sound7;

        private uint sound8;

        private uint sound9;

        public void Awake()
        {

            timeBeforeKillingSpreeEnd = EpicKillStreaksAnnouncer.ConfigWrappertimeBeforeKillingSpreeEnd.Value;
        }

        public void Start()
        {
            if (!NetworkServer.active)
            {
                this.isLocal = true;
            }
            else if (this.gameObject.GetComponent<CharacterBody>().master.GetComponent<PlayerCharacterMasterController>().networkUser.isLocalPlayer)
            {
                this.isLocal = true;
            }
        }

        public void initSoundsList(uint _sound1, uint _sound2, uint _sound3, uint _sound4, uint _sound5, uint _sound6, uint _sound7, uint _sound8, uint _sound9)
        {
            this.sound1 = _sound1;
            this.sound2 = _sound2;
            this.sound3 = _sound3;
            this.sound4 = _sound4;
            this.sound5 = _sound5;
            this.sound6 = _sound6;
            this.sound7 = _sound7;
            this.sound8 = _sound8;
            this.sound9 = _sound9;
        }

        public void FixedUpdate()
        {
            timeSinceLastKill += Time.fixedDeltaTime;
            if (timeSinceLastKill >= timeBeforeKillingSpreeEnd)
            {
                killSpreeCount = 0;
                currentSoundTypeId = 0;
            }
        }

        private void PlaySound(uint eventid)
        {

            if ((currentSoundTypeId != eventid) && (this.isLocal))
            {
                currentSoundTypeId = eventid;
                AkSoundEngine.StopPlayingID(currentSoundId);
                currentSoundId = AkSoundEngine.PostEvent(eventid, this.gameObject);

            }
        }

        public void RegisterKill()
        {
            this.timeSinceLastKill = 0;
            this.killSpreeCount++;

            if (killSpreeCount == EpicKillStreaksAnnouncer.ConfigWrapperHeadshot.Value)
            {
                PlaySound(sound1);

            }
            else if (killSpreeCount == EpicKillStreaksAnnouncer.ConfigWrapperDoubleKill.Value)
            {
                PlaySound(sound2);
            }
            else if (killSpreeCount == EpicKillStreaksAnnouncer.ConfigWrapperTripleKill.Value)
            {
                PlaySound(sound3);
                SendChat("made a triple kill... How lame !");
            }
            else if ((killSpreeCount >= EpicKillStreaksAnnouncer.ConfigWrapperMultiKill.Value) && (killSpreeCount < EpicKillStreaksAnnouncer.ConfigWrapperDominating.Value))
            {
                PlaySound(sound4);
            }
            else if ((killSpreeCount >= EpicKillStreaksAnnouncer.ConfigWrapperDominating.Value) && (killSpreeCount < EpicKillStreaksAnnouncer.ConfigWrapperRampage.Value))
            {
                PlaySound(sound5);
            }
            else if ((killSpreeCount >= EpicKillStreaksAnnouncer.ConfigWrapperRampage.Value) && (killSpreeCount < EpicKillStreaksAnnouncer.ConfigWrapperLudicrousKill.Value))
            {
                PlaySound(sound6);
            }
            else if ((killSpreeCount >= EpicKillStreaksAnnouncer.ConfigWrapperLudicrousKill.Value) && (killSpreeCount < EpicKillStreaksAnnouncer.ConfigWrapperGodLike.Value))
            {
                PlaySound(sound7);
                SendChat("just did a Ludicrous Kill");
            }
            else if ((killSpreeCount >= EpicKillStreaksAnnouncer.ConfigWrapperGodLike.Value) && (killSpreeCount < EpicKillStreaksAnnouncer.ConfigWrapperMonsterKill.Value))
            {
                PlaySound(sound8);
                SendChat("is reaching God Like");
            }
            else if (killSpreeCount >= EpicKillStreaksAnnouncer.ConfigWrapperMonsterKill.Value)
            {
                PlaySound(sound9);
                SendChat("MADE A M-M-MONSTER KILLLL !");
            }
        }

        private void SendChat(string message)
        {
            if (NetworkServer.active)
            {
                R2API.Utils.ChatMessage.SendColored(this.gameObject.GetComponent<CharacterBody>().master.GetComponent<PlayerCharacterMasterController>().networkUser.GetNetworkPlayerName().GetResolvedName() + " " + message, "#FFC300");
            }
        }
    }
}
