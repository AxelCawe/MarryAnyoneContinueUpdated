// Decompiled with JetBrains decompiler
// Type: MarryAnyone.MASubModule
// Assembly: MarryAnyone, Version=3.0.5.0, Culture=neutral, PublicKeyToken=null
// MVID: A722648B-7A05-48D0-93EC-C56CB18B6830
// Assembly location: C:\Users\Caleb\Downloads\MarryAnyone.dll

using HarmonyLib;
using MarryAnyone.Behaviors;
using MarryAnyone.Patches;
using MarryAnyone.Settings;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;


namespace MarryAnyone
{
    public class MASubModule : MBSubModuleBase
    {
        public static readonly Harmony Harmony = new Harmony("MarryAnyone");
        private CampaignGameStarter _campaignGameStarter;
        internal static MASubModule Instance;

        public CampaignGameStarter GameStarter() => this._campaignGameStarter != null ? this._campaignGameStarter : throw new Exception("CampaignGameStarter not referenced");

        public MASubModule() => MASubModule.Instance = this;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            new Harmony("mod.bannerlord.anyone.marry").PatchAll();
            
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);
            if (!(game.GameType is Campaign))
                return;
            Helper.Print("Campaign", Helper.PrintHow.PrintForceDisplay);
            CampaignGameStarter campaignGameStarter = (CampaignGameStarter)gameStarter;
          
            this._campaignGameStarter = campaignGameStarter;
            this.AddBehaviors(campaignGameStarter);
        }

        public override void OnGameLoaded(Game game, object initializerObject)
        {
            base.OnGameLoaded(game, initializerObject);
            Helper.MAEtape = Helper.Etape.EtapeLoad;
        }

        public override void OnGameEnd(Game game)
        {
            if (MARomanceCampaignBehavior.Instance != null)
                MARomanceCampaignBehavior.Instance.Dispose();
            EncyclopediaHeroPageVM_allRelatedHeroesPatch.Dispose();
            MASubModule.Instance = (MASubModule)null;
            this._campaignGameStarter = (CampaignGameStarter)null;
            base.OnGameEnd(game);
        }

        public override void OnGameInitializationFinished(Game game)
        {
            base.OnGameInitializationFinished(game);
            if (!(game.GameType is Campaign))
                return;
            MASubModule.Harmony.PatchAll();
        }

        private void AddBehaviors(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddBehavior((CampaignBehaviorBase)new MAPatchBehavior());
            campaignGameStarter.AddBehavior((CampaignBehaviorBase)new MAPerSaveCampaignBehavior());
            campaignGameStarter.AddBehavior((CampaignBehaviorBase)new MARomanceCampaignBehavior());
            campaignGameStarter.AddBehavior((CampaignBehaviorBase)new MAAdoptionCampaignBehavior());
        }
    }
}
