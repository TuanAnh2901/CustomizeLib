using BepInEx;
using BepInEx.Unity.IL2CPP;
using CustomizeLib.BepInEx;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Salmon.BepInEx
{
    [BepInPlugin("salmon.salmon", "Salmon", "1.0")]
    public class Core : BasePlugin
    {
        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        public override void Load()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
            ClassInjector.RegisterTypeInIl2Cpp<Salmon>();
            AssetBundle assetBundle = CustomCore.GetAssetBundle(Assembly.GetExecutingAssembly(), "salmon");
            CustomCore.RegisterCustomPlant<PeaShooter, Salmon>(Salmon.PlantID, assetBundle.GetAsset<GameObject>("SalmonPrefab"), assetBundle.GetAsset<GameObject>("SalmonPreview"), new List<ValueTuple<int, int>>(), 0f, 0f, int.MaxValue, int.MaxValue, 0f, 127);
            CustomCore.TypeMgrExtra.IsIcePlant.Add((PlantType)Salmon.PlantID);
            CustomCore.TypeMgrExtra.IsTallNut.Add((PlantType)Salmon.PlantID);
            CustomCore.AddPlantAlmanacStrings(Salmon.PlantID, $"鲑鱼({Salmon.PlantID})", "疑似代码现出原形。\n\n<color=#3D1400>韧性：</color><color=red>2147483647</color>\n<color=#3D1400>特点：</color><color=red>出场时生成奖杯，在场时代码杀所有非魅惑僵尸，并让所有植物的血量和最大血量提升至2147483647。</color>\n\n<color=#3D1400>鱼类成精了！变成了一个代码？？？“空引用能不能死一死啊！！！”</color>");
        }

        // Token: 0x06000002 RID: 2 RVA: 0x00002134 File Offset: 0x00000334
        public static void SpawnItem(string resourcePath)
        {
            GameObject gameObject = Resources.Load<GameObject>(resourcePath);
            bool flag = gameObject != null;
            if (flag)
            {
                UnityEngine.Object.Instantiate<GameObject>(gameObject, new Vector2(0f, 0f), Quaternion.identity, GameAPP.board.transform);
            }
        }
    }

    public class Salmon : MonoBehaviour
    {
        // Token: 0x06000004 RID: 4 RVA: 0x0000218A File Offset: 0x0000038A
        public Salmon() : base(ClassInjector.DerivedConstructorPointer<Salmon>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }

        // Token: 0x06000005 RID: 5 RVA: 0x0000219F File Offset: 0x0000039F
        public Salmon(IntPtr i) : base(i)
        {
        }

        // Token: 0x06000006 RID: 6 RVA: 0x000021AC File Offset: 0x000003AC
        public void Update()
        {
            bool flag = GameAPP.board != null && GameAPP.theGameStatus == 0;
            if (flag)
            {
                Board board;
                bool flag2 = GameAPP.board.TryGetComponent<Board>(out board);
                if (flag2)
                {
                    bool flag3 = board.zombieArray != null;
                    if (flag3)
                    {
                        for (int i = 0; i < board.zombieArray.Count; i++)
                        {
                            Zombie zombie = board.zombieArray[i];
                            bool flag4 = zombie != null && !zombie.isMindControlled;
                            if (flag4)
                            {
                                zombie.Die(0);
                                bool flag5 = zombie != null;
                                if (flag5)
                                {
                                    UnityEngine.Object.Destroy(zombie.gameObject);
                                    Board component = GameAPP.board.GetComponent<Board>();
                                    int theTotalNumOfZombie = component.theTotalNumOfZombie;
                                    component.theTotalNumOfZombie = theTotalNumOfZombie - 1;
                                }
                            }
                        }
                    }
                    bool flag6 = board.boardEntity.plantArray != null;
                    if (flag6)
                    {
                        for (int j = 0; j < board.boardEntity.plantArray.Count; j++)
                        {
                            Plant plant = board.boardEntity.plantArray[j];
                            bool flag7 = plant != null;
                            if (flag7)
                            {
                                plant.thePlantHealth = int.MaxValue;
                                plant.thePlantMaxHealth = int.MaxValue;
                            }
                        }
                    }
                }
            }
        }

        // Token: 0x06000007 RID: 7 RVA: 0x00002304 File Offset: 0x00000504
        public void Start()
        {
            try
            {
                this.plant.shoot = this.plant.gameObject.transform.GetChild(0);
                bool flag = GameAPP.theGameStatus == 0;
                if (flag)
                {
                    Core.SpawnItem("Board/Award/TrophyPrefab");
                }
            }
            catch (Exception)
            {
            }
        }

        // Token: 0x17000001 RID: 1
        // (get) Token: 0x06000008 RID: 8 RVA: 0x00002364 File Offset: 0x00000564
        public PeaShooter plant
        {
            get
            {
                return base.gameObject.GetComponent<PeaShooter>();
            }
        }

        // Token: 0x04000001 RID: 1
        public static int PlantID = 1905;
    }
}
