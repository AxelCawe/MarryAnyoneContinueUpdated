// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Patches.TaleWorlds.MountAndBlade.MissionPatch
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using HarmonyLib;
using Helpers;
using MarryAnyone.Behaviors;
using MarryAnyone.Helpers;
using MarryAnyone.MA;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;


namespace MarryAnyone.Patches.TaleWorlds.MountAndBlade
{
  [HarmonyPatch(typeof (Mission))]
  internal static class MissionPatch
  {
    private const int BORNE_TRAIT_POSITIF = 2;
    private static ShortLifeBiObjects _agents = new ShortLifeBiObjects(2000);

    [HarmonyPatch(typeof (Mission), "OnAgentRemoved", new Type[] {typeof (Agent), typeof (Agent), typeof (AgentState), typeof (KillingBlow)})]
    [HarmonyPostfix]
    private static void OnAgentRemovedPostfix(
      Agent affectedAgent,
      Agent affectorAgent,
      AgentState agentState,
      KillingBlow killingBlow)
    {
      if (MARomanceCampaignBehavior.Instance == null || !Helper.MASettings.ImproveBattleRelation || affectedAgent == affectorAgent || !MissionPatch._agents.Swap((object) affectedAgent, (object) affectorAgent) || affectedAgent == null || affectorAgent == null || affectedAgent.Character == null || affectorAgent.Character == null)
        return;
      if (affectedAgent.Mission != null)
        MARomanceCampaignBehavior.Instance.VerifyMission(affectedAgent.Mission);
      MATeam maTeam1 = MARomanceCampaignBehavior.Instance.ResolveMATeam(((MBObjectBase) affectedAgent.Character).StringId);
      MATeam maTeam2 = MARomanceCampaignBehavior.Instance.ResolveMATeam(((MBObjectBase) affectorAgent.Character).StringId);
      Hero hero1 = (Hero) null;
      Hero hero2 = (Hero) null;
      int num1 = 0;
      int num2 = 0;
      int num3 = 0;
      if (maTeam1 != null)
      {
        hero1 = maTeam1.CurrentHero();
        if (hero1 != null)
          num1 = HeroInteractionHelper.PositiveTraits(hero1);
      }
      if (maTeam2 != null)
      {
        hero2 = maTeam2.CurrentHero();
        if (hero2 != null)
          num2 = HeroInteractionHelper.PositiveTraits(hero2);
      }
      if (hero1 != null && hero2 != null)
        num3 = HeroInteractionHelper.CompatibleBattleTraits(hero1, hero2);
      if (hero1 == null && hero2 == null || !killingBlow.IsValid)
        return;
      int coeff1 = 0;
      TextObject raison = (TextObject) null;
      bool flag1 = hero2 != null && MARomanceCampaignBehavior.Instance.IsPlayerTeam(hero2);
      bool flag2 = hero1 != null && MARomanceCampaignBehavior.Instance.IsPlayerTeam(hero1);
      if (!(flag2 | flag1))
        return;
      if (flag2)
      {
        if (maTeam2 != null && num3 >= HeroInteractionHelper.MAX_COMPATIBLE_BATTLE_TRAIT_ON_7 && hero2.GetTraitLevel(DefaultTraits.Honor) < 0)
        {
          int num4 = -hero2.GetTraitLevel(DefaultTraits.Honor);
          coeff1 += num4;
          raison = hero1 != Hero.MainHero ? new TextObject("{=BattleRelationLikeWeakOpponent}{AFFECTORHERO.NAME} happily taunts {AFFECTEDHERO.NAME} as {?AFFECTEDHERO.GENDER}she{?}he{\\?} falls to the ground !", (Dictionary<string, object>) null) : new TextObject("{=BattleRelationLikeWeakOpponentAgainstPlayer}{AFFECTORHERO.NAME} happily taunts you as you fall to the ground !", (Dictionary<string, object>) null);
        }
        else if (maTeam2 != null && hero2.GetTraitLevel(DefaultTraits.Honor) > 0)
        {
          int traitLevel = hero2.GetTraitLevel(DefaultTraits.Honor);
          coeff1 += traitLevel;
          raison = hero1 != Hero.MainHero ? new TextObject("{=BattleRelationLikeStrongOpponent}{AFFECTORHERO.NAME} respects {AFFECTEDHERO.NAME} strength in battle.", (Dictionary<string, object>) null) : new TextObject("{=BattleRelationLikeStrongOpponentAgainstPlayer}{AFFECTORHERO.NAME} respects your strength in battle.", (Dictionary<string, object>) null);
        }
      }
      if (coeff1 != 0)
      {
        if (hero2 != null)
          StringHelpers.SetCharacterProperties("AFFECTORHERO", hero2.CharacterObject, raison, false);
        if (hero1 != null)
          StringHelpers.SetCharacterProperties("AFFECTEDHERO", hero1.CharacterObject, raison, false);
        HeroInteractionHelper.ChangeHeroRelation(hero2, hero1, coeff1, raison, showWhat: HeroInteractionHelper.ShowWhat.ShowFinalRelation);
      }
      int coeff2 = 0;
      if (flag2 && hero1 != Hero.MainHero)
      {
        float num5 = -1f;
        Hero hero3 = (Hero) null;
        Vec3 position1 = affectedAgent.Position;
        foreach (Tuple<Hero, Agent> hero4 in maTeam1._heroes)
        {
          Vec3 position2 = hero4.Item2.Position;
          float num6 = position1.Distance(position2);
          if (hero4.Item1 != hero1 && ((double) num5 == -1.0 || (double) num6 < (double) num5))
          {
            hero3 = hero4.Item1;
            num5 = num6;
          }
        }
        if (hero3 != null)
        {
          int num7 = HeroInteractionHelper.CompatibleBattleTraits(hero1, hero3);
          if (num7 < HeroInteractionHelper.MAX_COMPATIBLE_BATTLE_TRAIT_ON_7)
          {
            coeff2 = -2;
            raison = hero3 != Hero.MainHero ? new TextObject("{=BattleRelationLostAFreind}{AFFECTEDHERO.NAME} resents {OTHERHERO.NAME} because {?OTHERHERO.GENDER}she{?}he{\\?}'s looking away when {?AFFECTEDHERO.GENDER}she{?}he{\\?} falls to the ground.", (Dictionary<string, object>) null) : new TextObject("{=BattleRelationLostAFreindPlayer}{AFFECTEDHERO.NAME} resents you because you are looking away when {?AFFECTEDHERO.GENDER}she{?}he{\\?} falls to the ground.", (Dictionary<string, object>) null);
          }
          else if (num7 >= HeroInteractionHelper.MAX_COMPATIBLE_BATTLE_TRAIT_ON_7)
          {
            coeff2 = 2;
            raison = hero3 != Hero.MainHero ? new TextObject("{=BattleRelationNeedAFreind}{AFFECTEDHERO.NAME} is counting on {OTHERHERO.NAME} to avenge {?AFFECTEDHERO.GENDER}her{?}him{\\?}!", (Dictionary<string, object>) null) : new TextObject("{=BattleRelationNeedAFreindPlayer}{AFFECTEDHERO.NAME} is counting on you to avenge {?AFFECTEDHERO.GENDER}her{?}him{\\?}!", (Dictionary<string, object>) null);
          }
        }
        if (coeff2 != 0)
        {
          if (hero2 != null)
            StringHelpers.SetCharacterProperties("AFFECTEDHERO", hero1.CharacterObject, raison, false);
          if (hero3 != null)
            StringHelpers.SetCharacterProperties("OTHERHERO", hero3.CharacterObject, raison, false);
          HeroInteractionHelper.ChangeHeroRelation(hero1, hero3, coeff2, raison, showWhat: HeroInteractionHelper.ShowWhat.ShowFinalRelation);
        }
      }
      int coeff3 = 0;
      if (flag1 && !flag2)
      {
        if (num1 >= 2)
        {
          coeff3 = Math.Min(1, 2);
          raison = hero2 != Hero.MainHero ? new TextObject("{=BattleRelationRespectLostAgainstNPC}{AFFECTEDHERO.NAME} respects {AFFECTORHERO.NAME} strength in battle.", (Dictionary<string, object>) null) : new TextObject("{=BattleRelationRespectLostAgainstPlayer}{AFFECTEDHERO.NAME} respects your strength in battle.", (Dictionary<string, object>) null);
        }
        else if (num1 <= -2)
        {
          coeff3 = -Math.Max(1, 2);
          raison = hero2 != Hero.MainHero ? new TextObject("{=BattleRelationFrustatedLostAgainstNPC}{AFFECTEDHERO.NAME} holds a grudge against {AFFECTORHERO.NAME} when {?AFFECTEDHERO.GENDER}she{?}he{\\?} falls to the ground.", (Dictionary<string, object>) null) : new TextObject("{=BattleRelationFrustatedLostAgainstPlayer}{AFFECTEDHERO.NAME} holds a grudge against you when {?AFFECTEDHERO.GENDER}she{?}he{\\?} falls to the ground.", (Dictionary<string, object>) null);
        }
      }
      if (coeff3 != 0)
      {
        if (hero2 != null)
          StringHelpers.SetCharacterProperties("AFFECTORHERO", hero2.CharacterObject, raison, false);
        if (hero1 != null)
          StringHelpers.SetCharacterProperties("AFFECTEDHERO", hero1.CharacterObject, raison, false);
        HeroInteractionHelper.ChangeHeroRelation(hero2, hero1, coeff3, raison, showWhat: HeroInteractionHelper.ShowWhat.ShowFinalRelation);
      }
      int coeff4 = 0;
      if (!flag1)
        return;
      float num8 = -1f;
      Hero hero5 = (Hero) null;
      Vec3 position3 = affectorAgent.Position;
      foreach (Tuple<Hero, Agent> hero6 in maTeam2._heroes)
      {
        Vec3 position4 = hero6.Item2.Position;
        float num9 = position3.Distance(position4);
        if (hero6.Item1 != Hero.MainHero && hero6.Item1 != hero2 && ((double) num8 == -1.0 || (double) num9 < (double) num8))
        {
          hero5 = hero6.Item1;
          num8 = num9;
        }
      }
      if (hero5 != null && HeroInteractionHelper.CompatibleBattleTraits(hero2, hero5) >= HeroInteractionHelper.MAX_COMPATIBLE_BATTLE_TRAIT_ON_7)
      {
        coeff4 = 1;
        raison = hero2 != Hero.MainHero ? new TextObject("{=BattleRelationBattleWithFreind}{OTHERHERO.NAME} happily watches {AFFECTORHERO.NAME} beat enemies to the ground.", (Dictionary<string, object>) null) : new TextObject("{=BattleRelationBattleWithFreindPlayer}{OTHERHERO.NAME} happily watches you beat enemies to the ground.", (Dictionary<string, object>) null);
      }
      if (coeff4 == 0)
        return;
      if (hero2 != null)
        StringHelpers.SetCharacterProperties("AFFECTORHERO", hero2.CharacterObject, raison, false);
      if (hero5 != null)
        StringHelpers.SetCharacterProperties("OTHERHERO", hero5.CharacterObject, raison, false);
      HeroInteractionHelper.ChangeHeroRelation(hero2, hero5, coeff4, raison, showWhat: HeroInteractionHelper.ShowWhat.ShowFinalRelation);
    }

    [HarmonyPatch(typeof (Mission), "SetMissionMode", new Type[] {typeof (MissionMode), typeof (bool)})]
    [HarmonyPrefix]
    private static void SetMissionModePatch(bool atStart, MissionMode newMode, Mission __instance)
    {
      if (((int)newMode) != 2 && ((int)newMode) != 4 || newMode == __instance.Mode || MARomanceCampaignBehavior.Instance == null)
        return;
      MARomanceCampaignBehavior.Instance.VerifyMission(__instance, true);
      MissionPatch._agents.Done();
    }
  }
}
