using CustomizeLib.MelonLoader;
using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(QuickJacksonDriver.MelonLoader.Core), "QuickJacksonDriver", "1.0.0", "Salmon", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace QuickJacksonDriver.MelonLoader
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "quickjacsondriver");
            CustomCore.RegisterCustomZombie<JacksonDriver, QuickJacksonDriver>((ZombieType)QuickJacksonDriver.ZombieID,
                ab.GetAsset<GameObject>("QuickJacsonDriverPrefab"), -1, 0, 8000, 0, 0);
            CustomCore.AddZombieAlmanacStrings(QuickJacksonDriver.ZombieID, $"冲锋白舞王冰车僵尸({QuickJacksonDriver.ZombieID})", 
                "听说你的前排很硬？\n\n" +
                "<color=#3D1400>贴图作者：@林秋AutumnLin</color>\n" +
                "<color=#3D1400>韧性：</color><color=red>1350</color>\n" +
                "<color=#3D1400>特点：</color><color=red>出场1秒快速进场，期间90%减伤，抵达第五列右侧或持续时间结束。碾压植物，死亡时召唤舞王指挥官+雪橇冰车僵尸。</color>\n\n" +
                "<color=#3D1400>咕咕咕</color>");
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (Input.GetKeyDown(KeyCode.L))
            {
                CreateZombie.Instance.SetZombie(Mouse.Instance.theMouseRow, (ZombieType)QuickJacksonDriver.ZombieID, Mouse.Instance.theMouseColumn, false);
            }
        }
    }

    [RegisterTypeInIl2Cpp]
    public class QuickJacksonDriver : MonoBehaviour
    {
        public static int ZombieID = 81;
        public float quickCountDown = 1f;

        public void Awake()
        {
            zombie.currentSpeed *= 1000f;
            if (zombie.isPreview)
                gameObject.transform.FindChild("Trail_1").gameObject.SetActive(false);
        }

        /*public void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider != null && collider.gameObject != null && collider.gameObject.TryGetComponent<Plant>(out var plant) && plant != null)
            {
                if (TypeMgr.Is)
            }
        }*/

        public void Update()
        {
            if (GameAPP.theGameStatus == GameStatus.InGame && zombie is not null)
            {
                if (quickCountDown > 0)
                {
                    quickCountDown -= Time.deltaTime;
                }
                if (zombie.axis.transform.position.x <= Mouse.Instance.GetBoxXFromColumn(4))
                    quickCountDown = 0f;
                if (quickCountDown <= 0)
                {
                    zombie.currentSpeed /= 1000f;
                    gameObject.transform.FindChild("Trail_1").gameObject.SetActive(false);
                }

                if (zombie.theHealth <= zombie.theMaxHealth / 3)
                    gameObject.transform.FindChild("Zombie_zamboni_2/SmokePos").gameObject.SetActive(true);
            }
        }
        public JacksonDriver? zombie => gameObject.TryGetComponent<JacksonDriver>(out var z) ? z : null;
    }

    [HarmonyPatch(typeof(CreateZombie), nameof(CreateZombie.SetZombie))]
    public static class CreateZombie_SetZombie_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref int theRow, ref ZombieType theZombieType, ref float theX, ref bool isIdle)
        {
            if ((UnityEngine.Random.Range(0, 100) <= 5 && (theZombieType == ZombieType.Driver_a || theZombieType == ZombieType.JacksonDriver) ||
                (UnityEngine.Random.Range(0, 100) <= 10 && theZombieType == ZombieType.Driver_b)) && GameAPP.theGameStatus == GameStatus.InGame)
            {
                CreateZombie.Instance.SetZombie(theRow, (ZombieType)QuickJacksonDriver.ZombieID, theX, isIdle);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Zombie), nameof(Zombie.TakeDamage))]
    public static class Zombie_TakeDamage_Patch
    {
        [HarmonyPrefix]
        public static void Prefix(Zombie __instance, ref int theDamage)
        {
            if ((int)__instance.theZombieType == QuickJacksonDriver.ZombieID)
            {
                if (__instance.TryGetComponent<QuickJacksonDriver>(out var qj) && qj is not null && qj.quickCountDown > 0)
                {
                    theDamage = (int)(theDamage * 0.1f);
                }
            }
        }
    }

    [HarmonyPatch(typeof(JacksonDriver))]
    public static class JacksonDriver_DieEvent_Patch
    {
        [HarmonyPatch(nameof(JacksonDriver.DieEvent))]
        [HarmonyPrefix]
        public static bool Prefix(JacksonDriver __instance)
        {
            if ((int)__instance.theZombieType == QuickJacksonDriver.ZombieID)
            {
                GameAPP.PlaySound(43, 0.5f, 1.0f);

                ParticleManager.Instance.SetParticle(ParticleType.MachineExplodeRed, new Vector2(__instance.axis.transform.position.x, __instance.axis.transform.position.y + 0.6f));
                if (!__instance.isMindControlled)
                {
                    CreateZombie.Instance.SetZombie(__instance.theZombieRow, ZombieType.QuickJacksonZombie, __instance.axis.transform.position.x, false);
                    CreateZombie.Instance.SetZombie(__instance.theZombieRow, ZombieType.Driver_b, __instance.axis.transform.position.x, false);
                }
                else
                {
                    CreateZombie.Instance.SetZombieWithMindControl(__instance.theZombieRow, ZombieType.QuickJacksonZombie, __instance.axis.transform.position.x, false);
                    CreateZombie.Instance.SetZombieWithMindControl(__instance.theZombieRow, ZombieType.Driver_b, __instance.axis.transform.position.x, false);
                }
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(JacksonDriver.BodyTakeDamage))]
        [HarmonyPrefix]
        public static bool Prefix(JacksonDriver __instance, ref int theDamage)
        {
            if ((int)__instance.theZombieType == QuickJacksonDriver.ZombieID)
            {
                __instance.theHealth -= __instance.GetDamage(theDamage, DmgType.Normal, true);
                __instance.UpdateHealthText();
                if (__instance.theHealth <= 0) __instance.Die();
                if (__instance.theHealth < __instance.theMaxHealth / 3)
                    __instance.anim.SetTrigger("shake");
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(TypeMgr), nameof(TypeMgr.IsDriverZombie))]
    public static class TypeMgr_IsDriverZombie_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref ZombieType theZombieType, ref bool __result)
        {
            if ((int)theZombieType == QuickJacksonDriver.ZombieID)
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}