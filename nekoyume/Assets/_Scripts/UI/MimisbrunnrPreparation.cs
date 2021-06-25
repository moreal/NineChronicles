using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nekoyume.Battle;
using Nekoyume.BlockChain;
using Nekoyume.Game;
using Nekoyume.Game.Controller;
using Nekoyume.Model.BattleStatus;
using Nekoyume.Model.Item;
using Nekoyume.Model.Stat;
using Nekoyume.Model.State;
using Nekoyume.State;
using Nekoyume.UI.Model;
using Nekoyume.UI.Module;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using mixpanel;
using Nekoyume.Helper;
using Nekoyume.L10n;
using Nekoyume.Model.Elemental;
using Nekoyume.Model.Mail;
using Nekoyume.TableData;
using Toggle = Nekoyume.UI.Module.Toggle;

namespace Nekoyume.UI
{
    using UniRx;

    public class MimisbrunnrPreparation : Widget
    {
        private static readonly Color BattleStartButtonOriginColor = Color.white;
        private static readonly Color RequiredActionPointOriginColor = ColorHelper.HexToColorRGB("FFF3D4");
        private static readonly Color DimmedColor = ColorHelper.HexToColorRGB("848484");

        [SerializeField]
        private Module.Inventory inventory = null;

        [SerializeField]
        private TextMeshProUGUI consumableTitleText = null;

        [SerializeField]
        private EquipmentSlot[] consumableSlots = null;

        [SerializeField]
        private DetailedStatView[] statusRows = null;

        [SerializeField]
        private TextMeshProUGUI equipmentTitleText = null;

        [SerializeField]
        private EquipmentSlots equipmentSlots = null;

        [SerializeField]
        private Button startButton = null;

        [SerializeField]
        private SpriteRenderer[] startButtonImages = null;

        [SerializeField]
        private GameObject equipSlotGlow = null;

        [SerializeField]
        private TextMeshProUGUI requiredPointText = null;

        [SerializeField]
        private ParticleSystem[] particles = null;

        [SerializeField]
        private TMP_InputField levelField = null;

        [SerializeField]
        private Button simulateButton = null;

        [Header("ItemMoveAnimation")]
        [SerializeField]
        private Image actionPointImage = null;

        [SerializeField]
        private Transform buttonStarImageTransform = null;

        [SerializeField]
        private Toggle repeatToggle;

        [SerializeField, Range(.5f, 3.0f)]
        private float animationTime = 1f;

        [SerializeField]
        private bool moveToLeft = false;

        [SerializeField, Range(0f, 10f),
         Tooltip("Gap between start position X and middle position X")]
        private float middleXGap = 1f;

        private Stage _stage;

        private Game.Character.Player _player;

        private EquipmentSlot _weaponSlot;

        private readonly IntReactiveProperty _stageId = new IntReactiveProperty(GameConfig.MimisbrunnrWorldId);

        private int _requiredCost;

        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        private CharacterStats _tempStats;

        private bool _reset = true;

        public int StageId
        {
            set => _stageId.SetValueAndForceNotify(value);
        }

        // NOTE: questButton을 클릭한 후에 esc키를 눌러서 월드맵으로 벗어나는 것을 막는다.
        // 행동력이 _requiredCost 미만일 경우 퀘스트 버튼이 비활성화되므로 임시 방편으로 행동력도 비교함.
        public override bool CanHandleInputEvent =>
            base.CanHandleInputEvent &&
            (startButton.interactable || !CanStartBattle);

        private bool EnoughActionPoint =>
            States.Instance.CurrentAvatarState.actionPoint >= _requiredCost;

        private bool CanStartBattle => EnoughActionPoint && CheckEquipmentElementalType();

        #region override

        protected override void Awake()
        {
            base.Awake();

            CloseWidget = null;
            simulateButton.gameObject.SetActive(GameConfig.IsEditor);
            levelField.gameObject.SetActive(GameConfig.IsEditor);
        }

