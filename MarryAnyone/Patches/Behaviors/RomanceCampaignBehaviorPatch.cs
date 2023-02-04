// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Patches.Behaviors.RomanceCampaignBehaviorPatch
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using HarmonyLib;
using MarryAnyone.Behaviors;
using MarryAnyone.Helpers;
using MarryAnyone.Models;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.BarterSystem;
using TaleWorlds.CampaignSystem.BarterSystem.Barterables;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Localization;


namespace MarryAnyone.Patches.Behaviors
{
  [HarmonyPatch(typeof (RomanceCampaignBehavior))]
  internal static class RomanceCampaignBehaviorPatch
  {
    private static Hero _heroBeingProposedTo;

    [HarmonyPatch("conversation_courtship_initial_reaction_on_condition")]
    [HarmonyPostfix]
    private static void conversation_courtship_initial_reaction_on_conditionPostfix(
      ref bool __result)
    {
      if (__result && Romance.GetRomanticLevel(Hero.OneToOneConversationHero, Hero.MainHero) == Romance.RomanceLevelEnum.Ended)
        RomanceCampaignBehaviorPatch.TryToRetryCourtship();
      if (Hero.OneToOneConversationHero != null & __result)
      {
        if (Helper.HeroOccupiedAndCantMarried(Hero.OneToOneConversationHero))
          __result = false;
        if (Helper.MASettings.RelationLevelMinForRomance < 0 || Hero.OneToOneConversationHero.GetRelation(Hero.MainHero) >= Helper.MASettings.RelationLevelMinForRomance)
          return;
        __result = false;
      }
      else
      {
        if (Hero.OneToOneConversationHero == null || __result || !MADefaultMarriageModel.IsCoupleSuitableForMarriageStatic(Hero.MainHero, Hero.OneToOneConversationHero, false) || Helper.MASettings.RelationLevelMinForRomance != -1 && (Helper.MASettings.RelationLevelMinForRomance < 0 || Hero.OneToOneConversationHero.GetRelation(Hero.MainHero) <= Helper.MASettings.RelationLevelMinForRomance))
          return;
        __result = RomanceCampaignBehaviorPatch.TryToRetryCourtship();
      }
    }

    [HarmonyPatch("conversation_courtship_decline_reaction_to_player_on_condition")]
    [HarmonyPostfix]
    private static void conversation_courtship_decline_reaction_to_player_on_conditionPostfix(
      ref bool __result)
    {
      if (Hero.OneToOneConversationHero == null || __result)
        return;
      if (Helper.HeroOccupiedAndCantMarried(Hero.OneToOneConversationHero))
      {
        int relation = Hero.OneToOneConversationHero.GetRelation(Hero.MainHero);
        if (relation < 0)
          MBTextManager.SetTextVariable("COURTSHIP_DECLINE_REACTION", "{=ma_tooBusy}I am too busy {?PLAYER.GENDER}lady{?}lord{\\?},{newline}  just let me go.", false);
        else if (relation < Helper.MASettings.RelationLevelMinForRomance)
          MBTextManager.SetTextVariable("COURTSHIP_DECLINE_REACTION", "{=ma_tooBusyOther0}I am too busy {?PLAYER.GENDER}my lady{?}my lord{\\?},{newline}  we can see this another day.", false);
        else
          MBTextManager.SetTextVariable("COURTSHIP_DECLINE_REACTION", "{=ma_tooBusyOtherMinus}Can you help me about my little issue before {?PLAYER.GENDER}my lady{?}my lord{\\?},{newline}  I will be happy to talk about that after free my mind.", false);
        __result = true;
      }
      if (__result || Helper.MASettings.RelationLevelMinForRomance < 0 || Hero.OneToOneConversationHero.GetRelation(Hero.MainHero) >= Helper.MASettings.RelationLevelMinForRomance)
        return;
      MBTextManager.SetTextVariable("COURTSHIP_DECLINE_REACTION", "{=ma_notEnoughRelation}Sorry my {?PLAYER.GENDER}lady{?}lord{\\?}, we don't know enough to talk about this subject.", false);
      __result = true;
    }

