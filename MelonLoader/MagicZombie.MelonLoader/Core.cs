using CustomizeLib.MelonLoader;
using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Il2CppTMPro;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(MagicZombie.MelonLoader.Core), "MagicZombie", "1.0.0", "Salmon", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace MagicZombie.MelonLoader
{
    public class Core : MelonMod
    {
        // public static MelonPreferences_Category EnableMagicZombieConfig;
        // public static MelonPreferences_Entry<bool> EnableMagicZombie;
        public override void OnInitializeMelon()
        {
            // EnableMagicZombieConfig = MelonPreferences.CreateCategory("MagicZombie");
            //EnableMagicZombie = EnableMagicZombieConfig.CreateEntry<bool>("EnableMagicZombie", true, "EnableMagicZombie");

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "magiczombie");
            CustomCore.RegisterCustomZombie<NormalZombie, MagicZombie>((ZombieType)MagicZombie.ZombieID,
                ab.GetAsset<GameObject>("MagicZombie"), -1, 50, 1350, 0, 0);
            CustomCore.AddZombieAlmanacStrings(MagicZombie.ZombieID, $"魔术僵尸({MagicZombie.ZombieID})", "魔术技巧！喜好对植物表演的大魔术师。\n\n<color=#3D1400>贴图作者：@林秋AutumnLin</color>\n<color=#3D1400>韧性：</color><color=red>1350</color>\n<color=#3D1400>特点：</color><color=red>每5秒对在场随机植物进行一次魔术融合，对于无法融合的植物会使魔术失效。小概率会使植物消失并生成小鬼僵尸，若在场无植物时则在自身处生成小鬼。</color>\n\n<color=#3D1400>“我喜欢魔术，更喜欢表演魔术，”魔术僵尸热情高涨，游走在舞台中央，“所有人都会喜欢我的魔术，包括植物！”</color>");
        }
    }

    [RegisterTypeInIl2Cpp]
    public class MagicZombie : MonoBehaviour
    {
        public static int ZombieID = 80;
        public bool loseHead = false;
        public bool loseHand = false;
        public float magicCountDown = 5f;

        public void Awake()
        {
            if (GameAPP.theGameStatus == GameStatus.InGame && zombie is not null)
            {
                zombie.butterHead = gameObject.transform.FindChild("Zombie_head/hat").gameObject;
            }
        }

        public void Update()
        {
            if (GameAPP.theGameStatus == GameStatus.InGame && zombie is not null)
            {
                if (!loseHand && zombie.theHealth < zombie.theMaxHealth / 3 * 2)
                {
                    Destroy(zombie.transform.FindChild("Zombie_outerarm_lower").gameObject);
                    Destroy(zombie.transform.FindChild("Zombie_outerarm_hand").gameObject);
                    loseHand = true;
                }
                if (!loseHead && zombie.theHealth < zombie.theMaxHealth / 3)
                {
                    Destroy(zombie.transform.FindChild("Zombie_jaw").gameObject);
                    Destroy(zombie.transform.FindChild("Zombie_head").gameObject);
                    loseHead = true;
                }
                if (zombie.theSpeed != 0 && !loseHand && !zombie.isMindControlled && !loseHead)
                    magicCountDown -= Time.deltaTime * zombie.theSpeed;
                if (magicCountDown < 0 && !loseHand && !zombie.isMindControlled && !loseHead)
                {
                    zombie.anim.SetTrigger("magic");
                    magicCountDown = 5f;
                }
            }
        }

        public void AnimMagic()
        {
            if (Board.Instance is not null && !loseHand && zombie is not null)
            {
                if (Board.Instance.boardEntity.plantArray is null)
                {
                    CreateZombie.Instance.SetZombie(zombie.theZombieRow, ZombieType.ImpZombie, zombie.axis.transform.position.x);
                    ParticleManager.Instance.SetParticle(ParticleType.RandomCloud, zombie.axis.transform.position);
                    return;
                }
                var list = Board.Instance.boardEntity.plantArray.ToArray().ToList().Where(p => p is not null).ToList();
                if (list.Count <= 0)
                {
                    CreateZombie.Instance.SetZombie(zombie.theZombieRow, ZombieType.ImpZombie, zombie.axis.transform.position.x);
                    ParticleManager.Instance.SetParticle(ParticleType.RandomCloud, zombie.axis.transform.position);
                    return;
                }
                var index = UnityEngine.Random.Range(0, list.Count);
                var plant = list[index];
                if (plant is null)
                    return;
                if (UnityEngine.Random.Range(1, 101) < 95)
                {
                    var result = CreatePlant.Instance.SetPlant(plant.thePlantColumn, plant.thePlantRow, PlantType.Present);
                    ParticleManager.Instance.SetParticle(ParticleType.RandomCloud, plant.axis.transform.position);
                    if (result is not null)
                    result.GetComponent<Present>().AnimEvent();
                }
                else
                {
                    plant.Die();
                    CreateZombie.Instance.SetZombie(plant.thePlantRow, ZombieType.ImpZombie, plant.axis.transform.position.x);
                    ParticleManager.Instance.SetParticle(ParticleType.RandomCloud, plant.axis.transform.position);
                }
            }
        }

        public NormalZombie? zombie => gameObject.TryGetComponent<NormalZombie>(out var z) ? z : null;
    }

    [HarmonyPatch(typeof(CreateZombie), nameof (CreateZombie.SetZombie))]
    public static class CreateZombie_SetZombie_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref int theRow, ref ZombieType theZombieType, ref float theX, ref bool isIdle)
        {
            if (UnityEngine.Random.Range(0, 100) <= 20 && theZombieType == ZombieType.RandomZombie && GameAPP.theGameStatus == GameStatus.InGame)
            {
                CreateZombie.Instance.SetZombie(theRow, (ZombieType)MagicZombie.ZombieID, theX, isIdle);
                return false;
            }
            return true;
        }
    }

    /*[HarmonyPatch(typeof(UIMgr), nameof(UIMgr.EnterOptionMenu))]
    public static class UIMgr_EnterOtherMenu_Patch
    {
        [HarmonyPrefix]
        public static void Prefix()
        {
            MelonLogger.Msg("enter");
            var menu = GameAPP.canvas.FindChild("OptionMenu(Clone)").gameObject;
            if (menu is null)
                return;
            var button = menu.transform.FindChild("SolarEffect").gameObject;
            if (button is null)
                return;
            var enableMagicZombie = GameObject.Instantiate(button);
            enableMagicZombie.name = "EnableMagicZombie";
            enableMagicZombie.transform.SetParent(button.transform.parent);
            String text = Core.EnableMagicZombie.Value ? "启用魔术僵尸" : "禁用魔术僵尸";
            enableMagicZombie.transform.FindChild("text_1").GetComponent<TextMeshProUGUI>().text = text;
            enableMagicZombie.transform.FindChild("text").GetComponent<TextMeshProUGUI>().text = "魔术僵尸";
            enableMagicZombie.transform.FindChild("text2").GetComponent<TextMeshProUGUI>().text = "魔术僵尸";
        }
    }*/
}