        public override void Initialize()
        {
            base.Initialize();

            _weaponSlot = equipmentSlots.First(es => es.ItemSubType == ItemSubType.Weapon);

            inventory.SharedModel.DimmedFunc.Value = inventoryItem =>
                inventoryItem.ItemBase.Value.ItemType == ItemType.Costume ||
                inventoryItem.ItemBase.Value.ItemType == ItemType.Material;
            inventory.SharedModel.SelectedItemView
                .Subscribe(SubscribeInventorySelectedItem)
                .AddTo(gameObject);
            inventory.OnDoubleClickItemView
                .Subscribe(itemView =>
                {
                    if (itemView is null ||
                        itemView.Model is null ||
                        itemView.Model.Dimmed.Value)
                    {
                        return;
                    }

                    Equip(itemView.Model);
                })
                .AddTo(gameObject);
            inventory.OnResetItems.Subscribe(SubscribeInventoryResetItems).AddTo(gameObject);

            _stageId.Subscribe(SubscribeStage).AddTo(gameObject);

            startButton.OnClickAsObservable().Subscribe(_ => BattleClick(repeatToggle.isOn)).AddTo(gameObject);

            Game.Event.OnRoomEnter.AddListener(b => Close());

            foreach (var slot in equipmentSlots)
            {
                slot.ShowUnlockTooltip = true;
            }

            foreach (var slot in consumableSlots)
            {
                slot.ShowUnlockTooltip = true;
            }
        }

        public override void Show(bool ignoreShowAnimation = false)
        {
            base.Show(ignoreShowAnimation);
            inventory.SharedModel.State.Value = ItemType.Equipment;

            consumableTitleText.text = L10nManager.Localize("UI_EQUIP_CONSUMABLES");
            equipmentTitleText.text = L10nManager.Localize("UI_EQUIP_EQUIPMENTS");

            Mixpanel.Track("Unity/Click Stage");
            _stage = Game.Game.instance.Stage;
            _stage.repeatStage = false;
            repeatToggle.isOn = false;
            repeatToggle.interactable = true;
            _stage.LoadBackground("dungeon_02");
            _player = _stage.GetPlayer(_stage.questPreparationPosition);
            var currentAvatarState = Game.Game.instance.States.CurrentAvatarState;
            _player.Set(currentAvatarState);

            if (_player is null)
            {
                throw new NotFoundComponentException<Game.Character.Player>();
            }

            if (_reset)
            {
                _reset = false;

                // stop run immediately.
                _player.gameObject.SetActive(false);
                _player.gameObject.SetActive(true);
                _player.SpineController.Appear();

                equipmentSlots.SetPlayerEquipments(_player.Model, ShowTooltip, Unequip);
                // 인벤토리 아이템의 장착 여부를 `equipmentSlots`의 상태를 바탕으로 설정하기 때문에 `equipmentSlots.SetPlayer()`를 호출한 이후에 인벤토리 아이템의 장착 상태를 재설정한다.
                // 또한 인벤토리는 기본적으로 `OnEnable()` 단계에서 `OnResetItems` 이벤트를 일으키기 때문에 `equipmentSlots.SetPlayer()`와 호출 순서 커플링이 생기게 된다.
                // 따라서 강제로 상태를 설정한다.
                SubscribeInventoryResetItems(inventory);

                foreach (var consumableSlot in consumableSlots)
                {
                    consumableSlot.Set(_player.Level);
                }

                var costumeStatSheet = Game.Game.instance.TableSheets.CostumeStatSheet;
                _player.Model.SetCostumeStat(costumeStatSheet);
                var tuples = _player.Model.Stats.GetBaseAndAdditionalStats();
                var idx = 0;
                foreach (var (statType, value, additionalValue) in tuples)
                {
                    var info = statusRows[idx];
                    info.Show(statType, value, additionalValue);
                    ++idx;
                }
            }

            Find<BottomMenu>().Show(
                UINavigator.NavigationType.Back,
                SubscribeBackButtonClick,
                true,
                BottomMenu.ToggleableType.Mail,
                BottomMenu.ToggleableType.Quest,
                BottomMenu.ToggleableType.Chat,
                BottomMenu.ToggleableType.IllustratedBook);

            ReactiveAvatarState.ActionPoint
                .Subscribe(_ => UpdateBattleStartButton())
                .AddTo(_disposables);
            _tempStats = _player.Model.Stats.Clone() as CharacterStats;
            inventory.SharedModel.UpdateEquipmentNotification(GetElementalTypes());
            startButton.gameObject.SetActive(true);
        }

        public override void Close(bool ignoreCloseAnimation = false)
        {
            _reset = true;
            Find<BottomMenu>().Close(ignoreCloseAnimation);

            foreach (var slot in consumableSlots)
            {
                slot.Clear();
            }

            equipmentSlots.Clear();
            base.Close(ignoreCloseAnimation);
            _disposables.DisposeAllAndClear();
        }

        #endregion

        #region Tooltip