    [HarmonyPatch("conversation_player_eligible_for_marriage_with_conversation_hero_on_condition")]
    [HarmonyPostfix]
    private static void Postfix1(ref bool __result) => __result = Hero.OneToOneConversationHero != null && !Helper.HeroOccupiedAndCantMarried(Hero.OneToOneConversationHero) && Romance.GetCourtedHeroInOtherClan(Hero.MainHero, Hero.OneToOneConversationHero) == null && MARomanceModel.CourtshipPossibleBetweenNPCsStatic(Hero.MainHero, Hero.OneToOneConversationHero);

    [HarmonyPostfix]
    [HarmonyPatch("RomanceCourtshipAttemptCooldown", MethodType.Getter)]
    private static void RomanceCourtshipAttemptCooldownPostfix2(ref CampaignTime __result)
    {
      if (!Helper.MASettings.RetryCourtship)
        return;
      __result = CampaignTime.DaysFromNow(1f);
    }

    [HarmonyPrefix]
    [HarmonyPatch("conversation_player_can_open_courtship_on_condition")]
    private static bool conversation_player_can_open_courtship_on_conditionPrefix1(ref bool __result)
    {
      __result = RomanceCampaignBehaviorPatch.conversation_player_can_open_courtship_on_condition(false);
      return false;
    }

    public static bool conversation_player_can_open_courtship_on_condition(bool forBeginDiscussion)
    {
      if (Hero.OneToOneConversationHero == null)
        return false;
      bool flag1 = !Hero.OneToOneConversationHero.IsFemale;
      bool flag2 = MARomanceCampaignBehavior.Instance.SpouseOrNot(Hero.MainHero, Hero.OneToOneConversationHero);
      Romance.RomanceLevelEnum romanticLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
      if (MADefaultMarriageModel.IsCoupleSuitableForMarriageStatic(Hero.MainHero, Hero.OneToOneConversationHero, forBeginDiscussion))
      {
        if (romanticLevel == Romance.RomanceLevelEnum.Untested)
        {
          if (Hero.OneToOneConversationHero.IsLord || Hero.OneToOneConversationHero.IsMinorFactionHero)
          {
            if (Hero.OneToOneConversationHero.Spouse == null)
              MBTextManager.SetTextVariable("FLIRTATION_LINE", flag1 ? "{=lord_flirt}My lord, I see that you have not taken a spouse yet." : "{=v1hC6Aem}My lady, I wish to profess myself your most ardent admirer.", false);
            else
              MBTextManager.SetTextVariable("FLIRTATION_LINE", flag1 ? "{=lord_cheating_flirt}My lord, I see that you have not taken a spouse yet." : "{=v1hC6Aem}My lady, I wish to profess myself your most ardent admirer.", false);
          }
          else
            MBTextManager.SetTextVariable("FLIRTATION_LINE", flag1 ? "{=goodman_flirt}My good man, I see that you have not taken a spouse yet." : "{=goodwife_flirt}My dear lady, I wish to profess myself your most ardent admirer.", false);
          return true;
        }
        if (Helper.MASettings.RetryCourtship && (romanticLevel == Romance.RomanceLevelEnum.FailedInCompatibility || romanticLevel == Romance.RomanceLevelEnum.FailedInPracticalities || romanticLevel == Romance.RomanceLevelEnum.Ended && !flag2))
        {
          if (Hero.OneToOneConversationHero.IsLord || Hero.OneToOneConversationHero.IsMinorFactionHero)
            MBTextManager.SetTextVariable("FLIRTATION_LINE", flag1 ? "{=2WnhUBMM}My lord, may you give me another chance to prove myself?" : "{=4iTaEZKg}My lady, may you give me another chance to prove myself?", false);
          else
            MBTextManager.SetTextVariable("FLIRTATION_LINE", flag1 ? "{=goodman_chance}My good man, would you give me another chance to prove myself ?" : "{=goodwife_chance}My dear lady, would you give me another chance to prove myself ?", false);
          return true;
        }
      }
      return false;
    }

