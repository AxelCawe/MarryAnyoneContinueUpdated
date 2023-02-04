// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Patches.TaleWorlds.CampaignSystem.HeroPatch
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;


 
namespace MarryAnyone.Patches.TaleWorlds.CampaignSystem
{
  [HarmonyPatch(typeof (Hero))]
  internal static class HeroPatch
  {
    [HarmonyPatch(typeof (Hero), "UpdateHomeSettlement")]
    [HarmonyPrefix]
    private static bool UpdateHomeSettlementPrefix(Hero __instance)
    {
      if (__instance.Clan != null && !__instance.Clan.IsNeutralClan && __instance.Clan.HomeSettlement == null)
      {
        Settlement settlement = (Settlement) null;
        if (__instance.Clan.IsBanditFaction || __instance.Clan.IsOutlaw)
          settlement = (Extensions.GetRandomElementInefficiently<Settlement>(Settlement.FindAll((Func<Settlement, bool>) (x =>
          {
            if (!x.IsHideout || x.Culture != __instance.Culture)
              return false;
            return x.OwnerClan == null || x.OwnerClan == __instance.Clan;
          }))) ?? Extensions.GetRandomElementInefficiently<Settlement>(Settlement.FindAll((Func<Settlement, bool>) (x =>
          {
            if (!x.IsHideout)
              return false;
            return x.OwnerClan == null || x.OwnerClan == __instance.Clan;
          })))) ?? Extensions.GetRandomElementInefficiently<Settlement>(Settlement.FindAll((Func<Settlement, bool>) (x => x.IsHideout)));
        if (settlement == null && (__instance.Clan.IsClanTypeMercenary || __instance.Clan.IsMafia || __instance.Clan.IsNomad || __instance.Clan.IsRebelClan))
          settlement = (Extensions.GetRandomElementInefficiently<Settlement>(Settlement.FindAll((Func<Settlement, bool>) (x =>
          {
            if (!x.IsFortification || x.Culture != __instance.Culture)
              return false;
            return x.OwnerClan == null || x.OwnerClan == __instance.Clan;
          }))) ?? Extensions.GetRandomElementInefficiently<Settlement>(Settlement.FindAll((Func<Settlement, bool>) (x =>
          {
            if (!x.IsFortification)
              return false;
            return x.OwnerClan == null || x.OwnerClan == __instance.Clan;
          })))) ?? Extensions.GetRandomElementInefficiently<Settlement>(Settlement.FindAll((Func<Settlement, bool>) (x => x.IsFortification)));
        if (settlement == null)
          settlement = Extensions.GetRandomElementInefficiently<Settlement>((IEnumerable<Settlement>) Settlement.All);
        if (settlement == null)
        {
          Helper.Print(string.Format("UpdateHomeSettlementPrefix Settlement unresolved for hero {0}", (object) __instance.Name), Helper.PrintHow.PrintToLogAndWriteAndForceDisplay);
          throw new Exception(string.Format("UpdateHomeSettlementPrefix Settlement unresolved for hero {0}", (object) __instance.Name));
        }
        FieldInfo fieldInfo = AccessTools.Field(typeof (Clan), "_home");
        if (fieldInfo == (FieldInfo) null)
          throw new Exception("_home not resolved on Clan class");
        fieldInfo.SetValue((object) __instance.Clan, (object) settlement);
      }
      return true;
    }
  }
}
