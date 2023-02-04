// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Models.MARomanceModel
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;


 
namespace MarryAnyone.Models
{
  public class MARomanceModel : DefaultRomanceModel
  {
    public static bool CourtshipPossibleBetweenNPCsStatic(Hero person1, Hero person2) => (person1.MapFaction == null || person2.MapFaction == null || !FactionManager.IsAtWarAgainstFaction(person1.MapFaction, person2.MapFaction)) && Campaign.Current.Models.MarriageModel.IsCoupleSuitableForMarriage(person1, person2);
  }
}