    public static bool TryToRetryCourtship()
    {
      bool retryCourtship = false;
      Romance.RomanceLevelEnum romanticLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
      Romance.RomanceLevelEnum romanceLevelEnum = (Romance.RomanceLevelEnum) 4;
      int num = 0;
      if (romanticLevel == Romance.RomanceLevelEnum.FailedInCompatibility || romanticLevel == Romance.RomanceLevelEnum.FailedInPracticalities)
      {
        if (romanticLevel == Romance.RomanceLevelEnum.Ended)
          Util.CleanRomance(Hero.MainHero, Hero.OneToOneConversationHero);
        num = 2;
        romanceLevelEnum = (Romance.RomanceLevelEnum) 4;
        retryCourtship = true;
      }
      else if (romanticLevel == Romance.RomanceLevelEnum.FailedInPracticalities)
      {
        num = 3;
        romanceLevelEnum = (Romance.RomanceLevelEnum) 5;
        retryCourtship = true;
      }
      if (num != 0)
        ChangeRelationAction.ApplyPlayerRelation(Hero.OneToOneConversationHero, -num, false, true);
      ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, romanceLevelEnum);
      return retryCourtship;
    }

    [HarmonyPatch("conversation_player_opens_courtship_on_consequence")]
    [HarmonyPrefix]
    private static bool conversation_player_opens_courtship_on_consequencePrefix() => false;

