using CustomizeLib.MelonLoader;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(SuperShovelLevel.MelonLoader.Core), "SuperShovelLevel.MelonLoader", "1.0.0", "Administrator", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace SuperShovelLevel.MelonLoader
{
    public class Core : MelonMod
    {
        public static int levelID = -1;

        public override void OnInitializeMelon()
        {
            CustomLevelData customLevelData = new CustomLevelData();
            customLevelData.Name = () => "超级随机：铲子";
            customLevelData.SceneType = SceneType.Day_6;
            customLevelData.RowCount = 6;
            customLevelData.WaveCount = () => 100;
            customLevelData.BgmType = MusicType.Day_drum;
            customLevelData.Sun = () => 1000;
            customLevelData.Logo = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "supershovellevel").GetAsset<Sprite>("icon");
            customLevelData.ZombieList = () => new List<ZombieType>()
            {
                ZombieType.RandomZombie,
                ZombieType.RandomPlusZombie,
                ZombieType.DiamondRandomZombie
            };
            customLevelData.NeedSelectCard = true;
            customLevelData.AdvBuffs = () => new List<int>() { 16 };
            levelID = CustomCore.RegisterCustomLevel(customLevelData);
        }
    }

    [HarmonyPatch(typeof(CardUI), nameof(CardUI.Start))]
    public static class CardUIPatch
    {
        [HarmonyPostfix]
        public static void PostStart(CardUI __instance)
        {
            if (GameAPP.theBoardLevel == Core.levelID && (int)GameAPP.theBoardType == 66)
            {
                var plantType = (PlantType)__instance.theSeedType;
                if (plantType == PlantType.Present || plantType == PlantType.PresentZombie || plantType == PlantType.SnowPresent)
                    __instance.gameObject.SetActive(false);
            }
        }
    }

    [HarmonyPatch(typeof(Plant), nameof(Plant.Die))]
    public static class PlantPatch
    {
        [HarmonyPostfix]
        public static void PostDieEvent(Plant __instance, ref Plant.DieReason reason)
        {
            if (GameAPP.theBoardLevel == Core.levelID && (int)GameAPP.theBoardType == 66 && reason == Plant.DieReason.ByShovel)
            {
                var list = GameAPP.resourcesManager.allPlants.ToArray().ToList();
                list.Remove(PlantType.Nothing);
                list.Remove(PlantType.Pit);
                list.Remove(PlantType.Refrash);
                list.Remove(PlantType.Extract_single);
                list.Remove(PlantType.Extract_ten);
                list.Remove(PlantType.MagnetBox);
                list.Remove(PlantType.MagnetInterface);
                Lawnf.SetDroppedCard(__instance.axis.transform.position, list[UnityEngine.Random.Range(0, list.Count)], 50);

                if (UnityEngine.Random.Range(0, 100) <= 2)
                {
                    var mgr = GameAPP.gameAPP.GetComponent<TravelMgr>();
                    if (mgr == null)
                        return;
                    switch (UnityEngine.Random.Range(0, 3))
                    {
                        case 0:
                            {
                                var advUpgrades = mgr.advancedUpgrades.Where(b => !b).ToList();
                                int index = UnityEngine.Random.Range(0, advUpgrades.Count);
                                mgr.advancedUpgrades[index] = true;
                                if (TravelMgr.advancedBuffs.ContainsKey(index))
                                    InGameText.Instance.ShowText(TravelMgr.advancedBuffs[index], 3f);
                            }
                            break;
                        case 1:
                            {
                                var debuffs = mgr.debuff.Where(b => !b).ToList();
                                int index = UnityEngine.Random.Range(0, debuffs.Count);
                                mgr.debuff[index] = true;
                                if (TravelMgr.debuffs.ContainsKey(index))
                                    InGameText.Instance.ShowText(TravelMgr.debuffs[index], 3f);
                            }
                            break;
                        case 2:
                            {
                                var ultiUpgrades = mgr.ultimateUpgrades.Select(b => b == 0).ToList();
                                int index = UnityEngine.Random.Range(0, ultiUpgrades.Count);
                                if (mgr.ultimateUpgrades[index] == 0)
                                    mgr.ultimateUpgrades[index] = 1;
                                else
                                    mgr.ultimateUpgrades[index] = 2;
                                if (TravelMgr.ultimateBuffs.ContainsKey(index))
                                    InGameText.Instance.ShowText(TravelMgr.ultimateBuffs[index], 3f);
                            }
                            break;
                    }
                }
            }
        }
    }
}