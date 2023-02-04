

using HarmonyLib;
using MarryAnyone.Helpers;
using MarryAnyone.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using Extensions = TaleWorlds.Core.Extensions;


namespace MarryAnyone
{
  internal static class Helper
  {
    public const string MODULE_NAME = "MarryAnyone";
    private static MASettings _MASettings = (MASettings) null;
    public static Helper.Etape MAEtape;
    public const Helper.PrintHow PRINT_TRACE_WEDDING = Helper.PrintHow.PrintDisplay;
    public const Helper.PrintHow PRINT_TRACE_LOAD = Helper.PrintHow.PrintDisplay;
    public const Helper.PrintHow PRINT_TRACE_PREGNANCY = Helper.PrintHow.PrintDisplay;
    public const Helper.PrintHow PRINT_TRACE_ROMANCE = Helper.PrintHow.PrintRAS;
    public const Helper.PrintHow PRINT_TRACE_ROMANCE_IS_SUITABLE = Helper.PrintHow.PrintRAS;
    public const Helper.PrintHow PRINT_PATCH = Helper.PrintHow.PrintToLogAndWrite;
    public const Helper.PrintHow PRINT_TRACE_CREATE_CLAN = Helper.PrintHow.PrintDisplay;
    public const Helper.PrintHow PRINT_TRACE_ARENA_PARTICIPANT = Helper.PrintHow.PrintDisplay;
    public const Helper.PrintHow PRINT_TRACE_PATCHTOOMUCHWANDERER = Helper.PrintHow.PrintForceDisplay;
    private static Version _version = (Version) null;
    private static string _moduleName = (string) null;
    public static Color yellowCollor = new Color(0.0f, 0.8f, 0.4f, 1f);

    public static MASettings MASettings
    {
      get
      {
        if (Helper._MASettings == null)
          Helper._MASettings = new MASettings();
        return Helper._MASettings;
      }
    }

    public static void MASettingsClean() => Helper._MASettings = (MASettings) null;

    public static Version VersionGet
    {
      get
      {
        if (Helper._version == (Version) null)
          Helper._version = typeof (MASubModule).Assembly.GetName().Version;
        return Helper._version;
      }
    }

    public static string ModuleNameGet
    {
      get
      {
        if (Helper._moduleName == null)
        {
          Helper._moduleName = typeof (MASubModule).Assembly.GetName().Name.ToString();
          if (Helper._moduleName == null)
            Helper._moduleName = "Retrieve module name FAIL";
        }
        return Helper._moduleName;
      }
    }

    public static void Print(string message, Helper.PrintHow printHow = Helper.PrintHow.PrintRAS)
    {
            if ((Helper.MASettings.Debug && (printHow & Helper.PrintHow.PrintDisplay) != Helper.PrintHow.PrintRAS) || (printHow & Helper.PrintHow.PrintForceDisplay) != Helper.PrintHow.PrintRAS)
            {
                Color color = new Color(0.6f, 0.2f, 1f, 1f);
                InformationManager.DisplayMessage(new InformationMessage(message, color));
            }
        }

    public static void PrintWithColor(string message, uint color) => Helper.PrintWithColor(message, Color.FromUint(color));

    public static void PrintWithColor(string message, Color color) => InformationManager.DisplayMessage(new InformationMessage(message, color));

    public static void Error(Exception exception) => InformationManager.DisplayMessage(new InformationMessage(Helper.ModuleNameGet + ": " + exception.Message, Colors.Red));

    private static int NbSpouse(Hero hero) => (hero.Spouse != null ? 1 : 0) + hero.ExSpouses.Count;

    public static bool IsSpouseOrExSpouseOf(Hero hero, Hero spouse)
    {
      if (hero.Spouse == spouse)
        return true;
      return hero.ExSpouses != null && hero.ExSpouses.Contains(spouse);
    }

