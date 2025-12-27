using CustomizeLib.MelonLoader;
using Il2Cpp;
using MelonLoader;
using UnityEngine;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using static MelonLoader.MelonLogger;

[assembly: MelonInfo(typeof(MordenGatling.MelonLoader.Core), "MordenGatling.MelonLoader", "1.0.0", "Salmon", null)]
[assembly: MelonGame("LanPiaoPiao", "PlantsVsZombiesRH")]

namespace MordenGatling.MelonLoader
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            var ab = CustomCore.GetAssetBundle(MelonAssembly.Assembly, "mordengatling");
            CustomCore.RegisterCustomPlant<Plant, MordenGatling>(MordenGatling.PlantID, ab.GetAsset<GameObject>("NullNutPrefab"),
                ab.GetAsset<GameObject>("NullNutPreview"), [], 0f, 0f, 0, 4000, 60f, 750);
            CustomCore.AddPlantAlmanacStrings(MordenGatling.PlantID, $"&$%?坚果({MordenGatling.PlantID})",
                "#$%^&@!@#$%?>*&^$#^!#$%@\n\n<color=#3D1400>贴图作者：@林秋-AutumnLin</color>\n<color=#3D1400>韧性：</color><color=red>4000</color>\n<color=#3D1400>特点：</color><color=red>防碾压，每次受伤使啃咬自己的僵尸随机获得以下状态：红温状态、减速10秒、中毒10秒、不留坑的毁灭菇效果、余烬状态、金色状态，并且有20%的概率使该僵尸立刻死亡，每次受伤随机在0~50点范围</color>\n花费：<color=red>750</color>\n冷却时间：<color=red>60秒</color>\n<color=red>内。</color>\n\n<color=#3D1400>传说在融合版的初期，有个叫什么什么75的人制作了修改器和二创前置库，为后世的开发者们留下了一笔宝贵的财富，在融合1周年之际，他选择离开这片自己曾倾注心血的大地，而修改器和二创前置库则被后来的新生力量接力，延续着他的精神，这些新生力量们也同样相信，终有一天，待到他们也不得不离开自己亲手创造</color>\n花费：<color=red>750</color>\n冷却时间：<color=red>60秒</color>\n<color=#3D1400>的天地时，必有下一个盘古，延续这份力量……</color>\n\n\n\n\n\n\n\n\n\n花费：<color=red>750</color>\n冷却时间：<color=red>60秒</color>");
            CustomCore.RegisterCustomCardToColorfulCards((PlantType)MordenGatling.PlantID);
        }
    }

    [RegisterTypeInIl2Cpp]
    public class MordenGatling : MonoBehaviour
    {
        public static int PlantID = 1933;

        public static Vector2[] shoot = new Vector2[4];
        public bool init = false;
        public void Update()
        {
            if (!init)
            {
                init = !init;
            }
        }

        public Plant plant => gameObject.GetComponent<Plant>();
    }
}