        private void ShowTooltip(InventoryItemView view)
        {
            var tooltip = Find<ItemInformationTooltip>();
            if (view is null ||
                view.RectTransform == tooltip.Target)
            {
                tooltip.Close();

                return;
            }

            tooltip.Show(
                view.RectTransform,
                view.Model,
                value => !view.Model.Dimmed.Value,
                view.Model.EquippedEnabled.Value
                    ? L10nManager.Localize("UI_UNEQUIP")
                    : L10nManager.Localize("UI_EQUIP"),
                _ => Equip(tooltip.itemInformation.Model.item.Value),
                _ =>
                {
                    equipSlotGlow.SetActive(false);
                    inventory.SharedModel.DeselectItemView();
                });
        }

        private void ShowTooltip(EquipmentSlot slot)
        {
            var tooltip = Find<ItemInformationTooltip>();
            if (slot is null ||
                slot.RectTransform == tooltip.Target)
            {
                tooltip.Close();

                return;
            }

            if (inventory.SharedModel.TryGetEquipment(slot.Item as Equipment, out var item) ||
                inventory.SharedModel.TryGetConsumable(slot.Item as Consumable, out item))
            {
                tooltip.Show(
                    slot.RectTransform,
                    item,
                    _ => inventory.SharedModel.DeselectItemView());
            }
        }

        #endregion

        #region Subscribe

        private void SubscribeInventoryResetItems(Module.Inventory value)
        {
            if (_reset)
            {
                return;
            }

            foreach (var slot in equipmentSlots)
            {
                if (slot.Item != null)
                {
                    if (slot.Item.ItemType == ItemType.Equipment)
                    {
                        slot.SetDim(!IsExistElementalType(slot.Item.ElementalType));
                    }
                    else
                    {
                        slot.SetDim(false);
                    }
                }
            }

            UpdateBattleStartButton();

            inventory.SharedModel.DimmedFunc.Value = inventoryItem =>
                inventoryItem.ItemBase.Value.ItemType == ItemType.Costume ||
                inventoryItem.ItemBase.Value.ItemType == ItemType.Material ||
                (inventoryItem.ItemBase.Value.ItemType == ItemType.Equipment && !IsExistElementalType(inventoryItem.ItemBase.Value.ElementalType));

            inventory.SharedModel.EquippedEnabledFunc.SetValueAndForceNotify(inventoryItem =>
            {
                if (inventoryItem.ItemBase.Value.ItemType == ItemType.Costume &&
                    inventoryItem.ItemBase.Value is Costume costume)
                {
                    return costume.equipped;
                }

                return TryToFindSlotAlreadyEquip(inventoryItem.ItemBase.Value, out _);
            });

            inventory.SharedModel.UpdateEquipmentNotification(GetElementalTypes());
        }

        private void SubscribeInventorySelectedItem(InventoryItemView view)
        {
            // Fix me. 이미 장착한 아이템일 경우 장착 버튼 비활성화 필요.
            // 현재는 왼쪽 부분인 인벤토리와 아이템 정보 부분만 뷰모델을 적용했는데, 오른쪽 까지 뷰모델이 확장되면 가능.
            if (view is null ||
                view.Model is null ||
                view.Model.Dimmed.Value ||
                !(view.Model.ItemBase.Value is ItemUsable))
            {
                HideGlowEquipSlot();
            }
            else
            {
                UpdateGlowEquipSlot((ItemUsable) view.Model.ItemBase.Value);
            }

            ShowTooltip(view);
        }

        private void SubscribeBackButtonClick(BottomMenu bottomMenu)
        {
            if (!CanClose)
            {
                return;
            }

            var stageInfo = Find<StageInformation>();

            var SharedViewModel = new WorldMap.ViewModel
            {
                WorldInformation = States.Instance.CurrentAvatarState.worldInformation
            };
            SharedViewModel.SelectedWorldId.SetValueAndForceNotify(GameConfig.MimisbrunnrWorldId);
            SharedViewModel.SelectedStageId.SetValueAndForceNotify(_stageId.Value);

            Game.Game.instance.TableSheets.WorldSheet.TryGetValue(GameConfig.MimisbrunnrWorldId, out var worldRow, true);
            stageInfo.Show(SharedViewModel, worldRow, StageInformation.StageType.Mimisbrunnr);
            gameObject.SetActive(false);
        }

        private void UpdateBattleStartButton()
        {
            if (EnoughActionPoint)
            {
                SetBattleStartButton(CheckEquipmentElementalType());
            }
            else
            {
                SetBattleStartButton(false);
                requiredPointText.color = Color.red;
            }
        }

