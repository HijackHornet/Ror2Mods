namespace EpicKillStreaksAnnoncer
{
    using AssetPlus;
    using BepInEx;
    using BepInEx.Configuration;
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;

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

        private float timeBeforeKillingSpreeEnd = 3.0f;

        private float timeSinceLastKill = 0f;

        private int previousKillCount = 0;

        private int killSpreeCount = 0;

        private uint currentSoundId = 0;

        private uint currentSoundTypeId = 0;

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
            timeBeforeKillingSpreeEnd = ConfigWrappertimeBeforeKillingSpreeEnd.Value;
        }

        public void FixedUpdate()
        {
            if (RoR2.NetworkUser.readOnlyInstancesList.Count > 0)
            {
                int newKillCount;
                try
                {
                    newKillCount = RoR2.NetworkUser.readOnlyLocalPlayersList[0].GetCurrentBody().killCount - previousKillCount;
                    previousKillCount = RoR2.NetworkUser.readOnlyLocalPlayersList[0].GetCurrentBody().killCount;
                }
                catch
                {
                    return;
                }

                if (newKillCount > 0)
                {

                    timeSinceLastKill = 0;
                    killSpreeCount += newKillCount;
                   

                    if (killSpreeCount == ConfigWrapperHeadshot.Value)
                    {
                        PlaySound(Headshot);
                    }
                    else if (killSpreeCount == ConfigWrapperDoubleKill.Value)
                    {
                        PlaySound(DoubleKill);
                    }
                    else if (killSpreeCount == ConfigWrapperTripleKill.Value)
                    {
                        PlaySound(TripleKill);
                    }
                    else if ((killSpreeCount >= ConfigWrapperMultiKill.Value) && (killSpreeCount < ConfigWrapperDominating.Value))
                    {
                        PlaySound(MultiKill);
                    }
                    else if ((killSpreeCount >= ConfigWrapperDominating.Value) && (killSpreeCount < ConfigWrapperRampage.Value))
                    {
                        PlaySound(Dominating);
                    }
                    else if ((killSpreeCount >= ConfigWrapperRampage.Value) && (killSpreeCount < ConfigWrapperLudicrousKill.Value))
                    {
                        PlaySound(Rampage);
                    }
                    else if ((killSpreeCount >= ConfigWrapperLudicrousKill.Value) && (killSpreeCount < ConfigWrapperGodLike.Value))
                    {
                        PlaySound(LudicrousKill);
                    }
                    else if ((killSpreeCount >= ConfigWrapperGodLike.Value) && (killSpreeCount < ConfigWrapperMonsterKill.Value))
                    {
                        PlaySound(GodLike);
                    }
                    else if(killSpreeCount >= ConfigWrapperMonsterKill.Value)
                    {
                        PlaySound(MonsterKill);
                    }
                }
                else
                {
                    timeSinceLastKill += Time.fixedDeltaTime;
                }
                if (timeSinceLastKill >= timeBeforeKillingSpreeEnd)
                {
                    killSpreeCount = 0;
                    currentSoundTypeId = 0;
                }
            }
        }

        private void PlaySound(uint eventid)
        {

            if ((currentSoundTypeId != eventid))
            {
                currentSoundTypeId = eventid;
                AkSoundEngine.StopPlayingID(currentSoundId);
                currentSoundId = AkSoundEngine.PostEvent(eventid, RoR2.NetworkUser.readOnlyLocalPlayersList[0].GetCurrentBody().gameObject);

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
}
