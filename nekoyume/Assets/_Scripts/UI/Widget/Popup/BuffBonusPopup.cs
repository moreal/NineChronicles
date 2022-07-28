using Nekoyume.Helper;
using Nekoyume.L10n;
using Nekoyume.Model.Mail;
using Nekoyume.State;
using Nekoyume.UI.Module;
using Nekoyume.UI.Scroller;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using Nekoyume.Game.Controller;

namespace Nekoyume.UI
{
    using mixpanel;
    using Nekoyume.BlockChain;
    using System.Collections.Generic;
    using System.Linq;
    using UniRx;

    public class BuffBonusPopup : PopupWidget
    {
        [SerializeField]
        private ConditionalCostButton normalButton = null;

        [SerializeField]
        private ConditionalCostButton advancedButton = null;

        private BigInteger _normalCost;

        private BigInteger _advancedCost;

        private System.Action _onAttract;

        [SerializeField]
        private Button buffListButton = null;

        [SerializeField]
        private GameObject buffListView = null;

        [SerializeField]
        private Transform cellContainer;

        [SerializeField]
        private GameObject titlePrefab;

        [SerializeField]
        private GameObject buffPrefab;

        private List<GameObject> cellList = new List<GameObject>();

        protected override void Awake()
        {
            base.Awake();

            _onAttract = () =>
            {
                Close(true);
                Find<Menu>().Close();
                Find<WorldMap>().Close();
                Find<StageInformation>().Close();
                Find<BattlePreparation>().Close();
                Find<Grind>().Show();
            };

            normalButton.OnClickSubject
                .Subscribe(OnClickNormalButton)
                .AddTo(gameObject);
            advancedButton.OnClickSubject
                .Subscribe(OnClickAdvancedButton)
                .AddTo(gameObject);

            normalButton.OnClickDisabledSubject
                .Subscribe(OnInsufficientStar)
                .AddTo(gameObject);
            advancedButton.OnClickDisabledSubject
                .Subscribe(OnInsufficientStar)
                .AddTo(gameObject);

            buffListButton.onClick.AddListener(ShowBuffListView);
        }

        public override void Initialize()
        {
            normalButton.Text = L10nManager.Localize("UI_DRAW_NORMAL");
            advancedButton.Text = L10nManager.Localize("UI_DRAW_ADVANCED");
            base.Initialize();
        }

        public void Show(int stageId, bool hasEnoughStars)
        {
            var sheet = Game.Game.instance.TableSheets.CrystalStageBuffGachaSheet;
            _normalCost = CrystalCalculator.CalculateBuffGachaCost(stageId, false, sheet).MajorUnit;
            _advancedCost = CrystalCalculator.CalculateBuffGachaCost(stageId, true, sheet).MajorUnit;
            normalButton.SetCost(CostType.Crystal, (long) _normalCost);
            advancedButton.SetCost(CostType.Crystal, (long) _advancedCost);
            normalButton.Interactable = hasEnoughStars;
            advancedButton.Interactable = hasEnoughStars;
            base.Show();
        }

        private bool CheckCrystal(BigInteger cost)
        {
            if (States.Instance.CrystalBalance.MajorUnit < cost)
            {
                var message = L10nManager.Localize("UI_NOT_ENOUGH_CRYSTAL");
                Find<PaymentPopup>().ShowAttract(
                    CostType.Crystal,
                    _normalCost,
                    message,
                    L10nManager.Localize("UI_GO_GRINDING"),
                    _onAttract);
                return false;
            }
            return true;
        }

        private void OnClickNormalButton(ConditionalButton.State state)
        {
            if (CheckCrystal(_normalCost))
            {
                var usageMessage = L10nManager.Localize("UI_DRAW_NORMAL_BUFF");
                var balance = States.Instance.CrystalBalance;
                var content = balance.GetPaymentFormatText(usageMessage, _normalCost);

                Find<PaymentPopup>().Show(
                    CostType.Crystal,
                    balance.MajorUnit,
                    _normalCost,
                    content,
                    L10nManager.Localize("UI_NOT_ENOUGH_CRYSTAL"),
                    () => PushAction(false),
                    _onAttract);
            }
        }

        private void OnClickAdvancedButton(ConditionalButton.State state)
        {
            if (CheckCrystal(_advancedCost))
            {
                var usageMessage = L10nManager.Localize("UI_DRAW_ADVANCED_BUFF");
                var balance = States.Instance.CrystalBalance;
                var content = balance.GetPaymentFormatText(usageMessage, _advancedCost);

                Find<PaymentPopup>().Show(
                    CostType.Crystal,
                    balance.MajorUnit,
                    _advancedCost,
                    content,
                    L10nManager.Localize("UI_NOT_ENOUGH_CRYSTAL"),
                    () => PushAction(true),
                    _onAttract);
            }
        }

        private void OnInsufficientStar(Unit unit)
        {
            OneLineSystem.Push(
                MailType.System,
                L10nManager.Localize("UI_HAS_BUFF_NOT_ENOUGH_STAR"),
                NotificationCell.NotificationType.Alert);
        }

        private void PushAction(bool advanced)
        {
            Find<BuffBonusLoadingScreen>().Show();
            Find<HeaderMenuStatic>().Crystal.SetProgressCircle(true);

            Analyzer.Instance.Track("Unity/Purchase Crystal Bonus Skill", new Value
            {
                ["BurntCrystal"] = (long) (advanced ? _advancedCost : _normalCost),
                ["isAdvanced"] = advanced,
            });

            ActionManager.Instance.HackAndSlashRandomBuff(advanced).Subscribe();
            Close();
        }

        private void ShowBuffListView()
        {
            buffListView.SetActive(true);
            UpdateBuffListView();
        }

        private void UpdateBuffListView()
        {
            var randomBuffsheet = Game.Game.instance.TableSheets.CrystalRandomBuffSheet;
            var randomBuffGroupsByRank = randomBuffsheet.Select(line => line.Value).GroupBy(row => row.Rank);

            foreach (var cell in cellList)
            {
                Destroy(cell);
            }

            foreach (var randomBuffGroup in randomBuffGroupsByRank)
            {
                // Grade
                var cell = Instantiate(titlePrefab, cellContainer);
                cell.GetComponent<BuffBonusTitleCell>().Set(randomBuffGroup.Key);
                cellList.Add(cell);

                foreach (var randomBuffRow in randomBuffGroup)
                {
                    // buff
                    cell = Instantiate(buffPrefab, cellContainer);
                    cell.GetComponent<BuffBonusBuffCell>().Set(randomBuffRow);
                    cellList.Add(cell);
                }
            }
        }
    }
}
