using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using BepInEx;
using UnityEngine;
using BepInEx.Unity.IL2CPP;
using System.Reflection;
using Unity.VisualScripting;
using CustomizeLib.BepInEx;

namespace DoomGatlingBlover.BepInEx
{
    [BepInPlugin("salmon.doomgatlingblover", "DoomGatlingBlover", "1.0")]
    public class Core : BasePlugin
    {
        public override void Load()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            ClassInjector.RegisterTypeInIl2Cpp<DoomGatlingBlover>();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "doomgatlingblover");
            CustomCore.RegisterCustomPlant<UltimateGatlingBlover, DoomGatlingBlover>((int)DoomGatlingBlover.PlantID, ab.GetAsset<GameObject>("DoomGatlingBloverPrefab"),
                ab.GetAsset<GameObject>("DoomGatlingBloverPreview"), new List<(int, int)>
                {
                    ((int)PlantType.DoomGatling, (int)PlantType.Blover),
                    ((int)PlantType.Blover, (int)PlantType.DoomGatling)
                }, 1.5f, 0f, 300, 300, 7.5f, 525);
            CustomCore.AddPlantAlmanacStrings((int)DoomGatlingBlover.PlantID, $"毁灭爆破浮游炮({(int)DoomGatlingBlover.PlantID})",
                "毁灭爆破浮游炮，会与攻速更快的植物协同攻击提升火力\n" +
                "<color=#0000FF>毁灭机枪射手同人亚种</color>\n\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>转化配方：</color><color=red>铲除←→三叶草\n" +
                "<color=#3D1400>伤害：</color><color=red>300/1.5秒，1800</color>\n" +
                "<color=#3D1400>特性：</color><color=red>飞行</color>\n" +
                "<color=#3D1400>特点：</color><color=red>①下方植物攻击间隔低于1.5秒时，自身攻击改为与其一致。\n" +
                "②每8发子弹改为大毁灭菇子弹。</color>\n" +
                "<color=#3D1400>词条1:</color><color=red>枕戈待旦：攻击速度减半</color>\n" +
                "<color=#3D1400>词条2:</color><color=red>核能威慑：每4发必为大毁灭菇</color>\n\n" +
                "<color=#3D1400>咕咕咕</color>");
            CustomCore.TypeMgrExtra.FlyingPlants.Add(DoomGatlingBlover.PlantID);
        }
    }

    public class DoomGatlingBlover : MonoBehaviour
    {
        public static PlantType PlantID = (PlantType)1950;

        public UltimateGatlingBlover plant => gameObject.GetComponent<UltimateGatlingBlover>();
        public int doomTimes = 0;

        public void Start()
        {
            plant.shoot = gameObject.transform.FindChild("PeaShooter_Head/Shoot");
        }

        public void Update()
        {
            if (plant != null && GameAPP.theGameStatus == GameStatus.InGame)
            {
                if (Lawnf.TravelAdvanced((AdvBuff)2) && plant.thePlantAttackCountDown > 0.75f)
                    plant.thePlantAttackCountDown = 0.75f;
                plant.attributeCountdown = float.MaxValue; // fuck lpp
            }
        }

        public void AnimShoot_DoomGatlingBlover()
        {
            doomTimes++;

            // 根据条件选择子弹类型
            if (doomTimes < (Lawnf.TravelAdvanced((AdvBuff)3) ? 4 : 8))
            {
                // 发射普通子弹
                var bullet = CreateBullet.Instance.SetBullet(
                    plant.shoot.position.x,
                    plant.shoot.position.y,
                    plant.thePlantRow,
                    BulletType.Bullet_doom,
                    0, false);

                bullet.Damage = plant.attackDamage;
            }
            else
            {
                // 发射强力子弹
                var bullet = CreateBullet.Instance.SetBullet(
                    plant.shoot.position.x,
                    plant.shoot.position.y,
                    plant.thePlantRow,
                    BulletType.Bullet_doom_big,
                    0, false);
                // 设置强力子弹属性
                bullet.Damage = 6 * plant.attackDamage;
                bullet.theStatus = BulletStatus.Doom_big;

                // 重置射击计数
                doomTimes = 0;
            }

            GameAPP.PlaySound(UnityEngine.Random.Range(3, 5), 0.5f, 1f);
        }
    }

    [HarmonyPatch(typeof(UltimateGatlingBlover), nameof(UltimateGatlingBlover.UpdateInterval))]
    public static class UltimateGatlingBlover_UpdateInterval_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(UltimateGatlingBlover __instance)
        {
            if (__instance != null && __instance.thePlantType == DoomGatlingBlover.PlantID)
            {
                int plantColumn = __instance.thePlantColumn;
                int plantRow = __instance.thePlantRow;

                // 获取指定位置的植物列表
                var plants = Lawnf.Get1x1Plants(plantColumn, plantRow); // 原方法：Lawnf__Get1x1Plants

                float minInterval = Lawnf.TravelAdvanced((AdvBuff)2) ? 0.75f : 1.5f;

                if (plants != null)
                {
                    // 遍历所有植物，找到最小的攻击间隔
                    foreach (Plant plant in plants)
                    {
                        if (plant != null)
                        {
                            float plantInterval = plant.thePlantAttackInterval; // 原属性：*(float *)(plant + 280LL)

                            // 检查攻击间隔是否有效且小于当前最小值，并且不是自身
                            if (plantInterval > 0.0f && minInterval > plantInterval && plant != __instance)
                            {
                                minInterval = plantInterval;
                            }
                        }
                    }
                }

                // 设置植物攻击间隔
                __instance.thePlantAttackInterval = minInterval;

                // 确保最小间隔不小于0.1秒
                if (minInterval < 0.1f)
                {
                    minInterval = 0.1f;
                }

                // 更新动画速度
                if (__instance.anim != null)
                {
                    // 设置动画速度参数，速度与间隔成反比
                    __instance.anim.SetFloat("shootSpeed", 1.0f / minInterval); // 原字符串：StringLiteral_5471
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(UltimateGatlingBlover))]
    public static class UltimateGatlingBlover_DieEvent_Patch
    {
        [HarmonyPatch(nameof(UltimateGatlingBlover.DieEvent))]
        [HarmonyPrefix]
        public static bool Prefix(UltimateGatlingBlover __instance, ref Plant.DieReason reason)
        {
            if (__instance != null && __instance.thePlantType == DoomGatlingBlover.PlantID && reason == Plant.DieReason.ByShovel)
            {
                Lawnf.SetDroppedCard(__instance.shoot.position, PlantType.DoomGatling);
                return false;
            }
            return true;
        }
    }
}