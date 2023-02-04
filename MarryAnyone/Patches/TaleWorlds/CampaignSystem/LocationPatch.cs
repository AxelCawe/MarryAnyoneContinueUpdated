// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Patches.TaleWorlds.CampaignSystem.LocationPatch
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem.Settlements.Locations;


 
namespace MarryAnyone.Patches.TaleWorlds.CampaignSystem
{
  [HarmonyPatch(typeof (Location))]
  internal class LocationPatch
  {
    [HarmonyPatch(typeof (Location), "DeserializeDelegate", new Type[] {typeof (string)})]
    [HarmonyPrefix]
    public static bool DeserializeDelegatePatchPrefix(string text, ref CanUseDoor __result)
    {
      __result = (CanUseDoor) null;
      return !string.IsNullOrEmpty(text);
    }

    [HarmonyPatch(typeof (Location), "CanAIExit", new Type[] {typeof (LocationCharacter)})]
    [HarmonyPrefix]
    public static bool CanAIExitPatchPrefix(
      Location __instance,
      LocationCharacter character,
      ref bool __result)
    {
      __result = false;
      if (character == null)
      {
        Helper.Print(string.Format("CanAIExit on {0} pour {1} PATH return FALSE", (object) __instance.Name, character == null ? (object) "character NULL" : (object) ((object) character).ToString()), Helper.PrintHow.PrintToLogAndWrite);
        __result = false;
        return false;
      }
      Helper.Print(string.Format("CanAIExit on {0} pour {1} VA FAIRE", (object) __instance.Name, character == null ? (object) "character NULL" : (object) ((object) character).ToString()), Helper.PrintHow.PrintToLogAndWrite);
      return true;
    }
  }
}
