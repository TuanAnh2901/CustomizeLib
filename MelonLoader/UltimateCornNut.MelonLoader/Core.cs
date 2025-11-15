using CustomizeLib.MelonLoader;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(UltimateCornNut.MelonLoader.Core), "UltimateCornNut", "1.0.0", "Salmon", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace UltimateCornNut.MelonLoader
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "ultimatecornnut");
            CustomCore.RegisterCustomPlant<UltimatePortalNut, UltimateCornNut>(UltimateCornNut.PlantID, ab.GetAsset<GameObject>("UltimateCornNutPrefab"),
                ab.GetAsset<GameObject>("UltimateCornNutPreview"), new List<(int, int)> { }, 0f, 0f, 0, 16000, 90f, 750);
            CustomCore.AddPlantAlmanacStrings(UltimateCornNut.PlantID, $"究级超时空黄油坚果({UltimateCornNut.PlantID})",
                "回溯和免疫伤害能力极强的超时空黄油坚果\n" +
                "<color=#0000FF>究极超时空坚果的限定形态</color>\n\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>使用条件：</color><color=red>①融合或转化究级超时空坚果时有2%概率变异\n" +
                "②神秘模式\n" +
                "*可使用坚果切回究级超时空坚果</color>\n" +
                "<color=#3D1400>韧性：</color><color=red>16000，限伤1000</color>\n" +
                "<color=#3D1400>特点：</color><color=red>①铁植物\n" +
                "②免疫碾压并击退，有1/3概率免疫伤害，受到2000伤害后回溯3秒前血量并免疫伤害2秒，30秒未受伤害时，回复1倍韧性血量\n" +
                "③被啃咬时，对啃咬者施加2秒的黄油定身和3秒传送状态\n" +
                "<color=#3D1400>“未来无法改变，过去可以改变吗？我们生活的现在，是过去的延伸，也是未来的基石，也可以是更遥远的过去，我们已经经历的时间是无法改变的，他们有固定的节点和顺序，但是我们可以通过一种微妙的方式去改变它，首先要做的，就是改变我们自己，现在会影响到未来，未来也会影响到过去，我们在尝试改变，我们在努力改变，”究极超时空黄油坚果说完便消失在了原地，随后又突然出现，“死亡是无法跨越的时间节点，如果可以改变它，我愿意尝试。”</color>");
            CustomCore.TypeMgrExtra.IsNut.Add((PlantType)UltimateCornNut.PlantID);
            CustomCore.TypeMgrExtra.LevelPlants.Add((PlantType)UltimateCornNut.PlantID, CardLevel.Red);
            CustomCore.TypeMgrExtra.IsMagnetPlants.Add((PlantType)UltimateCornNut.PlantID);
            CustomCore.AddUltimatePlant((PlantType)UltimateCornNut.PlantID);
            CustomCore.AddFusion((int)PlantType.UltimatePortalNut, UltimateCornNut.PlantID, (int)PlantType.WallNut);
            CustomCore.AddFusion((int)PlantType.UltimatePortalNut, (int)PlantType.WallNut, UltimateCornNut.PlantID);
        }
    }

    [RegisterTypeInIl2Cpp]
    public class UltimateCornNut : MonoBehaviour
    {
        public static int PlantID = 1943;

        public UltimatePortalNut plant => gameObject.GetComponent<UltimatePortalNut>();
    }

    [HarmonyPatch(typeof(Zombie), nameof(Zombie.PlayEatSound))]
    public static class Zombie_PlayEatSound_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Zombie __instance)
        {
            if (__instance != null && __instance.theAttackTarget != null)
            {
                if (__instance.theAttackTarget.TryGetComponent<Plant>(out var plant) && plant != null && (int)plant.thePlantType == UltimateCornNut.PlantID)
                {
                    __instance.Buttered(2f);
                    __instance.SetPortaled(3f);
                    if (plant.attributeCountdown == 0f && UnityEngine.Random.Range(0, 4) == 0)
                    {
                        plant.attributeCountdown = 10f;

                        // 创建传送粒子效果
                        Vector3 zombiePos = __instance.axis.position;
                        zombiePos.y += 0.5f;
                        ParticleManager.Instance.SetParticle(ParticleType.RandomCloud, zombiePos, __instance.theZombieRow);

                        // 传送僵尸到右侧
                        GameObject zombieObj = __instance.gameObject;
                        float targetX = __instance.board.boardMaxX + 0.5f;
                        float targetY = Mouse.Instance.GetLandY(targetX, __instance.theZombieRow);
                        Vector3 targetPos = new Vector3(targetX, targetY, 0f);
                        __instance.AdjustPosition(targetPos);
                        __instance.BeSmall();
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(UltimatePortalNut), nameof(UltimatePortalNut.DecreaseHealth))]
    public static class UltimatePortalNut_DecreaseHealth_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(UltimatePortalNut __instance, ref int value)
        {
            if (__instance != null && (int)__instance.thePlantType == UltimateCornNut.PlantID)
            {
                if (__instance.attributeCount + value > 2000)
                {
                    __instance.attributeCount = 4001;
                    __instance.restoreCount = -1;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Plant), nameof(Plant.Start))]
    public static class Plant_Start_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(Plant __instance)
        {
            if (__instance != null && __instance.thePlantType == PlantType.UltimatePortalNut && UnityEngine.Random.Range(0, 100) <= 1)
            {
                var row = __instance.thePlantRow;
                var column = __instance.thePlantColumn;
                __instance.Die(Plant.DieReason.ByShovel);
                CreatePlant.Instance.SetPlant(column, row, (PlantType)UltimateCornNut.PlantID, isFreeSet: true);
            }
        }
    }

    [HarmonyPatch(typeof(TypeMgr), nameof(TypeMgr.UncrashablePlant))]
    public static class TypeMgr_UncrashablePlant_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(TypeMgr __instance, ref Plant plant, ref bool __result)
        {
            if (plant != null && (int)plant.thePlantType == UltimateCornNut.PlantID)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}