// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Settings.MCMSettings
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using Bannerlord.BUTR.Shared.Helpers;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.PerSave;
using MCM.Common;
using System.Collections.Generic;
using TaleWorlds.Localization;


namespace MarryAnyone.Settings
{
  internal class MCMSettings : AttributePerSaveSettings<MCMSettings>, ISettingsProvider
  {
    public override string Id { get; } = "MarryAnyone_v2";

    public override string DisplayName
    {
      get
      {
        Dictionary<string, object> attributes = new Dictionary<string, object>();
        attributes.Add("VERSION", new TextObject(Helper.VersionGet.ToString(3), (Dictionary<string, object>) null));
        return ((object) new TextObject("{=marryanyone}Marry Anyone {VERSION}", attributes))?.ToString() ?? "ERROR";
      }
    }

    [SettingPropertyDropdown("{=difficulty}Difficulty", HintText = "{=difficulty_desc}Very Easy - no mini-game | Easy - mini-game nobles only | Realistic - mini-game all", Order = 0, RequireRestart = false)]
    [SettingPropertyGroup("{=general}General")]
    public Dropdown<string> DifficultyDropdown { get; set; } = new Dropdown<string>((IEnumerable<string>) new string[3]
    {
      "Very Easy",
      "Easy",
      "Realistic"
    }, 1);

    [SettingPropertyDropdown("{=orientation}Sexual Orientation", HintText = "{=orientation_desc}Player character can choose what gender the player can marry", Order = 1, RequireRestart = false)]
    [SettingPropertyGroup("{=general}General")]
    public Dropdown<string> SexualOrientationDropdown { get; set; } = new Dropdown<string>((IEnumerable<string>) new string[3]
    {
      "Heterosexual",
      "Homosexual",
      "Bisexual"
    }, 0);

    [SettingPropertyBool("{=cheating}Cheating", HintText = "{=cheating_desc}Player character can enrol any character in his party and have sexual relation with", RequireRestart = false)]
    [SettingPropertyGroup("{=relationship}Relationship Options")]
    public bool Cheating { get; set; }

    [SettingPropertyBool("{=polygamy}Polygamy", HintText = "{=polygamy_desc}Player character can have polygamous relationships", Order = 1, RequireRestart = false)]
    [SettingPropertyGroup("{=relationship}Relationship Options")]
    public bool Polygamy { get; set; }

    [SettingPropertyBool("{=polyamory}Polyamory", HintText = "{=polyamory_desc}Player character's spouses can have relationships with each other", Order = 2, RequireRestart = false)]
    [SettingPropertyGroup("{=relationship}Relationship Options")]
    public bool Polyamory { get; set; }

    [SettingPropertyBool("{=incest}Incest", HintText = "{=incest_desc}Player character can have incestuous relationships", Order = 3, RequireRestart = false)]
    [SettingPropertyGroup("{=relationship}Relationship Options")]
    public bool Incest { get; set; }

    [SettingPropertyBool("{=notable}With notable", HintText = "{=notable_desc}Player character can marry notable", Order = 4, RequireRestart = false)]
    [SettingPropertyGroup("{=relationship}Relationship Options")]
    public bool Notable { get; set; }

    [SettingPropertyBool("{=ImproveRelation}Improve relation", HintText = "{=ImproveRelation_desc}Improve relation when heroes have sexual relation", Order = 5, RequireRestart = false)]
    [SettingPropertyGroup("{=relationship}Relationship Options")]
    public bool ImproveRelation { get; set; }

    [SettingPropertyBool("{=CanJoinUpperClanThroughMAPath}Can join upper clan through MA Path", HintText = "{=CanJoinUpperClanThroughMAPath_desc}Can join upper clan through MA Path (Not compatible with Calradia Expanded)", Order = 6, RequireRestart = false)]
    [SettingPropertyGroup("{=relationship}Relationship Options")]
    public bool CanJoinUpperClanThroughMAPath { get; set; }

    [SettingPropertyInteger("{=RelationLevelMinForRomance}Relation needed for romance", -1, 100, "0", HintText = "{=RelationLevelMinForRomance_desc}Relation needed to start a romance (-1 disable the control)", Order = 10, RequireRestart = false)]
    [SettingPropertyGroup("{=relationship}Relationship Options")]
    public int RelationLevelMinForRomance { get; set; } = 5;

