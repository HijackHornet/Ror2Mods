namespace EpicKillStreaksAnnoncer
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

    [BepInPlugin("com.hijackhornet.epickillstreaksannoncer", "Epic KillStreaks Annoncer", "1.0")]

    public class EpicKillStreaksAnnoncer : BaseUnityPlugin
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
                if ((self.master != null) && (self.master.GetComponent<PlayerCharacterMasterController>()))
                {
                    Debug.Log("Found one player.");
                    self.gameObject.AddComponent<Annoncer>().initSoundsList(Headshot, DoubleKill, TripleKill, MultiKill, Dominating, Rampage, LudicrousKill, GodLike, MonsterKill);
                }
            };
            On.RoR2.GlobalEventManager.OnCharacterDeath += (orig, self, report) =>
            {
                CharacterMaster attacker = null;
                if (report.attackerMaster)
                {
                    attacker = report.attackerMaster;
                }
                else if (report.attackerOwnerMaster)
                {
                    attacker = report.attackerOwnerMaster;
                }
                if (attacker != null)
                {
                    PlayerCharacterMasterController pc = null;
                    pc = attacker.GetComponent<PlayerCharacterMasterController>();
                    if (pc != null)
                    {
                        pc.master.GetBodyObject().GetComponent<Annoncer>().RegisterKill();
                    }
                }

                orig(self, report);
            };
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

    public class Annoncer : NetworkBehaviour
    {
        private float timeBeforeKillingSpreeEnd = 3.0f;

        private float timeSinceLastKill = 0f;

        private int killSpreeCount = 0;

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
            timeBeforeKillingSpreeEnd = EpicKillStreaksAnnoncer.ConfigWrappertimeBeforeKillingSpreeEnd.Value;
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

            if ((currentSoundTypeId != eventid)&&(this.gameObject.GetComponent<CharacterBody>().master.GetComponent<PlayerCharacterMasterController>().networkUser.isLocalPlayer))
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
            Debug.Log("Spree : " + killSpreeCount);

            if (killSpreeCount == EpicKillStreaksAnnoncer.ConfigWrapperHeadshot.Value)
            {
                PlaySound(sound1);
            }
            else if (killSpreeCount == EpicKillStreaksAnnoncer.ConfigWrapperDoubleKill.Value)
            {
                PlaySound(sound2);
            }
            else if (killSpreeCount == EpicKillStreaksAnnoncer.ConfigWrapperTripleKill.Value)
            {
                PlaySound(sound3);
            }
            else if ((killSpreeCount >= EpicKillStreaksAnnoncer.ConfigWrapperMultiKill.Value) && (killSpreeCount < EpicKillStreaksAnnoncer.ConfigWrapperDominating.Value))
            {
                PlaySound(sound4);
            }
            else if ((killSpreeCount >= EpicKillStreaksAnnoncer.ConfigWrapperDominating.Value) && (killSpreeCount < EpicKillStreaksAnnoncer.ConfigWrapperRampage.Value))
            {
                PlaySound(sound5);
            }
            else if ((killSpreeCount >= EpicKillStreaksAnnoncer.ConfigWrapperRampage.Value) && (killSpreeCount < EpicKillStreaksAnnoncer.ConfigWrapperLudicrousKill.Value))
            {
                PlaySound(sound6);
            }
            else if ((killSpreeCount >= EpicKillStreaksAnnoncer.ConfigWrapperLudicrousKill.Value) && (killSpreeCount < EpicKillStreaksAnnoncer.ConfigWrapperGodLike.Value))
            {
                PlaySound(sound7);
            }
            else if ((killSpreeCount >= EpicKillStreaksAnnoncer.ConfigWrapperGodLike.Value) && (killSpreeCount < EpicKillStreaksAnnoncer.ConfigWrapperMonsterKill.Value))
            {
                PlaySound(sound8);
                SendChat("is reaching God Like");
            }
            else if (killSpreeCount >= EpicKillStreaksAnnoncer.ConfigWrapperMonsterKill.Value)
            {
                PlaySound(sound9);
                SendChat("MADE A M-M-MONSTER KILLLL !");
            }
        }

        private void SendChat(string message)
        {
            if (NetworkServer.active)
            {
                Chat.AddMessage(this.gameObject.GetComponent<CharacterBody>().master.GetComponent<PlayerCharacterMasterController>().networkUser.userName + " " + message);
            }
        }
    }
}
