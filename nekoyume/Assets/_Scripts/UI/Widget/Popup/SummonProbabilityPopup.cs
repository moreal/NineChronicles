﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nekoyume.Helper;
using Nekoyume.TableData;
using Nekoyume.TableData.Summon;
using Nekoyume.UI.Scroller;
using TMPro;
using UnityEngine;

namespace Nekoyume.UI
{
    using UniRx;
    public class SummonProbabilityPopup : PopupWidget
    {
        [SerializeField]
        private SummonDetailScroll scroll;

        [SerializeField]
        private TextMeshProUGUI titleText;

        [SerializeField]
        private GameObject silverDustIconObj;

        [SerializeField]
        private GameObject goldDustIconObj;

        [SerializeField]
        private GameObject rubyDustIconObj;

        [SerializeField]
        private GameObject emeraldDustIconObj;

        private readonly List<IDisposable> _disposables = new();
        private readonly Dictionary<CostType, GameObject> _dustObjectDict = new();

        protected override void Awake()
        {
            base.Awake();
            _dustObjectDict[CostType.SilverDust] = silverDustIconObj;
            _dustObjectDict[CostType.GoldDust] = goldDustIconObj;
            _dustObjectDict[CostType.RubyDust] = rubyDustIconObj;
            _dustObjectDict[CostType.EmeraldDust] = emeraldDustIconObj;
        }

        public void Show(SummonResult summonResult)
        {
            titleText.SetText(summonResult.ToString());
            scroll.ContainedCostType.Clear();
            foreach (var dustObj in _dustObjectDict.Values)
            {
                dustObj.SetActive(false);
            }

            var summonRows = SummonFrontHelper.GetSummonRowsBySummonResult(summonResult);
            var tableSheets = Game.Game.instance.TableSheets;
            var equipmentItemSheet = tableSheets.EquipmentItemSheet;
            var equipmentItemRecipeSheet = tableSheets.EquipmentItemRecipeSheet;
            var runeSheet = tableSheets.RuneSheet;
            var equipmentItemSubRecipeSheet = tableSheets.EquipmentItemSubRecipeSheetV2;
            var modelDict = new Dictionary<int,SummonDetailCell.Model>();
            foreach (var row in summonRows)
            {
                float ratioSum = row.Recipes.Sum(pair => pair.Item2);
                foreach (var (recipeId, ratio) in row.Recipes.Where(pair => pair.Item1 > 0))
                {
                    EquipmentItemSheet.Row equipmentRow = null;
                    List<EquipmentItemSubRecipeSheetV2.OptionInfo> equipmentOptions = null;
                    if (equipmentItemRecipeSheet.TryGetValue(recipeId, out var recipeRow))
                    {
                        equipmentRow = equipmentItemSheet[recipeRow.ResultEquipmentId];
                        equipmentOptions = equipmentItemSubRecipeSheet[recipeRow.SubRecipeIds[0]]
                            .Options;
                    }

                    string runeTicker = null;
                    RuneOptionSheet.Row.RuneOptionInfo runeOptionInfo = null;
                    if (runeSheet.TryGetValue(recipeId, out var runeRow))
                    {
                        runeTicker = runeRow.Ticker;

                        var runeOptionSheet = tableSheets.RuneOptionSheet;
                        if (runeOptionSheet.TryGetValue(runeRow.Id, out var runeOptionRow))
                        {
                            runeOptionRow.LevelOptionMap.TryGetValue(1, out runeOptionInfo);
                        }
                    }

                    var costType = (CostType)row.CostMaterial;
                    _dustObjectDict[costType].SetActive(true);
                    scroll.ContainedCostType.Add(costType);
                    if (modelDict.TryGetValue(recipeId, out var model))
                    {
                        var cellRatio = ratio / ratioSum;
                        switch (costType)
                        {
                            case CostType.GoldDust:
                                model.GoldRatio = cellRatio;
                                break;
                            case CostType.RubyDust:
                                model.RubyRatio = cellRatio;
                                break;
                            case CostType.EmeraldDust:
                                model.EmeraldRatio = cellRatio;
                                break;
                            case CostType.SilverDust:
                                model.SilverRatio = cellRatio;
                                break;
                        }
                    }
                    else
                    {
                        var cellRatio = ratio / ratioSum;
                        var grade = equipmentRow?.Grade ?? Util.GetTickerGrade(runeTicker);
                        var cellModel = new SummonDetailCell.Model
                        {
                            EquipmentRow = equipmentRow,
                            EquipmentOptions = equipmentOptions,
                            RuneTicker = runeTicker,
                            RuneOptionInfo = runeOptionInfo,
                            Grade = grade,
                        };
                        switch (costType)
                        {
                            case CostType.GoldDust:
                                cellModel.GoldRatio = cellRatio;
                                break;
                            case CostType.RubyDust:
                                cellModel.RubyRatio = cellRatio;
                                break;
                            case CostType.EmeraldDust:
                                cellModel.EmeraldRatio = cellRatio;
                                break;
                            case CostType.SilverDust:
                                cellModel.SilverRatio = cellRatio;
                                break;
                        }

                        modelDict.Add(recipeId, cellModel);
                    }
                }
            }

            _disposables.DisposeAllAndClear();
            scroll.UpdateData(modelDict.Values.OrderByDescending(model => model.Grade), true);
            scroll.OnClickDetailButton.Subscribe(Find<SummonDetailPopup>().Show)
                .AddTo(_disposables);

            base.Show();
        }
    }
}