    public static void PatchHomeSettlement(Hero hero)
    {
      if (hero.HomeSettlement == null)
        hero.UpdateHomeSettlement();
      if (hero.HomeSettlement == null && hero.BornSettlement == null)
      {
        Settlement settlement = (Settlement) null;
        if (hero.Culture != null)
          settlement = Extensions.GetRandomElementInefficiently<Settlement>(Settlement.FindAll((Func<Settlement, bool>) (x => x.Culture == hero.Culture)));
        if (settlement == null)
          settlement = Extensions.GetRandomElementInefficiently<Settlement>((IEnumerable<Settlement>) Settlement.All);
        hero.BornSettlement = settlement;
        hero.UpdateHomeSettlement();
      }
      if (hero.HomeSettlement == null)
      {
        Helper.Print(string.Format("Settlement not resolved for {0} bornSettlement ?= {1}", (object) hero.Name, hero.BornSettlement != null ? (object) hero.BornSettlement.Name : "NULL"), Helper.PrintHow.PrintToLogAndWriteAndForceDisplay);
        throw new Exception(string.Format("Settlement not resolved for {0}", (object) hero.Name));
      }
    }

    public static void RemoveExSpouses(
      Hero hero,
      Helper.RemoveExSpousesHow comment = Helper.RemoveExSpousesHow.RAS,
      List<Hero> otherSpouses = null,
      Hero otherHero = null)
    {
      FieldInfo fieldInfo1 = AccessTools.Field(typeof (Hero), "_exSpouses");
      List<Hero> source1 = (List<Hero>) fieldInfo1.GetValue((object) hero);
      FieldInfo fieldInfo2 = AccessTools.Field(typeof (Hero), "ExSpouses");
      if (otherHero != null && hero.Spouse == otherHero && (comment & Helper.RemoveExSpousesHow.AddOtherHero) == Helper.RemoveExSpousesHow.RAS)
      {
        if ((comment & Helper.RemoveExSpousesHow.RemoveOnSpouseToo) != Helper.RemoveExSpousesHow.RAS && otherHero.Spouse == hero)
          Helper.SetSpouse(otherHero.Spouse, (Hero) null, Helper.enuSetSpouse.JustSet);
        Helper.SetSpouse(hero, (Hero) null, Helper.enuSetSpouse.JustSet);
        if (Romance.GetRomanticLevel(hero, otherHero) == Romance.RomanceLevelEnum.Marriage)
          Util.CleanRomance(hero, otherHero, Romance.RomanceLevelEnum.Ended);
      }
      List<Hero> source2 = source1 != null ? ((IEnumerable<Hero>) source1).Distinct<Hero>().ToList<Hero>() : new List<Hero>();
      if ((comment & Helper.RemoveExSpousesHow.AddMainHero) != Helper.RemoveExSpousesHow.RAS)
      {
        if (hero.Spouse != null)
          source2.Add(hero.Spouse);
        Helper.SetSpouse(hero, Hero.MainHero, Helper.enuSetSpouse.JustSet);
      }
      if ((comment & Helper.RemoveExSpousesHow.CompletelyRemove) != Helper.RemoveExSpousesHow.RAS && source2.Count > 0)
      {
        if ((comment & Helper.RemoveExSpousesHow.RemoveOnSpouseToo) != Helper.RemoveExSpousesHow.RAS || (comment & Helper.RemoveExSpousesHow.RemoveIfDeadToo) == Helper.RemoveExSpousesHow.RAS)
        {
          source2 = ((IEnumerable<Hero>) source2).Distinct<Hero>().ToList<Hero>();
          using (List<Hero>.Enumerator enumerator = ((comment & Helper.RemoveExSpousesHow.RemoveIfDeadToo) == Helper.RemoveExSpousesHow.RAS ? ((IEnumerable<Hero>) source2).Where<Hero>((Func<Hero, bool>) (exSpouse => exSpouse.IsAlive)).ToList<Hero>() : ((IEnumerable<Hero>) source2).ToList<Hero>()).GetEnumerator())
          {
label_16:
            if (enumerator.MoveNext())
            {
              Hero current = enumerator.Current;
              if ((comment & Helper.RemoveExSpousesHow.RemoveOnSpouseToo) != Helper.RemoveExSpousesHow.RAS && current.Spouse == hero)
                Helper.SetSpouse(current.Spouse, (Hero) null, Helper.enuSetSpouse.JustSet);
              while (source2.Remove(current))
                ;
              goto label_16;
            }
          }
        }
        if ((comment & Helper.RemoveExSpousesHow.RemoveIfDeadToo) != Helper.RemoveExSpousesHow.RAS)
          source2.Clear();
      }
      if (otherHero != null && (comment & Helper.RemoveExSpousesHow.AddOtherHero) != Helper.RemoveExSpousesHow.RAS)
      {
        if (hero.Spouse == null)
        {
          Helper.SetSpouse(hero, otherHero, Helper.enuSetSpouse.JustSet);
          if ((comment & Helper.RemoveExSpousesHow.AddOnSpouseToo) != Helper.RemoveExSpousesHow.RAS && otherHero.Spouse == null)
            Helper.SetSpouse(otherHero, hero, Helper.enuSetSpouse.JustSet);
        }
        else if (source2.IndexOf(otherHero) < 0)
          source2.Add(otherHero);
      }
      if (otherSpouses != null)
      {
        foreach (Hero otherSpouse in otherSpouses)
        {
          if (otherSpouse != hero && otherSpouse != hero.Spouse && source2.IndexOf(otherSpouse) < 0)
          {
            if ((comment & Helper.RemoveExSpousesHow.AddOnSpouseToo) != Helper.RemoveExSpousesHow.RAS && otherSpouse.Spouse == null)
              Helper.SetSpouse(otherSpouse, hero, Helper.enuSetSpouse.JustSet);
            source2.Add(otherSpouse);
          }
        }
      }
      if (hero.Spouse != null)
      {
        while (source2.Remove(hero.Spouse))
          ;
      }
      do
        ;
      while (source2.Remove(hero));
      do
        ;
      while (otherHero != null && (comment & Helper.RemoveExSpousesHow.AddOtherHero) == Helper.RemoveExSpousesHow.RAS && source2.Remove(otherHero));
      if ((comment & Helper.RemoveExSpousesHow.AddMainHero) == Helper.RemoveExSpousesHow.RAS && otherSpouses == null && hero.Spouse == null && source2.Count > 0)
      {
        Helper.SetSpouse(hero, source2[source2.Count - 1], Helper.enuSetSpouse.SetReciproqueIFNullOnReciproque);
        source2.RemoveAt(source2.Count - 1);
      }
      fieldInfo1.SetValue((object) hero, (object) source2);
      MBReadOnlyList<Hero> readOnlyList = source2.GetReadOnlyList();
      fieldInfo2.SetValue((object) hero, (object) readOnlyList);
    }

