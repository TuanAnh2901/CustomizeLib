using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomizeLib.BepInEx
{
    /// <summary>
    /// Add custom plant card to page automatically
    /// </summary>
    public static class SelectCustomPlants
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AddCardAutoPage(SeedLibrary __instance, int plantId)
        {
            Transform transform = __instance.transform.Find("Pages");
            Transform transform2 = transform.GetChild(transform.childCount - 1);
            if (transform2.childCount % 54 == 0 && transform2.childCount != 0)
            {
                transform2 = UnityEngine.Object.Instantiate<GameObject>(transform2.gameObject, transform2.parent).transform;
                int num = int.Parse(Regex.Match(transform2.name, "\\d+").Value);
                transform2.name = "Page" + (num + 1);
                for (int i = 0; i < transform2.childCount; i++)
                {
                    UnityEngine.Object.Destroy(transform2.GetChild(i).gameObject);
                }
            }
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(__instance.transform.Find("Pages/Page1/PeaShooter").gameObject);
            for (int j = 0; j < gameObject.transform.childCount; j++)
            {
                Transform transform3 = gameObject.transform.GetChild(j);
                if (transform3.name != "PacketBg")
                {
                    GameObject gameObject2 = transform3.gameObject;
                    CardUI cardUI = gameObject2.GetComponent<CardUI>();
                    if (cardUI != null)
                    {
                        cardUI.fullCD = PlantDataLoader.plantDatas[(PlantType)plantId].field_Public_Single_2;
                        cardUI.thePlantType = (PlantType)plantId;
                        cardUI.theSeedCost = PlantDataLoader.plantDatas[(PlantType)plantId].field_Public_Int32_1;
                        if (j == 1)
                        {
                            cardUI.theSeedCost *= 2;
                        }
                        Mouse.Instance.ChangeCardSprite((PlantType)plantId, cardUI);
                    }
                }
                else
                {
                    transform3.GetChild(0).GetComponent<Image>().sprite = GameAPP.resourcesManager.plantPreviews[(PlantType)plantId].GetComponent<SpriteRenderer>().sprite;
                    transform3.GetChild(1).GetComponent<TextMeshProUGUI>().text = PlantDataLoader.plantDatas[(PlantType)plantId].field_Public_Int32_1.ToString();
                }
            }
            gameObject.name = GameAPP.resourcesManager.plantPrefabs[(PlantType)plantId].name.Substring(0, GameAPP.resourcesManager.plantPrefabs[(PlantType)plantId].name.Length - 6);
            if (transform2 != null)
            {
                gameObject.transform.SetParent(transform2, false);
            }
            }
        }

        /// <summary>
    /// Patch for UIButton OnMouseUpAsButton
        /// </summary>
    [HarmonyPatch(typeof(UIButton), "OnMouseUpAsButton")]
    public static class UIButtonOnMouseUpAsButtonPatch
    {
        [HarmonyPostfix]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Postfix(UIButton __instance)
        {
            Transform transform = __instance.transform.parent.Find("Grid");
            SeedLibrary seedLibrary;
            if (transform == null)
            {
                seedLibrary = null;
            }
            else
            {
                GameObject gameObject = transform.gameObject;
                seedLibrary = ((gameObject != null) ? gameObject.GetComponent<SeedLibrary>() : null);
            }
            SeedLibrary seedLibrary2 = seedLibrary;
            if (__instance.name == "NextPage" && seedLibrary2 != null && seedLibrary2 != null && seedLibrary2 != null)
            {
                seedLibrary2.NextPage(__instance.transform);
            }
            if (__instance.name == "LastPage" && seedLibrary2 != null && seedLibrary2 != null && seedLibrary2 != null)
            {
                seedLibrary2.LastPage(__instance.transform);
            }
        }
    }

    /// <summary>
    /// Patch for SeedLibrary
    /// </summary>
    [HarmonyPatch(typeof(SeedLibrary))]
    public static class SeedLibratyPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("NextPage")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void PreNextPage(SeedLibrary __instance)
        {
            if (__instance == null)
            {
                return;
            }
            Transform transform = __instance.transform;
            DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
            defaultInterpolatedStringHandler.AppendLiteral("Pages/Page");
            defaultInterpolatedStringHandler.AppendFormatted<int>(SeedLibratyPatch.Field0);
            Transform transform2 = transform.Find(defaultInterpolatedStringHandler.ToStringAndClear());
            if (transform2 != null)
            {
                transform2.gameObject.SetActive(false);
            }
            if (transform2.parent.childCount > SeedLibratyPatch.Field0)
            {
                SeedLibratyPatch.Field0++;
            }
            Transform transform3 = __instance.transform;
            defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
            defaultInterpolatedStringHandler.AppendLiteral("Pages/Page");
            defaultInterpolatedStringHandler.AppendFormatted<int>(SeedLibratyPatch.Field0);
            Transform transform4 = transform3.Find(defaultInterpolatedStringHandler.ToStringAndClear());
            if (transform4 != null)
            {
                transform4.gameObject.SetActive(true);
            }
        }

        [HarmonyPatch("LastPage")]
        [HarmonyPostfix]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void PreLastPage(SeedLibrary __instance)
        {
            if (__instance == null)
            {
                return;
            }
            Transform transform = __instance.transform;
            DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
            defaultInterpolatedStringHandler.AppendLiteral("Pages/Page");
            defaultInterpolatedStringHandler.AppendFormatted<int>(SeedLibratyPatch.Field0);
            Transform transform2 = transform.Find(defaultInterpolatedStringHandler.ToStringAndClear());
            if (transform2 != null)
            {
                transform2.gameObject.SetActive(false);
            }
            if (SeedLibratyPatch.Field0 > 1)
            {
                SeedLibratyPatch.Field0--;
            }
            Transform transform3 = __instance.transform;
            defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
            defaultInterpolatedStringHandler.AppendLiteral("Pages/Page");
            defaultInterpolatedStringHandler.AppendFormatted<int>(SeedLibratyPatch.Field0);
            Transform transform4 = transform3.Find(defaultInterpolatedStringHandler.ToStringAndClear());
            if (transform4 != null)
            {
                transform4.gameObject.SetActive(true);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void PreAwake(SeedLibrary __instance)
        {
            GameObject gameObject = __instance.transform.parent.Find("NextPage").gameObject;
            GameObject gameObject2 = __instance.transform.parent.Find("LastPage").gameObject;
            gameObject.SetActive(true);
            gameObject2.SetActive(true);
            if (__instance.transform.Find("Pages").childCount < 3)
            {
                Vector2 vector = gameObject.transform.position;
                Vector2 vector2 = gameObject2.transform.position;
                vector2.y -= 0.1f;
                vector2.x += 2.2f;
                vector.y -= 0.8f;
                gameObject.transform.position = vector;
                gameObject2.transform.position = vector2;
                foreach (PlantType plantType in GameAPP.resourcesManager.allPlants)
                {
                    SelectCustomPlants.AddCardAutoPage(__instance, (int)plantType);
                }
            }
        }

        private static int Field0 = 1;
    }
}