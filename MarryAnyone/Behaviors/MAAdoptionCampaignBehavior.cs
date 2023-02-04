// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Behaviors.MAAdoptionCampaignBehavior
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using HarmonyLib;
using Helpers;
using MarryAnyone.Settings;
using SandBox;
using SandBox.Conversation;
using SandBox.Missions.AgentBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;


namespace MarryAnyone.Behaviors
{
  internal class MAAdoptionCampaignBehavior : CampaignBehaviorBase
  {
    private static int _agent;
    private static List<int> _adoptableAgents;
    private static List<int> _notAdoptableAgents;

    protected void AddDialogs(CampaignGameStarter starter)
    {
      // ISSUE: method pointer
      starter.AddPlayerLine("adoption_discussion_MA", "town_or_village_children_player_no_rhyme", "adoption_child_MA", "{=adoption_offer_child}I can tell you have no parents to go back to, my child. I can be your {?PLAYER.GENDER}mother{?}father{\\?} if you wish.", new ConversationSentence.OnConditionDelegate(conversation_adopt_child_on_condition), (ConversationSentence.OnConsequenceDelegate) null, 120, (ConversationSentence.OnClickableConditionDelegate) null, (ConversationSentence.OnPersuasionOptionDelegate) null);
      // ISSUE: method pointer
      starter.AddDialogLine("character_adoption_response_MA", "adoption_child_MA", "close_window", "{=adoption_response_child}You want to be my {?PLAYER.GENDER}Ma{?}Pa{\\?}? very well then![rf:happy][rb:very_positive]", (ConversationSentence.OnConditionDelegate) null, new ConversationSentence.OnConsequenceDelegate(conversation_adopt_child_on_consequence), 100, (ConversationSentence.OnClickableConditionDelegate) null);
      // ISSUE: method pointer
      starter.AddPlayerLine("adoption_discussion_MA", "town_or_village_player", "adoption_teen_MA", "{=adoption_offer_teen}Don’t you have any parents to take care of you, young {?CONVERSATION_CHARACTER.GENDER}woman{?}man{\\?} ? I'd love for you to be a part of my family.", new ConversationSentence.OnConditionDelegate(conversation_adopt_child_on_condition), (ConversationSentence.OnConsequenceDelegate) null, 120, (ConversationSentence.OnClickableConditionDelegate) null, (ConversationSentence.OnPersuasionOptionDelegate) null);
      // ISSUE: method pointer
      starter.AddDialogLine("character_adoption_response_MA", "adoption_teen_MA", "close_window", "{=adoption_response_teen}Thank you for allowing me to be a part of your family {?PLAYER.GENDER}my Lady{?}Sir{\\?}. I humbly accept![rf:happy][rb:very_positive]", (ConversationSentence.OnConditionDelegate) null, new ConversationSentence.OnConsequenceDelegate(conversation_adopt_child_on_consequence), 100, (ConversationSentence.OnClickableConditionDelegate) null);
    }

    private bool conversation_adopt_child_on_condition()
    {
      ISettingsProvider settingsProvider = (ISettingsProvider) new MASettings();
      StringHelpers.SetCharacterProperties("CONVERSATION_CHARACTER", CharacterObject.OneToOneConversationCharacter, (TextObject) null, false);
      MAAdoptionCampaignBehavior._agent = Math.Abs(((object) Campaign.Current.ConversationManager.OneToOneConversationAgent).GetHashCode());
      if (MAAdoptionCampaignBehavior._adoptableAgents == null || MAAdoptionCampaignBehavior._notAdoptableAgents == null)
      {
        MAAdoptionCampaignBehavior._adoptableAgents = new List<int>();
        MAAdoptionCampaignBehavior._notAdoptableAgents = new List<int>();
      }
      if (MAAdoptionCampaignBehavior._notAdoptableAgents.Contains(MAAdoptionCampaignBehavior._agent))
      {
        Helper.Print("Cannot Adopt", Helper.PrintHow.PrintDisplay);
        return false;
      }
      if (MAAdoptionCampaignBehavior._adoptableAgents.Contains(MAAdoptionCampaignBehavior._agent))
      {
        Helper.Print("Can Adopt", Helper.PrintHow.PrintDisplay);
        return true;
      }
      if ((double) Campaign.Current.ConversationManager.OneToOneConversationAgent.Age < (double) Campaign.Current.Models.AgeModel.HeroComesOfAge)
      {
        Helper.Print("Adoption: " + settingsProvider.Adoption.ToString());
        if (!settingsProvider.Adoption)
          return false;
        Helper.Print("Adoption Chance: " + settingsProvider.AdoptionChance.ToString(), Helper.PrintHow.PrintDisplay);
        float randomFloat = MBRandom.RandomFloat;
        Helper.Print("Random Number: " + randomFloat.ToString(), Helper.PrintHow.PrintDisplay);
        if ((double) randomFloat < (double) settingsProvider.AdoptionChance)
        {
          MAAdoptionCampaignBehavior._adoptableAgents.Add(MAAdoptionCampaignBehavior._agent);
          return true;
        }
        MAAdoptionCampaignBehavior._notAdoptableAgents.Add(MAAdoptionCampaignBehavior._agent);
      }
      return false;
    }

