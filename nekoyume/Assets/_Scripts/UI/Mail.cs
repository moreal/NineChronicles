using System;
using System.Linq;
using Nekoyume.BlockChain;
using Nekoyume.Game.Factory;
using Nekoyume.UI.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Nekoyume.UI
{
    public class Mail : Widget
    {
        public ScrollRect list;
        public MailInfo mailInfo;

        public override void Show()
        {
            var mailBox = States.Instance.currentAvatarState.Value.mailBox;
            mailInfo.gameObject.SetActive(true);
            foreach (var mail in mailBox)
            {
                var newInfo = Instantiate(mailInfo, list.content);
                newInfo.Set(mail);
            }
            mailInfo.gameObject.SetActive(false);
            base.Show();
        }

        private void OnDisable()
        {
            foreach (Transform child in  list.content.transform)
            {
                Destroy(child.gameObject);
            }

            list.verticalNormalizedPosition = 1f;
        }

        public void GetAttachment(MailInfo info)
        {
            var item = info.data.attachment.itemUsable;
            var popup = Find<CombinationResultPopup>();
            var materialItems = info.data.attachment.materials
                .Select(material => new {material, item = ItemFactory.CreateMaterial(material.id, Guid.Empty)})
                .Select(t => new CombinationMaterial(t.item, t.material.count, t.material.count, t.material.count))
                .ToList();
            var model = new UI.Model.CombinationResultPopup(new CountableItem(item, 1))
            {
                isSuccess = true,
                materialItems = materialItems
            };
            popup.Pop(model);
            info.Read();

            //게임상의 인벤토리 업데이트
            var player = Game.Game.instance.stage.GetPlayer();
            player.Inventory.AddNonFungibleItem(item);

            //아바타상태 인벤토리 업데이트
            ActionManager.instance.AddItem(item.ItemId);
        }

    }
}
