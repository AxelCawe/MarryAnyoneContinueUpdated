
using MCM.Abstractions.Base.PerSave;
using MCM.Common;
using Newtonsoft.Json;
using System;
using System.IO;
using TaleWorlds.Library;


namespace MarryAnyone.Settings
{
  internal class MASettings : ISettingsProvider
  {
    private const string CONFIG_FILE = "config.json";
    private const string USER_PATH_FOR_CONFIG = "/Mount and Blade II Bannerlord/Configs/ModSettings/MarryAnyone";
    private const string GAME_PATH_CONFIG = "Modules/MarryAnyoneContinueUpdated/config.json";
    public const string DIFFICULTY_VERY_EASY = "Very Easy";
    public const string DIFFICULTY_EASY = "Easy";
    public static bool UsingMCM;
    public static bool NoMCMWarning;
    public static bool NoConfigWarning;
    private readonly ISettingsProvider _provider;

    private static string ConfigPathUser => Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Mount and Blade II Bannerlord/Configs/ModSettings/MarryAnyone";

    private static void CopyConfig()
    {
      string configPathUser = MASettings.ConfigPathUser;
      string str1 = configPathUser + "/config.json";
      string str2 = BasePath.Name + "Modules/MarryAnyoneContinueUpdated/config.json";
      if (!File.Exists(str2))
        throw new Exception(string.Format("File {0} not found !", (object) str2));
      Directory.CreateDirectory(configPathUser);
      File.Copy(str2, str1);
      if (!File.Exists(str1))
        throw new Exception(string.Format("File {0} not found !", (object) str2));
    }

    public static string ConfigPath
    {
      get
      {
        string path = MASettings.ConfigPathUser + "/config.json";
        if (!File.Exists(path))
          MASettings.CopyConfig();
        return path;
      }
    }

    public bool Incest
    {
      get => this._provider.Incest;
      set => this._provider.Incest = value;
    }

    public bool Polygamy
    {
      get => this._provider.Polygamy;
      set => this._provider.Polygamy = value;
    }

    public bool Polyamory
    {
      get => this._provider.Polyamory;
      set => this._provider.Polyamory = value;
    }

    public bool Cheating
    {
      get => this._provider.Cheating;
      set => this._provider.Cheating = value;
    }

    public bool Notable
    {
      get => this._provider.Notable;
      set => this._provider.Notable = value;
    }

    public bool Debug
    {
      get => this._provider.Debug;
      set => this._provider.Debug = value;
    }

    public string Difficulty
    {
      get => this._provider.Difficulty;
      set => this._provider.Difficulty = value;
    }

    public string SexualOrientation
    {
      get => this._provider.SexualOrientation;
      set => this._provider.SexualOrientation = value;
    }

    public bool Adoption
    {
      get => this._provider.Adoption;
      set => this._provider.Adoption = value;
    }

    public float AdoptionChance
    {
      get => this._provider.AdoptionChance;
      set => this._provider.AdoptionChance = value;
    }

    public bool AdoptionTitles
    {
      get => this._provider.AdoptionTitles;
      set => this._provider.AdoptionTitles = value;
    }

    public bool RetryCourtship
    {
      get => this._provider.RetryCourtship;
      set => this._provider.RetryCourtship = value;
    }

    public bool SpouseJoinArena
    {
      get => this._provider.SpouseJoinArena;
      set => this._provider.SpouseJoinArena = value;
    }

    public int RelationLevelMinForRomance
    {
      get => this._provider.RelationLevelMinForRomance;
      set => this._provider.RelationLevelMinForRomance = value;
    }

    public int RelationLevelMinForCheating
    {
      get => this._provider.RelationLevelMinForCheating;
      set => this._provider.RelationLevelMinForCheating = value;
    }

    public int RelationLevelMinForSex
    {
      get => this._provider.RelationLevelMinForSex;
      set => this._provider.RelationLevelMinForSex = value;
    }

    public bool ImproveRelation
    {
      get => this._provider.ImproveRelation;
      set => this._provider.ImproveRelation = value;
    }

    public bool ImproveBattleRelation
    {
      get => this._provider.ImproveBattleRelation;
      set => this._provider.ImproveBattleRelation = value;
    }

