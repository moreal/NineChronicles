﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nekoyume.Model.Item;
using Nekoyume.TableData;
using Nekoyume.UI.Model;
using Nekoyume.UI.Module;
using UniRx;
using UnityEngine;

namespace Nekoyume.UI
{
    public class MonsterCollectionRewardsPopup : PopupWidget
    {
        private class Model
        {
            public readonly ReactiveProperty<List<StakingRewardSheet.RewardInfo>> RewardInfos
                = new ReactiveProperty<List<StakingRewardSheet.RewardInfo>>();
        }

        private readonly Model _model = new Model();
        
        // View
        [SerializeField]
        private List<SimpleCountableItemView> itemViews;
        
        [SerializeField]
        private SubmitButton submitButton;
        // ~View

        public IObservable<MonsterCollectionRewardsPopup> OnClickSubmit => submitButton.OnSubmitClick.Select(_ => this);

        protected override void Awake()
        {
            base.Awake();
            _model.RewardInfos.Subscribe(rewardInfos =>
            {
                for (var i = 0; i < itemViews.Count; i++)
                {
                    var itemView = itemViews[i];
                    if (rewardInfos is null ||
                        rewardInfos.Count <= i)
                    {
                        itemView.Hide();
                        continue;
                    }

                    var rewardInfo = rewardInfos[i];
                    var itemRow = Game.Game.instance.TableSheets.MaterialItemSheet.OrderedList.FirstOrDefault(e =>
                        e.Id.Equals(rewardInfo.ItemId));
                    if (itemRow is null)
                    {
                        Debug.LogWarning($"ItemId({rewardInfo.ItemId}) does not exist in MaterialItemSheet.");
                        itemView.Clear();
                        continue;
                    }
                    
                    var item = ItemFactory.CreateTradableMaterial(itemRow);
                    var data = new CountableItem(item, rewardInfo.Quantity);
                    itemView.SetData(data);
                    itemView.Show();
                }
            }).AddTo(gameObject);
            
            SubmitWidget = () => submitButton.OnSubmitClick.OnNext(submitButton);
        }

        public void Pop(List<StakingRewardSheet.RewardInfo> rewardInfos)
        {
            SetData(rewardInfos);
            base.Show();
        }

        private void SetData(List<StakingRewardSheet.RewardInfo> rewardInfos)
        {
            _model.RewardInfos.Value = rewardInfos;
        }

        public void Clear() => SetData(null);
    }
}
