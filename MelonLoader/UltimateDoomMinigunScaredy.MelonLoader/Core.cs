using CustomizeLib.MelonLoader;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using Unity.VisualScripting;
using UnityEngine;

[assembly: MelonInfo(typeof(UltimateDoomMinigunScaredy.MelonLoader.Core), "UltimateDoomMinigunScaredy", "1.0.0", "Salmon", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace UltimateDoomMinigunScaredy.MelonLoader
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "ultimatedoomminigunscaredy");
            CustomCore.RegisterCustomPlant<SuperDoomScaredy, UltimateDoomMinigunScaredy>((int)UltimateDoomMinigunScaredy.PlantID, ab.GetAsset<GameObject>("UltimateDoomMinigunScaredyPrefab"),
                ab.GetAsset<GameObject>("UltimateDoomMinigunScaredyPreview"), [], 0.5f, 0f, 300, 300, 90f, 1000);
            CustomCore.AddPlantAlmanacStrings((int)UltimateDoomMinigunScaredy.PlantID, $"究极速射毁灭机枪胆小菇({(int)UltimateDoomMinigunScaredy.PlantID})",
                "发射毁灭菇的加特林速射机枪胆小菇\n" +
                "<color=#0000FF>毁灭机枪胆小菇的限定形态</color>\n\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=#3D1400>使用条件：</color><color=red>①融合或转化毁灭机枪胆小菇时有2%概率变异\n" +
                "②神秘模式\n" +
                "*可使用胆小菇切回毁灭机枪胆小菇</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>300/0.5秒</color>\n" +
                "<color=#3D1400>特点：</color><color=red>①每攻击1次减少0.02秒攻击间隔，最低0.1秒\n" +
                "②启动射击需要预热1秒。\n" +
                "③每第16发为大毁灭菇\n" +
                "④3.5x3.5范围内有僵尸会害怕自爆并释放毁灭菇效果</color>\n\n" +
                "<color=#3D1400>呃啊</color>");
            CustomCore.TypeMgrExtra.LevelPlants.Add(UltimateDoomMinigunScaredy.PlantID, CardLevel.Red);
            CustomCore.AddFusion((int)PlantType.GatlingDoomScaredy, (int)UltimateDoomMinigunScaredy.PlantID, (int)PlantType.ScaredyShroom);
            CustomCore.AddFusion((int)PlantType.GatlingDoomScaredy, (int)PlantType.ScaredyShroom, (int)UltimateDoomMinigunScaredy.PlantID);
        }
    }

    [RegisterTypeInIl2Cpp]
    public class UltimateDoomMinigunScaredy : MonoBehaviour
    {
        public static PlantType PlantID = (PlantType)1953;

        public SuperDoomScaredy plant => gameObject.GetComponent<SuperDoomScaredy>();

        public void Awake()
        {
            plant.shoot = transform.FindChild("Shoot");
        }

        public void ScaredyShoot()
        {
            if (plant.thePlantAttackInterval > 0.1f)
            {
                plant.thePlantAttackInterval -= 0.02f;
                plant.anim.speed += 0.2f;
            }

            plant.doomTimes++;

            if (plant.doomTimes % ((Lawnf.TravelAdvanced((AdvBuff)3)) ? 4 : 16) == 0)
            {
                var bullet = CreateBullet.Instance.SetBullet(plant.shoot.position.x, plant.shoot.position.y, plant.thePlantRow, BulletType.Bullet_doom_big,
                    BulletMoveWay.MoveRight);
                bullet.Damage = plant.attackDamage * 6;
                bullet.theStatus = BulletStatus.Doom_big;
                bullet.normalSpeed *= 2;
                plant.doomTimes = 0;
            }
            else
            {
                var bullet = CreateBullet.Instance.SetBullet(plant.shoot.position.x, plant.shoot.position.y, plant.thePlantRow, BulletType.Bullet_doom,
                    BulletMoveWay.MoveRight);
                bullet.Damage = plant.attackDamage * 6;
                bullet.normalSpeed *= 2;
            }

            GameAPP.PlaySound(UnityEngine.Random.Range(3, 5), 0.5f, 1.0f);
        }
    }

    [HarmonyPatch(typeof(ScaredyShroom), nameof(ScaredyShroom.Shootable))]
    public static class ScaredyShroomPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ScaredyShroom __instance, ref bool __result)
        {
            if (__instance.thePlantType == UltimateDoomMinigunScaredy.PlantID)
            {
                __instance.anim.SetBool("shooting", __result);
                if (!__result)
                {
                    __instance.thePlantAttackInterval = 0.5f;
                    __instance.anim.speed = 1f;
                }
            }
        }
    }
}