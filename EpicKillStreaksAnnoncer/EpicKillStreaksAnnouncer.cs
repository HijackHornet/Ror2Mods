namespace EpicKillStreaksAnnouncer
{
    using BepInEx;
    using BepInEx.Configuration;
    using R2API.AssetPlus;
    using RoR2;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;
    using UnityEngine.Networking;

    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.hijackhornet.epickillstreaksannouncer", "Epic KillStreaks Announcer", "1.1.2")]

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

        public static ConfigEntry<int> ConfigWrapperHeadshot { get; set; }

        public static ConfigEntry<int> ConfigWrapperDoubleKill { get; set; }

        public static ConfigEntry<int> ConfigWrapperTripleKill { get; set; }

        public static ConfigEntry<int> ConfigWrapperMultiKill { get; set; }

        public static ConfigEntry<int> ConfigWrapperDominating { get; set; }

        public static ConfigEntry<int> ConfigWrapperRampage { get; set; }

        public static ConfigEntry<int> ConfigWrapperLudicrousKill { get; set; }

        public static ConfigEntry<int> ConfigWrapperGodLike { get; set; }

        public static ConfigEntry<int> ConfigWrapperMonsterKill { get; set; }

        public static ConfigEntry<float> ConfigWrappertimeBeforeKillingSpreeEnd { get; set; }

        public static ConfigEntry<bool> ConfigWrapperChatBroadcast { get; set; }

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
            ConfigWrapperHeadshot = Config.Bind<int>(
                "TriggerValues",
                "HeadShot",
                -1,
                "Choose at how many kills in a kill streak, this sound should trigger. -1 to desactivate."
                );
            ConfigWrapperDoubleKill = Config.Bind<int>(
                "TriggerValues",
                "DoubleKill",
                -1,
                "Choose at how many kills in a kill streak, this sound should trigger. -1 to desactivate."
                );
            ConfigWrapperTripleKill = Config.Bind<int>(
                "TriggerValues",
                "TripleKill",
                3,
                "Choose at how many kills in a kill streak, this sound should trigger. -1 to desactivate."
                );
            ConfigWrapperMultiKill = Config.Bind<int>(
                "TriggerValues",
                "MultiKill",
                5,
                "Choose at how many kills in a kill streak, this sound should trigger. -1 to desactivate."
                );
            ConfigWrapperDominating = Config.Bind<int>(
                "TriggerValues",
                "Domination",
                8,
                "Choose at how many kills in a kill streak, this sound should trigger. -1 to desactivate."
                );
            ConfigWrapperRampage = Config.Bind<int>(
                "TriggerValues",
                "Rampage",
                12,
                "Choose at how many kills in a kill streak, this sound should trigger. -1 to desactivate."
                );
            ConfigWrapperLudicrousKill = Config.Bind<int>(
                "TriggerValues",
                "LudicrousKill",
                16,
                "Choose at how many kills in a kill streak, this sound should trigger. -1 to desactivate."
                );
            ConfigWrapperGodLike = Config.Bind<int>(
                "TriggerValues",
                "GodLike",
                20,
                 "Choose at how many kills in a kill streak, this sound should trigger. -1 to desactivate."
                );
            ConfigWrapperMonsterKill = Config.Bind<int>(
                "TriggerValues",
                "MonsterKill",
                25,
                "Choose at how many kills in a kill streak, this sound should trigger. -1 to desactivate."
                );
            ConfigWrappertimeBeforeKillingSpreeEnd = Config.Bind<float>(
                "Timer",
                "MaxTimeToEnd",
                3.0f,
                "How much seconds are needed before the killing streak reset"
                );
            ConfigWrapperChatBroadcast = Config.Bind<bool>(
                "Host",
                "BroadcastInChat",
                true,
                "Do you want (when hosting) a message to be sent into the chat when one user reached GodLike or MonsterKill ?"
                );

            On.RoR2.CharacterBody.Start += (orig, self) =>
            {
                orig(self);
                if (self.master && self.master.GetComponent<PlayerCharacterMasterController>())
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
            if (pmc)
            {
                if (pmc.networkUser.isLocalPlayer)
                {
                    Announcer announcer = damageDealtMessage.attacker.GetComponent<Announcer>();
                    if (announcer)
                    {
                        if (damageDealtMessage.victim)
                        {
                            HealthComponent healthComp = damageDealtMessage.victim.GetComponent<HealthComponent>();
                            if (healthComp)
                            {
                                if (healthComp.combinedHealth <= damageDealtMessage.damage)
                                {

                                    announcer.RegisterKill();
#if DEBUG
                                    Debug.Log("Kill registered");
#endif
                                }
                                else
                                {
#if DEBUG
                                    Debug.Log("Hit! Damage = " + damageDealtMessage.damage + " on " + healthComp.combinedHealth + "/" + healthComp.fullCombinedHealth);
                                    Debug.Log("------");
#endif
                                }
                            }
                            else
                            {
#if DEBUG
                                Debug.LogWarning("Victim has no healthComp !");
#endif
                            }

                        }
                        else
                        {
#if DEBUG
                            Debug.Log("Damage isnt toward a victim.");
#endif
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

        private DamageReport damageReportRecieved = null;

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            if (damageReport != this.damageReportRecieved)
            {
                this.damageReportRecieved = damageReport;
                damageReport.attacker.gameObject.GetComponent<Announcer>().RegisterKill();
            }
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
#if DEBUG
            if (NetworkServer.active)
            {
                Debug.Log("Kill registerd " + this.killSpreeCount + " On " + this.gameObject.GetComponent<CharacterBody>().GetUserName());
            }
#endif
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
#if DEBUG
                SendChat("made a triple kill... How lame !");
#endif
            }
            else if (killSpreeCount == EpicKillStreaksAnnouncer.ConfigWrapperMultiKill.Value)
            {
                PlaySound(sound4);
            }
            else if (killSpreeCount == EpicKillStreaksAnnouncer.ConfigWrapperDominating.Value)
            {
                PlaySound(sound5);
            }
            else if (killSpreeCount == EpicKillStreaksAnnouncer.ConfigWrapperRampage.Value)
            {
                PlaySound(sound6);
            }
            else if (killSpreeCount == EpicKillStreaksAnnouncer.ConfigWrapperLudicrousKill.Value)
            {
                PlaySound(sound7);
                SendChat("just did a Ludicrous Kill");
            }
            else if (killSpreeCount == EpicKillStreaksAnnouncer.ConfigWrapperGodLike.Value)
            {
                PlaySound(sound8);
                SendChat("is reaching God Like");
            }
            else if (killSpreeCount == EpicKillStreaksAnnouncer.ConfigWrapperMonsterKill.Value)
            {
                PlaySound(sound9);
                SendChat("MADE A M-M-MONSTER KILLLL !");
            }
        }

        private void SendChat(string message)
        {
            if (NetworkServer.active && EpicKillStreaksAnnouncer.ConfigWrapperChatBroadcast.Value)
            {
                R2API.Utils.ChatMessage.SendColored(this.gameObject.GetComponent<CharacterBody>().master.GetComponent<PlayerCharacterMasterController>().networkUser.GetNetworkPlayerName().GetResolvedName() + " " + message, "#FFC300");
            }
        }
    }
}
