// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Patches.TaleWorlds.CampaignSystem.TournamentGame161
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;


 
namespace MarryAnyone.Patches.TaleWorlds.CampaignSystem
{
  [HarmonyPatch(typeof (FightTournamentGame))]
  public static class TournamentGame161
  {
    private static void GetUpgradeTargetsPatch(
      CharacterObject troop,
      ref List<CharacterObject> list)
    {
      if (!list.Contains(troop))
        list.Add(troop);
      if (troop.UpgradeTargets == null)
        return;
      foreach (CharacterObject upgradeTarget in troop.UpgradeTargets)
        TournamentGame161.GetUpgradeTargetsPatch(upgradeTarget, ref list);
    }

    private static int GetTroopPriorityPointForTournamentPatch(CharacterObject troop)
    {
      int num = 40000;
      if (troop == CharacterObject.PlayerCharacter)
        num += 80000;
      if (((BasicCharacterObject) troop).IsHero)
        num += 20000;
      return !((BasicCharacterObject) troop).IsHero || !troop.HeroObject.IsPlayerCompanion ? (troop.HeroObject?.Clan == null ? num + ((BasicCharacterObject) troop).Level : num + (int) troop.HeroObject.Clan?.Renown) : num + 10000;
    }

    [HarmonyPatch(typeof (FightTournamentGame), "GetParticipantCharacters", new Type[] {typeof (Settlement), typeof (bool)})]
    [HarmonyPrefix]
    internal static bool GetParticipantCharactersPatch(
      FightTournamentGame __instance,
      Settlement settlement,
      bool includePlayer,
      ref List<CharacterObject> __result)
    {
      if (!Helper.MASettings.SpouseJoinArena)
        return true;
      int participantCount = ((TournamentGame) __instance).MaximumParticipantCount;
      MethodInfo method = ((object) __instance).GetType().GetMethod("CanNpcJoinTournament", BindingFlags.Instance | BindingFlags.NonPublic);
      if (method == (MethodInfo) null)
        throw new Exception("CanNpcJoinTournament inacessible");
      MethodInfo methodInfo = AccessTools.Method(((object) __instance).GetType(), "SortTournamentParticipants", new Type[1]
      {
        typeof (List<CharacterObject>)
      });
      if (methodInfo == (MethodInfo) null)
        throw new Exception("methodInfoSortTournamentParticipants inacessible");
      List<CharacterObject> characterObjectList1 = new List<CharacterObject>();
      if (includePlayer)
        characterObjectList1.Add(CharacterObject.PlayerCharacter);
      int num1 = 0;
      int num2 = (int) ((double) participantCount / 2.0);
    
      {
        for (; num1 < settlement.Parties.Count && characterObjectList1.Count < num2; ++num1)
        {
          Hero leaderHero = settlement.Parties[num1].LeaderHero;
          if ((bool) method.Invoke((object) __instance, new object[3]
          {
            (object) leaderHero,
            (object) characterObjectList1,
            (object) true
          }) && leaderHero.IsLord)
            characterObjectList1.Add(leaderHero.CharacterObject);
        }
      }
      if (Settlement.CurrentSettlement == settlement)
      {
        int num3 = (int) ((double) participantCount * 2.0 / 3.0);
        int num4 = 0;
        foreach (TroopRosterElement troopRosterElement in MobileParty.MainParty.MemberRoster.GetTroopRoster())
        {
          if (characterObjectList1.Count < participantCount && num4 < num3 && ((BasicCharacterObject) troopRosterElement.Character).IsHero && !troopRosterElement.Character.HeroObject.IsWounded && (troopRosterElement.Character.HeroObject.IsPlayerCompanion && !troopRosterElement.Character.HeroObject.IsNoncombatant || troopRosterElement.Character.HeroObject.Spouse == Hero.MainHero || Hero.MainHero.ExSpouses.Contains(troopRosterElement.Character.HeroObject)) && !characterObjectList1.Contains(troopRosterElement.Character))
          {
            characterObjectList1.Add(troopRosterElement.Character);
            ++num4;
          }
        }
      }
      if (characterObjectList1.Count < participantCount)
      {
        foreach (Hero hero in settlement.HeroesWithoutParty)
        {
          if (!hero.IsNoncombatant && (double) hero.Age >= (double) Campaign.Current.Models.AgeModel.HeroComesOfAge && (hero.IsWanderer || hero.IsLord && hero.PartyBelongedTo == null))
          {
            characterObjectList1.Add(hero.CharacterObject);
            if (characterObjectList1.Count >= participantCount)
              break;
          }
        }
      }
      if (characterObjectList1.Count < participantCount)
      {
        List<CharacterObject> characterObjectList2 = new List<CharacterObject>();
        if (settlement.Parties != null)
        {
          using (List<MobileParty>.Enumerator enumerator = settlement.Parties.GetEnumerator())
          {
            while (enumerator.MoveNext())
            {
              foreach (TroopRosterElement troopRosterElement in enumerator.Current.MemberRoster.GetTroopRoster())
              {
                if (!((BasicCharacterObject) troopRosterElement.Character).IsHero && !((BasicCultureObject) troopRosterElement.Character.Culture).IsBandit && !characterObjectList2.Contains(troopRosterElement.Character))
                  characterObjectList2.Add(troopRosterElement.Character);
              }
            }
            goto label_55;
          }
        }
label_39:
        if (characterObjectList2.Count <= 0)
        {
          List<CharacterObject> list = new List<CharacterObject>();
          CultureObject troopCulture = settlement != null ? settlement.Culture : Game.Current.ObjectManager.GetObject<CultureObject>("empire");
          TournamentGame161.GetUpgradeTargetsPatch(CharacterObject.FindFirst((Predicate<CharacterObject>) (x => x.IsBasicTroop && x.Culture == troopCulture)), ref list);
          Extensions.Shuffle<CharacterObject>((IList<CharacterObject>) list);
          for (int index1 = 0; index1 < list.Count; ++index1)
          {
            if (characterObjectList1.Count >= participantCount)
            {
label_45:
              while (characterObjectList1.Count < participantCount)
              {
                Extensions.Shuffle<CharacterObject>((IList<CharacterObject>) list);
                int index2 = 0;
                while (true)
                {
                  if (index2 < list.Count && characterObjectList1.Count < participantCount)
                  {
                    characterObjectList1.Add(list[index2]);
                    ++index2;
                  }
                  else
                    goto label_45;
                }
              }
              goto label_55;
            }
            else if (!characterObjectList1.Contains(list[index1]))
              characterObjectList1.Add(list[index1]);
          }
label_53:
          while (characterObjectList1.Count < participantCount)
          {
            Extensions.Shuffle<CharacterObject>((IList<CharacterObject>) list);
            int index = 0;
            while (true)
            {
              if (index < list.Count && characterObjectList1.Count < participantCount)
              {
                characterObjectList1.Add(list[index]);
                ++index;
              }
              else
                goto label_53;
            }
          }
        }
        else
        {
          CharacterObject randomElement = Extensions.GetRandomElement<CharacterObject>((IReadOnlyList<CharacterObject>) characterObjectList2);
          characterObjectList1.Add(randomElement);
          characterObjectList2.Remove(randomElement);
        }
label_55:
        if (characterObjectList1.Count < participantCount)
          goto label_39;
      }
      methodInfo.Invoke((object) __instance, new object[1]
      {
        (object) characterObjectList1
      });
      __result = characterObjectList1;
      return false;
    }
  }
}
