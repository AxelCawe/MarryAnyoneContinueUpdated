// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Patches.Helpers.GetHeroRelationToHeroTextShortPatch
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using HarmonyLib;
using MarryAnyone.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Localization;


namespace MarryAnyone.Patches.Helpers
{
  [HarmonyPatch(typeof (ConversationHelper), "GetHeroRelationToHeroTextShort")]
  internal class GetHeroRelationToHeroTextShortPatch
  {
    private static string _stringResult;
    private static bool _isFemale;

    private static TextObject FindText(string id) => GameTexts.FindText(id, (string) null);

    private static bool ResultContains(string id) => GetHeroRelationToHeroTextShortPatch._stringResult.Contains(((object) GetHeroRelationToHeroTextShortPatch.FindText(id)).ToString());

    private static TextObject ReplaceResult(
      string idF,
      string idM,
      string nidF,
      string nidM)
    {
      GetHeroRelationToHeroTextShortPatch._stringResult = GetHeroRelationToHeroTextShortPatch._stringResult.Replace(((object) GetHeroRelationToHeroTextShortPatch.FindText(GetHeroRelationToHeroTextShortPatch._isFemale ? idF : idM)).ToString(), ((object) GetHeroRelationToHeroTextShortPatch.FindText(GetHeroRelationToHeroTextShortPatch._isFemale ? nidF : nidM)).ToString());
      return new TextObject(GetHeroRelationToHeroTextShortPatch._stringResult, (Dictionary<string, object>) null);
    }

    private static void Postfix(ref string __result, Hero queriedHero, Hero baseHero)
    {
      GetHeroRelationToHeroTextShortPatch._stringResult = ((object) __result).ToString();
      GetHeroRelationToHeroTextShortPatch._isFemale = queriedHero.IsFemale;
      if (queriedHero.IsAlive && baseHero.IsAlive)
      {
        if (GetHeroRelationToHeroTextShortPatch.ResultContains("str_ex_husband_motherinlaw") || GetHeroRelationToHeroTextShortPatch.ResultContains("str_ex_husband_fatherinlaw"))
          __result = GetHeroRelationToHeroTextShortPatch.ReplaceResult("str_ex_husband_motherinlaw", "str_ex_husband_fatherinlaw", "str_ma_husband_motherinlaw", "str_ma_husband_fatherinlaw").ToString();
        if (GetHeroRelationToHeroTextShortPatch.ResultContains("str_ex_wife_motherinlaw") || GetHeroRelationToHeroTextShortPatch.ResultContains("str_ex_wife_fatherinlaw"))
          __result = GetHeroRelationToHeroTextShortPatch.ReplaceResult("str_ex_wife_motherinlaw", "str_ex_wife_fatherinlaw", "str_ma_wife_motherinlaw", "str_ma_wife_fatherinlaw").ToString();
        if (GetHeroRelationToHeroTextShortPatch.ResultContains("str_ex_husband_sisterinlaw") || GetHeroRelationToHeroTextShortPatch.ResultContains("str_ex_husband_brotherinlaw"))
          __result = GetHeroRelationToHeroTextShortPatch.ReplaceResult("str_ex_husband_sisterinlaw", "str_ex_husband_brotherinlaw", "str_ma_husband_sisterinlaw", "str_ma_husband_brotherinlaw").ToString();
        if (GetHeroRelationToHeroTextShortPatch.ResultContains("str_ex_wife_sisterinlaw") || GetHeroRelationToHeroTextShortPatch.ResultContains("str_ex_wife_brotherinlaw"))
          __result = GetHeroRelationToHeroTextShortPatch.ReplaceResult("str_ex_wife_sisterinlaw", "str_ex_wife_brotherinlaw", "str_ma_wife_sisterinlaw", "str_ma_wife_brotherinlaw").ToString();
      }
      TextObject textObject = new TextObject(__result);
      __result = GetHeroRelationToHeroTextShortPatch.GetHeroRelationToHeroTextShort(queriedHero, baseHero).ToString();
      if (__result != string.Empty)
        return;
      __result = textObject.ToString();
    }

