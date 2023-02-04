// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Patches.TaleWorlds.CampaignSystem.GameComponents.DefaultClanFinanceModelPatch
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Localization;


 
namespace MarryAnyone.Patches.TaleWorlds.CampaignSystem.GameComponents
{
  [HarmonyPatch(typeof (DefaultClanFinanceModel))]
  internal static class DefaultClanFinanceModelPatch
  {
    [HarmonyPatch(typeof (DefaultClanFinanceModel), "AddExpenseFromLeaderParty")]
    [HarmonyPrefix]
    private static bool AddExpenseFromLeaderPartyPatch(
      Clan clan,
      ref ExplainedNumber goldChange,
      bool applyWithdrawals,
      DefaultClanFinanceModel __instance)
    {
      Hero leader = clan.Leader;
      MobileParty partyBelongedTo = leader?.PartyBelongedTo;
      if (partyBelongedTo == null || leader == partyBelongedTo.LeaderHero || partyBelongedTo.LeaderHero != Hero.MainHero)
        return true;
      Helper.Print(string.Format("AddExpenseFromLeaderPartyPatch for clan {0} leader ?= {1} playerClan ?= {2}", (object) clan.Name, (object) leader.Name, Clan.PlayerClan != null ? (object) Clan.PlayerClan.Name : "NULL"), Helper.PrintHow.PrintToLogAndWrite);
      return false;
    }
  }
}