    [HarmonyPatch("conversation_courtship_reaction_to_player_on_condition")]
    [HarmonyPostfix]
    private static void conversation_courtship_reaction_to_player_on_conditionPostfix(
      ref bool __result)
    {
      if (!__result)
        return;
      RomanceCampaignBehaviorPatch.TryToRetryCourtship();
      if (Helper.MASettings.Difficulty == "Easy")
        ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, (Romance.RomanceLevelEnum) 5);
      if (!(Helper.MASettings.Difficulty == "Very Easy"))
        return;
      ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, (Romance.RomanceLevelEnum) 6);
    }

    [HarmonyPatch("conversation_romance_at_stage_1_discussions_on_condition")]
    [HarmonyPrefix]
    private static bool conversation_romance_at_stage_1_discussions_on_conditionPrefix(
      ref bool __result)
    {
      if (Hero.OneToOneConversationHero == null)
      {
        __result = false;
        return false;
      }
      Romance.RomanceLevelEnum romanticLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
      if (!(Helper.MASettings.Difficulty == "Very Easy") && (!(Helper.MASettings.Difficulty == "Easy") || Hero.OneToOneConversationHero.IsLord || Hero.OneToOneConversationHero.IsMinorFactionHero))
        return true;
      if (romanticLevel == Romance.RomanceLevelEnum.CourtshipStarted)
        ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, (Romance.RomanceLevelEnum) 5);
      __result = false;
      return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch("conversation_romance_at_stage_2_discussions_on_condition")]
    private static bool conversation_romance_at_stage_2_discussions_on_conditionPatch(
      ref bool __result)
    {
      if (Hero.OneToOneConversationHero == null)
      {
        __result = false;
        return false;
      }
      Romance.RomanceLevelEnum romanticLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
      if (!(Helper.MASettings.Difficulty == "Very Easy"))
        return true;
      if (romanticLevel == Romance.RomanceLevelEnum.CoupleDecidedThatTheyAreCompatible)
        ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, (Romance.RomanceLevelEnum) 6);
      __result = false;
      return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch("conversation_finalize_courtship_for_hero_on_condition")]
    private static bool conversation_finalize_courtship_for_hero_on_conditionPatch(ref bool __result)
    {
      __result = RomanceCampaignBehaviorPatch.conversation_finalize_courtship_for_hero_on_condition(false);
      return false;
    }

    public static bool conversation_finalize_courtship_for_hero_on_condition(bool MAPath)
    {
      bool flag = true;
      if (Hero.OneToOneConversationHero == null)
        return false;
      if (flag)
      {
        Romance.RomanceLevelEnum romanticLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
        Romance.RomanticState romanticState = Romance.GetRomanticState(Hero.MainHero, Hero.OneToOneConversationHero);
        if (romanticState != null && (double) romanticState.ScoreFromPersuasion == 0.0)
          romanticState.ScoreFromPersuasion = 60f;
        flag = MADefaultMarriageModel.IsCoupleSuitableForMarriageStatic(Hero.MainHero, Hero.OneToOneConversationHero, false) && (Hero.OneToOneConversationHero.Clan == null || Hero.OneToOneConversationHero.Clan.Leader == Hero.OneToOneConversationHero) && romanticLevel == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;
      }
      if (flag && !MAPath && !Helper.MASettings.DifficultyNormalMode && Hero.OneToOneConversationHero.Clan != null && Hero.OneToOneConversationHero.Clan.Leader == Hero.OneToOneConversationHero && (Helper.MASettings.CanJoinUpperClanThroughMAPath || HeroInteractionHelper.CanIntegreSpouseInHeroClan(Hero.MainHero, Hero.OneToOneConversationHero)))
        flag = false;
      if (flag && !MAPath && (Hero.OneToOneConversationHero.Clan == null || Hero.OneToOneConversationHero.Clan == Hero.MainHero.Clan))
        flag = false;
      return flag;
    }

    [HarmonyPatch("conversation_finalize_marriage_barter_consequence")]
    [HarmonyPrefix]
        private static bool conversation_finalize_marriage_barter_consequencePatch(RomanceCampaignBehavior __instance)
        {
            RomanceCampaignBehaviorPatch._heroBeingProposedTo = Hero.OneToOneConversationHero;
            if (Hero.OneToOneConversationHero.Clan != null)
            {
                foreach (Hero hero in Hero.OneToOneConversationHero.Clan.Lords)
                {
                    if (Romance.GetRomanticLevel(Hero.MainHero, hero) == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage)
                    {
                        RomanceCampaignBehaviorPatch._heroBeingProposedTo = hero;
                        break;
                    }
                }
            }
            BarterManager instance = BarterManager.Instance;
            Hero mainHero = Hero.MainHero;
            Hero oneToOneConversationHero = Hero.OneToOneConversationHero;
            int persuasionCostReduction = 0;
            if (Romance.GetRomanticState(Hero.MainHero, RomanceCampaignBehaviorPatch._heroBeingProposedTo) != null)
            {
                persuasionCostReduction = (int)Romance.GetRomanticState(Hero.MainHero, RomanceCampaignBehaviorPatch._heroBeingProposedTo).ScoreFromPersuasion;
            }
            PartyBase mainParty = PartyBase.MainParty;
            MobileParty partyBelongedTo = Hero.OneToOneConversationHero.PartyBelongedTo;
            if (RomanceCampaignBehaviorPatch._heroBeingProposedTo.Clan != null && RomanceCampaignBehaviorPatch._heroBeingProposedTo.Spouse != mainHero)
            {
                MarriageBarterable marriageBarterable = new MarriageBarterable(Hero.MainHero, PartyBase.MainParty, RomanceCampaignBehaviorPatch._heroBeingProposedTo, Hero.MainHero);
                instance.StartBarterOffer(mainHero, oneToOneConversationHero, mainParty, (partyBelongedTo != null) ? partyBelongedTo.Party : null, null, (Barterable barterable, BarterData _args, object obj) => BarterManager.Instance.InitializeMarriageBarterContext(barterable, _args, new Tuple<Hero, Hero>(RomanceCampaignBehaviorPatch._heroBeingProposedTo, Hero.MainHero)), persuasionCostReduction, false, new Barterable[]
                {
                    marriageBarterable
                });
            }
            if (PlayerEncounter.Current != null)
            {
                PlayerEncounter.LeaveEncounter = true;
            }
            return false;
        }

        [HarmonyPatch("conversation_marriage_barter_successful_on_consequence")]
    [HarmonyPostfix]
    internal static void conversation_marriage_barter_successful_on_consequencePATCH()
    {
      if (RomanceCampaignBehaviorPatch._heroBeingProposedTo == null || MARomanceCampaignBehavior.Instance == null)
        return;
      MARomanceCampaignBehavior.Instance.PartnerRemove(RomanceCampaignBehaviorPatch._heroBeingProposedTo);
    }

    [HarmonyPatch("MarriageCourtshipPossibility")]
    [HarmonyPrefix]
    internal static bool MarriageCourtshipPossibilityPreFix(
      Hero person1,
      Hero person2,
      ref bool __result)
    {
      __result = MARomanceModel.CourtshipPossibleBetweenNPCsStatic(person1, person2);
      return false;
    }
  }
}