    [SettingPropertyInteger("{=RelationLevelMinForCheating}Relation needed for a cheating relation", -1, 100, "0", HintText = "{=RelationLevelMinForCheating_desc}Relation needed for a cheating relation (-1 disable the control)", Order = 11, RequireRestart = false)]
    [SettingPropertyGroup("{=relationship}Relationship Options")]
    public int RelationLevelMinForCheating { get; set; } = 10;

    [SettingPropertyInteger("{=RelationLevelMinForSex}Relation needed for sexual relation", -1, 100, "0", HintText = "{=RelationLevelMinForSex_desc}Relation needed for sexual relation (-1 disable the control)", Order = 12, RequireRestart = false)]
    [SettingPropertyGroup("{=relationship}Relationship Options")]
    public int RelationLevelMinForSex { get; set; } = 10;

    [SettingPropertyBool("{=retry_courtship}Retry Courtship", HintText = "{=retry_courtship_desc}Player can retry courtship after failure", RequireRestart = false)]
    [SettingPropertyGroup("{=courtship}Courtship", GroupOrder = 1)]
    public bool RetryCourtship { get; set; }

    public string Difficulty
    {
      get => this.DifficultyDropdown.SelectedValue;
      set => this.DifficultyDropdown.SelectedValue = value;
    }

    public string SexualOrientation
    {
      get => this.SexualOrientationDropdown.SelectedValue;
      set => this.SexualOrientationDropdown.SelectedValue = value;
    }

    [SettingPropertyBool("{=spousejoinarena}Spouse(s) join arena", HintText = "{=spousejoinarena_desc}Spouse will join arena with you", Order = 1, RequireRestart = false)]
    [SettingPropertyGroup("{=Side}Side Options", GroupOrder = 2)]
    public bool SpouseJoinArena { get; set; }

    [SettingPropertyBool("{=improvebattlerelation}Improve (or not) relation during battle", HintText = "{=improvebattlerelation_desc}Spouse(s) and other heroes improve relation during battle", Order = 2, RequireRestart = false)]
    [SettingPropertyGroup("{=Side}Side Options", GroupOrder = 2)]
    public bool ImproveBattleRelation { get; set; }

    [SettingPropertyBool("{=adoption}Adoption", HintText = "{=adoption_desc}Player can adopt children in towns and villages", IsToggle = true, RequireRestart = false)]
    [SettingPropertyGroup("{=adoption}Adoption", GroupOrder = 3)]
    public bool Adoption { get; set; }

    [SettingPropertyFloatingInteger("{=adoption_chance}Adoption Chance", 0.0f, 1f, "#0%", HintText = "{=adoption_chance_desc}Chance that a child is up for adoption", RequireRestart = false)]
    [SettingPropertyGroup("{=adoption}Adoption", GroupOrder = 3)]
    public float AdoptionChance { get; set; } = 0.05f;

    [SettingPropertyBool("{=adoption_titles}Adoption Titles", HintText = "{=adoption_titles_desc}Encyclopedia displays children without a parent as adopted", RequireRestart = false)]
    [SettingPropertyGroup("{=adoption}Adoption", GroupOrder = 3)]
    public bool AdoptionTitles { get; set; }

    [SettingPropertyBool("{=NotifyRelationImprovementWithinFamily}Notify relation improvement in your family", HintText = "{=NotifyRelationImprovementWithinFamily_desc}Display relation improvement in your family in the game's message log", Order = 1, RequireRestart = false)]
    [SettingPropertyGroup("{=Notification}Notification", GroupOrder = 4)]
    public bool NotifyRelationImprovementWithinFamily { get; set; }

    [SettingPropertyBool("{=debug}Debug", HintText = "{=debug_desc}Displays mod developer debug information in the game's message log", Order = 2, RequireRestart = false)]
    [SettingPropertyGroup("{=Notification}Notification", GroupOrder = 4)]
    public bool Debug { get; set; }

    [SettingPropertyBool("{=patchMAOnLoad}Patch MA on load", HintText = "{=patch_desc}Save and load the save to apply the patch again", Order = 1, RequireRestart = false)]
    [SettingPropertyGroup("{=Patch}Patch", GroupOrder = 5)]
    public bool Patch { get; set; }

    [SettingPropertyInteger("{=PatchMaxWandererInTavern}Remove wanderer in tavern if more then ", -1, 100, "0", HintText = "{=PatchMaxWandererInTavern_desc}Remove wanderer when loading in tavern if more then (-1 disable the patch)", Order = 2, RequireRestart = false)]
    [SettingPropertyGroup("{=Patch}Patch", GroupOrder = 5)]
    public int PatchMaxWanderer { get; set; } = -1;
  }
}
