using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System.Text;
using System.Text.Json;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Object;

///
///Credit to likefengzi(https://github.com/likefengzi)(https://space.bilibili.com/237491236)
///
namespace CustomizeLib.BepInEx
{
    /// <summary>
    /// ×¢²áÈÚºÏÑóÓóÅä·½
    /// </summary>
    [HarmonyPatch(typeof(MixBomb), nameof(MixBomb.AttributeEvent))]
    public class MixBombPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(MixBomb __instance)
        {
            bool success = false;
            if (__instance is not null)
            {
                List<Plant> plants = Lawnf.Get1x1Plants(__instance.thePlantColumn, __instance.thePlantRow).ToArray().ToList();
                if (plants is null)
                    return true;
                foreach (Plant plant in plants)
                {
                    if (plant is not null && CustomCore.CustomMixBombFusions.Keys.Any(k => k.Item2 == plant.thePlantType))
                    {
                        List<(PlantType, PlantType, PlantType)> mixBombFusions = CustomCore.CustomMixBombFusions
                            .Where(kvp => kvp.Key.Item2 == plant.thePlantType)
                            .Select(kvp => kvp.Key)
                            .ToList();
                        List<Plant> leftPlant = Lawnf.Get1x1Plants(__instance.thePlantColumn - 1, __instance.thePlantRow).ToArray().ToList();
                        List<Plant> rightPlant = Lawnf.Get1x1Plants(__instance.thePlantColumn + 1, __instance.thePlantRow).ToArray().ToList();
                        foreach ((PlantType, PlantType, PlantType) fusion in mixBombFusions)
                        {
                            Plant? firstLeftPlant = leftPlant.FirstOrDefault(p => p.thePlantType == fusion.Item1);
                            Plant? firstRightPlant = rightPlant.FirstOrDefault(p => p.thePlantType == fusion.Item3);
                            if (firstLeftPlant == null || firstRightPlant == null)
                            {
                                CustomCore.CustomMixBombFusions[fusion].Item2[UnityEngine.Random.Range(0, CustomCore.CustomMixBombFusions[fusion].Item2.Count)](firstLeftPlant, plant, firstRightPlant);
                                continue;
                            }
                            if (leftPlant.Any(p => p.thePlantType == fusion.Item1) && rightPlant.Any(p => p.thePlantType == fusion.Item3))
                            {
                                CustomCore.CustomMixBombFusions[fusion].Item1[UnityEngine.Random.Range(0, CustomCore.CustomMixBombFusions[fusion].Item1.Count)](firstLeftPlant, plant, firstRightPlant);
                                success = true;
                            }
                            else
                            {
                                CustomCore.CustomMixBombFusions[fusion].Item2[UnityEngine.Random.Range(0, CustomCore.CustomMixBombFusions[fusion].Item2.Count)](firstLeftPlant, plant, firstRightPlant);
                            }
                        }
                    }
                }
            }
            if (__instance is not null && success)
                __instance.Die();
            if (success)
                return false;
            return true;
        }
    }

    /// <summary>
    /// ×¢²á·ÊÁÏÊ¹ÓÃÊÂ¼þ
    /// </summary>
    [HarmonyPatch(typeof(Fertilize))]
    public class FertilizePatch
    {
        [HarmonyPatch(nameof(Fertilize.Upgrade))]
        [HarmonyPostfix]
        public static void PostUpgrade(Fertilize __instance)
        {
            if (__instance == null || __instance.theTargetPlant == null) return;

            int column = __instance.theTargetPlant.thePlantColumn;
            int row = __instance.theTargetPlant.thePlantRow;

            List<Plant> plants = Lawnf.Get1x1Plants(column, row).ToArray().ToList<Plant>(); // »ñÈ¡Ö²Îï£¬il2cppÎÑ°®Äã
            if (plants == null) return;

            for (int i = 0; i < plants.Count; i++)
            {
                Plant plant = plants[i];
                if (plant == null) continue;
                if (plant.thePlantColumn != column || plant.thePlantRow != row) continue;
                if (Board.Instance == null) return;

                if (CustomCore.CustomUseFertilize.ContainsKey(plant.thePlantType))
                {
                    CustomCore.CustomUseFertilize[plant.thePlantType](plant);
                }
            }

            UnityEngine.Object.Destroy(__instance.gameObject);
        }
    }

    [HarmonyPatch(typeof(AlmanacMenu))]
    public static class AlmanacMenuPatch
    {
        [HarmonyPatch(nameof(AlmanacMenu.Awake))]
        [HarmonyPostfix]
        public static void PostAwake(AlmanacMenu __instance)
        {
            __instance.transform.FindChild("AlmanacPlant2").FindChild("Cards").GetComponent<GridManager>().maxY = GameAPP.resourcesManager.allPlants.Count / 9 * 1.5f;
        }
    }

    /// <summary>
    /// ³õÊ¼»¯½áÊøÏÔÊ¾»»·ô°´Å¥£¬¼ÓÔØÆ¤·ô
    /// </summary>
    /// <param name="__instance"></param>
    /// <returns></returns>
    /// <summary>
    /// Ö²ÎïÍ¼¼ø
    /// </summary>
    [HarmonyPatch(typeof(AlmanacPlantBank))]
    public static class AlmanacMgrPatch
    {
        /// <summary>
        /// ³õÊ¼»¯½áÊøÏÔÊ¾»»·ô°´Å¥£¬¼ÓÔØÆ¤·ô
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void PostStart(AlmanacPlantBank __instance)
        {
            PlantType plantType = (PlantType)__instance.theSeedType;
            //³õ´Î¼ÓÔØÆ¤·ô
            if (!CustomCore.CustomPlantsSkin.ContainsKey(plantType))
            {
                //ÊÇ·ñÓÐÆ¤·ô³É¹¦
                bool buttonFlag = __instance.skinButton.active;
                //exeµÄÎ»ÖÃ
                string? fullName = Directory.GetParent(Application.dataPath)?.FullName;
                if (fullName != null)
                {
                    //Ñ°ÕÒMods/Skin/
                    string modsPath = Path.Combine(fullName, "BepInEx", "plugins", "Skin");
                    if (Directory.Exists(modsPath))
                    {
                        //Ö»Òªskin_¿ªÍ·µÄÎÄ¼þ
                        string[] files = Directory.GetFiles(modsPath, "skin_*");

                        foreach (string file in files)
                        {
                            try
                            {
                                //Èç¹ûÎÄ¼þÃû"Skin_"ºóÃæµÄidÆ¥Åä
                                if (((int)plantType).ToString() == Path.GetFileName(file)[5..])
                                {
                                    //¼ÓÔØ×ÊÔ´ÎÄ¼þ
                                    AssetBundle ab = AssetBundle.LoadFromFile(file);
                                    //³¢ÊÔ¼ÓÔØjson
                                    bool jsonFlag = false;
                                    CustomPlantData plantDataFromJson = default;
                                    CustomPlantAlmanac plantAlmanac = default;
                                    Dictionary<int, int> bulletTypesFormJson = [];
                                    foreach (string jsonFile in files)
                                    {
                                        try
                                        {
                                            if (((int)plantType) + ".json" ==
                                                Path.GetFileName(jsonFile)[5..])
                                            {
                                                // ¶ÁÈ¡ JSON ÎÄ¼þÄÚÈÝ
                                                string jsonContent = File.ReadAllText(jsonFile);

                                                // ·´ÐòÁÐ»¯ JSON ÄÚÈÝ
                                                var options = new JsonSerializerOptions
                                                {
                                                    PropertyNameCaseInsensitive = true // ÔÊÐí²»Çø·Ö´óÐ¡Ð´µÄÊôÐÔÃû³ÆÆ¥Åä
                                                };

                                                JsonSkinObject? root =
                                                    JsonSerializer.Deserialize<JsonSkinObject>(jsonContent, options);

                                                // ·ÃÎÊÊý¾Ý
                                                if (root != null)
                                                {
                                                    plantDataFromJson = root.CustomPlantData;
                                                    root.TypeMgrExtraSkin.AddValueToTypeMgrExtraSkinBackup(plantType);
                                                    bulletTypesFormJson = root.CustomBulletType;
                                                    plantAlmanac = root.PlantAlmanac;
                                                }

                                                //ÕÒµ½ÁËjsonÎÄ¼þ²¢³É¹¦¼ÓÔØ
                                                jsonFlag = true;
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e);
                                        }
                                    }

                                    //»ñµÃÐÂÆ¤·ôÔ¤ÖÆÌå
                                    GameObject? newPrefab = null;
                                    try
                                    {
                                        newPrefab = ab.GetAsset<GameObject>("Prefab");
                                        newPrefab.tag = "Plant";
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                    }

                                    //»ñµÃÐÂÆ¤·ôÔ¤ÀÀÍ¼
                                    GameObject? newPreview = null;
                                    try
                                    {
                                        newPreview = ab.GetAsset<GameObject>("Preview");
                                        newPreview.tag = "Preview";
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                    }

                                    //³É¹¦¼ÓÔØÔ¤ÖÆÌå
                                    if (newPrefab != null)
                                    {
                                        //¾ÉµÄÔ¤ÖÆÌå
                                        GameObject prefab;
                                        try
                                        {
                                            prefab = GameAPP.resourcesManager.plantPrefabs[jsonFlag ? (PlantType)plantDataFromJson.ID : plantType];
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e);
                                            prefab = GameAPP.resourcesManager.plantPrefabs[plantType];
                                        }

                                        //ÄÃµ½½Å±¾
                                        Plant plant = prefab.GetComponent<Plant>();
                                        //Ìí¼Óµ½ÐÂµÄÔ¤ÖÆÌåÉÏ
                                        newPrefab.AddComponent(plant.GetIl2CppType());
                                        CustomPlantMonoBehaviour temp =
                                            newPrefab.AddComponent<CustomPlantMonoBehaviour>();
                                        CustomPlantMonoBehaviour.BulletTypes.Add(plantType, bulletTypesFormJson);

                                        Plant newPlant = newPrefab.GetComponent<Plant>();

                                        //Ö¸¶¨id
                                        newPlant.thePlantType = plantType;

                                        //shoot³ÉÔ±¶¼ÓÐÎÊÌâ£¬Çå¿Õ
                                        newPlant.shoot = null;
                                        newPlant.shoot2 = null;
                                        //Ö¸¶¨shoot
                                        try
                                        {
                                            newPlant.FindShoot(newPrefab.transform);
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e);
                                        }
                                    }

                                    CustomPlantData newCustomPlantData = default;
                                    //ÅÐ¶ÏÊÇ·ñ³É¹¦¼ÓÔØ¶ÔÓ¦µÄjson
                                    if (jsonFlag)
                                    {
                                        //Ê¹ÓÃjsonÖÐµÄÊý¾Ý
                                        newCustomPlantData = new()
                                        {
                                            ID = (int)plantType,
                                            PlantData = plantDataFromJson.PlantData,
                                            Prefab = GameAPP.resourcesManager.plantPrefabs[plantType],
                                            Preview = GameAPP.resourcesManager.plantPreviews[plantType]
                                        };
                                    }
                                    else
                                    {
                                        //Ã»ÓÐjsonÎÄ¼þ£¬Ê¹ÓÃÄ¬ÈÏÊý¾Ý
                                        //Êý¾Ý¼ÓÔØµ½×Ô¶¨ÒåÆ¤·ôÖÐ
                                        newCustomPlantData = new()
                                        {
                                            ID = (int)plantType,
                                            PlantData = PlantDataLoader.plantDatas[plantType],
                                            Prefab = GameAPP.resourcesManager.plantPrefabs[plantType],
                                            Preview = GameAPP.resourcesManager.plantPreviews[plantType]
                                        };
                                    }

                                    //³É¹¦¶ÁÈ¡ÁËË­¾Í¼ÓÔØË­
                                    if (newPrefab != null)
                                    {
                                        newCustomPlantData.Prefab = newPrefab;
                                    }

                                    if (newPreview != null)
                                    {
                                        newCustomPlantData.Preview = newPreview;
                                    }

                                    CustomCore.CustomPlantsSkin.Add(plantType, newCustomPlantData);
                                    //¼ÓÔØÍ¼¼ø
                                    try
                                    {
                                        CustomCore.PlantsSkinAlmanac.Add(plantType, jsonFlag ?
                                            (plantAlmanac.Name, plantAlmanac.Description) : null);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                    }

                                    //ÓÐÆ¤·ô£¬°´Å¥¿ÉÒÔÏÔÊ¾
                                    buttonFlag = true;
                                    CustomCore.CustomPlantsSkinActive[plantType] = false;
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        }
                    }
                }

                __instance.skinButton.SetActive(buttonFlag);
            }
            else
            {
                //ÓÐÆ¤·ô£¬°´Å¥¿ÉÒÔÏÔÊ¾
                __instance.skinButton.SetActive(true);
            }

            if (CustomCore.CustomPlants.ContainsKey(plantType))
            {
                //¶þ´´Ö²Îï£¬°´Å¥¿ÉÒÔÏÔÊ¾
                __instance.skinButton.SetActive(CustomCore.CustomPlantsSkin.ContainsKey(plantType));
            }
        }

        /// <summary>
        /// ´Ójson¼ÓÔØÖ²ÎïÐÅÏ¢
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPatch("InitNameAndInfoFromJson")]
        [HarmonyPrefix]
        public static bool PreInitNameAndInfoFromJson(AlmanacPlantBank __instance)
        {
            //Èç¹û×Ô¶¨ÒåÖ²ÎïÍ¼¼øÐÅÏ¢°üº¬
            if (CustomCore.PlantsAlmanac.ContainsKey((PlantType)__instance.theSeedType))
            {
                //±éÀúÍ¼¼øÉÏµÄ×é¼þ
                for (int i = 0; i < __instance.transform.childCount; i++)
                {
                    Transform childTransform = __instance.transform.GetChild(i);
                    if (childTransform == null)
                    {
                        continue;
                    }

                    //Ö²ÎïÐÕÃû
                    if (childTransform.name == "Name")
                    {
                        childTransform.GetComponent<TextMeshPro>().text =
                            CustomCore.PlantsAlmanac[(PlantType)__instance.theSeedType].Item1;
                        childTransform.GetChild(0).GetComponent<TextMeshPro>().text =
                            CustomCore.PlantsAlmanac[(PlantType)__instance.theSeedType].Item1;
                    }

                    //Ö²ÎïÐÅÏ¢
                    if (childTransform.name == "Info")
                    {
                        TextMeshPro info = childTransform.GetComponent<TextMeshPro>();
                        info.overflowMode = TextOverflowModes.Page;
                        info.fontSize = 40;
                        info.text = CustomCore.PlantsAlmanac[(PlantType)__instance.theSeedType].Item2;
                        __instance.introduce = info;
                    }

                    //Ö²ÎïÑô¹â
                    if (childTransform.name == "Cost")
                    {
                        childTransform.GetComponent<TextMeshPro>().text = "";
                    }
                }

                //×è¶ÏÔ­Ê¼µÄ¼ÓÔØ
                return false;
            }

            if (CustomCore.CustomPlantsSkinActive.ContainsKey((PlantType)__instance.theSeedType) && CustomCore.PlantsSkinAlmanac.ContainsKey((PlantType)__instance.theSeedType) && CustomCore.CustomPlantsSkinActive[(PlantType)__instance.theSeedType])
            {
                var alm = CustomCore.PlantsSkinAlmanac[(PlantType)__instance.theSeedType];
                if (alm is null) return true;
                var almanac = alm.Value;
                //±éÀúÍ¼¼øÉÏµÄ×é¼þ
                for (int i = 0; i < __instance.transform.childCount; i++)
                {
                    Transform childTransform = __instance.transform.GetChild(i);
                    if (childTransform == null)
                    {
                        continue;
                    }

                    //Ö²ÎïÐÕÃû
                    if (childTransform.name == "Name")
                    {
                        childTransform.GetComponent<TextMeshPro>().text = almanac.Item1;
                        childTransform.GetChild(0).GetComponent<TextMeshPro>().text = almanac.Item1;
                    }

                    //Ö²ÎïÐÅÏ¢
                    if (childTransform.name == "Info")
                    {
                        TextMeshPro info = childTransform.GetComponent<TextMeshPro>();
                        info.overflowMode = TextOverflowModes.Page;
                        info.fontSize = 40;
                        info.text = almanac.Item2;
                        __instance.introduce = info;
                    }

                    //Ö²ÎïÑô¹â
                    if (childTransform.name == "Cost")
                    {
                        childTransform.GetComponent<TextMeshPro>().text = "";
                    }
                }

                //×è¶ÏÔ­Ê¼µÄ¼ÓÔØ
                return false;
            }

            return true;
        }

        /// <summary>
        /// Í¼¼øÖÐÊó±ê°´ÏÂ£¬ÓÃÓÚ·­Ò³
        /// </summary>
        /// <param name="__instance"></param>
        /// <returns></returns>
        [HarmonyPatch("OnMouseDown")]
        [HarmonyPrefix]
        public static bool PreOnMouseDown(AlmanacPlantBank __instance)
        {
            //ÓÒ²àÏÔÊ¾
            __instance.introduce =
                __instance.gameObject.transform.FindChild("Info").gameObject.GetComponent<TextMeshPro>();
            //Ò³Êý
            __instance.pageCount = __instance.introduce.m_pageNumber * 1;
            //ÏÂÒ»Ò³
            if (__instance.currentPage <= __instance.introduce.m_pageNumber)
            {
                ++__instance.currentPage;
            }
            else
            {
                __instance.currentPage = 1;
            }

            //·­Ò³
            __instance.introduce.pageToDisplay = __instance.currentPage;

            //×è¶ÏÔ­Ê¼·­Ò³
            return false;
        }
    }

    [HarmonyPatch(typeof(AlmanacMgrZombie))]
    public static class AlmanacMgrZombiePatch
    {
        [HarmonyPatch("InitNameAndInfoFromJson")]
        [HarmonyPrefix]
        public static bool PreInitNameAndInfoFromJson(AlmanacMgrZombie __instance)
        {
            if (CustomCore.ZombiesAlmanac.ContainsKey(__instance.theZombieType))
            {
                for (int i = 0; i < __instance.transform.childCount; i++)
                {
                    Transform childTransform = __instance.transform.GetChild(i);
                    if (childTransform == null)
                        continue;
                    if (childTransform.name == "Name")
                    {
                        childTransform.GetComponent<TextMeshPro>().text = CustomCore.ZombiesAlmanac[__instance.theZombieType].Item1;
                        childTransform.GetChild(0).GetComponent<TextMeshPro>().text = CustomCore.ZombiesAlmanac[__instance.theZombieType].Item1;
                    }
                    if (childTransform.name == "Info")
                    {
                        TextMeshPro info = childTransform.GetComponent<TextMeshPro>();
                        info.overflowMode = TextOverflowModes.Page;
                        info.fontSize = 40;
                        info.text = CustomCore.ZombiesAlmanac[__instance.theZombieType].Item2;
                        __instance.introduce = info;
                    }
                    if (childTransform.name == "Cost")
                        childTransform.GetComponent<TextMeshPro>().text = "";
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(ConveyBeltMgr))]
    public static class ConveyBeltMgrPatch
    {
        [HarmonyPatch(nameof(ConveyBeltMgr.Awake))]
        [HarmonyPostfix]
        public static void PostAwake(ConveyBeltMgr __instance)
        {
            if (Utils.IsCustomLevel(out var levelData) && levelData.BoardTag.isConvey && levelData.ConveyBeltPlantTypes().Count > 0)
            {
                __instance.plants = levelData.ConveyBeltPlantTypes().ToIl2CppList();
            }
        }

        [HarmonyPatch(nameof(ConveyBeltMgr.GetCardPool))]
        [HarmonyPostfix]
        public static void PostGetCardPool(ref Il2CppSystem.Collections.Generic.List<PlantType> __result)
        {
            if (Utils.IsCustomLevel(out var levelData) && levelData.BoardTag.isConvey && levelData.ConveyBeltPlantTypes().Count > 0)
            {
                __result = levelData.ConveyBeltPlantTypes().ToIl2CppList();
            }
        }
    }

    /// <summary>
    /// Îª¶þ´´Ö²Îï¸½¼ÓÖ²ÎïÌØÐÔ
    /// </summary>
    [HarmonyPatch(typeof(CreatePlant))]
    public static class CreatePlantPatch
    {
        [HarmonyPatch(nameof(CreatePlant.SetPlant))]
        [HarmonyPostfix]
        public static void Postfix_SetPlant(CreatePlant __instance, ref int newColumn, ref int newRow, ref GameObject __result)
        {
            if (__result is not null && __result.TryGetComponent<Plant>(out var plant) &&
                CustomCore.CustomPlantTypes.Contains(plant.thePlantType))
            {
                TypeMgr.GetPlantTag(plant);
            }
        }

        [HarmonyPatch(nameof(CreatePlant.LimTravel))]
        [HarmonyPostfix]
        public static void Postfix_LimTravel(CreatePlant __instance, ref PlantType theSeedType, ref bool __result)
        {
            bool isCanSet = false;
            if (TravelMgr.Instance != null && TravelMgr.Instance.ulockTemp.Contains(theSeedType))
                isCanSet = true;
            if (__instance.board.boardTag.enableAllTravelPlant || __instance.board.boardTag.enableTravelPlant || __instance.board.boardTag.isTravel)
                isCanSet = true;

            if (CustomCore.CustomUltimatePlants.Contains(theSeedType) && !isCanSet)
            {
                __result = true;
                InGameText.Instance.ShowText("¸ÃÅä·½½öÂÃÐÐÉú´æÏµÁÐ»òÉîÔ¨¿ÉÓÃ", 3f, false);
            }
        }

        [HarmonyPatch(nameof(CreatePlant.MixBombCheck))]
        [HarmonyPrefix]
        public static bool Prefix_MixBombCheck(CreatePlant __instance, ref int theBoxColumn, ref int theBoxRow, ref bool __result)
        {
            List<Plant> plants = Lawnf.Get1x1Plants(theBoxColumn, theBoxRow).ToArray().ToList();
            foreach (var plant in plants)
            {
                if (plant == null) continue;
                if (CustomCore.CustomMixBombFusions.Any(kvp => kvp.Key.Item2 == plant.thePlantType))
                {
                    __result = true;
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Lawnf))]
    public class LawnfPatch
    {
        [HarmonyPatch(nameof(Lawnf.GetUpgradedPlantCost))]
        [HarmonyPrefix]
        public static bool Prefix(ref PlantType thePlantType, ref int targetLevel, ref int __result)
        {
            if (CustomCore.CustomUltimatePlants.Contains(thePlantType))
            {
                __result = 1500 * (targetLevel) * (targetLevel + 1) / 2;
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(Lawnf.IsUltiPlant))]
        [HarmonyPrefix]
        public static bool Prefix(ref PlantType thePlantType, ref bool __result)
        {
            if (CustomCore.CustomPlantTypes.Contains(thePlantType))
            {
                __result = CustomCore.CustomUltimatePlants.Contains(thePlantType);
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(Lawnf.GetUltimatePlants))]
        [HarmonyPostfix]
        public static void Postfix(ref Il2CppSystem.Collections.Generic.List<PlantType> __result)
        {
            foreach (PlantType plantType in CustomCore.CustomUltimatePlants)
            {
                if (!__result.Contains(plantType))
                {
                    __result.Add(plantType);
                }
            }
        }
    }

    ///// <summary>
    ///// µã»÷ÆäËûButton£¬Òþ²Ø¶þ´´Ö²Îï½çÃæ
    ///// </summary>
    //[HarmonyPatch(typeof(UIButton))]
    //public static class HideCustomPlantCards
    //{
    //    [HarmonyPatch(nameof(UIButton.OnMouseUpAsButton))]
    //    [HarmonyPostfix]
    //    public static void Postfix()
    //    {
    //        if (SelectCustomPlants.MyPageParent != null && SelectCustomPlants.MyPageParent.active)
    //            SelectCustomPlants.MyPageParent.SetActive(false);
    //    }

    //    [HarmonyPatch(nameof(UIButton.Start))]
    //    [HarmonyPostfix]
    //    public static void PostfixStart(UIButton __instance)
    //    {
    //        if (__instance.name == "LastPage" && Board.Instance != null && Board.Instance.isIZ)
    //        {
    //            SelectCustomPlants.InitCustomCards_IZ();
    //        }
    //    }
    //}

    [HarmonyPatch(typeof(InGameUI))]
    public static class InGameUIPatch
    {
        [HarmonyPatch(nameof(InGameUI.SetUniqueText))]
        [HarmonyPostfix]
        public static void PostSetUniqueText(InGameUI __instance, ref Il2CppReferenceArray<TextMeshProUGUI> T)
        {
            if (GameAPP.theBoardType is (LevelType)66)
            {
                __instance.ChangeString(T, CustomCore.CustomLevels[GameAPP.theBoardLevel].Name());
            }
        }

        [HarmonyPatch(nameof(InGameUI.MoveCard))]
        [HarmonyPrefix]
        public static void PreMoveCard(ref CardUI card)
        {
            foreach (CheckCardState check in CustomCore.checkBehaviours)
            {
                if (check != null)
                {
                    check.movingCardUI = card;
                    check.CheckState();
                }
            }
        }

        [HarmonyPatch(nameof(InGameUI.RemoveCardFromBank))]
        [HarmonyPostfix]
        public static void PostReMoveCardFromBank(ref CardUI card)
        {
            foreach (CheckCardState check in CustomCore.checkBehaviours)
            {
                if (check != null)
                {
                    check.movingCardUI = card;
                    check.CheckState();
                }
            }
        }
    }

    [HarmonyPatch(typeof(InitBoard))]
    public static class InitBoardPatch
    {
        [HarmonyPatch(nameof(InitBoard.PreSelectCard))]
        [HarmonyPostfix]
        public static void PostPreSelectCard(InitBoard __instance)
        {
            if (GameAPP.theBoardType is (LevelType)66)
            {
                foreach (var c in CustomCore.CustomLevels[GameAPP.theBoardLevel].PreSelectCards())
                {
                    __instance.PreSelect(c);
                }
            }
        }

        [HarmonyPatch(nameof(InitBoard.RightMoveCamera))]
        [HarmonyPostfix]
        public static void PostRightMoveCamera()
        {
            if (GameAPP.theBoardType is not (LevelType)66) return;
            var levelData = CustomCore.CustomLevels[GameAPP.theBoardLevel];
            var travelMgr = GameAPP.gameAPP.GetOrAddComponent<TravelMgr>();
            foreach (var a in levelData.AdvBuffs())
            {
                if (a >= 0 && a < travelMgr.advancedUpgrades.Count)
                {
                    travelMgr.advancedUpgrades[a] = true;
                }
            }
            foreach (var u in levelData.UltiBuffs())
            {
                if (u.Item1 >= 0 && u.Item1 < travelMgr.ultimateUpgrades.Count && u.Item2 >= 0)
                {
                    travelMgr.ultimateUpgrades[u.Item1] = u.Item2;
                }
            }
            foreach (var p in levelData.UnlockPlants())
            {
                if (p >= 0 && p < travelMgr.unlockPlant.Count)
                {
                    travelMgr.unlockPlant[p] = true;
                }
            }
            foreach (var d in levelData.Debuffs())
            {
                if (d >= 0 && d < travelMgr.debuff.Count)
                {
                    travelMgr.debuff[d] = true;
                }
            }
        }

        [HarmonyPatch(nameof(InitBoard.MoveOverEvent))]
        [HarmonyPrefix]
        public static bool PreMoveOverEvent(InitBoard __instance, ref string direction)
        {
            if (GameAPP.theBoardType is not (LevelType)66) return true;
            var levelData = CustomCore.CustomLevels[GameAPP.theBoardLevel];
            if (direction == "right")
            {
                if (__instance.board is not null)
                {
                    if (__instance.board.cardSelectable)
                    {
                        // ÉèÖÃÓÎÏ·×´Ì¬
                        GameAPP.theGameStatus = GameStatus.Selecting;

                        // UI¿ØÖÆ
                        InGameUI.Instance.ConveyorBelt.SetActive(false);
                        InGameUI.Instance.Bottom.SetActive(true);

                        // Æô¶¯Ð­³ÌÒÆ¶¯UIÔªËØ
                        __instance.StartCoroutine(__instance.MoveDirection(InGameUI.Instance.SeedBank, 79f, 0));
                        __instance.StartCoroutine(__instance.MoveDirection(InGameUI.Instance.Bottom, 525f, 1));
                    }
                    else
                    {
                        // ÑÓ³ÙÖ´ÐÐ·½·¨
                        __instance.Invoke("LeftMoveCamera", 1.5f);
                        InGameUI.Instance.Bottom.SetActive(false);
                    }
                }
            }
            else if (direction == "left")
            {
                if (__instance.board is null) return false;

                if (!__instance.board.cardSelectable)
                {
                    if (__instance.board.cardBank)
                    {
                        __instance.StartCoroutine(__instance.MoveDirection(InGameUI.Instance.SeedBank, 79f, 0));
                        __instance.AddCard();
                    }
                    else
                    {
                        InGameUI.Instance.SeedBank.SetActive(false);
                    }
                    InGameUI.Instance.Bottom.SetActive(false);
                }

                // ÒôÁ¿½¥±äÐ­³Ì
                __instance.StartCoroutine(__instance.DecreaseVolume());

                // ½µµÍUIÎ»ÖÃ
                InGameUI.Instance.LowerUI();

                // ³õÊ¼»¯¸î²Ý»ú£¨ÌØ¶¨Ä£Ê½ÏÂ£©
                if (!__instance.board.boardTag.disableMower)
                {
                    __instance.InitMower();
                }

                // ÎíÐ§¹ûÒÆ¶¯
                if (__instance.board.fog != null)
                {
                    Vector3 fogPosition = __instance.board.fog.transform.position;
                    Vector3 boardPosition = __instance.board.background.transform.position;

                    FogMgr.Instance.MoveObject(
                        new(fogPosition.x,
                        fogPosition.y,
                        boardPosition.z),
                        10f  // ÒÆ¶¯ËÙ¶È
                    );
                }

                // BOSSÕ½ÌØÊâ´¦Àí
                float invokeDelay = 0.5f;
                if (__instance.board.boardTag.isBoss || __instance.board.boardTag.isBoss2)
                {
                    GameObject zombie = CreateZombie.Instance.SetZombie(0, levelData.RealBoss2 ? ZombieType.ZombieBoss2 : ZombieType.ZombieBoss, 0f);
                    Zombie zombieComp = zombie.GetComponent<Zombie>();

                    if (__instance.board.boss2)
                    {
                        Lawnf.SetZombieHealth(zombieComp, 5f);
                    }
                    invokeDelay = 3.5f;
                    __instance.board.boss2 = levelData.RealBoss2;
                }

                // ÑÓ³Ùµ÷ÓÃ·½·¨
                __instance.Invoke("ReadySetPlant", invokeDelay);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(InitZombieList))]
    public static class InitZombieListPatch
    {
        [HarmonyPatch(nameof(InitZombieList.InitZombie))]
        [HarmonyPostfix]
        public static void PostInitZombie()
        {
            if (Utils.IsCustomLevel(out var levelData))
            {
                foreach (var z in levelData.ZombieList())
                {
                    InitZombieList.zombieTypeList.Add(z);
                    InitZombieList.allow[(int)z] = true;
                    for (int i = 0; i < InitZombieList.zombieList.Count; i++)
                    {
                        Il2CppSystem.Collections.Generic.List<ZombieType> zombieList = InitZombieList.zombieList[i];
                        InitZombieList.zombieList.Clear();
                        int rand = UnityEngine.Random.Range(3, 10);
                        if (i % 10 == 0)
                            rand = UnityEngine.Random.Range(8, 15);
                        if (i <= 3)
                            rand = UnityEngine.Random.Range(1, 5);
                        for (int j = 0; j < rand; j++)
                        {
                            int rand_index = UnityEngine.Random.Range(0, levelData.ZombieList().Count);
                            zombieList.Add(levelData.ZombieList()[rand_index]);
                        }
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Money))]
    public static class MoneyPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("ReinforcePlant")]
        public static bool PreReinforcePlant(Money __instance, ref Plant plant)
        {
            if (CustomCore.SuperSkills.ContainsKey(plant.thePlantType))
            {
                var cost = CustomCore.SuperSkills[plant.thePlantType].Item1(plant);

                if (Board.Instance.theMoney < cost)
                {
                    InGameText.Instance.ShowText($"´óÕÐÐèÒª{cost}½ð±Ò", 5);
                    return false;
                }
                if (plant.SuperSkill())
                {
                    CustomCore.SuperSkills[plant.thePlantType].Item2(plant);
                    plant.AnimSuperShoot();
                    __instance.UsedEvent(plant.thePlantColumn, plant.thePlantRow, cost);
                    __instance.OtherSuperSkill(plant);
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Mouse))]
    public static class MousePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("GetPlantsOnMouse")]
        public static void PostGetPlantsOnMouse(ref Il2CppSystem.Collections.Generic.List<Plant> __result)
        {
            for (int i = __result.Count - 1; i >= 0; i--)
            {
                if (__result[i] is not null && TypeMgr.BigNut(__result[i].thePlantType))
                {
                    __result.RemoveAt(i);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("LeftClickWithNothing")]
        public static void PostLeftClickWithNothing()
        {
            foreach (GameObject gameObject in (List<GameObject>)[..from RaycastHit2D raycastHit2D in
                                           (RaycastHit2D[])Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition),
                                           Vector2.zero) select raycastHit2D.collider.gameObject])
            {
                if (gameObject.TryGetComponent<Plant>(out var plant) && CustomCore.CustomPlantClicks.ContainsKey(plant.thePlantType))
                {
                    CustomCore.CustomPlantClicks[plant.thePlantType](plant);
                    return;
                }
            }
        }
    }

    [HarmonyPatch(typeof(NoticeMenu), nameof(NoticeMenu.Start))]
    public static class NoticeMenuPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            #if false
            // À©ÈÝplantData
            if (CustomCore.CustomPlants.Count > 0)
            {
                long size_plantData = (int)CustomCore.CustomPlants.Keys.Max() < PlantDataLoader.plantData.Length ? PlantDataLoader.plantData.Length : (int)CustomCore.CustomPlants.Keys.Max();
                PlantDataLoader.PlantData_[] plantData = new PlantDataLoader.PlantData_[size_plantData + 1];
                Array.Copy(PlantDataLoader.plantData, plantData, PlantDataLoader.plantData.Length);
                PlantDataLoader.plantData = plantData;
            }

            // À©ÈÝparticlePrefab
            if (CustomCore.CustomParticles.Count > 0)
            {
                long size_particlePrefab = (int)CustomCore.CustomParticles.Keys.Max() < GameAPP.particlePrefab.Length ? GameAPP.particlePrefab.Length : (int)CustomCore.CustomParticles.Keys.Max();
                GameObject[] particlePrefab = new GameObject[size_particlePrefab + 1];
                Array.Copy(GameAPP.particlePrefab, particlePrefab, GameAPP.particlePrefab.Length);
                GameAPP.particlePrefab = particlePrefab;
            }

            // À©ÈÝspritePrefab
            if (CustomCore.CustomSprites.Count > 0)
            {
                long size_spritePrefab = CustomCore.CustomSprites.Keys.Max() < GameAPP.spritePrefab.Length ? GameAPP.spritePrefab.Length : CustomCore.CustomSprites.Keys.Max();
                Sprite[] spritePrefab = new Sprite[size_spritePrefab + 1];
                Array.Copy(GameAPP.spritePrefab, spritePrefab, GameAPP.spritePrefab.Length);
                GameAPP.spritePrefab = spritePrefab;
            }
            #endif
            foreach (var plant in CustomCore.CustomPlants)//¶þ´´Ö²Îï
            {
                GameAPP.resourcesManager.plantPrefabs[plant.Key] = plant.Value.Prefab;//×¢²áÔ¤ÖÆÌå
                GameAPP.resourcesManager.plantPrefabs[plant.Key].tag = "Plant";//±ØÐë´òtag
                if (!GameAPP.resourcesManager.allPlants.Contains(plant.Key))
                    GameAPP.resourcesManager.allPlants.Add(plant.Key);//×¢²áÖ²ÎïÀàÐÍ
                if (plant.Value.PlantData is not null)
                {
                    PlantDataLoader.plantData[(int)plant.Key] = plant.Value.PlantData;//×¢²áÖ²ÎïÊý¾Ý
                    PlantDataLoader.plantDatas.Add(plant.Key, plant.Value.PlantData);
                }
                GameAPP.resourcesManager.plantPreviews[plant.Key] = plant.Value.Preview;//×¢²áÖ²ÎïÔ¤ÀÀ
                GameAPP.resourcesManager.plantPreviews[plant.Key].tag = "Preview";//±ØÐÞ´òtag
            }
            Il2CppSystem.Array array = MixData.data.Cast<Il2CppSystem.Array>();//×¢²áÈÚºÏÅä·½
            foreach (var f in CustomCore.CustomFusions)
            {
                array.SetValue(f.Item1, f.Item2, f.Item3);
            }

            foreach (var z in CustomCore.CustomZombies)//×¢²á¶þ´´½©Ê¬
            {
                if (!GameAPP.resourcesManager.allZombieTypes.Contains(z.Key))
                    GameAPP.resourcesManager.allZombieTypes.Add(z.Key);//×¢²á½©Ê¬ÀàÐÍ
                GameAPP.resourcesManager.zombiePrefabs[z.Key] = z.Value.Item1;//×¢²á½©Ê¬Ô¤ÖÆÌå
                GameAPP.resourcesManager.zombiePrefabs[z.Key].tag = "Zombie";//±ØÐÞ´òtag
            }

            foreach (var bullet in CustomCore.CustomBullets)//×¢²á¶þ´´×Óµ¯
            {
                GameAPP.resourcesManager.bulletPrefabs[bullet.Key] = bullet.Value;//×¢²á×Óµ¯Ô¤ÖÆÌå
                if (!GameAPP.resourcesManager.allBullets.Contains(bullet.Key))
                    GameAPP.resourcesManager.allBullets.Add(bullet.Key);//×¢²á×Óµ¯ÀàÐÍ
            }

            foreach (var par in CustomCore.CustomParticles)//×¢²áÁ£×ÓÐ§¹û
            {
                GameAPP.particlePrefab[(int)par.Key] = par.Value;
                GameAPP.resourcesManager.particlePrefabs[par.Key] = par.Value;//×¢²áÁ£×ÓÐ§¹ûÔ¤ÖÆÌå
                if (!GameAPP.resourcesManager.allParticles.Contains(par.Key))
                    GameAPP.resourcesManager.allParticles.Add(par.Key);//×¢²áÁ£×ÓÐ§¹ûÀàÐÍ
            }

            foreach (var spr in CustomCore.CustomSprites)//×¢²á×Ô¶¨Òå¾«ÁéÌùÍ¼
            {
                GameAPP.spritePrefab[spr.Key] = spr.Value;
            }
        }
    }

    [HarmonyPatch(typeof(Plant))]
    public static class PlantPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("UseItem")]
        public static void PostUseItem(Plant __instance, ref BucketType type, ref Bucket bucket)
        {
            if (CustomCore.CustomUseItems.ContainsKey((__instance.thePlantType, type)))
            {
                CustomCore.CustomUseItems[(__instance.thePlantType, type)](__instance);
                UnityEngine.Object.Destroy(bucket.gameObject);
            }
        }
    }
    /// <summary>
    /// Ë¢ÐÂ¿¨ÅÆÌùÍ¼
    /// </summary>
    [HarmonyPatch(typeof(SeedLibrary))]
    public static class SeedLibraryPatch
    {
        [HarmonyPatch(nameof(SeedLibrary.Start))]
        [HarmonyPostfix]
        public static void PostStart(SeedLibrary __instance)
        {
            // ×¢²á×Ô¶¨Òå¿¨ÅÆ
            GameObject? MyColorfulCard = Utils.GetColorfulCardGameObject();
            Dictionary<PlantType, List<Transform?>> parents_colorful = new Dictionary<PlantType, List<Transform?>>();
            List<PlantType> cardsOnSeedBank = new List<PlantType>();
            Dictionary<PlantType, List<bool>> cardsOnSeedBankExtra = new Dictionary<PlantType, List<bool>>();
            GameObject? seedGroup = null;
            if (Board.Instance != null && !Board.Instance.isIZ)
                seedGroup = InGameUI.Instance.SeedBank.transform.GetChild(0).gameObject;
            else if (Board.Instance != null && Board.Instance.isIZ)
                seedGroup = InGameUI_IZ.Instance.transform.FindChild("SeedBank/SeedGroup").gameObject;
            if (seedGroup == null)
                return;
            for (int i = 0; i < seedGroup.transform.childCount; i++)
            {
                GameObject seed = seedGroup.transform.GetChild(i).gameObject;
                if (seed.transform.childCount > 0)
                {
                    cardsOnSeedBank.Add(seed.transform.GetChild(0).GetComponent<CardUI>().thePlantType);
                    if (!cardsOnSeedBankExtra.ContainsKey(seed.transform.GetChild(0).GetComponent<CardUI>().thePlantType))
                        cardsOnSeedBankExtra.Add(seed.transform.GetChild(0).GetComponent<CardUI>().thePlantType, new List<bool>() { seed.transform.GetChild(0).GetComponent<CardUI>().isExtra });
                    else
                        cardsOnSeedBankExtra[seed.transform.GetChild(0).GetComponent<CardUI>().thePlantType].Add(seed.transform.GetChild(0).GetComponent<CardUI>().isExtra);
                }
            }
            if (MyColorfulCard == null)
                return;
            foreach (var card in CustomCore.CustomCards)
            {
                foreach (Func<Transform?> cardFunc in card.Value)
                {
                    Transform? result = cardFunc();
                    if (!(parents_colorful.ContainsKey(card.Key) && parents_colorful[card.Key].Contains(result)))
                    {
                        GameObject TempCard = Instantiate(MyColorfulCard, result);
                        if (TempCard != null)
                        {
                            //ÉèÖÃ¸¸½Úµã
                            //¼¤»î
                            TempCard.SetActive(true);
                            //ÉèÖÃÎ»ÖÃ
                            TempCard.transform.position = MyColorfulCard.transform.position;
                            TempCard.transform.localPosition = MyColorfulCard.transform.localPosition;
                            TempCard.transform.localScale = MyColorfulCard.transform.localScale;
                            TempCard.transform.localRotation = MyColorfulCard.transform.localRotation;
                            //±³¾°Í¼Æ¬
                            // ÉèÖÃ±³¾°Ö²ÎïÍ¼±ê
                            Image image = TempCard.transform.GetChild(0).GetChild(0).GetComponent<Image>();
                            image.sprite = GameAPP.resourcesManager.plantPreviews[card.Key].GetComponent<SpriteRenderer>().sprite;
                            image.SetNativeSize();
                            // ÉèÖÃ±³¾°¼Û¸ñ
                            TempCard.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = PlantDataLoader.plantDatas[card.Key].field_Public_Int32_1.ToString();
                            //¿¨Æ¬
                            CardUI component = TempCard.transform.GetChild(1).GetComponent<CardUI>();
                            component.gameObject.SetActive(true);
                            //ÐÞ¸ÄÍ¼Æ¬
                            Mouse.Instance.ChangeCardSprite(card.Key, component);
                            // ÐÞ¸ÄËõ·Å
                            TempCard.transform.GetChild(1).GetComponent<BoxCollider2D>().enabled = true;
                            RectTransform bgRect = TempCard.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
                            RectTransform packetRect = TempCard.transform.GetChild(1).GetChild(0).GetComponent<RectTransform>();
                            bgRect.localScale = packetRect.localScale;
                            bgRect.sizeDelta = packetRect.sizeDelta;
                            //ÉèÖÃÊý¾Ý
                            component.thePlantType = card.Key;
                            component.theSeedType = (int)card.Key;
                            component.theSeedCost = PlantDataLoader.plantDatas[card.Key].field_Public_Int32_1;
                            component.fullCD = PlantDataLoader.plantDatas[card.Key].field_Public_Single_2;
                            if (cardsOnSeedBank.Contains(card.Key))
                                TempCard.transform.GetChild(1).gameObject.SetActive(false);
                            CheckCardState customComponent = TempCard.AddComponent<CheckCardState>();
                            customComponent.card = TempCard;
                            customComponent.cardType = component.thePlantType;
                            if (!parents_colorful.ContainsKey(card.Key))
                                parents_colorful.Add(card.Key, new List<Transform?>() { result });
                            else
                                parents_colorful[card.Key].Add(result);
                        }
                    }
                }
            }

            GameObject? MyNormalCard = Utils.GetNormalCardGameObject();
            Dictionary<PlantType, List<Transform?>> parents_normal = new Dictionary<PlantType, List<Transform?>>();
            if (MyNormalCard == null)
                return;
            foreach (var card in CustomCore.CustomNormalCards)
            {
                foreach (Func<Transform?> cardFunc in card.Value)
                {
                    Transform? result = cardFunc();
                    if (!(parents_normal.ContainsKey(card.Key) && parents_normal[card.Key].Contains(result)))
                    {
                        GameObject TempCard = Instantiate(MyNormalCard, result);
                        if (TempCard != null)
                        {
                            //ÉèÖÃ¸¸½Úµã
                            //¼¤»î
                            TempCard.SetActive(true);
                            //ÉèÖÃÎ»ÖÃ
                            TempCard.transform.position = MyNormalCard.transform.position;
                            TempCard.transform.localPosition = MyNormalCard.transform.localPosition;
                            TempCard.transform.localScale = MyNormalCard.transform.localScale;
                            TempCard.transform.localRotation = MyNormalCard.transform.localRotation;
                            //±³¾°Í¼Æ¬
                            // ÉèÖÃ±³¾°Ö²ÎïÍ¼±ê
                            Image image = TempCard.transform.GetChild(0).GetChild(0).GetComponent<Image>();
                            image.sprite = GameAPP.resourcesManager.plantPreviews[card.Key].GetComponent<SpriteRenderer>().sprite;
                            image.SetNativeSize();
                            // ÉèÖÃ±³¾°¼Û¸ñ
                            TempCard.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = PlantDataLoader.plantDatas[card.Key].field_Public_Int32_1.ToString();
                            //¿¨Æ¬
                            CardUI component = TempCard.transform.GetChild(2).GetComponent<CardUI>(); // Ö÷¿¨
                            component.gameObject.SetActive(true);
                            CardUI component1 = TempCard.transform.GetChild(1).GetComponent<CardUI>(); // ¸±¿¨
                            component1.gameObject.SetActive(true);
                            //ÐÞ¸ÄÍ¼Æ¬
                            Mouse.Instance.ChangeCardSprite(card.Key, component);
                            Mouse.Instance.ChangeCardSprite(card.Key, component1);
                            // ÐÞ¸ÄËõ·Å
                            TempCard.transform.GetChild(2).GetComponent<BoxCollider2D>().enabled = true;
                            TempCard.transform.GetChild(1).GetComponent<BoxCollider2D>().enabled = true;
                            RectTransform bgRect = TempCard.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
                            RectTransform packetRect = TempCard.transform.GetChild(2).GetChild(0).GetComponent<RectTransform>();
                            bgRect.localScale = packetRect.localScale;
                            bgRect.sizeDelta = packetRect.sizeDelta;
                            //ÉèÖÃÊý¾Ý
                            component.thePlantType = card.Key;
                            component.theSeedType = (int)card.Key;
                            component.theSeedCost = PlantDataLoader.plantDatas[card.Key].field_Public_Int32_1;
                            component.fullCD = PlantDataLoader.plantDatas[card.Key].field_Public_Single_2;
                            //ÉèÖÃ¸±¿¨Êý¾Ý
                            component1.thePlantType = card.Key;
                            component1.theSeedType = (int)card.Key;
                            component1.theSeedCost = PlantDataLoader.plantDatas[card.Key].field_Public_Int32_1 * 2;
                            component1.fullCD = PlantDataLoader.plantDatas[card.Key].field_Public_Single_2;
                            if (cardsOnSeedBankExtra.ContainsKey(card.Key) && cardsOnSeedBankExtra[card.Key].Contains(true))
                                TempCard.transform.GetChild(1).gameObject.SetActive(false);
                            if (cardsOnSeedBankExtra.ContainsKey(card.Key) && cardsOnSeedBankExtra[card.Key].Contains(false))
                                TempCard.transform.GetChild(2).gameObject.SetActive(false);
                            CheckCardState customComponent = TempCard.AddComponent<CheckCardState>();
                            customComponent.card = TempCard;
                            customComponent.cardType = component.thePlantType;
                            customComponent.isNormalCard = true;
                            if (!parents_normal.ContainsKey(card.Key))
                                parents_normal.Add(card.Key, new List<Transform?>() { result });
                            else
                                parents_normal[card.Key].Add(result);
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// ½øÈëÒ»¾ÖÓÎÏ·£¬ÏÔÊ¾¶þ´´Ö²ÎïButton
    /// </summary>
    //[HarmonyPatch(typeof(Board), nameof(Board.Start))]
    //public static class ShowCustomPlantCards
    //{
    //    [HarmonyPostfix]
    //    private static void Postfix()
    //    {
    //        SelectCustomPlants.InitCustomCards();
    //    }
    //}

    /// <summary>
    /// µã»÷»»·ô
    /// </summary>
    [HarmonyPatch(typeof(SkinButton), nameof(SkinButton.OnMouseUpAsButton))]
    public static class SkinButton_OnMouseUpAsButton
    {
        [HarmonyPrefix]
        public static bool Prefix(SkinButton __instance)
        {
            PlantType plantType = (PlantType)__instance.showPlant.theSeedType;
            if (CustomCore.CustomPlantsSkin.ContainsKey(plantType))
            {
                CustomPlantData customPlantData = CustomCore.CustomPlantsSkin[plantType];
                //½»»»Ô¤ÖÆÌåÒýÓÃ
                (GameAPP.resourcesManager.plantPrefabs[plantType], customPlantData.Prefab) =
                    (customPlantData.Prefab, GameAPP.resourcesManager.plantPrefabs[plantType]);

                //½»»»Ô¤ÀÀÍ¼
                (GameAPP.resourcesManager.plantPreviews[plantType], customPlantData.Preview) =
                    (customPlantData.Preview, GameAPP.resourcesManager.plantPreviews[plantType]);

                //½»»»Ö²ÎïÊý¾Ý
                if (customPlantData.PlantData is not null)
                {
                    (PlantDataLoader.plantData[(int)plantType], customPlantData.PlantData) =
                        (customPlantData.PlantData, PlantDataLoader.plantData[(int)plantType]);
                    PlantDataLoader.plantDatas[plantType] = PlantDataLoader.plantData[(int)plantType];
                }
                CustomCore.CustomPlantsSkin[plantType] = customPlantData;

                //½»»»ÌØÐÔÁÐ±í
                Extensions.SwapTypeMgrExtraSkinAndBackup(plantType);

                //GameObject prefab = GameAPP.resourcesManager.plantPrefabs[(PlantType)__instance.showPlant.theSeedType];

                //Transform transform = AlmanacMenu.Instance.currentShowCtrl.localShowPlant.transform.parent;

                //¾ÉµÄ£¬´«µÝÍêÊý¾Ý¾ÍÏú»Ù
                GameObject oldGameObject = AlmanacMenu.Instance.currentShowCtrl.localShowPlant;
                oldGameObject.name = "ToDestroy";
                // //ÊµÀý»¯ÐÂµÄ
                // AlmanacMenu.Instance.currentShowCtrl.localShowPlant = UnityEngine.Object.Instantiate(prefab, transform);
                // //Í¬²½Î»ÖÃ
                // AlmanacMenu.Instance.currentShowCtrl.localShowPlant.transform.position =
                //     oldGameObject.transform.position;
                // AlmanacMenu.Instance.currentShowCtrl.localShowPlant.transform.localPosition =
                //     oldGameObject.transform.localPosition;

                //Ïú»Ù¾ÉµÄ
                UnityEngine.Object.Destroy(oldGameObject);

                //±ê¼ÇÊÇ·ñ»»·ô
                CustomCore.CustomPlantsSkinActive[plantType] = !CustomCore.CustomPlantsSkinActive[plantType];
                //__instance.showPlant.gameObject.SetActive(false);
                __instance.showPlant.InitNameAndInfoFromJson();
                AlmanacMenu.Instance.currentShowCtrl.localShowPlant =
                    AlmanacMenu.Instance.currentShowCtrl.SetPlant((int)plantType);

                if (AlmanacMenu.Instance.currentShowCtrl.localShowPlant.GetComponent<CustomPlantMonoBehaviour>() !=
                    null)
                {
                    UnityEngine.Object.Destroy(AlmanacMenu.Instance.currentShowCtrl.localShowPlant
                        .GetComponent<CustomPlantMonoBehaviour>());
                }

                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// ¶þ´´´ÊÌõÎÄ±¾È¾É«
    /// </summary>
    [HarmonyPatch(typeof(TravelBuffOptionButton))]
    public static class TravelBuffOptionButtonPatch
    {
        [HarmonyPatch(nameof(TravelBuffOptionButton.SetBuff))]
        public static void PostSetBuff(TravelBuffOptionButton __instance, ref BuffType buffType, ref int buffIndex)
        {
            if (buffType is BuffType.AdvancedBuff && CustomCore.CustomAdvancedBuffs.ContainsKey(buffIndex)
                && CustomCore.CustomAdvancedBuffs[buffIndex].Item5 is not null)
            {
                __instance.introduce.text = $"<color={CustomCore.CustomAdvancedBuffs[buffIndex].Item5}>{__instance.introduce.text}</color>";
            }
        }
    }

    [HarmonyPatch(typeof(TravelBuff))]
    public static class TravelBuffPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("ChangeSprite")]
        public static void PreChangeSprite(TravelBuff __instance)
        {
            if (__instance.theBuffType == 1 && CustomCore.CustomAdvancedBuffs.ContainsKey(__instance.theBuffNumber))
            {
                __instance.thePlantType = CustomCore.CustomAdvancedBuffs[__instance.theBuffNumber].Item1;
            }
            if (__instance.theBuffType == 2 && CustomCore.CustomUltimateBuffs.ContainsKey(__instance.theBuffNumber))
            {
                __instance.thePlantType = CustomCore.CustomUltimateBuffs[__instance.theBuffNumber].Item1;
            }
        }
    }

    /// <summary>
    /// ¶þ´´´ÊÌõÎÄ±¾È¾É«
    /// </summary>
    [HarmonyPatch(typeof(TravelLookBuff))]
    public static class TravelLookBuffPatch
    {
        [HarmonyPatch(nameof(TravelLookBuff.SetBuff))]
        public static void PostSetBuff(TravelLookBuff __instance, ref BuffType buffType, ref int buffIndex)
        {
            if (buffType is BuffType.AdvancedBuff && CustomCore.CustomAdvancedBuffs.ContainsKey(buffIndex)
                && CustomCore.CustomAdvancedBuffs[buffIndex].Item5 is not null)
            {
                __instance.introduce.text = $"<color={CustomCore.CustomAdvancedBuffs[buffIndex].Item5}>{__instance.introduce.text}</color>";
            }
        }
    }

    [HarmonyPatch(typeof(TravelMgr))]
    public static class TravelMgrPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        public static void PostAwake(TravelMgr __instance)
        {
            if (CustomCore.CustomAdvancedBuffs.Count > 0)
            {
                bool[] newAdv = new bool[__instance.advancedUpgrades.Count + CustomCore.CustomAdvancedBuffs.Count];
                int[] newAdvUnlock = new int[__instance.advancedUnlockRound.Count + CustomCore.CustomAdvancedBuffs.Count];
                Array.Copy(__instance.advancedUpgrades, newAdv, __instance.advancedUpgrades.Length);
                Array.Copy(__instance.advancedUnlockRound, newAdvUnlock, __instance.advancedUnlockRound.Length);
                __instance.advancedUpgrades = newAdv;
                __instance.advancedUnlockRound = newAdvUnlock;
            }
            if (CustomCore.CustomUltimateBuffs.Count > 0)
            {
                int[] newUlti = new int[__instance.ultimateUpgrades.Count + CustomCore.CustomUltimateBuffs.Count];
                Array.Copy(__instance.ultimateUpgrades, newUlti, __instance.ultimateUpgrades.Length);
                __instance.ultimateUpgrades = newUlti;
            }
            if (CustomCore.CustomDebuffs.Count > 0)
            {
                bool[] newdeb = new bool[__instance.debuff.Count + CustomCore.CustomDebuffs.Count];
                Array.Copy(__instance.debuff, newdeb, __instance.debuff.Length);
                __instance.debuff = newdeb;
            }

            foreach (PlantType plantType in CustomCore.CustomUltimatePlants) // ×¢²áÇ¿¾¿Ö²Îï
            {
                TravelMgr.allStrongUltimtePlant.Add(plantType);
            }
        }

        [HarmonyPatch("GetAdvancedBuffPool")]
        [HarmonyPostfix]
        public static void PostGetAdvancedBuffPool(ref Il2CppSystem.Collections.Generic.List<int> __result)
        {
            for (int i = __result.Count - 1; i >= 0; i--)
            {
                if (CustomCore.CustomAdvancedBuffs.ContainsKey(__result[i]) && !CustomCore.CustomAdvancedBuffs[__result[i]].Item3())
                {
                    __result.Remove(__result[i]);
                }
            }
        }

        [HarmonyPatch(nameof(TravelMgr.GetAdvancedText))]
        [HarmonyPostfix]
        public static void PostGetAdvancedText(ref int index, ref string __result)
        {
            if (CustomCore.CustomAdvancedBuffs.ContainsKey(index) && CustomCore.CustomAdvancedBuffs[index].Item5 is not null)
            {
                __result = $"<color={CustomCore.CustomAdvancedBuffs[index].Item5}>{__result}</color>";
            }
        }

        [HarmonyPatch(nameof(TravelMgr.GetPlantTypeByAdvBuff))]
        [HarmonyPostfix]
        public static void PostGetPlantTypeByAdvBuff(ref int index, ref PlantType __result)
        {
            if (CustomCore.CustomAdvancedBuffs.ContainsKey(index) && CustomCore.CustomAdvancedBuffs[index].Item1 is not PlantType.Nothing)
            {
                __result = CustomCore.CustomAdvancedBuffs[index].Item1;
            }
        }
    }

    [HarmonyPatch(typeof(TravelStore))]
    public static class TravelStorePatch
    {
        [HarmonyPatch("RefreshBuff")]
        [HarmonyPostfix]
        public static void PostRefreshBuff(TravelStore __instance)
        {
            foreach (var travelBuff in __instance.gameObject.GetComponentsInChildren<TravelBuff>())
            {
                if (travelBuff.theBuffType is (int)BuffType.AdvancedBuff && CustomCore.CustomAdvancedBuffs.ContainsKey(travelBuff.theBuffNumber))
                {
                    travelBuff.cost = CustomCore.CustomAdvancedBuffs[travelBuff.theBuffNumber].Item4;
                    travelBuff.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = $"£¤{CustomCore.CustomAdvancedBuffs[travelBuff.theBuffNumber].Item4}";
                }
                if (travelBuff.theBuffType is (int)BuffType.UltimateBuff && CustomCore.CustomUltimateBuffs.ContainsKey(travelBuff.theBuffNumber))
                {
                    travelBuff.cost = CustomCore.CustomUltimateBuffs[travelBuff.theBuffNumber].Item3;
                    travelBuff.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = $"£¤{CustomCore.CustomUltimateBuffs[travelBuff.theBuffNumber].Item4}";
                }
            }
        }
    }

    [HarmonyPatch(typeof(TypeMgr))]
    public static class TypeMgrPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("BigNut")]
        public static bool PreBigNut(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.BigNut.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.BigNut.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("BigZombie")]
        public static bool PreBigZombie(ref ZombieType theZombieType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.BigZombie.Contains(theZombieType))
            {
                __result = true;
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("DoubleBoxPlants")]
        public static bool PreDoubleBoxPlants(ref PlantType thePlantType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.DoubleBoxPlants.Contains(thePlantType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.DoubleBoxPlants.TryGetValue(thePlantType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        /*[HarmonyPrefix]
        [HarmonyPatch("EliteZombie")]
        public static bool PreEliteZombie(ref ZombieType theZombieType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.EliteZombie.Contains(theZombieType))
            {
                __result = true;
                return false;
            }

            return true;
        }*/

        [HarmonyPrefix]
        [HarmonyPatch("FlyingPlants")]
        public static bool PreFlyingPlants(ref PlantType thePlantType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.FlyingPlants.Contains(thePlantType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.FlyingPlants.TryGetValue(thePlantType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("GetPlantTag")]
        public static bool PreGetPlantTag(ref Plant plant)
        {
            if (CustomCore.CustomPlantTypes.Contains(plant.thePlantType))
            {
                plant.plantTag = new()
                {
                    icePlant = TypeMgr.IsIcePlant(plant.thePlantType),
                    caltropPlant = TypeMgr.IsCaltrop(plant.thePlantType),
                    doubleBoxPlant = TypeMgr.DoubleBoxPlants(plant.thePlantType),
                    firePlant = TypeMgr.IsFirePlant(plant.thePlantType),
                    flyingPlant = TypeMgr.FlyingPlants(plant.thePlantType),
                    lanternPlant = TypeMgr.IsPlantern(plant.thePlantType),
                    smallLanternPlant = TypeMgr.IsSmallRangeLantern(plant.thePlantType),
                    magnetPlant = TypeMgr.IsMagnetPlants(plant.thePlantType),
                    nutPlant = TypeMgr.IsNut(plant.thePlantType),
                    tallNutPlant = TypeMgr.IsTallNut(plant.thePlantType),
                    potatoPlant = TypeMgr.IsPotatoMine(plant.thePlantType),
                    potPlant = TypeMgr.IsPot(plant.thePlantType),
                    puffPlant = TypeMgr.IsPuff(plant.thePlantType),
                    pumpkinPlant = TypeMgr.IsPumpkin(plant.thePlantType),
                    spickRockPlant = TypeMgr.IsSpickRock(plant.thePlantType),
                    tanglekelpPlant = TypeMgr.IsTangkelp(plant.thePlantType),
                    waterPlant = TypeMgr.IsWaterPlant(plant.thePlantType),
                };

                return false;
            }

            if (CustomCore.CustomPlantsSkin.ContainsKey(plant.thePlantType))
            {
                plant.plantTag = new()
                {
                    icePlant = TypeMgr.IsIcePlant(plant.thePlantType),
                    caltropPlant = TypeMgr.IsCaltrop(plant.thePlantType),
                    doubleBoxPlant = TypeMgr.DoubleBoxPlants(plant.thePlantType),
                    firePlant = TypeMgr.IsFirePlant(plant.thePlantType),
                    flyingPlant = TypeMgr.FlyingPlants(plant.thePlantType),
                    lanternPlant = TypeMgr.IsPlantern(plant.thePlantType),
                    smallLanternPlant = TypeMgr.IsSmallRangeLantern(plant.thePlantType),
                    magnetPlant = TypeMgr.IsMagnetPlants(plant.thePlantType),
                    nutPlant = TypeMgr.IsNut(plant.thePlantType),
                    tallNutPlant = TypeMgr.IsTallNut(plant.thePlantType),
                    potatoPlant = TypeMgr.IsPotatoMine(plant.thePlantType),
                    potPlant = TypeMgr.IsPot(plant.thePlantType),
                    puffPlant = TypeMgr.IsPuff(plant.thePlantType),
                    pumpkinPlant = TypeMgr.IsPumpkin(plant.thePlantType),
                    spickRockPlant = TypeMgr.IsSpickRock(plant.thePlantType),
                    tanglekelpPlant = TypeMgr.IsTangkelp(plant.thePlantType),
                    waterPlant = TypeMgr.IsWaterPlant(plant.thePlantType)
                };

                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsCaltrop")]
        public static bool PreIsCaltrop(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsCaltrop.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsCaltrop.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsFirePlant")]
        public static bool PreIsFirePlant(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsFirePlant.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsFirePlant.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsIcePlant")]
        public static bool PreIsIcePlant(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsIcePlant.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsIcePlant.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsMagnetPlants")]
        public static bool PreIsMagnetPlants(ref PlantType thePlantType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsMagnetPlants.Contains(thePlantType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsMagnetPlants.TryGetValue(thePlantType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsNut")]
        public static bool PreIsNut(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsNut.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsNut.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsPlantern")]
        public static bool PreIsPlantern(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsPlantern.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsPlantern.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsPot")]
        public static bool PreIsPot(ref PlantType thePlantType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsPot.Contains(thePlantType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsPot.TryGetValue(thePlantType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsPotatoMine")]
        public static bool PreIsPotatoMine(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsPotatoMine.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsPotatoMine.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsPuff")]
        public static bool PreIsPuff(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsPuff.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsPuff.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsPumpkin")]
        public static bool PreIsPumpkin(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsPumpkin.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsPumpkin.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsSmallRangeLantern")]
        public static bool PreIsSmallRangeLantern(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsSmallRangeLantern.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsSmallRangeLantern.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsSpecialPlant")]
        public static bool PreIsSpecialPlant(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsSpecialPlant.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsSpecialPlant.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsSpickRock")]
        public static bool PreIsSpickRock(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsSpickRock.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsSpickRock.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsTallNut")]
        public static bool PreIsTallNut(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsTallNut.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsTallNut.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsTangkelp")]
        public static bool PreIsTangkelp(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsTangkelp.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsTangkelp.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("IsWaterPlant")]
        public static bool PreIsWaterPlant(ref PlantType theSeedType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.IsWaterPlant.Contains(theSeedType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.IsWaterPlant.TryGetValue(theSeedType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("UmbrellaPlants")]
        public static bool PreUmbrellaPlants(ref PlantType thePlantType, ref bool __result)
        {
            if (CustomCore.TypeMgrExtra.UmbrellaPlants.Contains(thePlantType))
            {
                __result = true;
                return false;
            }

            if (CustomCore.TypeMgrExtraSkin.UmbrellaPlants.TryGetValue(thePlantType, out int value))
            {
                switch (value)
                {
                    case -1:
                        return true;

                    case 0:
                        __result = false;
                        return false;

                    case 1:
                        __result = true;
                        return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(UIMgr))]
    public static class UIMgrPatch
    {
        [HarmonyPatch(nameof(UIMgr.EnterChallengeMenu))]
        [HarmonyPostfix]
        public static void PostEnterChallengeMenu()
        {
            var levels = GameAPP.canvas.GetChild(0).FindChild("Levels");
            var firstBtns = levels.FindChild("FirstBtns");
            if (firstBtns.FindChild("CustomLevels") is null || firstBtns.FindChild("CustomLevels").IsDestroyed())
            {
                GameObject custom = UnityEngine.Object.Instantiate(firstBtns.GetChild(0).gameObject, firstBtns);
                custom.name = "CustomLevels";
                custom.transform.localPosition = new(-150, 30, 0);
                var window = custom.transform.FindChild("Window");
                window.FindChild("Name").GetComponent<TextMeshProUGUI>().text = "¶þ´´¹Ø¿¨";
                var adv = levels.FindChild("PageAdvantureLevel");
                var customLevels = UnityEngine.Object.Instantiate(adv.gameObject, levels);
                customLevels.active = false;
                customLevels.name = "PageCustomLevel";
                var pages = customLevels.transform.FindChild("Pages");
                var levelSample = UnityEngine.Object.Instantiate(pages.FindChild("Page1").FindChild("Lv1").gameObject);
                foreach (var l in pages.FindChild("Page1").GetComponentsInChildren<Transform>(true))
                {
                    UnityEngine.Object.Destroy(l.gameObject);
                }
                var pageSample = UnityEngine.Object.Instantiate(pages.FindChild("Page1").gameObject);
                UnityEngine.Object.Destroy(pages.FindChild("Page1").gameObject);
                UnityEngine.Object.Destroy(pages.FindChild("Page2").gameObject);
                UnityEngine.Object.Destroy(pages.FindChild("Page3").gameObject);
                int levelIndex = 0;
                int columnIndex = 0;
                int rowIndex = 0;
                int pageIndex = 0;
                foreach (var level in CustomCore.CustomLevels)
                {
                    if (levelIndex % 18 is 0)
                    {
                        UnityEngine.Object.Instantiate(pageSample, pages).name = $"Pages{levelIndex / 18 + 1}";
                    }
                    columnIndex = levelIndex % 6;
                    rowIndex = levelIndex / 6;
                    pageIndex = rowIndex / 3;
                    var levelBtn = UnityEngine.Object.Instantiate(levelSample, pages.FindChild($"Pages{levelIndex / 18 + 1}"));
                    levelBtn.transform.localPosition = new(-50 + 150 * columnIndex, 60 - 130 * rowIndex, 0);
                    levelBtn.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = level.Logo;
                    levelBtn.transform.GetChild(1).GetComponent<Advanture_Btn>().levelType = (LevelType)66;
                    levelBtn.transform.GetChild(1).GetComponent<Advanture_Btn>().buttonNumber = level.ID;
                    levelBtn.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = level.Name();
                    levelIndex++;
                }
                window.GetComponent<FirstBtns>().pageToOpen = customLevels;
                window.GetComponent<FirstBtns>().originPosition = new(-150, 30, 0);
                UnityEngine.Object.Destroy(pageSample);
                UnityEngine.Object.Destroy(levelSample);
            }
        }

        [HarmonyPatch(nameof(UIMgr.EnterGame))]
        [HarmonyPrefix]
        public static bool PreEnterGame(ref int levelType, ref int levelNumber, ref int id, ref string name)
        {
            if (levelType is not 66) return true;
            var levelData = CustomCore.CustomLevels[levelNumber];

            // ÇåÀíUI×ÊÔ´
            GameAPP.UIManager.PopAll();

            // ÖØÖÃÏà»ú
            CamaraFollowMouse.Instance.ResetCamera();

            // ÉèÖÃÓÎÏ·ËÙ¶È
            Time.timeScale = GameAPP.gameSpeed;

            // ÉèÖÃµ±Ç°¹Ø¿¨ÐÅÏ¢
            GameAPP.theBoardType = (LevelType)levelType;
            GameAPP.theBoardLevel = levelNumber;

            // ÇåÀíÏÖÓÐµÄTravel¹ÜÀíÆ÷
            if (TravelMgr.Instance != null)
            {
                UnityEngine.Object.Destroy(TravelMgr.Instance);
                TravelMgr.Instance = null;
            }

            // ´´½¨ÓÎÏ·°å
            GameObject boardGO = new("Board");
            GameAPP.board = boardGO;
            Board board = boardGO.AddComponent<Board>();
            board.boardTag = levelData.BoardTag;
            board.rowNum = levelData.RowCount;
            board.theMaxWave = levelData.WaveCount();
            board.cardSelectable = levelData.NeedSelectCard;
            board.theSun = levelData.Sun();
            board.zombieDamageAdder = levelData.ZombieHealthRate();
            board.seedPool = levelData.SeedRainPlantTypes().ToIl2CppList();
            levelData.PostBoard(board);
            // »ñÈ¡³¡¾°ÀàÐÍºÍµØÍ¼Â·¾¶
            string mapPath = MapData_cs.GetMapPath(levelData.SceneType);

            // ¼ÓÔØ²¢ÊµÀý»¯µØÍ¼
            GameObject mapInstance = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(mapPath), boardGO.transform);
            board.ChangeMap(mapInstance);

            InitZombieList.InitZombie((LevelType)levelType, levelNumber);

            // ²¥·ÅÒôÀÖ²¢¿ªÊ¼ÓÎÏ·
            GameAPP.Instance.PlayMusic(MusicType.SelectCard);
            GameAPP.theGameStatus = GameStatus.InInterlude;

            // ³õÊ¼»¯ÓÎÏ·°å
            levelData.PreInitBoard();

            levelData.PostInitBoard(board.gameObject.AddComponent<InitBoard>());
            foreach (var p in levelData.PrePlants())
            {
                CreatePlant.Instance.SetPlant(p.Item1, p.Item2, p.Item3);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(ZombieDataManager))]
    public static class ZombieDataPatch
    {
        [HarmonyPatch("LoadData")]
        [HarmonyPostfix]
        public static void PostLoadData()
        {
            foreach (var z in CustomCore.CustomZombies)
            {
                ZombieDataManager.zombieDataDic[(ZombieType)z.Key] = z.Value.Item3;
            }
        }
    }
}