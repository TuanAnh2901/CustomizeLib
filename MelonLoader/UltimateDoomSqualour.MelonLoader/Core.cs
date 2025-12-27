using CustomizeLib.MelonLoader;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using System.Collections;
using UnityEngine;

[assembly: MelonInfo(typeof(UltimateDoomSqualour.MelonLoader.Core), "UltimateDoomSqualour", "1.0.0", "Salmon", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace UltimateDoomSqualour.MelonLoader
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "ultimatedoomsqualour");
            CustomCore.RegisterCustomPlant<Squalour, UltimateDoomSqualour>(
                (int)UltimateDoomSqualour.PlantID,
                ab.GetAsset<GameObject>("UltimateDoomSqualourPrefab"),
                ab.GetAsset<GameObject>("UltimateDoomSqualourPreview"),
                new List<(int, int)>
                {
                    ((int)PlantType.Squalour, (int)PlantType.NuclearDoomCherry),
                    ((int)PlantType.NuclearDoomCherry, (int)PlantType.Squalour)
                },
                0f, 0f, 1800, 300, 0f, 250
            );
            CustomCore.AddPlantAlmanacStrings((int)UltimateDoomSqualour.PlantID,
                $"聚爆猫瓜({(int)UltimateDoomSqualour.PlantID})",
                "蕴含着核能力量的红温猫瓜，可千万不要招惹她…\n" +
                "<color=#0000FF>核爆樱桃同人亚种</color>\n\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>转化配方：</color><color=red>樱桃炸弹←→猫瓜</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>3600</color>\n" +
                "<color=#3D1400>特点：</color><color=red>①每碾压1个僵尸，聚爆核爆的基础威力增加1200，最多7200\n" +
                "②碾压火红莲造成7200的毁灭菇爆炸，不留坑，并返还卡片\n" +
                "③碾压僵尸火红莲造成等同于核爆樱桃的爆炸，不留坑，并返还卡片</color>\n\n" +
                "<color=#3D1400>窝红温辣</color>"
            );
            CustomCore.AddFusion((int)PlantType.NuclearDoomCherry, (int)UltimateDoomSqualour.PlantID, (int)PlantType.CherryBomb);
            CustomCore.AddFusion((int)PlantType.NuclearDoomCherry, (int)PlantType.CherryBomb, (int)UltimateDoomSqualour.PlantID);
        }
    }

    [RegisterTypeInIl2Cpp]
    public class UltimateDoomSqualour : MonoBehaviour
    {
        public static PlantType PlantID = (PlantType)1952;

        public void Update()
        {
            if (plant == null) return;

        }

        public Squalour plant => gameObject.GetComponent<Squalour>();
    }

    [HarmonyPatch(typeof(Squalour))]
    public class SqualourPatch
    {
        [HarmonyPatch(nameof(Squalour.LourDie))]
        [HarmonyPrefix]
        public static bool Prefix(Squalour __instance)
        {
            if (__instance.thePlantType == UltimateDoomSqualour.PlantID)
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(Squalour.ActionOnZombie))]
        [HarmonyPrefix]
        public static bool Prefix(Squalour __instance, ref Zombie zombie)
        {
            if (__instance.thePlantType == UltimateDoomSqualour.PlantID)
            {
                if (zombie == null) return false;
                zombie.TakeDamage(DmgType.Squash, 1800);
                zombie.TakeDamage(DmgType.Carred, 3600);
                __instance.squashCount++;
                __instance.squashed = true;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Squash))]
    public class SquashPatch
    {
        [HarmonyPatch(nameof(Squash.AttackZombie))]
        [HarmonyPrefix]
        public static bool Prefix(Squash __instance)
        {
            if (__instance.thePlantType == UltimateDoomSqualour.PlantID)
            {
                {
                    Vector3 position = __instance.axis.position;
                    Vector2 centerPos = new Vector2(position.x, position.y);

                    // 检测区域内的僵尸
                    int zombieLayer = LayerMask.GetMask("Zombie");
                    Collider2D[] colliders = Physics2D.OverlapBoxAll(
                        centerPos,
                        new Vector2(1f, 3f),
                        0f,
                        zombieLayer
                    );

                    if (colliders != null)
                    {
                        foreach (var collider in colliders)
                        {
                            Zombie zombie;
                            if (collider.TryGetComponent<Zombie>(out zombie))
                            {
                                // 尝试攻击僵尸
                                bool attacked = __instance.AttackLandZombie(zombie);

                                // 检查是否符合特殊攻击条件
                                if (attacked && (zombie.theZombieRow == __instance.thePlantRow || zombie.theZombieType == ZombieType.DancePolZombie2))
                                {
                                    __instance.ActionOnZombie(zombie);
                                }
                            }
                        }
                    }

                    // 检查当前位置的格子类型
                    int col = Lawnf.GetColumnFromX(position.x);
                    int row = __instance.thePlantRow;

                    if (__instance.board.GetBoxType(col, row) != BoxType.Water)
                    {
                        GameAPP.PlaySound(74, 0.5f, 1f);
                        ScreenShake.shakeDuration = 0.05f;
                    }
                    else // 正常格子
                    {
                        // 创建特效
                        GameObject effectPrefab = Resources.Load<GameObject>("Particle/Anim/Water/WaterSplashPrefab");
                        Vector3 spawnPos = new Vector3(position.x, position.y - 1.75f, 0f);
                        Transform boardTransform = __instance.board.transform;
                        GameObject.Instantiate(effectPrefab, spawnPos, Quaternion.identity, boardTransform);

                        GameAPP.PlaySound(75, 0.5f, 1f);
                        __instance.Die();
                    }
                }
                {
                    int damage = 3600 + 1200 * Mathf.Min(__instance.GetComponent<Squalour>().squashCount, 3);
                    var position = __instance.axis.transform.position;
                    position.y -= 0.5f;
                    __instance.board.SetDoom(__instance.thePlantColumn, __instance.thePlantRow, false, false, position, damage);

                    Doom.SetDoom(__instance.board, __instance.axis.transform.position, DoomType.Nuclear);

                    var radiation = UnityEngine.Object.Instantiate(
                        Resources.Load<GameObject>("plants/cherrybomb/nucleardoomcherry/Radiation"),
                        __instance.axis.transform.position,
                        Quaternion.identity, __instance.board.transform).GetComponent<Radiation>();
                    radiation.damage = damage;
                    // 调用死亡方法
                    __instance.Die();
                }
                return false;
            }
            return true;
        }
    }
}