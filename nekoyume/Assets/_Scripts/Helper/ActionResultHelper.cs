﻿using System;
using System.Collections.Generic;
using Nekoyume.Action;
using Nekoyume.Battle;
using Nekoyume.BlockChain;
using Nekoyume.Game;
using Nekoyume.Model.State;
using Nekoyume.UI;
using Nekoyume.UI.Model;

namespace Nekoyume.Helper
{
    public static class ActionResultHelper
    {
        public static BattleResultPopup.Model GetHackAndSlashReward(this ActionBase.ActionEvaluation<HackAndSlash> eval,
            AvatarState avatarState,
            List<Model.Skill.Skill> skillsOnWaveStart,
            TableSheets sheets,
            out StageSimulatorV1 firstStageSimulator)
        {
            firstStageSimulator = null;
            var model = new BattleResultPopup.Model();
            var random = new ActionRenderHandler.LocalRandom(eval.RandomSeed);
            for (var i = 0; i < eval.Action.PlayCount; i++)
            {
                var prevExp = avatarState.exp;
                var simulator = new StageSimulatorV1(
                    random,
                    avatarState,
                    i == 0 ? eval.Action.Foods : new List<Guid>(),
                    i == 0 ? skillsOnWaveStart : new List<Model.Skill.Skill>(),
                    eval.Action.WorldId,
                    eval.Action.StageId,
                    sheets.GetStageSimulatorSheets(),
                    sheets.CostumeStatSheet,
                    StageSimulatorV1.ConstructorVersionV100080);
                simulator.Simulate(1);

                if (simulator.Log.IsClear)
                {
                    simulator.Player.worldInformation.ClearStage(
                        eval.Action.WorldId,
                        eval.Action.StageId,
                        eval.BlockIndex,
                        sheets.WorldSheet,
                        sheets.WorldUnlockSheet
                    );
                }

                avatarState.Update(simulator);
                firstStageSimulator ??= simulator;
                model.Exp += simulator.Player.Exp.Current - prevExp;
                model.ClearedWaves[simulator.Log.clearedWaveNumber]++;
                foreach (var itemBase in simulator.Reward)
                {
                    model.AddReward(new CountableItem(itemBase, 1));
                }
            }

            return model;
        }
    }
}
