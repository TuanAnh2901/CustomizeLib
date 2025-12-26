using CustomizeLib.MelonLoader;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(SolarHypno.MelonLoader.Core), "SolarHypno", "1.0.0", "Salmon", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace SolarHypno.MelonLoader
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "solarhypno");
            CustomCore.RegisterCustomPlant<SolarCabbage, SolarHypno>((int)SolarHypno.PlantID, ab.GetAsset<GameObject>("SolarHypnoSkinPrefab"),
                ab.GetAsset<GameObject>("SolarHypnoSkinPreview"), new List<(int, int)>
                {
                    ((int)PlantType.UltimateCabbage, (int)PlantType.HypnoShroom),
                    ((int)PlantType.HypnoShroom, (int)PlantType.UltimateCabbage)
                }, 2f, 0f, 300, 300, 7.5f, 450);
            CustomCore.AddPlantAlmanacStrings((int)SolarHypno.PlantID, $"究极太阳神魅惑菇({(int)SolarHypno.PlantID})",
                "光明天降，普度众生。\n" +
                "<color=#0000FF>究极太阳神卷心菜同人亚种</color>\n\n" +
                "<color=#3D1400>贴图作者：@方染Fran、@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>转化配方：</color><color=red>卷心菜←→魅惑菇</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>300x5/2秒</color>\n" +
                "<color=#3D1400>特点：</color><color=red>①被啃咬时魅惑啃咬者（5次后消失）\n" +
                "②魅惑或击中僵尸时产生弹力阳光\n" +
                "③攻击有概率魅惑血量低于50%的僵尸，概率最高为25%\n" +
                "④每30秒1次，召唤太阳</color>\n" +
                "<color=#3D1400>弹力阳光：</color><color=red>①命中目标时，造成300伤害并掉落25阳光，如果目标是魅惑僵尸，则最高造成200伤害并掉落50阳光\n" +
                "②命中目标后，立刻弹向新的目标，最多15次，消失后掉落25阳光</color>\n" +
                "<color=#3D1400>大招：</color><color=red>消耗1000金钱，召唤太阳</color>\n" +
                "<color=#3D1400>词条1：</color><color=red>金光闪闪：究极太阳神魅惑菇的子弹会消耗超过15000阳光部分的0.5%阳光，使该子弹产生的弹力阳光增加（30x消耗阳光）的伤害</color>\n" +
                "<color=#3D1400>词条2：</color><color=red>人造太阳：太阳伤害x3，弹力阳光伤害x3</color>\n" +
                "<color=#3D1400>连携词条：</color><color=red>星神合一：究极杨桃大帝（及其亚种）与究极太阳神卷心菜（及究极太阳神魅惑菇）的数量均不小于10时：太阳持续时间无限且伤害x5，固定每3秒召唤太阳神流星，伤害为1800×(1+大帝数量)×(1+0.5×太阳神数量)，分裂出180个伤害400的子弹，并掉落1250阳光</color>\n" +
                "<color=#3D1400>咕咕咕</color>"); 
            CustomCore.RegisterCustomBullet<Bullet_sunCabbage>(SolarHypno.BulletID, ab.GetAsset<GameObject>("Bullet_HypnoSunCabbage"));
            CustomCore.RegisterCustomBullet<Bullet_sunCabbage>(SolarHypno.BulletSkinID, ab.GetAsset<GameObject>("Bullet_HypnoSunCabbageSkin"));

            CustomCore.AddFusion((int)PlantType.UltimateCabbage, (int)SolarHypno.PlantID, (int)PlantType.Cabbagepult);
            CustomCore.AddFusion((int)PlantType.UltimateCabbage, (int)PlantType.Cabbagepult, (int)SolarHypno.PlantID);
        }
    }

    [RegisterTypeInIl2Cpp]
    public class SolarHypno : MonoBehaviour
    {
        public static PlantType PlantID = (PlantType)1954;
        public static BulletType BulletID = (BulletType)1954;
        public static BulletType BulletSkinID = (BulletType)1955;

        public SolarCabbage plant => gameObject.GetComponent<SolarCabbage>();

        public void Awake()
        {
            if (plant == null) return;
            plant.shoot = transform.FindChild("Back/Shoot");
            plant.attributeCount = 5;
        }

        public void Update()
        {
            if (plant == null) return;
            if (plant.attributeCount <= 0) plant.Die();
        }

        public void CreateSun()
        {

        }
    }

    [HarmonyPatch(typeof(SolarCabbage))]
    public static class SolarCabbage_Patch
    {
        [HarmonyPatch(nameof(SolarCabbage.GetBulletType))]
        [HarmonyPostfix]
        public static void Postfix(SolarCabbage __instance, ref BulletType __result)
        {
            if (__instance.thePlantType == SolarHypno.PlantID)
            {
                if (__instance.name.Contains("Skin"))
                    __result = SolarHypno.BulletSkinID;
                else
                    __result = SolarHypno.BulletID;
            }
        }
    }

    [HarmonyPatch(typeof(Thrower), nameof(Thrower.Shoot1))]
    public static class Thrower_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Thrower __instance, ref Bullet __result)
        {
            if (__instance != null && __instance.thePlantType == SolarHypno.PlantID && __result != null)
                __result.from = __instance;
        }
    }

    [HarmonyPatch(typeof(Zombie), nameof(Zombie.PlayEatSound))]
    public static class Zombie_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(Zombie __instance)
        {
            if (__instance != null && __instance.theAttackTarget != null && __instance.theAttackTarget.GetComponent<Plant>().thePlantType == SolarHypno.PlantID)
            {
                __instance.theAttackTarget.GetComponent<Plant>().attributeCount--;
                __instance.SetMindControl();
            }
        }
    }

    [HarmonyPatch(typeof(UltimateFootballZombie), nameof(UltimateFootballZombie.AttackEffect))]
    public static class UltimateFootballZombie_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref Plant plant)
        {
            if (plant != null && plant.thePlantType == SolarHypno.PlantID)
            {
                return false;
            }
            return true;
        }
    }
}