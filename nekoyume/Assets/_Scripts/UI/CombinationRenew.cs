using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Nekoyume.Action;
using Nekoyume.Game;
using Nekoyume.Game.Character;
using Nekoyume.UI.ItemInfo;
using Nekoyume.UI.ItemView;
using Nekoyume.UI.Model;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Nekoyume.UI
{
    public class CombinationRenew : Widget
    {
        private Model.Combination _data;

        public InventoryRenew inventoryRenew;
        public ButtonedItemInfo selectedItemInfo;
        public CombinationStagedItemView[] stagedItems;
        public Button combinationButton;
        public Image combinationButtonImage;
        public Text combinationButtonText;
        public Button closeButton;

        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        private Stage _stage;
        private Player _player;

        private SelectItemCountPopup _selectItemCountPopup;
        private CombinationResultPopup _resultPopup;
        private LoadingScreen _loadingScreen;

        #region Mono

        private void Awake()
        {
            if (ReferenceEquals(inventoryRenew, null) ||
                ReferenceEquals(selectedItemInfo, null) ||
                ReferenceEquals(combinationButton, null) ||
                ReferenceEquals(combinationButtonImage, null) ||
                ReferenceEquals(combinationButtonText, null) ||
                ReferenceEquals(closeButton, null))
            {
                throw new SerializeFieldNullException();
            }

            combinationButton.OnClickAsObservable()
                .Subscribe(_ => _data.OnClickCombination.OnNext(_data))
                .AddTo(_disposables);

            closeButton.OnClickAsObservable()
                .Subscribe(_ => Close())
                .AddTo(_disposables);
        }

        private void OnEnable()
        {
            _stage = GameObject.Find("Stage").GetComponent<Stage>();
            if (ReferenceEquals(_stage, null))
            {
                throw new NotFoundComponentException<Stage>();
            }
        }

        private void OnDisable()
        {
            _stage = null;
            _player = null;
        }

        private void OnDestroy()
        {
            _disposables.ForEach(d => d.Dispose());
        }

        #endregion

        public override void Show()
        {
            _selectItemCountPopup = Find<SelectItemCountPopup>();
            if (ReferenceEquals(_selectItemCountPopup, null))
            {
                throw new NotFoundComponentException<SelectItemCountPopup>();
            }

            _resultPopup = Find<CombinationResultPopup>();
            if (ReferenceEquals(_resultPopup, null))
            {
                throw new NotFoundComponentException<CombinationResultPopup>();
            }

            _loadingScreen = Find<LoadingScreen>();
            if (ReferenceEquals(_loadingScreen, null))
            {
                throw new NotFoundComponentException<LoadingScreen>();
            }

            base.Show();

            _stage.LoadBackground("combination");

            _player = FindObjectOfType<Player>();
            if (ReferenceEquals(_player, null))
            {
                throw new NotFoundComponentException<Player>();
            }

            _player.gameObject.SetActive(false);

            _data = new Model.Combination(ActionManager.Instance.Avatar.Items, stagedItems.Length);
            _data.SelectedItemInfo.Value.Item.Subscribe(OnDataSelectedItemInfoItem);
            _data.SelectItemCountPopup.Value.Item.Subscribe(OnDataPopupItem);
            _data.SelectItemCountPopup.Value.OnClickClose.Subscribe(OnDataPopupOnClickClose);
            _data.StagedItems.ObserveAdd().Subscribe(OnDataStagedItemsAdd);
            _data.StagedItems.ObserveRemove().Subscribe(OnDataStagedItemsRemove);
            _data.StagedItems.ObserveReplace().Subscribe(OnDataStagedItemsReplace);
            _data.ReadyForCombination.Subscribe(SetActiveCombinationButton);
            _data.OnClickCombination.Subscribe(RequestCombination);
            _data.ResultPopup.Subscribe(SubscribeResultPopup);

            inventoryRenew.SetData(_data.Inventory.Value);
            inventoryRenew.Show();
            selectedItemInfo.SetData(_data.SelectedItemInfo.Value);
            UpdateStagedItems();
        }

        public override void Close()
        {
            _data.Dispose();

            _player.gameObject.SetActive(true);
            _stage.LoadBackground("room");

            Find<Status>()?.Show();
            Find<Menu>()?.Show();

            base.Close();
        }

        private void OnDataSelectedItemInfoItem(Model.Inventory.Item data)
        {
            if (ReferenceEquals(data, null) ||
                data.Dimmed.Value ||
                _data.IsStagedItemsFulled)
            {
                _data.SelectedItemInfo.Value.ButtonEnabled.Value = false;
            }
            else
            {
                _data.SelectedItemInfo.Value.ButtonEnabled.Value = true;
            }
        }

        private void OnDataPopupItem(Model.Inventory.Item data)
        {
            if (ReferenceEquals(data, null))
            {
                _selectItemCountPopup.Close();
                return;
            }

            _selectItemCountPopup.Pop(_data.SelectItemCountPopup.Value);
        }

        private void OnDataPopupOnClickClose(SelectItemCountPopup<Model.Inventory.Item> data)
        {
            _data.SelectItemCountPopup.Value.Item.Value = null;
            _selectItemCountPopup.Close();
        }

        private void OnDataStagedItemsAdd(CollectionAddEvent<CountEditableItem<Model.Inventory.Item>> e)
        {
            if (e.Index >= stagedItems.Length)
            {
                _data.StagedItems.RemoveAt(e.Index);
                throw new AddOutOfSpecificRangeException<CollectionAddEvent<CountEditableItem<Model.Inventory.Item>>>(
                    stagedItems.Length);
            }

            stagedItems[e.Index].SetData(e.Value);
        }

        private void OnDataStagedItemsRemove(CollectionRemoveEvent<CountEditableItem<Model.Inventory.Item>> e)
        {
            if (e.Index >= stagedItems.Length)
            {
                return;
            }

            var dataCount = _data.StagedItems.Count;
            for (var i = e.Index; i <= dataCount; i++)
            {
                var item = stagedItems[i];

                if (i < dataCount)
                {
                    item.SetData(_data.StagedItems[i]);
                }
                else
                {
                    item.Clear();
                }
            }
        }

        private void OnDataStagedItemsReplace(CollectionReplaceEvent<CountEditableItem<Model.Inventory.Item>> e)
        {
            if (ReferenceEquals(e.NewValue, null))
            {
                UpdateStagedItems();
            }
        }

        private void SetActiveCombinationButton(bool isActive)
        {
            if (isActive)
            {
                combinationButton.enabled = true;
                combinationButtonImage.sprite = Resources.Load<Sprite>("ui/button_blue_02");
            }
            else
            {
                combinationButton.enabled = false;
                combinationButtonImage.sprite = Resources.Load<Sprite>("ui/button_black_01");
            }
        }

        private IDisposable _combinationDisposable;

        private void RequestCombination(Model.Combination data)
        {
            _loadingScreen.Show();
            _combinationDisposable = Action.CombinationRenew.EndOfExecuteSubject.ObserveOnMainThread().Subscribe(ResponseCombination);
            ActionManager.Instance.Combination(_data.StagedItems.ToList());
        }

        /// <summary>
        /// 결과를 직접 받아서 데이타에 넣어주는 방법 보다는,
        /// 네트워크 결과를 핸들링하는 곳에 핸들링 인터페이스를 구현한 데이타 모델을 등록하는 방법이 좋겠다. 
        /// </summary>
        private void ResponseCombination(Action.CombinationRenew action)
        {
            _combinationDisposable.Dispose();
            
            var result = action.Result;
            if (result.ErrorCode == ActionBase.ErrorCode.Success)
            {
                var itemData = ActionManager.Instance.tables.GetItem(result.Item.Id);
                if (ReferenceEquals(itemData, null))
                {
                    _loadingScreen.Close();
                    throw new InvalidActionException("`CombinationRenew` action's `Result` is invalid.");
                }
                
                var itemModel = new Model.Inventory.Item(
                    new Game.Item.Inventory.InventoryItem(itemData, action.Result.Item.Count));

                _data.ResultPopup.Value = new CombinationResultPopup<Model.Inventory.Item>()
                {
                    IsSuccess = true,
                    ResultItem = itemModel,
                    MaterialItems = _data.StagedItems
                };
            }
            else
            {
                _data.ResultPopup.Value = new CombinationResultPopup<Model.Inventory.Item>()
                {
                    IsSuccess = false,
                    MaterialItems = _data.StagedItems
                };
            }
        }

        private void SubscribeResultPopup(CombinationResultPopup<Model.Inventory.Item> data)
        {
            if (ReferenceEquals(data, null))
            {
                _resultPopup.Close();
            }
            else
            {
                _loadingScreen.Close();
                _resultPopup.Pop(_data.ResultPopup.Value);
            }
        }

        private void UpdateStagedItems(int startIndex = 0)
        {
            var dataCount = _data.StagedItems.Count;
            for (var i = startIndex; i < stagedItems.Length; i++)
            {
                var item = stagedItems[i];
                if (i < dataCount)
                {
                    item.SetData(_data.StagedItems[i]);
                }
                else
                {
                    item.Clear();
                }
            }
        }
    }
}