        private void SetBattleStartButton(bool interactable)
        {
            startButton.interactable = interactable;
            if (interactable)
            {
                requiredPointText.color = RequiredActionPointOriginColor;

                foreach (var image in startButtonImages)
                {
                    image.color = BattleStartButtonOriginColor;
                }

                foreach (var particle in particles)
                {
                    particle.Play();
                }
            }
            else
            {
                requiredPointText.color = DimmedColor;

                foreach (var image in startButtonImages)
                {
                    image.color = DimmedColor;
                }

                foreach (var particle in particles)
                {
                    particle.Stop();
                }
            }
        }

        private void SubscribeStage(int stageId)
        {
            var stage =
                Game.Game.instance.TableSheets.StageSheet.Values.FirstOrDefault(
                    i => i.Id == stageId);
            if (stage is null)
                return;
            _requiredCost = stage.CostAP;
            requiredPointText.text = _requiredCost.ToString();
        }

        #endregion

        private void BattleClick(bool repeat)
        {
            if (!CheckEquipmentElementalType())
            {
                Notification.Push(
                    MailType.System,
                    L10nManager.Localize("UI_MIMISBRUNNR_START_FAIELD"));

                return;
            }

            _stage.IsInStage = true;
            StartCoroutine(CoBattleClick(repeat));
            startButton.interactable = false;
            repeatToggle.interactable = false;
        }

        private IEnumerator CoBattleClick(bool repeat)
        {
            var animation = ItemMoveAnimation.Show(actionPointImage.sprite,
                actionPointImage.transform.position,
                buttonStarImageTransform.position,
                Vector2.one,
                moveToLeft,
                true,
                animationTime,
                middleXGap);
            LocalLayerModifier.ModifyAvatarActionPoint(
                States.Instance.CurrentAvatarState.address,
                -_requiredCost);
            yield return new WaitWhile(() => animation.IsPlaying);
            Battle(repeat);
            AudioController.PlayClick();
        }

        #region slot

        private void Equip(CountableItem countableItem)
        {
            if (!(countableItem is InventoryItem inventoryItem))
            {
                return;
            }

            var itemBase = inventoryItem.ItemBase.Value;
            // 이미 장착중인 아이템이라면 해제한다.
            if (TryToFindSlotAlreadyEquip(itemBase, out var slot))
            {
                Unequip(slot);
                return;
            }

            // 아이템을 장착할 슬롯을 찾는다.
            if (!TryToFindSlotToEquip(itemBase, out slot))
            {
                return;
            }

            // 이미 슬롯에 아이템이 있다면 해제한다.
            if (!slot.IsEmpty)
            {
                if (inventory.SharedModel.TryGetEquipment(
                        slot.Item as Equipment,
                        out var inventoryItemToUnequip) ||
                    inventory.SharedModel.TryGetConsumable(
                        slot.Item as Consumable,
                        out inventoryItemToUnequip))
                {
                    inventoryItemToUnequip.EquippedEnabled.Value = false;
                    LocalStateItemEquipModify(slot.Item, false);
                }
            }

            inventoryItem.EquippedEnabled.Value = true;
            slot.Set(itemBase, ShowTooltip, Unequip);

            if (slot.Item.ItemType == ItemType.Equipment)
            {
                slot.SetDim(!IsExistElementalType(slot.Item.ElementalType));
            }
            else
            {
                slot.SetDim(false);
            }

            LocalStateItemEquipModify(slot.Item, true);
            HideGlowEquipSlot();
            PostEquipOrUnequip(slot);
        }

        private void Unequip(EquipmentSlot slot)
        {
            if (slot.IsEmpty)
            {
                equipSlotGlow.SetActive(false);
                foreach (var item in inventory.SharedModel.Equipments)
                {
                    item.GlowEnabled.Value =
                        item.ItemBase.Value.ItemSubType == slot.ItemSubType;
                }

                return;
            }

            if (inventory.SharedModel.TryGetEquipment(
                    slot.Item as Equipment,
                    out var inventoryItem) ||
                inventory.SharedModel.TryGetConsumable(
                    slot.Item as Consumable,
                    out inventoryItem))
            {
                inventoryItem.EquippedEnabled.Value = false;
                LocalStateItemEquipModify(slot.Item, false);
            }

            slot.Clear();
            PostEquipOrUnequip(slot);
        }

