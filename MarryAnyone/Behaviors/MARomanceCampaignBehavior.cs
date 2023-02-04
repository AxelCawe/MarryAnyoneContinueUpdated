using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Helpers;
using MarryAnyone.Helpers;
using MarryAnyone.MA;
using MarryAnyone.Models;
using MarryAnyone.Patches.Behaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Conversation.Persuasion;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.LogEntries;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace MarryAnyone.Behaviors
{
    internal class MARomanceCampaignBehavior : CampaignBehaviorBase
    {
        // Token: 0x06000214 RID: 532 RVA: 0x0000A8E7 File Offset: 0x00008AE7
        public void PartnerRemove(Hero hero)
        {
            if (this.Partners != null)
            {
                while (this.Partners.Remove(hero))
                {
                }
                if (this.Partners.Count == 0)
                {
                    this.Partners = null;
                }
            }
        }

        // Token: 0x06000215 RID: 533 RVA: 0x0000A913 File Offset: 0x00008B13
        public MARomanceCampaignBehavior()
        {
            MARomanceCampaignBehavior.Instance = this;
        }

        // Token: 0x06000216 RID: 534 RVA: 0x0000A950 File Offset: 0x00008B50
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
            CampaignEvents.HeroesMarried.AddNonSerializedListener(this, new Action<Hero, Hero, bool>(this.OnHeroesMarried));
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, new Action(this.OnHourTickEvent));
            CampaignEvents.OnMissionStartedEvent.AddNonSerializedListener(this, new Action<IMission>(this.OnMissionStarted));
            CampaignEvents.OnPlayerBattleEndEvent.AddNonSerializedListener(this, new Action<MapEvent>(this.OnPlayerBattleEnd));
            CampaignEvents.AfterMissionStarted.AddNonSerializedListener(this, new Action<IMission>(this.OnAfterMissionStarted));
        }

        // Token: 0x06000217 RID: 535 RVA: 0x0000A9E7 File Offset: 0x00008BE7
        public void Dispose()
        {
            this.Partners = null;
            this.NoMoreSpouse = null;
            this._previousCheatPersuasionAttempts = null;
            this._allReservations = null;
            this._buggedSpouses = null;
            PregnancyCampaignBehaviorPatch.Done();
            this._mission = null;
            this._maTeams = null;
            MARomanceCampaignBehavior.Instance = null;
        }

        private Hero ResolveOriginalSpouseForPartner(Hero partner)
        {
            List<Hero> list = partner.ExSpouses.ToList<Hero>();
            if (partner.Spouse != null)
            {
                list.Add(partner.Spouse);
            }
            list.RemoveAll((Hero x) => x == Hero.MainHero || Hero.MainHero.ExSpouses.Contains(x));
            Hero hero = null;
            if (list.Count > 0)
            {
                hero = list.FirstOrDefault((Hero x) => x.IsAlive && x.Clan == partner.Clan && x.Spouse == partner);
                if (hero == null)
                {
                    hero = list.FirstOrDefault((Hero x) => x.IsAlive && x.Clan == partner.Clan && x.Spouse == null);
                }
                if (hero == null)
                {
                    hero = list.FirstOrDefault((Hero x) => x.IsAlive && x.Spouse == partner);
                }
                if (hero == null)
                {
                    hero = list.FirstOrDefault((Hero x) => x.IsAlive && x.Spouse == null);
                }
            }
            return hero;
        }

        // Token: 0x06000219 RID: 537 RVA: 0x0000AB0B File Offset: 0x00008D0B
        public bool PartnerOfPlayer(Hero partner)
        {
            return this.Partners != null && this.Partners.IndexOf(partner) >= 0;
        }

        // Token: 0x0600021A RID: 538 RVA: 0x0000AB27 File Offset: 0x00008D27
        public bool SpouseOfPlayer(Hero spouse)
        {
            return (Hero.MainHero.Spouse == spouse || Hero.MainHero.ExSpouses.IndexOf(spouse) >= 0) && (this.NoMoreSpouse == null || this.NoMoreSpouse.IndexOf(spouse) < 0);
        }

        // Token: 0x0600021B RID: 539 RVA: 0x0000AB64 File Offset: 0x00008D64
        public bool SpouseOrNot(Hero spouseA, Hero spouseB)
        {
            if (spouseA == Hero.MainHero)
            {
                return (this.NoMoreSpouse == null || this.NoMoreSpouse.IndexOf(spouseB) < 0) && (this.Partners == null || this.Partners.IndexOf(spouseB) < 0) && (Hero.MainHero.Spouse == spouseB || Hero.MainHero.ExSpouses.IndexOf(spouseB) >= 0);
            }
            if (spouseB == Hero.MainHero)
            {
                return (this.NoMoreSpouse == null || this.NoMoreSpouse.IndexOf(spouseA) < 0) && (this.Partners == null || this.Partners.IndexOf(spouseA) < 0) && (Hero.MainHero.Spouse == spouseA || Hero.MainHero.ExSpouses.IndexOf(spouseA) >= 0);
            }
            return spouseA.Spouse == spouseB || spouseB.Spouse == spouseA;
        }

        public Hero FirstHeroExSpouseOkToDoIt()
        {
            Hero result = null;
            if (Hero.MainHero.ExSpouses != null)
            {
                result = Hero.MainHero.ExSpouses.LastOrDefault((Hero h) => h.IsAlive && this.NoMoreSpouse.IndexOf(h) < 0 && HeroInteractionHelper.OkToDoIt(Hero.MainHero, h, true));
            }
            return result;
        }

        public Hero FirstHeroExSpouse()
        {
            Hero result = null;
            if (Hero.MainHero.ExSpouses != null)
            {
                result = Hero.MainHero.ExSpouses.LastOrDefault((Hero h) => h.IsAlive && this.NoMoreSpouse.IndexOf(h) < 0);
            }
            return result;
        }

        // Token: 0x17000094 RID: 148
        // (get) Token: 0x0600021E RID: 542 RVA: 0x0000ACB8 File Offset: 0x00008EB8
        public List<Hero> Spouses
        {
            get
            {
                List<Hero> list = Hero.MainHero.ExSpouses.ToList<Hero>();
                if (Hero.MainHero.Spouse != null)
                {
                    if (list.Count > 0)
                    {
                        list.Insert(0, Hero.MainHero.Spouse);
                    }
                    else
                    {
                        list.Add(Hero.MainHero.Spouse);
                    }
                }
                if (this.Partners != null)
                {
                    list.RemoveAll((Hero x) => this.Partners.IndexOf(x) >= 0);
                }
                if (this.NoMoreSpouse != null)
                {
                    list.RemoveAll((Hero x) => this.NoMoreSpouse.IndexOf(x) >= 0);
                }
                return list;
            }
        }

        // Token: 0x0600021F RID: 543 RVA: 0x0000AD44 File Offset: 0x00008F44
        public void RemoveMainHeroSpouse(Hero oldSpouse, Hero withHero)
        {
            if (Romance.GetRomanticLevel(oldSpouse, withHero) == Romance.RomanceLevelEnum.Marriage)
            {
                Util.CleanRomance(oldSpouse, withHero, Romance.RomanceLevelEnum.Untested);
            }
            Helper.RemoveExSpouses(withHero, Helper.RemoveExSpousesHow.RemoveOtherHero, null, oldSpouse);
        }

        // Token: 0x06000220 RID: 544 RVA: 0x0000AD64 File Offset: 0x00008F64
        public void RemoveMainHeroSpouse(Hero oldSpouse, bool removeHeroFromParty = true)
        {
            Hero hero = this.ResolveOriginalSpouseForPartner(oldSpouse);
            Clan clan = (hero != null) ? hero.Clan : null;
            if (clan == null && oldSpouse.Mother != null && oldSpouse.Mother.Clan != null && !oldSpouse.Mother.Clan.IsEliminated)
            {
                clan = oldSpouse.Mother.Clan;
            }
            if (clan == null && oldSpouse.Father != null && oldSpouse.Father.Clan != null && !oldSpouse.Father.Clan.IsEliminated)
            {
                clan = oldSpouse.Father.Clan;
            }
            this.RemoveMainHeroSpouse(oldSpouse, Hero.MainHero);
            foreach (Hero hero2 in Hero.MainHero.ExSpouses)
            {
                if (hero2 != oldSpouse)
                {
                    this.RemoveMainHeroSpouse(oldSpouse, hero2);
                }
            }
            Helper.RemoveExSpouses(oldSpouse, (Helper.RemoveExSpousesHow)97, null, hero);
            if (clan != null && clan != oldSpouse.Clan)
            {
                Helper.SwapClan(oldSpouse, oldSpouse.Clan, clan);
            }
            else
            {
                Helper.RemoveFromClan(oldSpouse, Clan.PlayerClan, false);
                Helper.OccupationToCompanion(oldSpouse.CharacterObject);
            }
            if (removeHeroFromParty)
            {
                PartyHelper.SwapPartyBelongedTo(oldSpouse, null);
                if (MobileParty.MainParty.Party.MemberRoster.FindIndexOfTroop(oldSpouse.CharacterObject) >= 0)
                {
                    MobileParty.MainParty.Party.MemberRoster.RemoveTroop(oldSpouse.CharacterObject, 1, default(UniqueTroopDescriptor), 0);
                }
            }
        }

        // Token: 0x06000221 RID: 545 RVA: 0x0000AED8 File Offset: 0x000090D8
        private void MAWeddingDo(Hero hero, Hero spouse)
        {
            PropertyInfo propertyInfo = AccessTools.Property(typeof(Hero), "PartyBelongedTo");
            if (propertyInfo == null)
            {
                throw new Exception("property PartyBelongedTo not resolved on Hero class");
            }
            bool flag = false;
            if (spouse.PartyBelongedTo == hero.PartyBelongedTo)
            {
                propertyInfo.SetValue(spouse, null, null);
                flag = true;
            }
            ChangeRomanticStateAction.Apply(hero, spouse, Romance.RomanceLevelEnum.Marriage);
            if (flag)
            {
                propertyInfo.SetValue(spouse, MobileParty.MainParty, null);
            }
            if (!spouse.HasMet)
            {
                spouse.HasMet = true;
            }
            if (!spouse.IsActive)
            {
                spouse.ChangeState(Hero.CharacterStates.Active);
            }
            if (spouse.IsPlayerCompanion)
            {
                spouse.CompanionOf = null;
            }
        }

        // Token: 0x06000222 RID: 546 RVA: 0x0000AF70 File Offset: 0x00009170
        public void MAWedding(Hero hero, Hero spouse)
        {
            Hero spouse2 = hero.Spouse;
            Hero spouse3 = spouse.Spouse;
            Clan clan = null;
            Clan heroLeaveClan = null;
            bool flag = hero == Hero.MainHero;
            try
            {
                this._MAWedding = true;
                if (flag)
                {
                    this.PartnerRemove(spouse);
                }
                if (spouse.IsFactionLeader && !spouse.IsMinorFactionHero)
                {
                    if (hero.Clan.Kingdom != spouse.Clan.Kingdom)
                    {
                        Kingdom kingdom = hero.Clan.Kingdom;
                        if (((kingdom != null) ? kingdom.Leader : null) != hero)
                        {
                            if (!Helper.MASettings.CanJoinUpperClanThroughMAPath)
                            {
                                throw new Exception("conversation_courtship_success_on_consequence TU spouse IS MAIN FAIL");
                            }
                            bool flag2 = false;
                            heroLeaveClan = hero.Clan;
                            MobileParty mobilePartyDest = null;
                            if (spouse.CurrentSettlement == hero.CurrentSettlement || (hero.PartyBelongedTo == MobileParty.MainParty && spouse.PartyBelongedTo != null && spouse.PartyBelongedTo == MobileParty.ConversationParty))
                            {
                                mobilePartyDest = spouse.PartyBelongedTo;
                            }
                            if (hero.Clan.Leader == hero)
                            {
                                flag2 = true;
                            }
                            Action<Hero> action = delegate (Hero h)
                            {
                                bool flag3 = false;
                                if (h.PartyBelongedTo == hero.PartyBelongedTo)
                                {
                                    flag3 = true;
                                }
                                RemoveCompanionAction.ApplyByFire(heroLeaveClan, h);
                                AddCompanionAction.Apply(spouse.Clan, h);
                                if (flag3)
                                {
                                    if (mobilePartyDest != null)
                                    {
                                        AddHeroToPartyAction.Apply(h, mobilePartyDest, true);
                                        return;
                                    }
                                    if (MobileParty.MainParty.MemberRoster.FindIndexOfTroop(h.CharacterObject) < 0)
                                    {
                                        AddHeroToPartyAction.Apply(h, hero.PartyBelongedTo, false);
                                    }
                                }
                            };
                            foreach (Hero obj in hero.Clan.Companions.ToList<Hero>())
                            {
                                action(obj);
                            }
                            if (flag && Helper.MASettings.Polygamy)
                            {
                                foreach (Hero hero2 in hero.ExSpouses)
                                {
                                    if (this.SpouseOfPlayer(hero2))
                                    {
                                        action(hero2);
                                    }
                                }
                            }
                            Helper.SwapClan(hero, heroLeaveClan, spouse.Clan);
                            if (mobilePartyDest != null)
                            {
                                MobileParty mainParty = MobileParty.MainParty;
                                if (mainParty != null)
                                {
                                    TroopRoster prisonRoster = mainParty.PrisonRoster;
                                    if (prisonRoster != null && prisonRoster.Count > 0)
                                    {
                                        mobilePartyDest.Party.AddPrisoners(mainParty.PrisonRoster);
                                        mainParty.PrisonRoster.Clear();
                                    }
                                    ItemRoster itemRoster = mainParty.ItemRoster;
                                    if (itemRoster != null && itemRoster.Count > 0)
                                    {
                                        mobilePartyDest.ItemRoster.Add(mainParty.ItemRoster);
                                        mainParty.ItemRoster.Clear();
                                    }
                                }
                                AddHeroToPartyAction.Apply(hero, mobilePartyDest, true);
                                PartyHelper.SwapMainParty(mobilePartyDest);
                                PartyHelper.SwapPartyBelongedTo(hero, mobilePartyDest);
                                mobilePartyDest.ChangePartyLeader(Hero.MainHero);
                                mobilePartyDest.Party.SetCustomOwner(Hero.MainHero);
                                PartyHelper.SetLeaderAtTop(mobilePartyDest.Party);
                                if (mainParty != null)
                                {
                                    MergePartiesAction.Apply(mobilePartyDest.Party, mainParty.Party);
                                }
                            }
                            Helper.FamilyAdoptChild(spouse, hero, heroLeaveClan);
                            Helper.FamilyJoinClan(hero, heroLeaveClan, spouse.Clan);
                            if (flag2 && this.HeroLeaveClanLeaderAndDestroyClan(heroLeaveClan, spouse.Clan))
                            {
                                heroLeaveClan = null;
                            }
                            Campaign value = Traverse.Create<Campaign>().Property("Current", null).GetValue<Campaign>();
                            Traverse.Create(value).Property("PlayerDefaultFaction", null).SetValue(spouse.Clan);
                            ChangeClanLeaderAction.ApplyWithSelectedNewLeader(spouse.Clan, Hero.MainHero);
                        }
                        else
                        {
                            clan = spouse.Clan;
                            ChangeClanLeaderAction.ApplyWithoutSelectedNewLeader(spouse.Clan);
                        }
                    }
                }
                else if (spouse.IsFactionLeader && spouse.IsMinorFactionHero)
                {
                    clan = spouse.Clan;
                    ChangeClanLeaderAction.ApplyWithoutSelectedNewLeader(spouse.Clan);
                }
                if (clan != null)
                {
                    Helper.FamilyAdoptChild(spouse, hero, clan);
                }
                if (spouse.Clan == null)
                {
                    if (clan != null)
                    {
                        Helper.FamilyJoinClan(spouse, clan, hero.Clan);
                    }
                    else
                    {
                        Helper.SwapClan(spouse, spouse.Clan, hero.Clan);
                    }
                }
                Helper.OccupationToLord(spouse.CharacterObject);
                
                Clan clan2 = hero.Clan;
                if (((clan2 != null) ? clan2.Lords.FirstOrDefault((Hero x) => x == spouse) : null) == null)
                {
                    hero.Clan.Lords.AddItem(spouse);
                    Helper.Print("Add Spouse to Noble", Helper.PrintHow.PrintDisplay);
                }
                this.MAWeddingDo(hero, spouse);
                if (spouse2 != null)
                {
                    Helper.RemoveExSpouses(spouse2, Helper.RemoveExSpousesHow.RAS, null, null);
                }
                if (spouse3 != null)
                {
                    Helper.RemoveExSpouses(spouse3, Helper.RemoveExSpousesHow.CompletelyRemove, null, null);
                }
                Helper.RemoveExSpouses(spouse, Helper.RemoveExSpousesHow.CompletelyRemove, null, null);
                Helper.RemoveExSpouses(hero, Helper.RemoveExSpousesHow.RAS, null, null);
                Helper.RemoveExSpouses(spouse, Helper.RemoveExSpousesHow.RAS, null, null);
                if (PlayerEncounter.Current != null)
                {
                    PlayerEncounter.LeaveEncounter = true;
                }
                if ((spouse.CurrentSettlement != null && spouse.CurrentSettlement == hero.CurrentSettlement) || (hero.PartyBelongedTo == MobileParty.MainParty && spouse.PartyBelongedTo != null && spouse.PartyBelongedTo == MobileParty.ConversationParty))
                {
                    AddHeroToPartyAction.Apply(spouse, MobileParty.MainParty, true);
                }
            }
            finally
            {
                this._MAWedding = false;
            }
        }

        // Token: 0x06000223 RID: 547 RVA: 0x0000B678 File Offset: 0x00009878
        public bool IsPlayerTeam(Hero hero)
        {
            if (hero.PartyBelongedTo == Hero.MainHero.PartyBelongedTo)
            {
                return true;
            }
            if (hero.Clan != null && hero.Clan == Hero.MainHero.Clan)
            {
                return true;
            }
            Clan clan = hero.Clan;
            Kingdom kingdom = (clan != null) ? clan.Kingdom : null;
            Clan clan2 = Hero.MainHero.Clan;
            return kingdom == ((clan2 != null) ? clan2.Kingdom : null);
        }

        // Token: 0x06000224 RID: 548 RVA: 0x0000B6E4 File Offset: 0x000098E4
        protected void AddDialogs(CampaignGameStarter starter)
        {
            starter.AddPlayerLine("main_option_discussions_MA", "hero_main_options", "lord_talk_speak_diplomacy_MA", "{=lord_conversations_343}There is something I'd like to discuss.", new ConversationSentence.OnConditionDelegate(this.conversation_begin_courtship_for_hero_on_condition), null, 120, null, null);
            starter.AddDialogLine("character_agrees_to_discussion_MA", "lord_talk_speak_diplomacy_MA", "lord_talk_speak_diplomacy_2", "{=OD1m1NYx}{STR_INTRIGUE_AGREEMENT}", new ConversationSentence.OnConditionDelegate(this.conversation_character_agrees_to_discussion_on_condition), null, 100, null);
            starter.AddPlayerLine("player_cheat_persuasion_start", "lord_talk_speak_diplomacy_2", "acceptcheatingornot", "{=Cheat_engage_courtship}I would love to spend some time with you. Will you join my party for a few days ?", new ConversationSentence.OnConditionDelegate(this.conversation_characacter_agreed_to_cheat), null, 100, null, null);
            starter.AddPlayerLine("player_Divorce_start", "lord_talk_speak_diplomacy_2", "goodbySpouse", "{=Divorce_engage_dialog}We have been together for too long my {RELATION_TEXT}{newline}, I think it is time to explore new horizons. I want a divorce {INTERLOCUTOR.NAME}.", new ConversationSentence.OnConditionDelegate(this.conversation_can_divorce), delegate
            {
                this.conversation_do_divorce(false);
            }, 80, null, null);
            starter.AddPlayerLine("player_DivorceBug_start", "lord_talk_speak_diplomacy_2", "close_window", "{=DivorceBug_engage_dialog}There is a bug in the Marry Anyone, just leave my team {INTERLOCUTOR.NAME}!", new ConversationSentence.OnConditionDelegate(this.conversation_can_divorce), delegate
            {
                this.conversation_do_divorce(true);
            }, 60, null, null);
            starter.AddPlayerLine("player_LeaveCheat_start", "lord_talk_speak_diplomacy_2", "close_window", "{=LeaveCheat_engage_dialog}I think you need to go away {INTERLOCUTOR.NAME}, I have other things to do.", new ConversationSentence.OnConditionDelegate(this.conversation_can_LeaveCheat), new ConversationSentence.OnConsequenceDelegate(this.conversaion_do_LeaveCheat), 40, null, null);
            starter.AddPlayerLine("player_RemarryBug_start", "lord_talk_speak_diplomacy_2", "player_RemarryBug_try", "{=marryAgain_engage_dialog}We are not well married {INTERLOCUTOR.NAME} (regarding the log events).{newline} Do you want we married again my {RELATION_TEXT} ?", new ConversationSentence.OnConditionDelegate(this.conversation_can_marryAgain), null, 40, null, null);
            starter.AddDialogLine("hero_remarry_player", "player_RemarryBug_try", "close_window", "{=marryAgain_OK}I'm happy to hear that, we can formalize our bugged wedding !", new ConversationSentence.OnConditionDelegate(this.Conversation_can_marryAgainOK), new ConversationSentence.OnConsequenceDelegate(this.conversation_do_marryAgain), 100, null);
            starter.AddDialogLine("hero_remarry_player", "player_RemarryBug_try", "close_window", "{=marryAgain_Cancel}I'm borried with this wobbly situation, i give up !", null, new ConversationSentence.OnConsequenceDelegate(this.conversation_cancel_marryAgain), 80, null);
            starter.AddDialogLine("hero_leave_party", "goodbySpouse", "close_window", "{=LeaveSpouseParty}Hope we meet again, maybe in the arena{newline}, so long ...", null, null, 100, null);
            starter.AddDialogLine("hero_cheat_persuasion_no", "acceptcheatingornot", "lort_pretalk", "{=!}{CHEAT_DECLINE_REACTION}", new ConversationSentence.OnConditionDelegate(this.conversation_characacter_notagreed_to_cheat_VariantTest), null, 120, null);
            starter.AddDialogLine("hero_cheat_persuasion_start_nomore", "acceptcheatingornot", "lort_pretalk", "{=allready_reply}I have already given you my answer, off you go.", () => !this.conversation_characacter_notagreed_to_cheat_VariantTest() && this.conversation_cheat_allready_done(), null, 100, null);
            starter.AddDialogLine("hero_cheat_persuasion_start", "acceptcheatingornot", "heroPersuasionNextQuestion", "{=bW3ygxro}Yes, it's good to have a chance to get to know each other.", () => !this.conversation_characacter_notagreed_to_cheat_VariantTest() && !this.conversation_cheat_allready_done(), new ConversationSentence.OnConsequenceDelegate(this.conversation_characacter_test_to_cheat), 80, null);
            starter.AddDialogLine("hero_cheat_persuasion_fail", "heroPersuasionNextQuestion", "lort_pretalk", "{=!}{FAILED_PERSUASION_LINE}", new ConversationSentence.OnConditionDelegate(this.Persuasion_fail), new ConversationSentence.OnConsequenceDelegate(this.conversation_characacter_fail_to_cheat_go), 100, null);
            starter.AddDialogLine("hero_cheat_persuasion_attempt", "heroPersuasionNextQuestion", "player_courtship_argument", "{=!}{PERSUASION_TASK_LINE}", new ConversationSentence.OnConditionDelegate(this.persuasion_go_nextStep), null, 100, null);
            starter.AddDialogLine("hero_cheat_persuasion_success", "heroPersuasionNextQuestion", "close_window", "{=Cheat_success}Yes, let's have some fun ! I will join your party.", null, new ConversationSentence.OnConsequenceDelegate(this.conversation_characacter_success_to_cheat_go), 100, null);
            starter.AddDialogLine("hero_courtship_persuasion_attempt", "heroPersuasionQuestion", "player_courtship_argument", "{=!}{PERSUASION_TASK_LINE}", new ConversationSentence.OnConditionDelegate(this.persuasion_conversation_dialog_line), null, 100, null);
            starter.AddPlayerLine("player_courtship_argument_0", "player_courtship_argument", "hero_courtship_reaction_forcheat", "{=!}{ROMANCE_PERSUADE_ATTEMPT_0}", () => this.persuasion_conversation_player_line(0), delegate
            {
                this.persuasion_conversation_player_line_clique(0);
            }, 100, delegate (out TextObject explanation)
            {
                return this.persuasion_conversation_player_clickable(0, out explanation);
            }, () => this.persuasion_conversation_player_get_optionArgs(0));
            starter.AddPlayerLine("player_courtship_argument_1", "player_courtship_argument", "hero_courtship_reaction_forcheat", "{=!}{ROMANCE_PERSUADE_ATTEMPT_1}", () => this.persuasion_conversation_player_line(1), delegate
            {
                this.persuasion_conversation_player_line_clique(1);
            }, 100, delegate (out TextObject explanation)
            {
                return this.persuasion_conversation_player_clickable(1, out explanation);
            }, () => this.persuasion_conversation_player_get_optionArgs(1));
            starter.AddPlayerLine("player_courtship_argument_2", "player_courtship_argument", "hero_courtship_reaction_forcheat", "{=!}{ROMANCE_PERSUADE_ATTEMPT_2}", () => this.persuasion_conversation_player_line(2), delegate
            {
                this.persuasion_conversation_player_line_clique(2);
            }, 100, delegate (out TextObject explanation)
            {
                return this.persuasion_conversation_player_clickable(2, out explanation);
            }, () => this.persuasion_conversation_player_get_optionArgs(2));
            starter.AddPlayerLine("player_courtship_argument_3", "player_courtship_argument", "hero_courtship_reaction_forcheat", "{=!}{ROMANCE_PERSUADE_ATTEMPT_3}", () => this.persuasion_conversation_player_line(3), delegate
            {
                this.persuasion_conversation_player_line_clique(3);
            }, 100, delegate (out TextObject explanation)
            {
                return this.persuasion_conversation_player_clickable(3, out explanation);
            }, () => this.persuasion_conversation_player_get_optionArgs(3));
            starter.AddPlayerLine("lord_ask_recruit_argument_no_answer", "player_courtship_argument", "lord_pretalk", "{=!}{TRY_HARDER_LINE}", new ConversationSentence.OnConditionDelegate(this.persuasion_conversation_player_line_tryLater), new ConversationSentence.OnConsequenceDelegate(this.persuation_abandon_courtship), 100, null, null);
            starter.AddDialogLine("lord_ask_recruit_argument_reaction", "hero_courtship_reaction_forcheat", "heroPersuasionNextQuestion", "{=!}{PERSUASION_REACTION}", new ConversationSentence.OnConditionDelegate(this.persuasion_go_next), new ConversationSentence.OnConsequenceDelegate(this.Persuasion_go_next_clique), 100, null);
            starter.AddDialogLine("hero_courtship_persuasion_2_success", "lord_start_courtship_response_3", "lord_conclude_courtship_stage_2", "{=xwS10c1b}Yes... I think I would be honored to accept your proposal.", new ConversationSentence.OnConditionDelegate(this.MAconversation_finalize_courtship_for_hero_on_condition), null, 120, null);
            starter.AddDialogLine("hero_courtship_goto_finalize", "lord_start_courtship_response_3", "lord_conclude_courtship_stage_2", "{=xwS10c1b}Yes... I think I would be honored to accept your proposal.", new ConversationSentence.OnConditionDelegate(this.MAconversation_goto_barter), null, 140, null);
            starter.AddPlayerLine("hero_romance_conclusion_direct", "hero_main_options", "close_window", "{=2aW6NC3Q}Let us discuss the final terms of our marriage.", new ConversationSentence.OnConditionDelegate(this.MAconversation_finalize_courtship_for_hero_on_condition), new ConversationSentence.OnConsequenceDelegate(this.conversation_courtship_success_on_consequence), 90, null, null);
            starter.AddPlayerLine("hero_romance_task_pt3b", "hero_main_options", "hero_courtship_final_barter", "{=jd4qUGEA}I wish to discuss the final terms of my marriage with {COURTSHIP_PARTNER}.", new ConversationSentence.OnConditionDelegate(this.conversation_finalize_courtship_for_other_on_condition), null, 100, null, null);
            starter.AddDialogLine("persuasion_leave_faction_npc_result_success_2", "lord_conclude_courtship_stage_2", "close_window", "{=k7nGxksk}Splendid! Let us conduct the ceremony, then.", new ConversationSentence.OnConditionDelegate(this.MAconversation_finalize_courtship_for_hero_on_condition), new ConversationSentence.OnConsequenceDelegate(this.conversation_courtship_success_on_consequence), 140, null);
            starter.AddPlayerLine("hero_want_to_marry", "hero_main_options", "lord_pretalk", "{=endcourthip}Sorry {INTERLOCUTOR.NAME}, I don't want to marry you anymore.", new ConversationSentence.OnConditionDelegate(this.conversation_player_end_courtship), new ConversationSentence.OnConsequenceDelegate(this.conversation_player_end_courtship_do), 100, null, null);
        }

        // Token: 0x06000225 RID: 549 RVA: 0x0000BCD6 File Offset: 0x00009ED6
        private bool Conversation_can_marryAgainOK()
        {
            return this.conversation_can_marryAgain() && (Helper.MASettings.RelationLevelMinForRomance < 0 || Hero.OneToOneConversationHero.GetRelation(Hero.MainHero) >= Helper.MASettings.RelationLevelMinForRomance);
        }

        // Token: 0x06000226 RID: 550 RVA: 0x0000BD0B File Offset: 0x00009F0B
        private bool conversation_can_marryAgain()
        {
            if (this._buggedSpouses != null && this._buggedSpouses.Contains(Hero.OneToOneConversationHero))
            {
                StringHelpers.SetCharacterProperties("INTERLOCUTOR", Hero.OneToOneConversationHero.CharacterObject, null, false);
                return true;
            }
            return false;
        }

        // Token: 0x06000227 RID: 551 RVA: 0x0000BD41 File Offset: 0x00009F41
        private void conversation_cancel_marryAgain()
        {
            this.RemoveMainHeroSpouse(Hero.OneToOneConversationHero, true);
        }

        // Token: 0x06000228 RID: 552 RVA: 0x0000BD4F File Offset: 0x00009F4F
        private void conversation_do_marryAgain()
        {
            this.MAWeddingDo(Hero.MainHero, Hero.OneToOneConversationHero);
        }

        // Token: 0x06000229 RID: 553 RVA: 0x0000BD61 File Offset: 0x00009F61
        private bool conversation_courtship_initial_reaction_on_condition()
        {
            MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION", "{=KdhnBhZ1}Yes, we are considering offers. These things are not rushed into.", false);
            return true;
        }

        // Token: 0x0600022A RID: 554 RVA: 0x0000BD74 File Offset: 0x00009F74
        private void conversaion_do_LeaveCheat()
        {
            float relationWithPlayer = Hero.OneToOneConversationHero.GetRelationWithPlayer();
            int num = (int)(relationWithPlayer / 40f);
            if (relationWithPlayer > 0f && num <= 0)
            {
                num = 1;
            }
            ChangeRelationAction.ApplyPlayerRelation(Hero.OneToOneConversationHero, -num, false, true);
            if (MobileParty.MainParty.Party.MemberRoster.FindIndexOfTroop(Hero.OneToOneConversationHero.CharacterObject) >= 0)
            {
                MobileParty.MainParty.Party.MemberRoster.RemoveTroop(Hero.OneToOneConversationHero.CharacterObject, 1, default(UniqueTroopDescriptor), 0);
            }
            this.PartnerRemove(Hero.OneToOneConversationHero);
            this.RemoveMainHeroSpouse(Hero.OneToOneConversationHero, true);
        }

        // Token: 0x0600022B RID: 555 RVA: 0x0000BE12 File Offset: 0x0000A012
        private bool conversation_can_LeaveCheat()
        {
            if (this.PartnerOfPlayer(Hero.OneToOneConversationHero))
            {
                StringHelpers.SetCharacterProperties("INTERLOCUTOR", Hero.OneToOneConversationHero.CharacterObject, null, false);
                return true;
            }
            return false;
        }

        // Token: 0x0600022C RID: 556 RVA: 0x0000BE3C File Offset: 0x0000A03C
        private void conversation_do_divorce(bool forBug)
        {
            float relationWithPlayer = Hero.OneToOneConversationHero.GetRelationWithPlayer();
            int num = (int)(relationWithPlayer / 40f);
            if (relationWithPlayer > 0f && num <= 0)
            {
                num = 1;
            }
            Clan clan = null;
            if (Hero.OneToOneConversationHero.Father != null && Hero.OneToOneConversationHero.Father.Clan != null && !Hero.OneToOneConversationHero.Father.Clan.IsEliminated)
            {
                clan = Hero.OneToOneConversationHero.Father.Clan;
            }
            if (clan == null && Hero.OneToOneConversationHero.Mother != null && Hero.OneToOneConversationHero.Mother.Clan != null && !Hero.OneToOneConversationHero.Mother.Clan.IsEliminated)
            {
                clan = Hero.OneToOneConversationHero.Mother.Clan;
            }
            if (forBug)
            {
                Helper.RemoveExSpouses(Hero.MainHero, Helper.RemoveExSpousesHow.RAS, null, Hero.OneToOneConversationHero);
                Helper.RemoveExSpouses(Hero.OneToOneConversationHero, Helper.RemoveExSpousesHow.RAS, null, Hero.MainHero);
            }
            else
            {
                Hero.OneToOneConversationHero.Spouse = null;
                Helper.RemoveExSpouses(Hero.MainHero, Helper.RemoveExSpousesHow.RAS, null, null);
                this.NoMoreSpouse.Add(Hero.OneToOneConversationHero);
            }
            if (clan != null)
            {
                Helper.SwapClan(Hero.OneToOneConversationHero, Clan.PlayerClan, clan);
            }
            else
            {
                Helper.RemoveFromClan(Hero.OneToOneConversationHero, Clan.PlayerClan, false);
                Helper.OccupationToCompanion(Hero.OneToOneConversationHero.CharacterObject);
            }
            PartyHelper.SwapPartyBelongedTo(Hero.OneToOneConversationHero, null);
            if (MobileParty.MainParty.Party.MemberRoster.FindIndexOfTroop(Hero.OneToOneConversationHero.CharacterObject) >= 0)
            {
                MobileParty.MainParty.Party.MemberRoster.RemoveTroop(Hero.OneToOneConversationHero.CharacterObject, 1, default(UniqueTroopDescriptor), 0);
            }
            ChangeRelationAction.ApplyPlayerRelation(Hero.OneToOneConversationHero, (int)(-(int)(relationWithPlayer / 5f)), false, true);
            if (num > 0)
            {
                foreach (Hero hero in this.Spouses)
                {
                    float num2 = (float)hero.GetRelation(Hero.OneToOneConversationHero);
                    float relationWithPlayer2 = hero.GetRelationWithPlayer();
                    int num3 = (int)(num2 / 20f);
                    if (num3 < 1)
                    {
                        num3 = 1;
                    }
                    int num4 = (int)(relationWithPlayer2 / 30f);
                    if (num4 < 1)
                    {
                        num4 = 1;
                    }
                    ChangeRelationAction.ApplyPlayerRelation(hero, -(num * num3 * num4), false, true);
                }
            }
        }

        // Token: 0x0600022D RID: 557 RVA: 0x0000C07C File Offset: 0x0000A27C
        private bool conversation_can_divorce()
        {
            if (this.SpouseOfPlayer(Hero.OneToOneConversationHero))
            {
                GameTexts.SetVariable("RELATION_TEXT", ConversationHelper.GetHeroRelationToHeroTextShort(Hero.MainHero, Hero.OneToOneConversationHero, false));
                StringHelpers.SetCharacterProperties("INTERLOCUTOR", Hero.OneToOneConversationHero.CharacterObject, null, false);
                return true;
            }
            return false;
        }

        // Token: 0x0600022E RID: 558 RVA: 0x0000C0C9 File Offset: 0x0000A2C9
        private void conversation_player_end_courtship_do()
        {
            if (Hero.OneToOneConversationHero != null)
            {
                Util.CleanRomance(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.Untested);
                ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.Untested);
                ChangeRelationAction.ApplyPlayerRelation(Hero.OneToOneConversationHero, -10, true, true);
            }
        }

        // Token: 0x0600022F RID: 559 RVA: 0x0000C100 File Offset: 0x0000A300
        private bool conversation_player_end_courtship()
        {
            if (Hero.OneToOneConversationHero != null && Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage)
            {
                StringHelpers.SetCharacterProperties("INTERLOCUTOR", Hero.OneToOneConversationHero.CharacterObject, null, false);
                return true;
            }
            return false;
        }

        // Token: 0x06000230 RID: 560 RVA: 0x0000C138 File Offset: 0x0000A338
        private bool conversation_begin_courtship_for_hero_on_condition()
        {
            if (Hero.OneToOneConversationHero != null)
            {
                bool flag = RomanceCampaignBehaviorPatch.conversation_player_can_open_courtship_on_condition(true);
                if (flag)
                {
                    flag = Helper.MarryEnabledPathMA(Hero.OneToOneConversationHero, Hero.MainHero, false);
                }
                if (Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.Untested)
                {
                    bool flag2 = MARomanceCampaignBehavior.Instance.SpouseOrNot(Hero.MainHero, Hero.OneToOneConversationHero);
                    if (flag2)
                    {
                        Util.CleanRomance(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.Marriage);
                        Helper.Print("MARomanceCampaignBehavior::conversation_begin_courtship_for_hero_on_condition::PATCH Married New Romantic Level : " + Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero).ToString(), Helper.PrintHow.PrintToLogAndWrite);
                    }
                    else
                    {
                        Util.CleanRomance(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.Untested);
                    }
                }
                return flag;
            }
            return false;
        }

        // Token: 0x06000231 RID: 561 RVA: 0x0000C1E8 File Offset: 0x0000A3E8
        private bool conversation_characacter_notagreed_to_cheat_VariantTest()
        {
            bool result = false;
            if (Hero.OneToOneConversationHero.Occupation == Occupation.Lord)
            {
                if (Helper.FactionAtWar(Hero.MainHero, Hero.OneToOneConversationHero))
                {
                    MBTextManager.SetTextVariable("CHEAT_DECLINE_REACTION", "{=maCheatNotPossibleKingdomAtWar}I am terribly sorry. Our kingdom are actualy at war.", false);
                    result = true;
                }
                else if (Hero.OneToOneConversationHero.PartyBelongedTo != null && Hero.OneToOneConversationHero.PartyBelongedTo.LeaderHero == Hero.OneToOneConversationHero && (Hero.MainHero.CurrentSettlement == null || (!Hero.MainHero.CurrentSettlement.IsTown && !Hero.MainHero.CurrentSettlement.IsCastle)))
                {
                    MBTextManager.SetTextVariable("CHEAT_DECLINE_REACTION", "{=maCheatNotPossibleCantCancelParty}You are a funny {?PLAYER.GENDER}woman my lady{?}man my lord{\\?}. I Can't cancel my party in country like that.", false);
                    result = true;
                }
            }
            return result;
        }

        // Token: 0x06000232 RID: 562 RVA: 0x0000C290 File Offset: 0x0000A490
        private bool conversation_characacter_agreed_to_cheat()
        {
            bool result = false;
            if (Hero.OneToOneConversationHero != null && !this.PartnerOfPlayer(Hero.OneToOneConversationHero))
            {
                result = RomanceCampaignBehaviorPatch.conversation_player_can_open_courtship_on_condition(true);
            }
            return result;
        }

        // Token: 0x06000233 RID: 563 RVA: 0x0000C2BC File Offset: 0x0000A4BC
        private Tuple<TraitObject, int>[] GetTraitCorrelations(int valor = 0, int mercy = 0, int honor = 0, int generosity = 0, int calculating = 0)
        {
            return new Tuple<TraitObject, int>[]
            {
                new Tuple<TraitObject, int>(DefaultTraits.Valor, valor),
                new Tuple<TraitObject, int>(DefaultTraits.Mercy, mercy),
                new Tuple<TraitObject, int>(DefaultTraits.Honor, honor),
                new Tuple<TraitObject, int>(DefaultTraits.Generosity, generosity),
                new Tuple<TraitObject, int>(DefaultTraits.Calculating, calculating)
            };
        }

        // Token: 0x06000234 RID: 564 RVA: 0x0000C318 File Offset: 0x0000A518
        private List<PersuasionTask> GetPersuasionTasksForCheat()
        {
            StringHelpers.SetCharacterProperties("PLAYER", Hero.MainHero.CharacterObject, null, false);
            StringHelpers.SetCharacterProperties("INTERLOCUTOR", Hero.OneToOneConversationHero.CharacterObject, null, false);
            List<PersuasionTask> list = new List<PersuasionTask>();
            PersuasionTask persuasionTask = new PersuasionTask(0);
            list.Add(persuasionTask);
            persuasionTask.FinalFailLine = new TextObject("{=cheatTestFail}I am not interested.", null);
            persuasionTask.TryLaterLine = new TextObject("{=cheatTestRetry}Well, I think you may want to try some other time.", null);
            persuasionTask.SpokenLine = new TextObject("{=cheatFirstToken}What do you have in mind ?", null);
            Tuple<TraitObject, int>[] traitCorrelations = this.GetTraitCorrelations(1, -1, 0, 1, -1);
            PersuasionArgumentStrength argumentStrengthBasedOnTargetTraits = Campaign.Current.Models.PersuasionModel.GetArgumentStrengthBasedOnTargetTraits(CharacterObject.OneToOneConversationCharacter, traitCorrelations);
            PersuasionOptionArgs option = new PersuasionOptionArgs(DefaultSkills.Leadership, DefaultTraits.Valor, TraitEffect.Positive, argumentStrengthBasedOnTargetTraits, false, new TextObject("{=CheatLeaderChoice}Don't worry, just follow me {?INTERLOCUTOR.GENDER}Miss{?}Mister{\\?}.", null), traitCorrelations, false, true, false);
            persuasionTask.AddOptionToTask(option);
            Tuple<TraitObject, int>[] traitCorrelations2 = this.GetTraitCorrelations(1, 0, 0, -1, 1);
            PersuasionArgumentStrength argumentStrengthBasedOnTargetTraits2 = Campaign.Current.Models.PersuasionModel.GetArgumentStrengthBasedOnTargetTraits(CharacterObject.OneToOneConversationCharacter, traitCorrelations2);
            PersuasionOptionArgs option2 = new PersuasionOptionArgs(DefaultSkills.Roguery, DefaultTraits.Valor, TraitEffect.Positive, argumentStrengthBasedOnTargetTraits2, false, new TextObject("{=CheatCalculateChoice}If we are both thinking the same thing, then let's go", null), traitCorrelations2, false, true, false);
            persuasionTask.AddOptionToTask(option2);
            Tuple<TraitObject, int>[] traitCorrelations3 = this.GetTraitCorrelations(0, 1, 1, 0, -1);
            PersuasionArgumentStrength argumentStrengthBasedOnTargetTraits3 = Campaign.Current.Models.PersuasionModel.GetArgumentStrengthBasedOnTargetTraits(CharacterObject.OneToOneConversationCharacter, traitCorrelations3);
            PersuasionOptionArgs option3 = new PersuasionOptionArgs(DefaultSkills.Charm, DefaultTraits.Mercy, TraitEffect.Positive, argumentStrengthBasedOnTargetTraits3, false, new TextObject("{=CheatMercyChoice}I cannot let {?INTERLOCUTOR.GENDER}a beautiful woman{?}a handsome young man{\\?} get hurt in this dangerous world, I'll protect you.", null), traitCorrelations3, false, true, false);
            persuasionTask.AddOptionToTask(option3);
            Tuple<TraitObject, int>[] traitCorrelations4 = this.GetTraitCorrelations(-1, 0, -1, -1, 0);
            PersuasionArgumentStrength argumentStrengthBasedOnTargetTraits4 = Campaign.Current.Models.PersuasionModel.GetArgumentStrengthBasedOnTargetTraits(CharacterObject.OneToOneConversationCharacter, traitCorrelations4);
            PersuasionOptionArgs option4 = new PersuasionOptionArgs(DefaultSkills.Charm, DefaultTraits.Generosity, TraitEffect.Negative, argumentStrengthBasedOnTargetTraits4, false, new TextObject("{=CheatGenerosityCharm}It is a beautiful day outside, and {?INTERLOCUTOR.GENDER}an adventurous woman{?}an adventurous man{\\?} like you, needs to enjoy the fresh air.", null), traitCorrelations4, false, true, false);
            persuasionTask.AddOptionToTask(option4);
            return list;
        }

        // Token: 0x06000235 RID: 565 RVA: 0x0000C4F4 File Offset: 0x0000A6F4
        private void conversation_characacter_test_to_cheat()
        {
            this._allReservations = this.GetPersuasionTasksForCheat();
            this._maximumScoreCap = (float)this._allReservations.Count<PersuasionTask>() * 1f;
            float initialProgress = 0f;
            ConversationManager.StartPersuasion(this._maximumScoreCap, this._successValue, this._failValue, this._criticalSuccessValue, this._criticalFailValue, initialProgress, Helper.MASettings.DifficultyNormalMode ? PersuasionDifficulty.Hard : PersuasionDifficulty.Medium);
        }

        // Token: 0x06000236 RID: 566 RVA: 0x0000C55F File Offset: 0x0000A75F
        private void conversation_characacter_fail_to_cheat_go()
        {
            if (PlayerEncounter.Current != null)
            {
                PlayerEncounter.LeaveEncounter = true;
            }
            this._allReservations = null;
            ConversationManager.EndPersuasion();
        }

        // Token: 0x06000237 RID: 567 RVA: 0x0000C57C File Offset: 0x0000A77C
        private void conversation_characacter_success_to_cheat_go()
        {
            float num = ConversationManager.GetPersuasionProgress() - ConversationManager.GetPersuasionGoalValue();
            if (this.Partners == null)
            {
                this.Partners = new List<Hero>();
                this.Partners.Add(Hero.OneToOneConversationHero);
            }
            if (Hero.OneToOneConversationHero.PartyBelongedTo != MobileParty.MainParty)
            {
                AddHeroToPartyAction.Apply(Hero.OneToOneConversationHero, MobileParty.MainParty, true);
            }
            this._allReservations = null;
            ConversationManager.EndPersuasion();
        }

        // Token: 0x06000238 RID: 568 RVA: 0x0000C5E8 File Offset: 0x0000A7E8
        private void persuasionAttemptCheatClean(Hero forHero)
        {
            if (this._previousCheatPersuasionAttempts != null)
            {
            
                for (int i = 0; i < _previousCheatPersuasionAttempts.Count; ++i)
                {
                    IEnumerable<PersuasionAttempt> previousCheatPersuasionAttempts = this._previousCheatPersuasionAttempts;
                    Func<PersuasionAttempt, bool> predicate;
                    predicate = (PersuasionAttempt x) => x.PersuadedHero == forHero;
                    PersuasionAttempt item;
                    if ((item = previousCheatPersuasionAttempts.FirstOrDefault(predicate)) == null)
                    {
                        break;
                    }
                    this._previousCheatPersuasionAttempts.Remove(item);
                }
            }
        }

        // Token: 0x06000239 RID: 569 RVA: 0x0000C64C File Offset: 0x0000A84C
        private bool conversation_cheat_allready_done()
        {
            Hero forHero = Hero.OneToOneConversationHero;
            if (this._previousCheatPersuasionAttempts != null)
            {
                PersuasionAttempt persuasionAttempt = this._previousCheatPersuasionAttempts.FirstOrDefault((PersuasionAttempt x) => x.PersuadedHero == forHero && ((x.Result != PersuasionOptionResult.CriticalFailure && x.GameTime.ElapsedDaysUntilNow < 1f) || (x.Result == PersuasionOptionResult.CriticalFailure && x.GameTime.ElapsedWeeksUntilNow < 2f)));
                bool flag = persuasionAttempt != null;
                if (!flag)
                {
                    this.persuasionAttemptCheatClean(forHero);
                }
                return flag;
            }
            return false;
        }

        // Token: 0x0600023A RID: 570 RVA: 0x0000C6A1 File Offset: 0x0000A8A1
        private bool conversation_cheat_easy_mode()
        {
            return Helper.MASettings.DifficultyVeryEasyMode;
        }

        // Token: 0x0600023B RID: 571 RVA: 0x0000C6B0 File Offset: 0x0000A8B0
        private PersuasionTask GetCurrentPersuasionTask()
        {
            foreach (PersuasionTask persuasionTask in this._allReservations)
            {
                if (!persuasionTask.Options.All((PersuasionOptionArgs x) => x.IsBlocked))
                {
                    return persuasionTask;
                }
            }
            return this._allReservations.Last<PersuasionTask>();
        }


        private PersuasionTask FindTaskOfOption(PersuasionOptionArgs optionChosenWithLine)
        {
            foreach (PersuasionTask persuasionTask in this._allReservations)
            {
                using (List<PersuasionOptionArgs>.Enumerator enumerator2 = persuasionTask.Options.GetEnumerator())
                {
                    while (enumerator2.MoveNext())
                    {
                        if (enumerator2.Current.Line == optionChosenWithLine.Line)
                        {
                            return persuasionTask;
                        }
                    }
                }
            }
            return null;
        }

        // Token: 0x0600023D RID: 573 RVA: 0x0000C7D8 File Offset: 0x0000A9D8
        private bool persuasion_conversation_dialog_line()
        {
            PersuasionTask currentPersuasionTask = this.GetCurrentPersuasionTask();
            if (currentPersuasionTask == this._allReservations.Last<PersuasionTask>())
            {
                if (currentPersuasionTask.Options.All((PersuasionOptionArgs x) => x.IsBlocked))
                {
                    return false;
                }
            }
            if (!ConversationManager.GetPersuasionProgressSatisfied())
            {
                MBTextManager.SetTextVariable("PERSUASION_TASK_LINE", currentPersuasionTask.SpokenLine, false);
                return true;
            }
            return false;
        }

        // Token: 0x0600023E RID: 574 RVA: 0x0000C844 File Offset: 0x0000AA44
        private bool persuasion_conversation_player_line(int noOption)
        {
            PersuasionTask currentPersuasionTask = this.GetCurrentPersuasionTask();
            if (currentPersuasionTask.Options.Count<PersuasionOptionArgs>() > noOption)
            {
                TextObject textObject = new TextObject("{=bSo9hKwr}{PERSUASION_OPTION_LINE} {SUCCESS_CHANCE}", null);
                textObject.SetTextVariable("SUCCESS_CHANCE", PersuasionHelper.ShowSuccess(currentPersuasionTask.Options.ElementAt(noOption), true));
                textObject.SetTextVariable("PERSUASION_OPTION_LINE", currentPersuasionTask.Options.ElementAt(noOption).Line);
                MBTextManager.SetTextVariable("ROMANCE_PERSUADE_ATTEMPT_" + noOption.ToString(), textObject, false);
                return true;
            }
            return false;
        }

        // Token: 0x0600023F RID: 575 RVA: 0x0000C8C8 File Offset: 0x0000AAC8
        private bool persuasion_conversation_player_clickable(int noOption, out TextObject hintText)
        {
            hintText = TextObject.Empty;
            PersuasionTask currentPersuasionTask = this.GetCurrentPersuasionTask();
            if (currentPersuasionTask.Options.Any<PersuasionOptionArgs>())
            {
                return !currentPersuasionTask.Options.ElementAt(noOption).IsBlocked;
            }
            hintText = new TextObject("{=9ACJsI6S}Blocked", null);
            return false;
        }

        // Token: 0x06000240 RID: 576 RVA: 0x0000C913 File Offset: 0x0000AB13
        private PersuasionOptionArgs persuasion_conversation_player_get_optionArgs(int noOption)
        {
            return this.GetCurrentPersuasionTask().Options.ElementAt(noOption);
        }

        // Token: 0x06000241 RID: 577 RVA: 0x0000C928 File Offset: 0x0000AB28
        private void persuasion_conversation_player_line_clique(int noOption)
        {
            PersuasionTask currentPersuasionTask = this.GetCurrentPersuasionTask();
            if (currentPersuasionTask.Options.Count > noOption)
            {
                currentPersuasionTask.Options[noOption].BlockTheOption(true);
            }
        }

        // Token: 0x06000242 RID: 578 RVA: 0x0000C95C File Offset: 0x0000AB5C
        private bool persuasion_conversation_player_line_tryLater()
        {
            PersuasionTask currentPersuasionTask = this.GetCurrentPersuasionTask();
            MBTextManager.SetTextVariable("TRY_HARDER_LINE", currentPersuasionTask.TryLaterLine, false);
            return true;
        }

        // Token: 0x06000243 RID: 579 RVA: 0x0000C982 File Offset: 0x0000AB82
        private void persuation_abandon_courtship()
        {
            if (PlayerEncounter.Current != null)
            {
                PlayerEncounter.LeaveEncounter = true;
            }
            this._allReservations = null;
            ConversationManager.EndPersuasion();
        }

        // Token: 0x06000244 RID: 580 RVA: 0x0000C9A0 File Offset: 0x0000ABA0
        private bool persuasion_go_nextStep()
        {
            PersuasionTask currentPersuasionTask = this.GetCurrentPersuasionTask();
            if (currentPersuasionTask.Options.All((PersuasionOptionArgs x) => x.IsBlocked) && !ConversationManager.GetPersuasionProgressSatisfied())
            {
                MBTextManager.SetTextVariable("FAILED_PERSUASION_LINE", currentPersuasionTask.FinalFailLine, false);
                return false;
            }
            if (this.conversation_cheat_easy_mode())
            {
                return false;
            }
            if (!ConversationManager.GetPersuasionProgressSatisfied())
            {
                MBTextManager.SetTextVariable("PERSUASION_TASK_LINE", currentPersuasionTask.SpokenLine, false);
                return true;
            }
            return false;
        }

        // Token: 0x06000245 RID: 581 RVA: 0x0000CA20 File Offset: 0x0000AC20
        private bool persuasion_go_next()
        {
            PersuasionOptionResult item = ConversationManager.GetPersuasionChosenOptions().Last<Tuple<PersuasionOptionArgs, PersuasionOptionResult>>().Item2;
            if ((item == PersuasionOptionResult.Failure || item == PersuasionOptionResult.CriticalFailure) && this.GetCurrentPersuasionTask().ImmediateFailLine != null)
            {
                MBTextManager.SetTextVariable("PERSUASION_REACTION", this.GetCurrentPersuasionTask().ImmediateFailLine, false);
                if (item != PersuasionOptionResult.CriticalFailure)
                {
                    return true;
                }
                using (List<PersuasionTask>.Enumerator enumerator = this._allReservations.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        PersuasionTask persuasionTask = enumerator.Current;
                        persuasionTask.BlockAllOptions();
                    }
                    return true;
                }
            }
            MBTextManager.SetTextVariable("PERSUASION_REACTION", PersuasionHelper.GetDefaultPersuasionOptionReaction(item), false);
            return true;
        }

        // Token: 0x06000246 RID: 582 RVA: 0x0000CAC8 File Offset: 0x0000ACC8
        private void Persuasion_go_next_clique()
        {
            PersuasionTask currentPersuasionTask = this.GetCurrentPersuasionTask();
            Tuple<PersuasionOptionArgs, PersuasionOptionResult> tuple = ConversationManager.GetPersuasionChosenOptions().Last<Tuple<PersuasionOptionArgs, PersuasionOptionResult>>();
            float difficulty = Campaign.Current.Models.PersuasionModel.GetDifficulty(PersuasionDifficulty.Medium);
            float moveToNextStageChance;
            float blockRandomOptionChance;
            Campaign.Current.Models.PersuasionModel.GetEffectChances(tuple.Item1, out moveToNextStageChance, out blockRandomOptionChance, difficulty);
            this.FindTaskOfOption(tuple.Item1).ApplyEffects(moveToNextStageChance, blockRandomOptionChance);
            PersuasionAttempt item = new PersuasionAttempt(Hero.OneToOneConversationHero, CampaignTime.Now, tuple.Item1, tuple.Item2, currentPersuasionTask.ReservationType);
            if (this._previousCheatPersuasionAttempts == null)
            {
                this._previousCheatPersuasionAttempts = new List<PersuasionAttempt>();
            }
            this._previousCheatPersuasionAttempts.Add(item);
        }

        // Token: 0x06000247 RID: 583 RVA: 0x0000CB74 File Offset: 0x0000AD74
        private bool Persuasion_fail()
        {
            PersuasionTask currentPersuasionTask = this.GetCurrentPersuasionTask();
            if (currentPersuasionTask.Options.All((PersuasionOptionArgs x) => x.IsBlocked) && !ConversationManager.GetPersuasionProgressSatisfied())
            {
                MBTextManager.SetTextVariable("FAILED_PERSUASION_LINE", currentPersuasionTask.FinalFailLine, false);
                return true;
            }
            return false;
        }

        // Token: 0x06000248 RID: 584 RVA: 0x0000CBCF File Offset: 0x0000ADCF
        private bool conversation_character_agrees_to_discussion_on_condition()
        {
            MBTextManager.SetTextVariable("STR_INTRIGUE_AGREEMENT", Campaign.Current.ConversationManager.FindMatchingTextOrNull("str_lord_intrigue_accept", CharacterObject.OneToOneConversationCharacter), false);
            return true;
        }

        // Token: 0x06000249 RID: 585 RVA: 0x0000CBF8 File Offset: 0x0000ADF8
        private bool MAconversation_goto_barter()
        {
            Romance.RomanceLevelEnum romanticLevel = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
            Romance.RomanticState romanticState = Romance.GetRomanticState(Hero.MainHero, Hero.OneToOneConversationHero);
            return romanticState.Level == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage && Hero.OneToOneConversationHero.Clan != Hero.MainHero.Clan && Hero.OneToOneConversationHero.Clan.Leader != null && !Hero.OneToOneConversationHero.Clan.IsBanditFaction && !Hero.OneToOneConversationHero.Clan.IsOutlaw && Hero.OneToOneConversationHero.Clan.Leader != Hero.OneToOneConversationHero;
        }

        // Token: 0x0600024A RID: 586 RVA: 0x0000CC90 File Offset: 0x0000AE90
        private bool MAconversation_finalize_courtship_for_hero_on_condition()
        {
            Romance.RomanceLevelEnum romanceLevelEnum = Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero);
            Romance.RomanticState romanticState = Romance.GetRomanticState(Hero.MainHero, Hero.OneToOneConversationHero);
            if (!Helper.MASettings.CanJoinUpperClanThroughMAPath && !HeroInteractionHelper.CanIntegreSpouseInHeroClan(Hero.MainHero, Hero.OneToOneConversationHero))
            {
                return false;
            }
            bool flag = MARomanceModel.CourtshipPossibleBetweenNPCsStatic(Hero.MainHero, Hero.OneToOneConversationHero) && (Hero.OneToOneConversationHero.Clan == null || Hero.OneToOneConversationHero.Clan.IsBanditFaction || Hero.OneToOneConversationHero.Clan.IsOutlaw || Hero.OneToOneConversationHero.Clan.Leader == Hero.OneToOneConversationHero || Hero.OneToOneConversationHero.Clan == Hero.MainHero.Clan);
            if (flag)
            {
                if ((romanceLevelEnum == Romance.RomanceLevelEnum.CourtshipStarted || romanceLevelEnum == Romance.RomanceLevelEnum.CoupleDecidedThatTheyAreCompatible) && Helper.MASettings.Difficulty == "Very Easy")
                {
                    ChangeRomanticStateAction.Apply(Hero.MainHero, Hero.OneToOneConversationHero, Romance.RomanceLevelEnum.CoupleAgreedOnMarriage);
                    romanceLevelEnum = Romance.RomanceLevelEnum.CoupleAgreedOnMarriage;
                }
                flag = (romanceLevelEnum == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage);
            }
            if (flag && romanticState != null && romanticState.ScoreFromPersuasion == 0f)
            {
                romanticState.ScoreFromPersuasion = 60f;
            }
            return flag;
        }

        // Token: 0x0600024B RID: 587 RVA: 0x0000CDA8 File Offset: 0x0000AFA8
        private bool conversation_finalize_courtship_for_other_on_condition()
        {
            if (Hero.OneToOneConversationHero != null)
            {
                Clan clan = Hero.OneToOneConversationHero.Clan;
                if (clan != null && clan.Leader == Hero.OneToOneConversationHero)
                {
                    foreach (Hero hero in clan.Lords)
                    {
                        if (hero != Hero.OneToOneConversationHero && MARomanceModel.CourtshipPossibleBetweenNPCsStatic(Hero.MainHero, hero) && Romance.GetRomanticLevel(Hero.MainHero, hero) == Romance.RomanceLevelEnum.CoupleAgreedOnMarriage)
                        {
                            MBTextManager.SetTextVariable("COURTSHIP_PARTNER", hero.Name, false);
                            return true;
                        }
                    }
                    return false;
                }
            }
            return false;
        }

        // Token: 0x0600024C RID: 588 RVA: 0x0000CE54 File Offset: 0x0000B054
        private bool conversation_marriage_barter_successful_on_condition()
        {
            return Campaign.Current.BarterManager.LastBarterIsAccepted;
        }

        // Token: 0x0600024D RID: 589 RVA: 0x0000CE68 File Offset: 0x0000B068
        private void conversation_courtship_success_on_consequence()
        {
            Hero mainHero = Hero.MainHero;
            Hero oneToOneConversationHero = Hero.OneToOneConversationHero;
            this.MAWedding(mainHero, oneToOneConversationHero);
        }

        // Token: 0x0600024E RID: 590 RVA: 0x0000CE8C File Offset: 0x0000B08C
        private bool HeroLeaveClanLeaderAndDestroyClan(Clan fromClan, Clan newClan = null)
        {
            Hero ancLeader = fromClan.Leader;
            Hero hero = null;
            bool result = false;
            Dictionary<Hero, int> heirApparents = fromClan.GetHeirApparents();
            if (heirApparents.Count > 0)
            {
                int max = (from x in heirApparents.AsEnumerable<KeyValuePair<Hero, int>>()
                           where x.Key != ancLeader
                           select x).Max((KeyValuePair<Hero, int> x) => x.Value);
                hero = heirApparents.AsEnumerable<KeyValuePair<Hero, int>>().FirstOrDefault((KeyValuePair<Hero, int> x) => x.Key != ancLeader && x.Value == max).Key;
            }
            if (hero != null)
            {
                ancLeader.Clan = fromClan;
                ChangeClanLeaderAction.ApplyWithSelectedNewLeader(fromClan, hero);
            }
            else
            {
                Helper.RemoveFromClan(ancLeader, fromClan, false);
                DestroyClanAction.Apply(fromClan);
                result = true;
            }
            if (ancLeader != null)
            {
                ancLeader.Clan = newClan;
            }
            return result;
        }

        // Token: 0x0600024F RID: 591 RVA: 0x0000CF80 File Offset: 0x0000B180
        private void OnHourTickEvent()
        {
            if (Hero.MainHero.Spouse != null && HeroInteractionHelper.OkToDoIt(Hero.MainHero, Hero.MainHero.Spouse, true))
            {
                return;
            }
            Hero hero = this.FirstHeroExSpouseOkToDoIt();
            Helper.Print(string.Format("Set Player Spouse {0}", (hero != null) ? hero.Name.ToString() : "noone:/"), Helper.PrintHow.PrintToLogAndWriteAndDisplay);
            if (hero != null)
            {
                Hero.MainHero.Spouse = hero;
                Helper.RemoveExSpouses(Hero.MainHero, Helper.RemoveExSpousesHow.RAS, null, null);
                Helper.RemoveExSpouses(hero, Helper.RemoveExSpousesHow.RAS, null, null);
            }
        }

        // Token: 0x06000250 RID: 592 RVA: 0x0000D000 File Offset: 0x0000B200
        private void OnHeroesMarried(Hero arg1, Hero arg2, bool arg3)
        {
            if (!this._MAWedding && (arg1 == Hero.MainHero || arg2 == Hero.MainHero) && arg1.CurrentSettlement != null && arg1.CurrentSettlement == arg2.CurrentSettlement)
            {
                if (arg1 == Hero.MainHero && arg2.PartyBelongedTo != MobileParty.MainParty)
                {
                    AddHeroToPartyAction.Apply(arg2, MobileParty.MainParty, true);
                }
                if (arg2 == Hero.MainHero && arg1.PartyBelongedTo != MobileParty.MainParty)
                {
                    AddHeroToPartyAction.Apply(arg1, MobileParty.MainParty, true);
                }
            }
        }


        internal void VerifyMission(Mission mission, bool init = false)
        {
            if (this._mission == mission && !init && this._maTeams != null && this._maTeams.Count == mission.Teams.Count)
            {
                return;
            }
            if (mission != null)
            {
                bool flag = false;
                this._maTeams = new List<MATeam>();
                foreach (Team team in mission.Teams)
                {
                    flag |= (team.TeamAgents.Count > 0);
                    this._maTeams.Add(new MATeam(team));
                }
                if (!flag)
                {
                    mission = null;
                }
            }
            this._mission = mission;
        }

        // Token: 0x06000252 RID: 594 RVA: 0x0000D134 File Offset: 0x0000B334
        private void OnMissionStarted(IMission obj)
        {
            if (obj != null && obj is Mission)
            {
                Mission mission = (Mission)obj;
                this.VerifyMission(mission, true);
            }
        }

        // Token: 0x06000253 RID: 595 RVA: 0x0000D15C File Offset: 0x0000B35C
        private void OnAfterMissionStarted(IMission obj)
        {
            if (obj != null && obj is Mission)
            {
                Mission mission = (Mission)obj;
                this.VerifyMission(mission, false);
            }
        }


        public MATeam ResolveMATeam(string heroStringID)
        {
            if (this._maTeams != null)
            {
                return this._maTeams.FirstOrDefault((MATeam x) => x.Resolve(heroStringID) >= 0);
            }
            return null;
        }

        // Token: 0x06000255 RID: 597 RVA: 0x0000D1BF File Offset: 0x0000B3BF
        private void OnPlayerBattleEnd(MapEvent obj)
        {
            this._maTeams = null;
            this._mission = null;
        }

        // Token: 0x06000256 RID: 598 RVA: 0x0000D1D0 File Offset: 0x0000B3D0
        private bool SaveVersionOlderThen(string versionChaine)
        {
            if (this.SaveVersion == null)
            {
                return true;
            }
            Version v = new Version(versionChaine);
            return this.SaveVersion < v;
        }

        // Token: 0x06000257 RID: 599 RVA: 0x0000D200 File Offset: 0x0000B400
        private void patchClanLeader(Clan clan)
        {
            Hero ancLeader = clan.Leader;
            Hero hero = null;
            bool flag = false;
            Helper.Print(string.Format("Nb Heroes in clan {0} ?= {1}", clan.Name, clan.Heroes.Count), Helper.PrintHow.PrintToLogAndWrite);
            Helper.Print(string.Format("clan({1}).leader.clan ?= {0}", (clan.Leader != null && clan.Leader.Clan != null) ? clan.Leader.Clan.Name.ToString() : "NULL", clan.Name.ToString()), Helper.PrintHow.PrintToLogAndWrite);
            ancLeader.Clan = clan;
            Dictionary<Hero, int> heirApparents = clan.GetHeirApparents();
            if (heirApparents.Count > 0)
            {
                int max = (from x in heirApparents.AsEnumerable<KeyValuePair<Hero, int>>()
                           where x.Key != ancLeader
                           select x).Max((KeyValuePair<Hero, int> x) => x.Value);
                hero = heirApparents.AsEnumerable<KeyValuePair<Hero, int>>().FirstOrDefault((KeyValuePair<Hero, int> x) => x.Key != ancLeader && x.Value == max).Key;
            }
            if (hero != null)
            {
                ChangeClanLeaderAction.ApplyWithSelectedNewLeader(clan, hero);
                ancLeader.Clan = Hero.MainHero.Clan;
            }
            else
            {
                ancLeader.Clan = Hero.MainHero.Clan;
                Helper.Print(string.Format("AncLeader {0} is alive {1} his clan {2}", ancLeader.Name, ancLeader.IsAlive.ToString(), (ancLeader.Clan != null) ? ancLeader.Clan.Name.ToString() : "NULL"), Helper.PrintHow.PrintToLogAndWrite);
                if (ancLeader.IsAlive)
                {
                    Helper.Print("ancLeader TRY to leave the clan", Helper.PrintHow.PrintToLogAndWrite);
                    Helper.RemoveFromClan(ancLeader, clan, false);
                }
                DestroyClanAction.Apply(clan);
                flag = true;
            }
            if (flag)
            {
                Helper.Print(string.Format("PATCH Leader for the clan {0} ERASE the clan", clan.Name), Helper.PrintHow.PrintToLogAndWrite);
                return;
            }
            if (clan.Leader == ancLeader)
            {
                Helper.Print(string.Format("PATCH Leader for the clan {0} FAIL because leader unchanged", clan.Name), Helper.PrintHow.PrintToLogAndWrite);
                return;
            }
            Helper.Print(string.Format("PATCH Leader for the clan {0} SUCCESS swap the leader from {1} to {2}", clan.Name, ancLeader.Name, (clan.Leader == null) ? "NULL" : clan.Leader.Name.ToString()), Helper.PrintHow.PrintToLogAndWrite);
        }


        private bool patchParent(Hero children, Hero mainFemaleSpouseHero, Hero mainMaleSpouseHero)
        {
            bool flag = mainFemaleSpouseHero != null || mainMaleSpouseHero != null;
            bool isFemale = Hero.MainHero.IsFemale;
            if (flag && children.Father == Hero.MainHero && children.Mother == Hero.MainHero)
            {
                Helper.Print(string.Format("Will Patch Parent of {0}", children.Name), Helper.PrintHow.PrintToLogAndWrite);
                if (isFemale)
                {
                    children.Father = (mainMaleSpouseHero ?? mainFemaleSpouseHero);
                }
                else
                {
                    children.Mother = (mainFemaleSpouseHero ?? mainMaleSpouseHero);
                }
                return true;
            }
            if (children.Father == null)
            {
                Helper.Print(string.Format("Will patch Father of {0}", children.Name), Helper.PrintHow.PrintToLogAndWrite);
                children.Father = ((isFemale && mainMaleSpouseHero != null) ? mainMaleSpouseHero : Hero.MainHero);
                return true;
            }
            if (children.Mother == null)
            {
                Helper.Print(string.Format("Will patch Mother of {0}", children.Name), Helper.PrintHow.PrintToLogAndWrite);
                children.Mother = ((!isFemale && mainFemaleSpouseHero != null) ? mainFemaleSpouseHero : Hero.MainHero);
                return true;
            }
            return false;
        }

        // Token: 0x06000259 RID: 601 RVA: 0x0000D544 File Offset: 0x0000B744
        private void LogLectureAdd(List<CharacterMarriedLogEntry> lecture, Hero otherHero, CharacterMarriedLogEntry characterMarriedLogEntry)
        {
            CharacterMarriedLogEntry characterMarriedLogEntry2 = lecture.Find((CharacterMarriedLogEntry x) => x.MarriedHero == otherHero || x.MarriedTo == otherHero);
            if (characterMarriedLogEntry2 != null && characterMarriedLogEntry2.GameTime < characterMarriedLogEntry.GameTime)
            {
                lecture.Remove(characterMarriedLogEntry2);
                characterMarriedLogEntry2 = null;
            }
            if (characterMarriedLogEntry2 == null)
            {
                lecture.Add(characterMarriedLogEntry);
            }
        }

        // Token: 0x0600025A RID: 602 RVA: 0x0000D59C File Offset: 0x0000B79C
        public void LogLectureVerify(List<CharacterMarriedLogEntry> lecture, List<Hero> spouses, List<Hero> logSpouses)
        {
            using (List<CharacterMarriedLogEntry>.Enumerator enumerator = lecture.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    CharacterMarriedLogEntry characterMarriedLogEntry = enumerator.Current;
                    Hero otherHero = (characterMarriedLogEntry.MarriedHero == Hero.MainHero) ? characterMarriedLogEntry.MarriedTo : characterMarriedLogEntry.MarriedHero;
                    if (!Campaign.Current.LogEntryHistory.GetGameActionLogs<CharacterMarriedLogEntry>((CharacterMarriedLogEntry x) => x.GameTime > characterMarriedLogEntry.GameTime && ((x.MarriedHero == otherHero && x.MarriedTo != Hero.MainHero) || (x.MarriedTo == otherHero && x.MarriedHero != Hero.MainHero))).Any<CharacterMarriedLogEntry>())
                    {
                        if (spouses.IndexOf(otherHero) < 0)
                        {
                            spouses.Add(otherHero);
                        }
                        if (logSpouses.IndexOf(otherHero) < 0)
                        {
                            logSpouses.Add(otherHero);
                        }
                    }
                }
            }
        }

        // Token: 0x0600025B RID: 603 RVA: 0x0000D67C File Offset: 0x0000B87C
        private void patchSpouses(CampaignGameStarter campaignGameStarter)
        {
            bool flag = Helper.MASettings.Patch;
            bool flag2 = false;
            bool flag3 = false;
            int num = 0;
            if (Hero.MainHero.Spouse != null && Hero.MainHero.Spouse.HeroState == Hero.CharacterStates.Disabled)
            {
                Hero.MainHero.Spouse.ChangeState(Hero.CharacterStates.Active);
            }
            foreach (Hero hero in Hero.MainHero.ExSpouses)
            {
                if (hero.HeroState == Hero.CharacterStates.Disabled && hero.IsAlive)
                {
                    hero.ChangeState(Hero.CharacterStates.Active);
                }
            }
            List<Hero> list = new List<Hero>();
            if (Hero.MainHero.Spouse != null && Hero.MainHero.Spouse != Hero.MainHero)
            {
                list.Add(Hero.MainHero.Spouse);
            }
            if (Hero.MainHero.ExSpouses != null)
            {
                int count = Hero.MainHero.ExSpouses.Count;
                Helper.RemoveExSpouses(Hero.MainHero, Helper.RemoveExSpousesHow.RAS, null, null);
                if (Hero.MainHero.Spouse != null)
                {
                    Helper.RemoveExSpouses(Hero.MainHero.Spouse, Helper.RemoveExSpousesHow.RAS, list, null);
                }
                num = count;
                foreach (Hero hero2 in Hero.MainHero.ExSpouses)
                {
                    if (this.Partners != null && this.Partners.IndexOf(hero2) >= 0)
                    {
                        flag2 = true;
                        if (hero2.Spouse == Hero.MainHero || (hero2.ExSpouses != null && hero2.ExSpouses.Contains(Hero.MainHero)))
                        {
                            flag = true;
                        }
                    }
                    else if (hero2.IsAlive && this.NoMoreSpouse.IndexOf(hero2) < 0 && hero2 != Hero.MainHero)
                    {
                        list.Add(hero2);
                    }
                }
            }
            List<Hero> list2 = new List<Hero>();
            List<Hero> list3 = new List<Hero>();
            bool flag4 = false;
            if (flag2 || flag || this.SaveVersionOlderThen("2.6.11"))
            {
                List<CharacterMarriedLogEntry> lecture = new List<CharacterMarriedLogEntry>();
                List<CharacterMarriedLogEntry> lecture2 = new List<CharacterMarriedLogEntry>();
                foreach (CharacterMarriedLogEntry characterMarriedLogEntry in Campaign.Current.LogEntryHistory.GetGameActionLogs<CharacterMarriedLogEntry>((CharacterMarriedLogEntry logEntry) => logEntry.MarriedHero == Hero.MainHero || logEntry.MarriedTo == Hero.MainHero))
                {
                    Hero hero3 = (characterMarriedLogEntry.MarriedHero == Hero.MainHero) ? characterMarriedLogEntry.MarriedTo : characterMarriedLogEntry.MarriedHero;
                    if (hero3.IsAlive)
                    {
                        if (!this.SpouseOfPlayer(hero3))
                        {
                            this.LogLectureAdd(lecture, hero3, characterMarriedLogEntry);
                        }
                        this.LogLectureAdd(lecture2, hero3, characterMarriedLogEntry);
                    }
                }
                this.LogLectureVerify(lecture, list, list2);
                this.LogLectureVerify(lecture2, list, list3);
                if (flag || this.SaveVersionOlderThen("2.6.11"))
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        Hero spouse = list[i];
                        if (list3.IndexOf(spouse) < 0)
                        {
                            string text = string.Format("Your spouse {0} is not in the log\r\nDo you want to remove her/him ?", spouse.Name);
                            Helper.Print(text, Helper.PrintHow.PrintToLogAndWriteAndForceDisplay);
                            InformationManager.ShowInquiry(new InquiryData(GameTexts.FindText("str_warning", null).ToString(), text, true, true, "Remove", "Keep", delegate ()
                            {
                                this.RemoveMainHeroSpouse(spouse, true);
                            }, delegate ()
                            {
                                if (this._buggedSpouses == null)
                                {
                                    this._buggedSpouses = new List<Hero>();
                                }
                                this._buggedSpouses.Add(spouse);
                            }, "", 0f, null), false);
                        }
                    }
                    if (list2.Count > 0 || (list.Count > 0 && flag4))
                    {
                        Helper.RemoveExSpouses(Hero.MainHero, Helper.RemoveExSpousesHow.CompletelyRemove, list, null);
                    }
                }
            }
            bool flag5 = Hero.MainHero.Spouse != null;
            Hero mainMaleSpouseHero = this.Spouses.FirstOrDefault((Hero x) => !x.IsFemale);
            Hero mainFemaleSpouseHero = this.Spouses.FirstOrDefault((Hero x) => x.IsFemale);
            for (int i = 0; i < Hero.MainHero.Children.Count; i++)
            {
                Hero children2 = Hero.MainHero.Children[i];
                if (this.patchParent(children2, mainFemaleSpouseHero, mainMaleSpouseHero))
                {
                    i--;
                }
            }
            if (list2.Count > 0)
            {
                foreach (Hero hero4 in list2)
                {
                    for (int i = 0; i < hero4.Children.Count; i++)
                    {
                        Hero children = hero4.Children[i];
                        if (!Hero.MainHero.Children.Any((Hero x) => x == children) && children.Clan == Hero.MainHero.Clan && !this.SpouseOfPlayer(children))
                        {
                            if (this.patchParent(children, mainFemaleSpouseHero, mainMaleSpouseHero))
                            {
                                i--;
                            }
                            else
                            {
                                Hero.MainHero.Children.Add(children);
                            }
                        }
                    }
                }
            }
            foreach (Clan clan in Clan.FindAll((Clan c) => c.IsClan))
            {
                if (clan.Leader != null && this.SpouseOfPlayer(clan.Leader))
                {
                    this.patchClanLeader(clan);
                }
            }
            foreach (Hero hero5 in list)
            {
                if (hero5.IsAlive)
                {
                    Helper.PatchHeroPlayerClan(hero5, false, true);
                }
                if (hero5.ExSpouses != null)
                {
                    int count2 = hero5.ExSpouses.Count;
                }
                Romance.RomanceLevelEnum romanticLevel = Romance.GetRomanticLevel(hero5, Hero.MainHero);
                if (romanticLevel == Romance.RomanceLevelEnum.Ended || romanticLevel == Romance.RomanceLevelEnum.Untested)
                {
                    Util.CleanRomance(hero5, Hero.MainHero, Romance.RomanceLevelEnum.Marriage);
                }
                if (num != Hero.MainHero.ExSpouses.Count)
                {
                    Helper.RemoveExSpouses(Hero.MainHero, Helper.RemoveExSpousesHow.RAS, null, null);
                    num = Hero.MainHero.ExSpouses.Count;
                }
                Helper.RemoveExSpouses(hero5, Helper.RemoveExSpousesHow.AddMainHero | ((flag4 || flag2) ? Helper.RemoveExSpousesHow.CompletelyRemove : Helper.RemoveExSpousesHow.RAS), list, null);
            }
            if (this.Partners != null && flag2)
            {
                foreach (Hero hero6 in this.Partners)
                {
                    if (!list3.Contains(hero6))
                    {
                        this.RemoveMainHeroSpouse(hero6, false);
                    }
                    else
                    {
                        while (this.Partners.Remove(hero6))
                        {
                        }
                    }
                }
            }
            Helper.Print(string.Format("patchClanLeader {0}", flag3 ? "OK SUCCESS" : "RAS"), Helper.PrintHow.PrintToLogAndWrite | (flag3 ? Helper.PrintHow.PrintForceDisplay : Helper.PrintHow.PrintRAS));
        }

        // Token: 0x0600025C RID: 604 RVA: 0x0000DDE8 File Offset: 0x0000BFE8
        public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            Helper.MASettingsClean();
            Helper.MAEtape = Helper.Etape.EtapeLoadPas2;
            if (this.NoMoreSpouse == null)
            {
                this.NoMoreSpouse = new List<Hero>();
            }
            this.patchSpouses(campaignGameStarter);
            foreach (Hero hero in Hero.AllAliveHeroes.ToList<Hero>())
            {
                if (hero.HomeSettlement == null)
                {
                }
                if (hero.HomeSettlement == null)
                {
                    Helper.PatchHomeSettlement(hero);
                }
                if ((hero.Spouse == Hero.MainHero || (hero.ExSpouses != null && hero.ExSpouses.Contains(Hero.MainHero))) && !this.SpouseOfPlayer(hero))
                {
                    Helper.RemoveExSpouses(hero, Helper.RemoveExSpousesHow.RAS, null, Hero.MainHero);
                }
                else if (this.SpouseOfPlayer(hero))
                {
                    Helper.OccupationToLord(hero.CharacterObject);
                    Helper.PatchHeroPlayerClan(hero, false, true);
                }
                else if (hero.Spouse == null || (hero.ExSpouses != null && hero.ExSpouses.Count > 0))
                {
                    Helper.RemoveExSpouses(hero, Helper.RemoveExSpousesHow.RAS, null, null);
                }
            }
            Helper.MASettings.Patch = false;
            this.AddDialogs(campaignGameStarter);
        }

        // Token: 0x0600025D RID: 605 RVA: 0x0000DF1C File Offset: 0x0000C11C
        private void AfterLoad()
        {
        }

        // Token: 0x0600025E RID: 606 RVA: 0x0000DF20 File Offset: 0x0000C120
        public override void SyncData(IDataStore dataStore)
        {
            string text = null;
            if (dataStore.IsSaving)
            {
                text = Helper.VersionGet.ToString();
            }
            dataStore.SyncData<List<Hero>>("Partners", ref this.Partners);
            dataStore.SyncData<List<Hero>>("NoMoreSpouse", ref this.NoMoreSpouse);
            dataStore.SyncData<List<PersuasionAttempt>>("PreviousCheatPersuasionAttempts", ref this._previousCheatPersuasionAttempts);
            dataStore.SyncData<string>("SaveVersion", ref text);
        }

        public List<Hero> Partners;

        // Token: 0x040000A0 RID: 160
        public List<Hero> NoMoreSpouse;

        // Token: 0x040000A1 RID: 161
        private List<Hero> _buggedSpouses;


        private Version SaveVersion;

        private List<PersuasionAttempt> _previousCheatPersuasionAttempts;

        // Token: 0x040000A4 RID: 164
        private List<PersuasionTask> _allReservations;

        // Token: 0x040000A5 RID: 165
        private float _maximumScoreCap;

        // Token: 0x040000A6 RID: 166
        private float _successValue = 1f;

        // Token: 0x040000A7 RID: 167
        private float _criticalSuccessValue = 2f;

        // Token: 0x040000A8 RID: 168
        private float _criticalFailValue = 2f;

        // Token: 0x040000A9 RID: 169
        private float _failValue = 1f;

        // Token: 0x040000AA RID: 170
        private bool _MAWedding;

        // Token: 0x040000AB RID: 171
        private List<MATeam> _maTeams;

        // Token: 0x040000AC RID: 172
        private Mission _mission;

        // Token: 0x040000AD RID: 173
        public static MARomanceCampaignBehavior Instance;
    }
}
