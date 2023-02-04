// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Helpers.Util
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;


 
namespace MarryAnyone.Helpers
{
  internal static class Util
  {
    public static void CleanRomance(
      Hero hero,
      Hero otherHero,
      Romance.RomanceLevelEnum newRomanceLevel = 0)
    {
      Romance.RomanticState romanticState1 = (Romance.RomanticState) null;
      int num = 0;
      while (true)
      {
        Romance.RomanticState romanticState2 = ((IEnumerable<Romance.RomanticState>) Romance.RomanticStateList).FirstOrDefault<Romance.RomanticState>((Func<Romance.RomanticState, bool>) (x =>
        {
          if (x.Person1 == hero && x.Person2 == otherHero)
            return true;
          return x.Person2 == hero && x.Person1 == otherHero;
        }));
        if (romanticState2 != null)
        {
          if (newRomanceLevel != Romance.RomanceLevelEnum.Untested)
          {
            if (romanticState1 == null)
              romanticState1 = romanticState2;
            else if (romanticState2.Level == romanticState1.Level)
            {
              if ((double) romanticState2.LastVisit > (double) romanticState1.LastVisit)
                romanticState1 = romanticState2;
            }
            else if (romanticState2.Level == newRomanceLevel)
              romanticState1 = romanticState2;
            else if (romanticState1.Level != newRomanceLevel && (double) romanticState2.LastVisit > (double) romanticState1.LastVisit)
              romanticState1 = romanticState2;
          }
          Romance.RomanticStateList.Remove(romanticState2);
          ++num;
        }
        else
          break;
      }
      if (num > 0)
        Helper.Print(string.Format("Clean romances between {0} and {1} => {2} relations erased", (object) hero.Name, (object) otherHero.Name, (object) num), Helper.PrintHow.PrintToLogAndWriteAndDisplay);
      if (newRomanceLevel == Romance.RomanceLevelEnum.Untested)
        return;
      Romance.RomanticState romanticState3;
      if (romanticState1 == null)
      {
        romanticState3 = new Romance.RomanticState();
        romanticState3.Person1 = hero;
        romanticState3.Person2 = otherHero;
      }
      else
        romanticState3 = romanticState1;
      romanticState3.Level = newRomanceLevel;
      Romance.RomanticStateList.Add(romanticState3);
    }
  }
}