        private static void LocalStateItemEquipModify(ItemBase itemBase, bool equip)
        {
            if (itemBase.ItemType != ItemType.Equipment)
            {
                return;
            }

            var equipment = (Equipment) itemBase;
            LocalLayerModifier.SetItemEquip(
                States.Instance.CurrentAvatarState.address,
                equipment.ItemId,
                equip,
                false);
        }

        private void PostEquipOrUnequip(EquipmentSlot slot)
        {
            UpdateStats();
            Find<ItemInformationTooltip>().Close();

            if (slot.ItemSubType == ItemSubType.Armor)
            {
                var armor = (Armor) slot.Item;
                var weapon = (Weapon) _weaponSlot.Item;
                _player.EquipEquipmentsAndUpdateCustomize(armor, weapon);
            }
            else if (slot.ItemSubType == ItemSubType.Weapon)
            {
                _player.EquipWeapon((Weapon) slot.Item);
            }

            Game.Event.OnUpdatePlayerEquip.OnNext(_player);
            AudioController.instance.PlaySfx(slot.ItemSubType == ItemSubType.Food
                ? AudioController.SfxCode.ChainMail2
                : AudioController.SfxCode.Equipment);
            inventory.SharedModel.UpdateEquipmentNotification(GetElementalTypes());
            Find<BottomMenu>().UpdateInventoryNotification();
            UpdateBattleStartButton();
        }

        private bool TryToFindSlotAlreadyEquip(ItemBase item, out EquipmentSlot slot)
        {
            switch (item.ItemType)
            {
                case ItemType.Consumable:
                    foreach (var consumableSlot in consumableSlots.Where(consumableSlot =>
                        !consumableSlot.IsLock && !consumableSlot.IsEmpty))
                    {
                        if (!consumableSlot.Item.Equals(item))
                            continue;

                        slot = consumableSlot;
                        return true;
                    }

                    slot = null;
                    return false;
                case ItemType.Equipment:
                    return equipmentSlots.TryGetAlreadyEquip(item, out slot);
                default:
                    slot = null;
                    return false;
            }
        }

        private bool TryToFindSlotToEquip(ItemBase item, out EquipmentSlot slot)
        {
            switch (item.ItemType)
            {
                case ItemType.Consumable:
                    slot = consumableSlots.FirstOrDefault(s => !s.IsLock && s.IsEmpty)
                           ?? consumableSlots[0];
                    return true;
                case ItemType.Equipment:
                    return equipmentSlots.TryGetToEquip((Equipment) item, out slot);
                default:
                    slot = null;
                    return false;
            }
        }

        private void UpdateGlowEquipSlot(ItemUsable itemUsable)
        {
            var itemType = itemUsable.ItemType;
            EquipmentSlot equipmentSlot;
            switch (itemType)
            {
                case ItemType.Consumable:
                case ItemType.Equipment:
                    TryToFindSlotToEquip(itemUsable, out equipmentSlot);
                    break;
                case ItemType.Material:
                    HideGlowEquipSlot();
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (equipmentSlot && equipmentSlot.transform.parent)
            {
                equipSlotGlow.transform.SetParent(equipmentSlot.transform);
                equipSlotGlow.transform.localPosition = Vector3.zero;
            }
            else
            {
                HideGlowEquipSlot();
            }
        }

        private void HideGlowEquipSlot()
        {
            equipSlotGlow.SetActive(false);
        }

        #endregion

        private void UpdateStats()
        {
            var equipments = equipmentSlots
                .Where(slot => !slot.IsLock && !slot.IsEmpty)
                .Select(slot => slot.Item as Equipment)
                .Where(item => !(item is null))
                .ToList();
            var consumables = consumableSlots
                .Where(slot => !slot.IsLock && !slot.IsEmpty)
                .Select(slot => slot.Item as Consumable)
                .Where(item => !(item is null))
                .ToList();

            var stats = _tempStats.SetAll(
                _tempStats.Level,
                equipments,
                consumables,
                Game.Game.instance.TableSheets.EquipmentItemSetEffectSheet
            );
            using (var enumerator = stats.GetBaseAndAdditionalStats().GetEnumerator())
            {
                foreach (var statView in statusRows)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }

                    var (statType, baseValue, additionalValue) = enumerator.Current;
                    statView.Show(statType, baseValue, additionalValue);
                }
            }
        }