    internal static bool SetSpouse(Hero hero, Hero spouse, Helper.enuSetSpouse comment)
    {
      if ((comment & Helper.enuSetSpouse.TestNull) != Helper.enuSetSpouse.JustSet && hero.Spouse != null)
        return false;
      if ((comment & Helper.enuSetSpouse.UseStandartAffectation) != Helper.enuSetSpouse.JustSet)
      {
        if ((comment & Helper.enuSetSpouse.TestNullReciproque) != Helper.enuSetSpouse.JustSet && spouse != null && spouse.Spouse != null)
          return false;
        hero.Spouse = spouse;
        return true;
      }
      FieldInfo fieldInfo = AccessTools.Field(typeof (Hero), "_spouse");
      if (fieldInfo == (FieldInfo) null)
        throw new Exception("_spouse property nof found on Hero Class !");
      fieldInfo.SetValue((object) hero, (object) spouse);
      if (spouse != null && (comment & Helper.enuSetSpouse.SetReciproque) != Helper.enuSetSpouse.JustSet && ((comment & Helper.enuSetSpouse.TestNullReciproque) == Helper.enuSetSpouse.JustSet || spouse.Spouse == null))
        fieldInfo.SetValue((object) spouse, (object) hero);
      return true;
    }

    public static void RemoveDuplicatedHero()
    {
      CampaignObjectManager campaignObjectManager = Campaign.Current.CampaignObjectManager;
      if (AccessTools.Field(typeof (CampaignObjectManager), "<AliveHeroes>k__BackingField") == (FieldInfo) null)
        throw new Exception("Property AliveHeroes not found on CampaignObjectManager instance");
      List<Hero> list = ((IEnumerable<Hero>) campaignObjectManager.AliveHeroes).ToList<Hero>();
      list.Sort((Comparison<Hero>) ((x, y) => string.Compare(((MBObjectBase) x).StringId, ((MBObjectBase) y).StringId, StringComparison.Ordinal)));
      for (int index = 0; index < list.Count - 1; ++index)
      {
        Hero hero1 = list[index];
        Hero hero2 = list[index + 1];
      }
      Helper.Print(string.Format("RemoveDuplicatedHero parcours {0} heroes", (object) list.Count), Helper.PrintHow.PrintToLogAndWrite);
    }

