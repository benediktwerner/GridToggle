using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace GridToggleNS;

[BepInPlugin("GridToggle", "GridToggle", "0.2.0")]
public class GridTogglePlugin : BaseUnityPlugin
{
	public static ManualLogSource L;
	public static void Log(string s)
	{
		L.LogInfo((object)(DateTime.Now.ToString("HH:MM:ss") + ": " + s));
	}
	public static bool SnapActive = false; 

	public static Harmony HarmonyInstance;

/*
	public static Dictionary<string, AudioClip> MyAudioClips = new Dictionary<string, AudioClip>();
	void LoadAudio(string path, string key)
	{
		StartCoroutine(
			ResourceHelper.GetAudioClip(
				path,
				(AudioClip ac) => { MyAudioClips.Add(key, ac);Log("loaded sound: "+key);},
				()=>Log("Failed to load the sound")));
	}
*/
	public void Awake()
	{
		L = ((GridTogglePlugin)this).Logger;

		try
		{
			HarmonyInstance = new Harmony("GridTogglePlugin");
			HarmonyInstance.PatchAll(typeof(GridTogglePlugin));
		}
		catch (Exception ex3)
		{
			Log("Patching failed: " + ex3.Message);
		}
	}

	[HarmonyPatch(typeof(WorldManager), "Update")]
	[HarmonyPostfix]
	public static void WorldManager_Update_Postfix(WorldManager __instance)
	{
		if (__instance.IsPlaying && InputController.instance.SnapCardsTriggered())
		{
			SnapActive = !SnapActive;
		}
		if (__instance.CurrentGameState == WorldManager.GameState.InMenu && SnapActive)
		{
			SnapActive = false;
		}
		if (SnapActive)
			__instance.SnapCardsToGrid();
	}

	
	[HarmonyPatch(typeof(WorldManager), "SnapCardsToGrid")]
	[HarmonyPrefix]
	public static bool WorldManager_SnapCardsToGrid_Prefix(WorldManager __instance, bool __runOriginal)
	{
		if (!__runOriginal) {
			// The FixGrid mod already performed the snapping
			return;
		}

		__instance.gridAlpha = 1f;
		foreach (GameCard allCard in __instance.AllCards)
		{
			if (!allCard.HasParent && !(allCard.CardData is Mob) && !allCard.BeingDragged && (!allCard.Velocity.HasValue || allCard.Velocity.Value.magnitude < 0.01))
			{
				Vector3 position = allCard.transform.position;
				position.x = (float)Mathf.RoundToInt(position.x / __instance.GridWidth) * __instance.GridWidth;
				position.z = (float)Mathf.RoundToInt(position.z / __instance.GridHeight) * __instance.GridHeight;
				allCard.TargetPosition = position;
			}
		}
		return false;
	}
	
}
