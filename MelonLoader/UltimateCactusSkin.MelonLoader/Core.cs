using CustomizeLib.MelonLoader;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(UltimateCactusSkin.MelonLoader.Core), "UltimateCactusSkin", "1.0.0", "Salmon, AutumnLin", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace UltimateCactusSkin.MelonLoader
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "skin_933");
            CustomCore.RegisterCustomPlantSkin<UltimateCactus>((int)PlantType.UltimateCactus, ab.GetAsset<GameObject>("Prefab"),
                ab.GetAsset<GameObject>("Preview"), (p) =>
                {
                    p.shoot = p.transform.FindChild("Shoot");
                    p.shoot2 = p.transform.FindChild("Shoot2");
                });
            CustomCore.RegisterCustomBullet<Bullet_ultimateCactus>((BulletType)1800, ab.GetAsset<GameObject>("Bullet_ultimateCactus"));
        }
    }

    [HarmonyPatch(typeof(UltimateCactus), nameof(UltimateCactus.GetBulletType))]
    public static class UltimateCactusPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(UltimateCactus __instance, ref BulletType __result)
        {
            if (__instance.gameObject.name == "Prefab")
            {
                __result = (BulletType)1800;
                return false;
            }
            return true;
        }
    }
}