    public static void OccupationToLord(CharacterObject character)
    {
      if (character.Occupation == Occupation.Lord)
        return;
      character.HeroObject?.SetNewOccupation((Occupation) 3);
      AccessTools.Field(typeof (CharacterObject), "_occupation").SetValue((object) character, (object) (Occupation) 3);
      if (CharacterObject.PlayerCharacter != null)
        AccessTools.Field(typeof (CharacterObject), "_originCharacter").SetValue((object) character, (object) CharacterObject.PlayerCharacter);
      Helper.Print(string.Format("Swap Occupation To Lord for {0} newOccupation ?= {1}", (object) ((object) ((BasicCharacterObject) character).Name).ToString(), (object) character.Occupation.ToString()), Helper.PrintHow.PrintToLogAndWriteAndDisplay);
    }

    public static void OccupationToCompanion(CharacterObject character)
    {
      if (character.Occupation == Occupation.Wanderer)
        return;
      character.HeroObject?.SetNewOccupation((Occupation) 16);
      AccessTools.Field(typeof (CharacterObject), "_occupation").SetValue((object) character, (object) (Occupation) 16);
      AccessTools.Field(typeof (CharacterObject), "_originCharacter").SetValue((object) character, (object) CharacterObject.PlayerCharacter);
    }

    public static void RemoveFromClan(Hero hero, Clan fromClan, bool canPatchLeader = false)
    {
      List<Hero> list1 = ((IEnumerable<Hero>) fromClan.Lords).ToList<Hero>();
      if (list1.IndexOf(hero) >= 0)
      {
        while (list1.IndexOf(hero) >= 0)
          list1.Remove(hero);
        FieldInfo fieldInfo = AccessTools.Field(typeof (Clan), "<Lords>k__BackingField");
        if (fieldInfo == (FieldInfo) null)
          throw new Exception("<Lords>k__BackingField not found");
        fieldInfo.SetValue((object) fromClan, (object) new MBReadOnlyList<Hero>(list1));
      }
      List<Hero> list2 = ((IEnumerable<Hero>) fromClan.Heroes).ToList<Hero>();
      if (list2.IndexOf(hero) >= 0)
      {
        while (list2.IndexOf(hero) >= 0)
          list2.Remove(hero);
        FieldInfo fieldInfo = AccessTools.Field(typeof (Clan), "<Lords>k__BackingField");
        if (fieldInfo == (FieldInfo) null)
          throw new Exception("<Heroes>k__BackingField not found");
        fieldInfo.SetValue((object) fromClan, (object) new MBReadOnlyList<Hero>(list2));
      }
      if (!canPatchLeader || fromClan.Leader != hero)
        return;
      if (AccessTools.Field(typeof (Clan), "_leader") == (FieldInfo) null)
        throw new Exception("_leader not found");
      Helper.Print(string.Format("Patch Clan Leader of clan {0} set Leader = null", (object) ((object) fromClan.Name).ToString()), Helper.PrintHow.PrintToLogAndWrite);
    }

    public static void SwapClan(Hero hero, Clan fromClan, Clan toClan)
    {
      hero.Clan = (Clan) null;
      if (hero.CharacterObject.Occupation != Occupation.Lord)
        Helper.OccupationToLord(hero.CharacterObject);
      hero.Clan = toClan;
      if (toClan != null)
      {
        if (((IEnumerable<Hero>) toClan.Lords).FirstOrDefault<Hero>((Func<Hero, bool>) (x => x == hero)) == null)
          ((IEnumerable<Hero>) toClan.Lords).AddItem<Hero>(hero);
        if (fromClan != null && (fromClan.Lords.IndexOf(hero) >= 0 || fromClan.Heroes.IndexOf(hero) >= 0))
          Helper.RemoveFromClan(hero, fromClan);
      }
      if (toClan == hero.Clan)
        return;
      Helper.Print(string.Format("SwapClan:: FAIL for Hero {0} to Clan {1}", (object) hero.Name, toClan != null ? (object) toClan.Name : "NULL"), Helper.PrintHow.PrintToLogAndWrite);
    }

