using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Nekoyume.Action;
using Nekoyume.Game.Item;

namespace Nekoyume.Game.Mail
{
    [Serializable]
    public class Mail
    {
        public bool New;
        public Combination.Result attachment;
        public long blockIndex;

        public Mail(Combination.Result actionResult, long blockIndex)
        {
            New = true;
            attachment = actionResult;
            this.blockIndex = blockIndex;
        }

        public string ToInfo()
        {
            return "조합 완료";
        }
    }

    [Serializable]
    public class MailBox: IEnumerable<Mail>
    {
        private readonly List<Mail> _mails = new List<Mail>();
        public IEnumerator<Mail> GetEnumerator()
        {
            return _mails.OrderByDescending(i => i.blockIndex).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(Mail mail)
        {
            _mails.Add(mail);
        }
    }
}