    public bool CanJoinUpperClanThroughMAPath
    {
      get => this._provider.CanJoinUpperClanThroughMAPath;
      set => this._provider.CanJoinUpperClanThroughMAPath = value;
    }

    public bool NotifyRelationImprovementWithinFamily
    {
      get => this._provider.NotifyRelationImprovementWithinFamily;
      set => this._provider.NotifyRelationImprovementWithinFamily = value;
    }

    public bool DifficultyEasyMode => string.Equals(this._provider.Difficulty, "Easy", StringComparison.OrdinalIgnoreCase);

    public bool DifficultyVeryEasyMode => string.Equals(this._provider.Difficulty, "Very Easy", StringComparison.OrdinalIgnoreCase);

    public bool DifficultyNormalMode => this._provider.Difficulty == null || !this._provider.Difficulty.EndsWith("Easy", StringComparison.OrdinalIgnoreCase);

    public bool Patch
    {
      get => this._provider.Patch;
      set => this._provider.Patch = value;
    }

    public int PatchMaxWanderer
    {
      get => this._provider.PatchMaxWanderer;
      set => this._provider.PatchMaxWanderer = value;
    }

    public MASettings()
    {
      MCMSettings instance = PerSaveSettings<MCMSettings>.Instance;
      if (instance != null)
      {
        this._provider = (ISettingsProvider) instance;
        MASettings.NoMCMWarning = MASettings.NoConfigWarning = false;
        MASettings.UsingMCM = true;
      }
      else
      {
        MASettings.UsingMCM = false;
        MAConfig.Instance = new MAConfig();
        bool flag = false;
        if (File.Exists(MASettings.ConfigPath))
        {
          while (true)
          {
            try
            {
              MAConfig maConfig = JsonConvert.DeserializeObject<MAConfig>(File.ReadAllText(MASettings.ConfigPath));
              MAConfig.Instance.Polygamy = maConfig.Polygamy;
              MAConfig.Instance.Polyamory = maConfig.Polyamory;
              MAConfig.Instance.Incest = maConfig.Incest;
              MAConfig.Instance.Cheating = maConfig.Cheating;
              MAConfig.Instance.Notable = maConfig.Notable;
              MAConfig.Instance.Debug = maConfig.Debug;
              MAConfig.Instance.SpouseJoinArena = maConfig.SpouseJoinArena;
              MAConfig.Instance.Warning = maConfig.Warning;
              MAConfig.Instance.Difficulty = maConfig.Difficulty;
              MAConfig.Instance.SexualOrientation = maConfig.SexualOrientation;
              MAConfig.Instance.Adoption = maConfig.Adoption;
              MAConfig.Instance.AdoptionChance = maConfig.AdoptionChance;
              MAConfig.Instance.AdoptionTitles = maConfig.AdoptionTitles;
              MAConfig.Instance.RetryCourtship = maConfig.RetryCourtship;
              MAConfig.Instance.RelationLevelMinForCheating = maConfig.RelationLevelMinForCheating;
              MAConfig.Instance.RelationLevelMinForRomance = maConfig.RelationLevelMinForRomance;
              MAConfig.Instance.RelationLevelMinForSex = maConfig.RelationLevelMinForSex;
              MAConfig.Instance.ImproveRelation = maConfig.ImproveRelation;
              MAConfig.Instance.ImproveBattleRelation = maConfig.ImproveBattleRelation;
              MAConfig.Instance.NotifyRelationImprovementWithinFamily = maConfig.NotifyRelationImprovementWithinFamily;
              MAConfig.Instance.CanJoinUpperClanThroughMAPath = maConfig.CanJoinUpperClanThroughMAPath;
              MAConfig.Instance.Patch = maConfig.Patch;
              MAConfig.Instance.PatchMaxWanderer = maConfig.PatchMaxWanderer;
              MASettings.NoMCMWarning = true;
              MASettings.NoConfigWarning = false;
              break;
            }
            catch (Exception ex)
            {
              if (flag)
              {
                Helper.Error(ex);
                break;
              }
            }
            flag = true;
            MASettings.CopyConfig();
          }
        }
        else
        {
          MASettings.NoConfigWarning = true;
          MASettings.NoMCMWarning = false;
        }
        this._provider = (ISettingsProvider) MAConfig.Instance;
      }
    }
  }
}