        private void Battle(bool repeat)
        {
            Find<BottomMenu>().Close(true);
            Find<LoadingScreen>().Show();

            startButton.gameObject.SetActive(false);
            _player.StartRun();
            ActionCamera.instance.ChaseX(_player.transform);

            var costumes = _player.Costumes;

            var equipments = equipmentSlots
                .Where(slot => !slot.IsLock && !slot.IsEmpty)
                .Select(slot => (Equipment) slot.Item)
                .ToList();

            var consumables = consumableSlots
                .Where(slot => !slot.IsLock && !slot.IsEmpty)
                .Select(slot => (Consumable) slot.Item)
                .ToList();

            _stage.isExitReserved = false;
            _stage.repeatStage = repeat;
            _stage.foodCount = consumables.Count;
            ActionRenderHandler.Instance.Pending = true;
            Game.Game.instance.ActionManager
                .MimisbrunnrBattle(
                    costumes,
                    equipments,
                    consumables,
                    GameConfig.MimisbrunnrWorldId,
                    _stageId.Value
                )
                .Subscribe(
                    _ =>
                    {
                        LocalLayerModifier.ModifyAvatarActionPoint(
                            States.Instance.CurrentAvatarState.address,
                            _requiredCost);
                    }, e => ActionRenderHandler.BackToMain(false, e))
                .AddTo(this);
        }

        public void GoToStage(BattleLog battleLog)
        {
            Game.Event.OnStageStart.Invoke(battleLog);
            Find<LoadingScreen>().Close();
            Close(true);
        }

        public void SimulateBattle()
        {
            var level = States.Instance.CurrentAvatarState.level;
            if (!string.IsNullOrEmpty(levelField.text))
                level = int.Parse(levelField.text);
            // 레벨 범위가 넘어간 값이면 만렙으로 설정
            if (!Game.Game.instance.TableSheets.CharacterLevelSheet.ContainsKey(level))
            {
                level = Game.Game.instance.TableSheets.CharacterLevelSheet.Keys.Last();
            }

            Find<LoadingScreen>().Show();

            startButton.gameObject.SetActive(false);
            _player.StartRun();
            ActionCamera.instance.ChaseX(_player.transform);

            var stageId = _stageId.Value;
            if (!Game.Game.instance.TableSheets.WorldSheet.TryGetByStageId(stageId,
                out var worldRow))
                throw new KeyNotFoundException(
                    $"WorldSheet.TryGetByStageId() {nameof(stageId)}({stageId})");

            var avatarState = new AvatarState(States.Instance.CurrentAvatarState) {level = level};
            List<Guid> consumables = consumableSlots
                .Where(slot => !slot.IsLock && !slot.IsEmpty)
                .Select(slot => ((Consumable) slot.Item).ItemId)
                .ToList();
            var equipments = equipmentSlots
                .Where(slot => !slot.IsLock && !slot.IsEmpty)
                .Select(slot => (Equipment) slot.Item)
                .ToList();
            var inventoryEquipments = avatarState.inventory.Items
                .Select(i => i.item)
                .OfType<Equipment>()
                .Where(i => i.equipped).ToList();

            foreach (var equipment in inventoryEquipments)
            {
                equipment.Unequip();
            }

            foreach (var equipment in equipments)
            {
                if (!avatarState.inventory.TryGetNonFungibleItem(equipment,
                    out ItemUsable outNonFungibleItem))
                {
                    continue;
                }

                ((Equipment) outNonFungibleItem).Equip();
            }

            var tableSheets = Game.Game.instance.TableSheets;
            var simulator = new StageSimulator(
                new Cheat.DebugRandom(),
                avatarState,
                consumables,
                worldRow.Id,
                stageId,
                tableSheets.GetStageSimulatorSheets(),
                tableSheets.CostumeStatSheet
            );
            simulator.Simulate();
            GoToStage(simulator.Log);
        }

        private bool CheckEquipmentElementalType()
        {
            return equipmentSlots
                .Where(x => x.Item != null)
                .All(x => IsExistElementalType(x.Item.ElementalType));
        }

        private bool IsExistElementalType(ElementalType elementalType)
        {
            return GetElementalTypes().Exists(x => x == elementalType);
        }

        private List<ElementalType> GetElementalTypes()
        {
            var mimisbrunnrSheet = Game.Game.instance.TableSheets.MimisbrunnrSheet;
            if (!mimisbrunnrSheet.TryGetValue(_stageId.Value, out var mimisbrunnrSheetRow))
            {
                throw new KeyNotFoundException(
                    $"mimisbrunnrSheet.TryGetValue() {nameof(_stageId)}({_stageId})");
            }

            return mimisbrunnrSheetRow.ElementalTypes;
        }
    }
}