    public static void FamilyJoinClan(Hero hero, Clan fromClan, Clan toClan)
    {
      if (hero.Clan == fromClan)
        Helper.SwapClan(hero, fromClan, toClan);
      for (int index = 0; index < fromClan.Lords.Count; ++index)
      {
        Hero lord = fromClan.Lords[index];
        if (lord.Father == hero || lord.Mother == hero)
        {
          Helper.FamilyJoinClan(lord, fromClan, toClan);
          --index;
        }
      }
      if (hero.Spouse == null || hero.Spouse.Clan != fromClan)
        return;
      Helper.FamilyJoinClan(hero.Spouse, fromClan, toClan);
    }

    public static void FamilyAdoptChild(Hero hero, Hero toHero, Clan fromClan)
    {
      bool flag = hero.IsFemale != toHero.IsFemale;
      foreach (Hero lord in fromClan.Lords)
      {
        if (lord.Mother == hero && hero.IsFemale)
        {
          if (lord.Father == null || lord.Father != null && (lord.Father == hero || !lord.Father.IsAlive))
          {
            Helper.Print(string.Format("Hero {0} adopt a child {1} like father", (object) toHero.Name, (object) lord.Name), Helper.PrintHow.PrintDisplay);
            lord.Father = toHero;
          }
        }
        else if (lord.Father == hero && !hero.IsFemale)
        {
          if (lord.Mother == null || lord.Mother != null && (lord.Mother == hero || !lord.Mother.IsAlive))
          {
            Helper.Print(string.Format("Hero {0} adopt a child {1} like mother", (object) toHero.Name, (object) lord.Name), Helper.PrintHow.PrintDisplay);
            lord.Mother = toHero;
          }
        }
        else if (lord.Mother == hero && (lord.Father == null || lord.Father != null && (lord.Father == hero || lord.Father.IsDead)))
        {
          if (flag)
          {
            lord.Father = hero;
            lord.Mother = toHero;
          }
          else
            lord.Father = toHero;
        }
        else if (lord.Father == hero && (lord.Mother == null || lord.Mother != null && (lord.Mother == hero || lord.Mother.IsDead)))
        {
          if (flag)
          {
            lord.Mother = hero;
            lord.Father = toHero;
          }
          else
            lord.Mother = toHero;
        }
      }
    }

    public static bool PatchHeroPlayerClan(Hero hero, bool canBeOtherClan = false, bool etSpouseMainHero = false)
    {
      bool flag = false;
      if (!canBeOtherClan && hero.Clan != Clan.PlayerClan || canBeOtherClan && hero.Clan == null || !canBeOtherClan && Clan.PlayerClan != null && Clan.PlayerClan.Lords.IndexOf(hero) < 0)
      {
        hero.Clan = (Clan) null;
        if (hero.CharacterObject.Occupation != Occupation.Lord)
                    Helper.OccupationToLord(hero.CharacterObject);
        hero.Clan = Clan.PlayerClan;
        if (((IEnumerable<Hero>) Hero.MainHero.Clan.Lords).FirstOrDefault<Hero>((Func<Hero, bool>) (x => x == hero)) == null)
        {
          ((IEnumerable<Hero>) Hero.MainHero.Clan.Lords).AddItem<Hero>(hero);
          Helper.Print("Add hero to Noble of the clan", Helper.PrintHow.PrintToLogAndWriteAndDisplay);
        }
        flag = true;
      }
      if (etSpouseMainHero && hero.Spouse == null)
        hero.Spouse = Hero.MainHero;
      if (flag)
        Helper.Print(string.Format("Patch Hero {0} with PlayerClan {1} => {2}", (object) ((object) hero.Name).ToString(), (object) ((object) Clan.PlayerClan.Name).ToString(), (object) ((object) hero.Clan.Name).ToString()), Helper.PrintHow.PrintForceDisplay);
      return flag;
    }

    public static int TraitCompatibility(Hero hero1, Hero hero2, TraitObject trait)
    {
      int traitLevel1 = hero1.GetTraitLevel(trait);
      int traitLevel2 = hero2.GetTraitLevel(trait);
      if (traitLevel1 == 0 || traitLevel2 == 0)
        return 0;
      return traitLevel1 > 0 && traitLevel2 > 0 ? (traitLevel1 < traitLevel2 ? traitLevel1 : traitLevel2) : (traitLevel1 < 0 && traitLevel2 < 0 ? (traitLevel1 < traitLevel2 ? -traitLevel2 : -traitLevel1) : (traitLevel1 > 0 ? -traitLevel1 + traitLevel2 : traitLevel1 - traitLevel2));
    }

