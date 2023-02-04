// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Settings.MAConfig
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll



namespace MarryAnyone.Settings
{
  internal class MAConfig : ISettingsProvider
  {
    public static MAConfig Instance;

    public bool Polygamy { get; set; }

    public bool Polyamory { get; set; }

    public bool Incest { get; set; }

    public bool Cheating { get; set; }

    public bool Debug { get; set; }

    public bool Warning { get; set; } = true;

    public string Difficulty { get; set; } = "Easy";

    public string SexualOrientation { get; set; } = "Heterosexual";

    public bool Adoption { get; set; } = true;

    public float AdoptionChance { get; set; } = 0.05f;

    public bool AdoptionTitles { get; set; }

    public bool RetryCourtship { get; set; }

    public bool SpouseJoinArena { get; set; } = true;

    public int RelationLevelMinForRomance { get; set; } = 5;

    public int RelationLevelMinForCheating { get; set; } = 10;

    public int RelationLevelMinForSex { get; set; } = 10;

    public bool ImproveRelation { get; set; } = true;

    public bool ImproveBattleRelation { get; set; } = true;

    public bool CanJoinUpperClanThroughMAPath { get; set; }

    public bool NotifyRelationImprovementWithinFamily { get; set; }

    public bool Notable { get; set; }

    public bool Patch { get; set; }

    public int PatchMaxWanderer { get; set; } = -1;
  }
}
