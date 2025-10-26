using CustomizeLib.MelonLoader;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static MelonLoader.MelonLogger;

[assembly: MelonInfo(typeof(UltimateDoomMinigun.MelonLoader.Core), "UltimateDoomMinigun", "1.0.0", "Salmon", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace UltimateDoomMinigun.MelonLoader
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "ultimatedoomminigun");
            CustomCore.RegisterCustomPlant<UltimateMinigun, UltimateDoomMinigun>(UltimateDoomMinigun.PlantID, ab.GetAsset<GameObject>("UltimateDoomMinigunPrefab"),
                ab.GetAsset<GameObject>("UltimateDoomMinigunPreview"), [], 0.5f, 0f, 300, 300, 90f, 1000);
            CustomCore.AddPlantAlmanacStrings(UltimateDoomMinigun.PlantID, $"究级速射毁灭机枪射手({UltimateDoomMinigun.PlantID})",
                "发射毁灭子弹的加特林速射机枪\n" +
                "<color=#0000FF>毁灭菇机枪射手的限定形态</color>\n\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>使用条件：</color><color=red>①融合或转化毁灭机枪射手时有2%概率变异\n" +
                "②神秘模式\n" +
                "*可使用豌豆射手切回毁灭机枪射手</color>\n" + 
                "<color=#3D1400>伤害：</color><color=red>300x6/0.5秒，1800</color>\n" +
                "<color=#3D1400>特点：</color><color=red>①每次发射有5%概率改为大毁灭菇子弹，每第16发必改为大毁灭菇子弹，大毁灭菇伤害1800，半径3格无衰减溅射。\n" +
                "②启动射击需要预热1.5秒。</color>\n\n" +
                "<color=#3D1400>咕咕咕</color>");
            CustomCore.TypeMgrExtra.LevelPlants.Add((PlantType)UltimateDoomMinigun.PlantID, CardLevel.Red);
            CustomCore.AddFusion((int)PlantType.DoomGatling, UltimateDoomMinigun.PlantID, (int)PlantType.Peashooter);
            CustomCore.AddFusion((int)PlantType.DoomGatling, (int)PlantType.Peashooter, UltimateDoomMinigun.PlantID);
        }
    }

    [RegisterTypeInIl2Cpp]
    public class UltimateDoomMinigun : MonoBehaviour
    {
        public static int PlantID = 1933;
        public int shootCount = 0;
        public void Start()
        {
            plant.shoot = plant.gameObject.transform.FindChild("GatlingPea_head/Shoot");
            if (plant.board != null)
                plant.board.OnPlantCreate(plant);
            plant.UpdateText();
            plant.ReplaceSprite();
            plant.FirstMeet();
        }

        public Bullet AnimShoot_Doom()
        {
            shootCount++;

            BulletType bulletType = BulletType.Bullet_doom;
            int damage = plant.attackDamage;
            bool isBigBullet = false;
            if (shootCount % 16 == 0)
            {
                shootCount = 0;
                bulletType = BulletType.Bullet_doom_big;
                damage = 6 * plant.attackDamage;
                isBigBullet = true;
            }
            if (UnityEngine.Random.Range(0, 100) <= 5)
            {
                bulletType = BulletType.Bullet_doom_big;
                damage = 6 * plant.attackDamage;
                isBigBullet = true;
            }

            // 创建子弹
            Bullet bullet = CreateBullet.Instance.SetBullet(
                plant.shoot.transform.position.x,
                plant.shoot.transform.position.y,
                plant.thePlantRow,
                bulletType,
                BulletMoveWay.MoveRight,
                false);

            if (bullet != null)
            {
                // 设置子弹伤害
                bullet.Damage = damage;

                // 加倍子弹速度
                bullet.normalSpeed *= 2f; // 猜测字段名为speed
                if (isBigBullet)
                    bullet.theStatus = BulletStatus.Doom_big;
                // 播放随机射击音效
                GameAPP.PlaySound(UnityEngine.Random.Range(3, 5), 0.5f, 1.0f); // 猜测音效参数
            }

            return bullet;
        }

        public UltimateMinigun plant => gameObject.GetComponent<UltimateMinigun>();
    }

    [HarmonyPatch(typeof(Plant), nameof(Plant.Start))]
    public static class Plant_Start_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(Plant __instance)
        {
            if (__instance != null && __instance.thePlantType == PlantType.DoomGatling && UnityEngine.Random.Range(0, 100) <= 2 &&
                GameAPP.theGameStatus == GameStatus.InGame)
            {
                int column = __instance.thePlantColumn;
                int row = __instance.thePlantRow;
                PlantType firstParent = __instance.firstParent;
                PlantType secondParent = __instance.secondParent;
                __instance.Die();
                var result = CreatePlant.Instance.SetPlant(column, row, (PlantType)UltimateDoomMinigun.PlantID, isFreeSet: true).GetComponent<Plant>();
                result.firstParent = firstParent;
                result.secondParent = secondParent;
            }
        }
    }

    [HarmonyPatch(typeof(UltimateMinigun), nameof(UltimateMinigun.Start))]
    public static class UltimateMinigun_Start_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(UltimateMinigun __instance)
        {
            if (__instance.thePlantType == (PlantType)UltimateDoomMinigun.PlantID)
            {
                return false;
            }
            return true;
        }
    }
}