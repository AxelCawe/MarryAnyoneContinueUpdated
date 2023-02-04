// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Helpers.PartyHelper
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using HarmonyLib;
using System;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;


namespace MarryAnyone.Helpers
{
  internal static class PartyHelper
  {
    public static void SwapMainParty(MobileParty newMainParty)
    {
      FieldInfo fieldInfo = AccessTools.Field(typeof (Campaign), "<MainParty>k__BackingField");
      if (fieldInfo == (FieldInfo) null)
        throw new Exception("Property MainParty not found on Campaign instance");
      fieldInfo.SetValue((object) Campaign.Current, (object) newMainParty);
    }

    public static void SwapPartyBelongedTo(Hero hero, MobileParty party)
    {
      Helper.Print(string.Format("Swap PartyBelongedTo for Hero {0} to party {1}", (object) ((object) hero.Name).ToString(), party == null ? (object) "NULL" : (object) ((object) party.Name).ToString()), Helper.PrintHow.PrintToLogAndWrite);
      FieldInfo field = typeof (Hero).GetField("_partyBelongedTo", BindingFlags.Instance | BindingFlags.NonPublic);
      if (field == (FieldInfo) null)
        throw new Exception("_partyBelongedTo no found on Hero");
      field.SetValue((object) hero, (object) party);
    }

    public static void SetLeaderAtTop(PartyBase party)
    {
      Hero leaderHero = party.LeaderHero;
      if (leaderHero == null)
        return;
      CharacterObject characterObject = leaderHero.CharacterObject;
      party.MemberRoster.RemoveTroop(characterObject, 1, new UniqueTroopDescriptor(), 0);
      party.MemberRoster.AddToCounts(characterObject, 1, true, 0, 0, true, -1);
    }
  }
}
