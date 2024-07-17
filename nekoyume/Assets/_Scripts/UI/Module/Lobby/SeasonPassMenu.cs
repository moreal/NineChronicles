using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Nekoyume.ApiClient;

namespace Nekoyume.UI.Module.Lobby
{
    using UniRx;

    public class SeasonPassMenu : MainMenu
    {
        [SerializeField]
        private GameObject premiumIcon;

        [SerializeField]
        private GameObject premiumPlusIcon;

        [SerializeField]
        private TextMeshProUGUI levelText;

        [SerializeField]
        private TextMeshProUGUI timeText;

        [SerializeField]
        private GameObject notificationObj;

        [SerializeField]
        private GameObject dim;

        [SerializeField]
        private GameObject iconRoot;

        protected override void Awake()
        {
            base.Awake();

            dim.SetActive(false);
            iconRoot.SetActive(true);
            var seasonPassService = ApiClients.Instance.SeasonPassServiceManager;
            seasonPassService.AvatarInfo.Subscribe((info) =>
            {
                dim.SetActive(info == null);
                iconRoot.SetActive(info != null);
                if (info == null)
                {
                    return;
                }

                premiumIcon.SetActive(info.IsPremium);
                premiumPlusIcon.SetActive(info.IsPremiumPlus);
                notificationObj.SetActive(info.LastNormalClaim != info.Level);

                if (info.Level >= SeasonPass.SeasonPassMaxLevel)
                {
                    levelText.text = SeasonPass.MaxLevelString;
                }
                else
                {
                    levelText.text = $"Lv.{info.Level}";
                }
            }).AddTo(gameObject);

            ApiClients.Instance.SeasonPassServiceManager.RemainingDateTime.Subscribe((endDate) => { timeText.text = $"<Style=Clock> {endDate}"; });
        }
    }
}
