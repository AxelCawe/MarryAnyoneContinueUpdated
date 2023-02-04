// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Patches.Models.DefaultMarriageModel_IsSuitableForMarriage_Patch
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using HarmonyLib;
using MarryAnyone.Models;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;


 
namespace MarryAnyone.Patches.Models
{
  [HarmonyPatch(typeof (DefaultMarriageModel), "IsSuitableForMarriage", new Type[] {typeof (Hero)})]
  public class DefaultMarriageModel_IsSuitableForMarriage_Patch
  {
    [HarmonyPrefix]
    public static bool Prefix(
      DefaultMarriageModel __instance,
      Hero maidenOrSuitor,
      ref bool __result)
    {
      __result = MADefaultMarriageModel.IsSuitableForMarriageStatic(maidenOrSuitor);
      return false;
    }
  }
}