    private void conversation_adopt_child_on_consequence()
    {
      if (MAAdoptionCampaignBehavior._notAdoptableAgents != null)
        MAAdoptionCampaignBehavior._notAdoptableAgents.Add(MAAdoptionCampaignBehavior._agent);
      Agent conversationAgent = (Agent) Campaign.Current.ConversationManager.OneToOneConversationAgent;
      CharacterObject character = CharacterObject.OneToOneConversationCharacter;
      Hero specialHero = HeroCreator.CreateSpecialHero(character, Settlement.CurrentSettlement, (Clan) null, (Clan) null, (int) conversationAgent.Age);
      int becomeChildAge = Campaign.Current.Models.AgeModel.BecomeChildAge;
      CharacterObject first = CharacterObject.FindFirst((Predicate<CharacterObject>) (t => t.Culture == character.Culture && (double) ((BasicCharacterObject) t).Age <= (double) becomeChildAge && ((BasicCharacterObject) t).IsFemale == ((BasicCharacterObject) character).IsFemale && t.Occupation == Occupation.Lord));
      if (first != null)
      {
        Equipment equipment1 = first.FirstCivilianEquipment.Clone(false);
        Equipment equipment2 = new Equipment(false);
        equipment2.FillFrom(equipment1, false);
        EquipmentHelper.AssignHeroEquipmentFromEquipment(specialHero, equipment1);
        EquipmentHelper.AssignHeroEquipmentFromEquipment(specialHero, equipment2);
      }
      Helper.OccupationToLord(specialHero.CharacterObject);
      specialHero.Clan = Clan.PlayerClan;
      AccessTools.Method(typeof (HeroDeveloper), "CheckInitialLevel").Invoke((object) specialHero.HeroDeveloper, (object[]) null);
      ((BasicCharacterObject) specialHero.CharacterObject).IsFemale = ((BasicCharacterObject) character).IsFemale;
      BodyProperties bodyPropertiesValue = conversationAgent.BodyPropertiesValue;
      AccessTools.Property(typeof (Hero), "StaticBodyProperties").SetValue((object) specialHero, (object) bodyPropertiesValue.StaticProperties);
      if (Hero.MainHero.IsFemale)
        specialHero.Mother = Hero.MainHero;
      else
        specialHero.Father = Hero.MainHero;
      specialHero.SetNewOccupation(Occupation.Lord);
      specialHero.HasMet = true;
      AccessTools.Field(typeof (Agent), "_name").SetValue((object) conversationAgent, (object) specialHero.Name);
      this.OnHeroAdopted(Hero.MainHero, specialHero);
      Campaign.Current.ConversationManager.ConversationEndOneShot += new Action(MAAdoptionCampaignBehavior.FollowMainAgent);
      int heroComesOfAge = Campaign.Current.Models.AgeModel.HeroComesOfAge;
      CampaignEventDispatcher root = Traverse.Create<CampaignEventDispatcher>().Property("Instance").GetValue<CampaignEventDispatcher>();
      CampaignTime campaignTime;
      if ((double) specialHero.Age <= (double) becomeChildAge)
      {
        if ((double) specialHero.Age == (double) becomeChildAge)
        {
          campaignTime = specialHero.BirthDay;
          int getDayOfYear1 = campaignTime.GetDayOfYear;
          campaignTime = CampaignTime.Now;
          int getDayOfYear2 = campaignTime.GetDayOfYear;
          if (getDayOfYear1 >= getDayOfYear2)
            goto label_11;
        }
        else
          goto label_11;
      }
      Traverse.Create((object) root).Method("OnHeroGrowsOutOfInfancy", new Type[1]
      {
        typeof (Hero)
      }, (object[]) null).GetValue((object) specialHero);
label_11:
      if ((double) specialHero.Age <= (double) heroComesOfAge)
      {
        if ((double) specialHero.Age != (double) heroComesOfAge)
          return;
        campaignTime = specialHero.BirthDay;
        int getDayOfYear3 = campaignTime.GetDayOfYear;
        campaignTime = CampaignTime.Now;
        int getDayOfYear4 = campaignTime.GetDayOfYear;
        if (getDayOfYear3 >= getDayOfYear4)
          return;
      }
      Traverse.Create((object) root).Method("OnHeroComesOfAge", new Type[1]
      {
        typeof (Hero)
      }, (object[]) null).GetValue((object) specialHero);
    }

    private static void FollowMainAgent()
    {
      DailyBehaviorGroup behaviorGroup = ConversationMission.OneToOneConversationAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.GetBehaviorGroup<DailyBehaviorGroup>();
      ((AgentBehaviorGroup) behaviorGroup).AddBehavior<FollowAgentBehavior>().SetTargetAgent(Agent.Main);
      ((AgentBehaviorGroup) behaviorGroup).SetScriptedBehavior<FollowAgentBehavior>();
    }

    private void OnHeroAdopted(Hero adopter, Hero adoptedHero)
    {
      TextObject textObject = new TextObject("{=adopted}{ADOPTER.LINK} adopted {ADOPTED_HERO.LINK}.", (Dictionary<string, object>) null);
      StringHelpers.SetCharacterProperties("ADOPTER", adopter.CharacterObject, textObject, false);
      StringHelpers.SetCharacterProperties("ADOPTED_HERO", adoptedHero.CharacterObject, textObject, false);
      MBInformationManager.AddQuickInformation(textObject, 0, (BasicCharacterObject) null, "event:/ui/notification/child_born");
    }

    public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
    {
      foreach (Hero hero in ((IEnumerable<Hero>) Hero.AllAliveHeroes).ToList<Hero>())
      {
        if (Hero.MainHero.Children.Contains(hero))
        {
          Helper.OccupationToLord(hero.CharacterObject);
          Helper.PatchHeroPlayerClan(hero, true);
        }
      }
      this.AddDialogs(campaignGameStarter);
    }

    public override void RegisterEvents() => CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object) this, new Action<CampaignGameStarter>(this.OnSessionLaunched));

    public override void SyncData(IDataStore dataStore)
    {
    }
  }
}
