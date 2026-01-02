using CustomizeLib.MelonLoader;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using Unity.VisualScripting;
using UnityEngine;
using static MelonLoader.MelonLogger;

[assembly: MelonInfo(typeof(UltimateSpikeTorch.Core), "UltimateSpikeTorch", "1.0", "Salmon", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]

namespace UltimateSpikeTorch
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "ultimatespiketorch");
            CustomCore.RegisterCustomBullet<Bullet_firePea>((BulletType)Bullet_superFirePea_small.BulletID, ab.GetAsset<GameObject>("Bullet_superFirePea_small"));
            CustomCore.RegisterCustomBullet<Bullet_ironPea>((BulletType)Bullet_puffFireIronPea.BulletID, ab.GetAsset<GameObject>("Bullet_superFirePea_small"));
            CustomCore.RegisterCustomPlant<CaltropTorch, UltimateSpikeTorch>(UltimateSpikeTorch.PlantID, ab.GetAsset<GameObject>("UltimateSpikeTorchPrefab"),
                ab.GetAsset<GameObject>("UltimateSpikeTorchPreview"), new List<(int, int)>
                {
                    ((int)PlantType.UltimateTorch, (int)PlantType.Caltrop),
                    ((int)PlantType.Caltrop, (int)PlantType.UltimateTorch),
                }, 1f, 0f, 300, 300, 7.5f, 525);
            CustomCore.TypeMgrExtra.IsFirePlant.Add((PlantType)UltimateSpikeTorch.PlantID);
            CustomCore.TypeMgrExtra.IsCaltrop.Add((PlantType)UltimateSpikeTorch.PlantID);
            CustomCore.AddFusion((int)PlantType.UltimateTorch, (int)PlantType.TorchWood, UltimateSpikeTorch.PlantID);
            CustomCore.AddFusion((int)PlantType.UltimateTorch, UltimateSpikeTorch.PlantID, (int)PlantType.TorchWood);
            CustomCore.AddPlantAlmanacStrings(UltimateSpikeTorch.PlantID, $"究极火爆窝炬地刺({UltimateSpikeTorch.PlantID})",
                "蕴含着迷你火爆窝瓜的火炬地刺，炙烤能力极强，会产生迷你火爆窝瓜。\n\n" +
                "<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n" +
                "<color=blue>究极火爆窝炬同人亚种</color>\n\n" +
                "<color=#3D1400>使用条件：</color><color=red>旅行模式抽取配方</color>\n" + 
                "<color=#3D1400>转换配方：</color><color=red>地刺←→火炬树桩</color>\n" +
                "<color=#3D1400>伤害：</color><color=red>300/1秒</color>\n" +
                "<color=#3D1400>特性：</color><color=red>低矮</color>\n" +
                "<color=#3D1400>特点：</color><color=red>①仅经过特点低矮平射子弹。小豌豆，低级迷你火焰豌豆：伤害+40；小魅惑豌豆：伤害+30；小铁豆：伤害x4；迷你爆炸樱桃：变为迷你爆炸窝瓜子弹\n" +
                                                       "②每影响51发子弹，生成一个300伤害的迷你火爆窝瓜，每次落地时释放火爆辣椒效果，6次弹跳后消失</color>\n" +
                "<color=#3D1400>词条1：</color><color=red>事半功倍：生成迷你火爆窝瓜子弹需求降至26。</color>\n" +
                "<color=#3D1400>词条2：</color><color=red>窝红温了：迷你火爆窝瓜伤害x2。</color>\n" +
                "<color=#3D1400>连携词条：</color><color=red>火力全开：究极小樱桃射手攻击时发射三行等量子弹，上下子弹将以正余弦函数轨迹飞行，且命中的僵尸体型血量变为0.7倍，究极火爆窝炬地刺生成的迷你火爆窝瓜效果为全场生成火爆辣椒效果。</color>\n" +
                "<color=#3D1400>咕咕咕</color>");
            UltimateSpikeTorch.jala = Resources.Load<GameObject>("Assets/Resources/items/littlesquash/LittleSquash_jala");
        }
    }

    [RegisterTypeInIl2Cpp]
    public class UltimateSpikeTorch : MonoBehaviour
    {
        public static int PlantID = 1934;
        public static GameObject jala = null;
        public int fireTimes = 0;

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.TryGetComponent<Bullet>(out var bullet) && bullet is not null && !bullet.IsDestroyed())
            {
                if (bullet.torchWood == plant.gameObject || bullet.fromZombie || bullet.theBulletRow != plant.thePlantRow)
                    return;
                if (bullet.theBulletType != BulletType.Bullet_puffPea && (int)bullet.theBulletType != 1922 && bullet.theBulletType != BulletType.Bullet_puffIronPea && bullet.theBulletType != BulletType.Bullet_firePea_small)
                    return;
                Bullet newBullet = null;
                switch ((int)bullet.theBulletType)
                {
                    case (int)BulletType.Bullet_puffPea:
                    case (int)BulletType.Bullet_firePea_small:
                        {
                            newBullet = CreateBullet.Instance.SetBullet(bullet.transform.position.x, bullet.transform.position.y, bullet.theBulletRow, (BulletType)Bullet_superFirePea_small.BulletID, BulletMoveWay.MoveRight, false);
                            newBullet.Damage = bullet.Damage + 40;
                        }
                        break;
                    case (int)BulletType.Bullet_puffIronPea:
                        {
                            newBullet = CreateBullet.Instance.SetBullet(bullet.transform.position.x, bullet.transform.position.y, bullet.theBulletRow, (BulletType)Bullet_puffFireIronPea.BulletID, BulletMoveWay.MoveRight, false);
                            newBullet.Damage = bullet.Damage * 4;
                        }
                        break;
                    case 1922:
                        {
                            newBullet = CreateBullet.Instance.SetBullet(bullet.transform.position.x, bullet.transform.position.y, bullet.theBulletRow, (BulletType)1923, BulletMoveWay.MoveRight, false);
                            newBullet.Damage = bullet.Damage + 30;
                        }
                        break;
                }
                newBullet.torchWood = plant;
                newBullet.theExistTime = bullet.theExistTime;
                UnityEngine.Object.Destroy(newBullet.rb);
                newBullet.rb = newBullet.AddComponent<Rigidbody2D>();
                newBullet.rb.velocity = bullet.rb.velocity;
                newBullet.normalSpeed = bullet.normalSpeed;
                newBullet.transform.rotation = bullet.transform.rotation;
                newBullet.rogueStatus = bullet.rogueStatus;
                newBullet.theStatus = bullet.theStatus;
                if (newBullet.normalSpeed == 0)
                    newBullet.normalSpeed = 6f;
                fireTimes++;
                int need = Lawnf.TravelAdvanced((AdvBuff)51) ? 25 : 50;
                if (fireTimes > need)
                {
                    fireTimes = 0;

                    if (jala is not null)
                    {
                        var instance = UnityEngine.Object.Instantiate(
                            jala,
                            plant.axis.position,
                            Quaternion.identity,
                            plant.board.transform
                        ).GetComponent<LittleSquash>();
                        instance.theDamage = 300;
                        // 设置小型Squash的位置
                        if (instance is not null)
                        {
                            instance.theRow = plant.thePlantRow;
                            Vector3 position = plant.axis.position;

                            // 播放特效和音效
                            CreateParticle.SetParticle(1, position, plant.thePlantRow, true);
                            GameAPP.PlaySound(22, 0.5f, 1.0f);
                        }
                    }
                }
                bullet.Die();
            }
        }

        public CaltropTorch plant => gameObject.GetComponent<CaltropTorch>();
    }

    [RegisterTypeInIl2Cpp]
    public class Bullet_superFirePea_small : MonoBehaviour
    {
        public static int BulletID = 1934;
    }

    [RegisterTypeInIl2Cpp]
    public class Bullet_puffFireIronPea : MonoBehaviour
    {
        public static int BulletID = 1935;
    }
}