    private static TextObject GetHeroRelationToHeroTextShort(
      Hero queriedHero,
      Hero baseHero)
    {
      if (baseHero.Father == queriedHero && (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero) || baseHero.ExSpouses.Contains(queriedHero)))
        return GameTexts.FindText("str_fatherhusband", (string) null);
      if (baseHero.Mother == queriedHero && (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero) || baseHero.ExSpouses.Contains(queriedHero)))
        return GameTexts.FindText("str_motherwife", (string) null);
      if (baseHero.Siblings.Contains<Hero>(queriedHero) && (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero) || baseHero.ExSpouses.Contains(queriedHero)))
        return !queriedHero.IsFemale ? GameTexts.FindText("str_brotherhusband", (string) null) : GameTexts.FindText("str_sisterwife", (string) null);
      if (baseHero.Children.Contains(queriedHero) && (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero) || baseHero.ExSpouses.Contains(queriedHero)))
        return !queriedHero.IsFemale ? GameTexts.FindText("str_sonhusband", (string) null) : GameTexts.FindText("str_daughterwife", (string) null);
      ISettingsProvider settingsProvider = (ISettingsProvider) new MASettings();
      if (settingsProvider.AdoptionTitles && settingsProvider.Adoption)
      {
        if (baseHero.Mother == null != (baseHero.Father == null))
        {
          if (baseHero.Mother == queriedHero)
            return GameTexts.FindText("str_adoptivemother", (string) null);
          if (baseHero.Father == queriedHero)
            return GameTexts.FindText("str_adoptivefather", (string) null);
        }
        if (queriedHero.Mother == null != (queriedHero.Father == null) && baseHero.Children.Contains(queriedHero))
          return !queriedHero.IsFemale ? GameTexts.FindText("str_adoptedson", (string) null) : GameTexts.FindText("str_adopteddaughter", (string) null);
      }
      if (baseHero.Spouse == queriedHero || queriedHero.ExSpouses.Contains(baseHero) || baseHero.ExSpouses.Contains(queriedHero))
        return !queriedHero.IsAlive || !baseHero.IsAlive ? (!queriedHero.IsFemale ? GameTexts.FindText("str_exhusband", (string) null) : GameTexts.FindText("str_exwife", (string) null)) : (!queriedHero.IsFemale ? GameTexts.FindText("str_husband", (string) null) : GameTexts.FindText("str_wife", (string) null));
      if (baseHero.Spouse != null)
      {
        foreach (Hero exSpouse in baseHero.Spouse.ExSpouses)
        {
          foreach (Hero hero in ((IEnumerable<Hero>) exSpouse.ExSpouses).Where<Hero>((Func<Hero, bool>) (x => x.IsAlive)).ToList<Hero>())
          {
            if (hero == queriedHero)
              return GetHeroRelationToHeroTextShortPatch.SpousesSpouse(exSpouse, queriedHero);
          }
        }
      }
      foreach (Hero spouse in ((IEnumerable<Hero>) baseHero.ExSpouses).Where<Hero>((Func<Hero, bool>) (x => x.IsAlive)).ToList<Hero>())
      {
        if (spouse.Spouse == queriedHero)
          return GetHeroRelationToHeroTextShortPatch.SpousesSpouse(spouse, queriedHero);
        foreach (Hero hero in ((IEnumerable<Hero>) spouse.ExSpouses).Where<Hero>((Func<Hero, bool>) (x => x.IsAlive)).ToList<Hero>())
        {
          if (hero == queriedHero)
            return GetHeroRelationToHeroTextShortPatch.SpousesSpouse(spouse, queriedHero);
        }
      }
      return TextObject.Empty;
    }

    private static TextObject SpousesSpouse(Hero spouse, Hero queriedHero) => !spouse.IsFemale ? (!queriedHero.IsFemale ? (!Helper.MASettings.Polyamory ? GetHeroRelationToHeroTextShortPatch.FindText("str_husbands_husband") : GetHeroRelationToHeroTextShortPatch.FindText("str_husband")) : (!Helper.MASettings.Polyamory ? GetHeroRelationToHeroTextShortPatch.FindText("str_husbands_wife") : GetHeroRelationToHeroTextShortPatch.FindText("str_wife"))) : (!queriedHero.IsFemale ? (!Helper.MASettings.Polyamory ? GetHeroRelationToHeroTextShortPatch.FindText("str_wifes_husband") : GetHeroRelationToHeroTextShortPatch.FindText("str_husband")) : (!Helper.MASettings.Polyamory ? GetHeroRelationToHeroTextShortPatch.FindText("str_wifes_wife") : GetHeroRelationToHeroTextShortPatch.FindText("str_wife")));
  }
}