    public static bool CheatEnabled(Hero hero, Hero mainHero)
    {
      if (!Helper.MASettings.Cheating || !hero.IsAlive || hero.IsTemplate || !Helper.MASettings.Notable && (Helper.MASettings.Notable || hero.IsNotable))
        return false;
      return Helper.MASettings.RelationLevelMinForRomance == -1 || hero.GetRelation(mainHero) >= Helper.MASettings.RelationLevelMinForCheating;
    }

    public static bool IsSuitableForMarriagePathMA(Hero maidenOrSuitor) => maidenOrSuitor.IsAlive && !maidenOrSuitor.IsTemplate && (Helper.MASettings.Notable || !maidenOrSuitor.IsNotable);

    public static bool FactionAtWar(Hero hero, Hero otherHero)
    {
      IFaction ifaction1 = (IFaction) null;
      IFaction ifaction2 = (IFaction) null;
      if (hero.Clan != null)
        ifaction1 = hero.Clan.MapFaction;
      if (otherHero.Clan != null)
        ifaction2 = otherHero.Clan.MapFaction;
      return ifaction1 != null && ifaction2 != null && ifaction1.IsAtWarWith(ifaction2);
    }

    public static bool HeroOccupiedAndCantMarried(Hero hero)
    {
      if (hero.CharacterObject.Occupation == Occupation.Lord || Campaign.Current == null)
        return false;
      IssueBase issueBase;
      Campaign.Current.IssueManager.Issues.TryGetValue(hero, out issueBase);
      return issueBase != null;
    }

    public static bool MarryEnabledPathMA(Hero hero, Hero mainHero, bool testRelationLevel = true)
    {
      if (hero.CharacterObject.Occupation == Occupation.Lord || !hero.IsAlive || !Helper.MASettings.Notable && (Helper.MASettings.Notable || hero.IsNotable))
        return false;
      if (!testRelationLevel)
        return true;
      if (!testRelationLevel)
        return false;
      return Helper.MASettings.RelationLevelMinForRomance == -1 || hero.GetRelation(mainHero) >= Helper.MASettings.RelationLevelMinForRomance;
    }

    public static List<Hero> ListClanLord(Hero hero)
    {
      List<Hero> heroList = new List<Hero>();
      heroList.Add(hero);
      if (hero.Clan != null)
      {
        foreach (Hero lord in hero.Clan.Lords)
        {
          if (lord != hero)
            heroList.Add(lord);
        }
      }
      return heroList;
    }

    public enum PrintHow
    {
      PrintRAS = 0,
      PrintDisplay = 1,
      PrintForceDisplay = 2,
      PrintToLog = 4,
      UpdateLog = 8,
      PrintToLogAndWrite = 12, // 0x0000000C
      PrintToLogAndWriteAndDisplay = 13, // 0x0000000D
      PrintToLogAndWriteAndForceDisplay = 14, // 0x0000000E
      CanInitLogPath = 16, // 0x00000010
      PrintToLogAndWriteAndInit = 28, // 0x0000001C
      PrintToLogAndWriteAndInitAndForceDisplay = 30, // 0x0000001E
    }

    public enum Etape
    {
      EtapeInitialize = 1,
      EtapeLoad = 2,
      EtapeLoadPas2 = 4,
    }

    public enum RemoveExSpousesHow
    {
      RAS = 0,
      CompletelyRemove = 1,
      RemoveMainHero = 2,
      AddMainHero = 4,
      OtherSpousesStrict = 8,
      RemoveOtherHero = 16, // 0x00000010
      AddOtherHero = 32, // 0x00000020
      RemoveOnSpouseToo = 64, // 0x00000040
      AddOnSpouseToo = 128, // 0x00000080
      RemoveIfDeadToo = 256, // 0x00000100
    }

    public enum enuSetSpouse
    {
      JustSet = 0,
      SetReciproque = 1,
      TestNullReciproque = 2,
      SetReciproqueIFNullOnReciproque = 3,
      TestNull = 4,
      UseStandartAffectation = 8,
    }
  }
}
