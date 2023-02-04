// Decompiled with JetBrains decompiler
// Type: MarryAnyone.Behaviors.MAPerSaveCampaignBehavior
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using MarryAnyone.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;


namespace MarryAnyone.Behaviors
{
  internal class MAPerSaveCampaignBehavior : CampaignBehaviorBase
  {
    public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
    {
      MASettings maSettings = new MASettings();
      MAConfig instance = MAConfig.Instance;
      if ((instance != null ? (instance.Warning ? 1 : 0) : 0) == 0)
        return;
      if (MASettings.NoMCMWarning)
      {
        InformationManager.ShowInquiry(new InquiryData(((object) GameTexts.FindText("str_warning", (string) null)).ToString(), ((object) GameTexts.FindText("str_no_mcm_info", (string) null)).ToString(), true, true, ((object) GameTexts.FindText("str_ok", (string) null)).ToString(), ((object) GameTexts.FindText("str_dontshowagain", (string) null)).ToString(), (Action) null, new Action(this.DontShowAgain), "", 0.0f, (Action) null), false);
      }
      else
      {
        if (!MASettings.NoConfigWarning)
          return;
        InformationManager.ShowInquiry(new InquiryData(((object) GameTexts.FindText("str_warning", (string) null)).ToString(), ((object) GameTexts.FindText("str_no_config_info", (string) null)).ToString(), true, true, ((object) GameTexts.FindText("str_ok", (string) null)).ToString(), ((object) GameTexts.FindText("str_dontshowagain", (string) null)).ToString(), (Action) null, new Action(this.DontShowAgain), "", 0.0f, (Action) null), false);
      }
    }

    private void DontShowAgain()
    {
      try
      {
        if (!(JsonConvert.DeserializeObject(File.ReadAllText(MASettings.ConfigPath)) is JObject jobject))
          return;
        jobject.SelectToken("Warning").Replace((JToken) false);
        File.WriteAllText(MASettings.ConfigPath, jobject.ToString());
      }
      catch (Exception ex)
      {
        Helper.Error(ex);
      }
    }

    public override void RegisterEvents() => CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object) this, new Action<CampaignGameStarter>(this.OnSessionLaunched));

    public override void SyncData(IDataStore dataStore)
    {
    }
  }
}
