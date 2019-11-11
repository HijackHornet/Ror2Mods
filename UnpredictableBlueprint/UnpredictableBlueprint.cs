namespace UnpredictableBlueprint
{
    using BepInEx;
    using BepInEx.Configuration;
    using UnityEngine;
    using RoR2;
    [BepInDependency("com.bepis.r2api")]

    [BepInPlugin("com.hijackhornet.unpredictableblueprint", "Unpredictable Blueprint", "1.0")]

    public class UnpredictableBlueprint : BaseUnityPlugin
    {
        private float chanceOfBeingUnpredictable = 1.0f;

        public void Awake()
        {
            On.RoR2.BlueprintTerminal.Start += (orig, self) =>
            {
                Debug.Log("BluePrint Init");
                orig(self);
                Debug.Log("BluePrint Init2");
                self.gameObject.AddComponent(typeof(UnpredictableBlueprintDataComponent));
                if (Random.Range(0.0f, 1.0f) <= chanceOfBeingUnpredictable)
                {
                    Debug.Log("BluePrint is Unpredictable");
                    this.GetComponent<UnpredictableBlueprintDataComponent>().isUnpredictable = true;
                }
                
            };
            On.RoR2.BlueprintTerminal.GrantUnlock += BlueprintTerminal_GrantUnlock;
        }

        private void BlueprintTerminal_GrantUnlock(On.RoR2.BlueprintTerminal.orig_GrantUnlock orig, BlueprintTerminal self, Interactor interactor)
        {
            Debug.Log("Asking for item");
            orig(self,interactor);
            if (this.GetComponent<UnpredictableBlueprintDataComponent>().isUnpredictable)
            {
                Debug.Log("Rerolling");
                self.GetComponent<BlueprintTerminal>().Start();
            }
        }
    }
    public class UnpredictableBlueprintDataComponent : MonoBehaviour
    {
        public bool isUnpredictable = false;
    }
}
