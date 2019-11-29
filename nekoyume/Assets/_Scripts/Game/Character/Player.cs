using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Nekoyume.Game.Controller;
using Nekoyume.Game.Item;
using Nekoyume.Game.VFX;
using Nekoyume.Helper;
using Nekoyume.Manager;
using Nekoyume.TableData;
using Nekoyume.UI;
using UniRx;
using UnityEngine;

namespace Nekoyume.Game.Character
{
    // todo: 경험치 정보를 `CharacterBase`로 옮기는 것이 좋겠음.
    public class Player : CharacterBase
    {
        private readonly List<IDisposable> _disposablesForModel = new List<IDisposable>();

        public long EXP = 0;
        public long EXPMax { get; private set; }

        public Item.Inventory Inventory;
        public TouchHandler touchHandler;

        public new readonly ReactiveProperty<Model.Player> Model = new ReactiveProperty<Model.Player>();

        public List<Equipment> Equipments =>
            Inventory.Items.Select(i => i.item).OfType<Equipment>().Where(e => e.equipped).ToList();

        protected override float RunSpeedDefault => Model.Value.RunSpeed;

        protected override Vector3 DamageTextForce => new Vector3(-0.1f, 0.5f);
        protected override Vector3 HudTextPosition => transform.TransformPoint(0f, 1.7f, 0f);

        #region Mono

        protected override void Awake()
        {
            base.Awake();

            OnUpdateHPBar.Subscribe(_ => Event.OnUpdatePlayerStatus.OnNext(this)).AddTo(gameObject);

            Animator = new PlayerAnimator(this);
            Animator.OnEvent.Subscribe(OnAnimatorEvent);
            Animator.TimeScale = AnimatorTimeScale;

            Inventory = new Item.Inventory();

            touchHandler.OnClick.Subscribe(_ =>
                {
                    if (Game.instance.stage.IsInStage)
                        return;

                    Animator.Touch();
                })
                .AddTo(gameObject);

            TargetTag = Tag.Enemy;
        }

        private void OnDestroy()
        {
            Animator.Dispose();
        }

        #endregion

        public override void Set(Model.CharacterBase model, bool updateCurrentHP = false)
        {
            if (!(model is Model.Player playerModel))
                throw new ArgumentException(nameof(model));

            Set(playerModel, updateCurrentHP);
        }

        public void Set(Model.Player model, bool updateCurrentHP)
        {
            base.Set(model, updateCurrentHP);

            _disposablesForModel.DisposeAllAndClear();
            Model.SetValueAndForceNotify(model);

            InitStats(model);
            UpdateEquipments(model.armor, model.weapon);
            UpdateCustomize();

            if (ReferenceEquals(SpeechBubble, null))
            {
                SpeechBubble = Widget.Create<SpeechBubble>();
            }

            SpeechBubble.speechBreakTime = GameConfig.PlayerSpeechBreakTime;
        }

        protected override IEnumerator Dying()
        {
            SpeechBubble?.Clear();
            ShowSpeech("PLAYER_LOSE");
            StopRun();
            Animator.Die();
            yield return new WaitForSeconds(.5f);
            DisableHUD();
            yield return new WaitForSeconds(.8f);
            OnDead();
        }

        protected override void OnDead()
        {
            gameObject.SetActive(false);
            Event.OnPlayerDead.Invoke();
        }

        public void UpdateCustomize()
        {
            UpdateEye(Model.Value.lensIndex);
            UpdateEar(Model.Value.earIndex);
            UpdateTail(Model.Value.tailIndex);
        }

        public void UpdateEquipments(Armor armor, Weapon weapon = null)
        {
            UpdateArmor(armor);
            UpdateWeapon(weapon);
        }

        private void UpdateArmor(Armor armor)
        {
            var armorId = armor?.Data.Id ?? GameConfig.DefaultAvatarArmorId;
            var spineResourcePath = armor?.Data.SpineResourcePath ?? $"Character/Player/{armorId}";
            
            if (!(Animator.Target is null))
            {
                var animatorTargetName = spineResourcePath.Split('/').Last(); 
                if (Animator.Target.name.Contains(animatorTargetName))
                    return;

                Animator.DestroyTarget();
            }

            var origin = Resources.Load<GameObject>(spineResourcePath);
            var go = Instantiate(origin, gameObject.transform);
            Animator.ResetTarget(go);
        }

        public void UpdateWeapon(Weapon weapon)
        {
            var controller = GetComponentInChildren<SkeletonAnimationController>();
            if (!controller)
                return;

            var sprite = weapon.GetPlayerSpineTexture();
            controller.UpdateWeapon(sprite);
        }

        public void UpdateEar(int index)
        {
            UpdateEar($"ear_{index + 1:d4}_left", $"ear_{index + 1:d4}_right");
        }

        public void UpdateEar(string earLeftResource, string earRightResource)
        {
            if (string.IsNullOrEmpty(earLeftResource))
            {
                earLeftResource = $"ear_{Model.Value.earIndex + 1:d4}_left";
            }

            if (string.IsNullOrEmpty(earRightResource))
            {
                earRightResource = $"ear_{Model.Value.earIndex + 1:d4}_right";
            }

            var controller = GetComponentInChildren<SkeletonAnimationController>();
            if (!controller)
                return;

            var spriteLeft = SpriteHelper.GetPlayerSpineTextureEarLeft(earLeftResource);
            var spriteRight = SpriteHelper.GetPlayerSpineTextureEarRight(earRightResource);
            controller.UpdateEar(spriteLeft, spriteRight);
        }

        public void UpdateEye(int index)
        {
            UpdateEye(CostumeSheet.GetEyeOpenResourceByIndex(index), CostumeSheet.GetEyeHalfResourceByIndex(index));
        }

        public void UpdateEye(string eyeOpenResource, string eyeHalfResource)
        {
            if (string.IsNullOrEmpty(eyeOpenResource))
            {
                eyeOpenResource = CostumeSheet.GetEyeOpenResourceByIndex(Model.Value.lensIndex);
            }

            if (string.IsNullOrEmpty(eyeHalfResource))
            {
                eyeHalfResource = CostumeSheet.GetEyeHalfResourceByIndex(Model.Value.lensIndex);
            }

            var controller = GetComponentInChildren<SkeletonAnimationController>();
            if (!controller)
                return;

            var eyeOpenSprite = SpriteHelper.GetPlayerSpineTextureEyeOpen(eyeOpenResource);
            var eyeHalfSprite = SpriteHelper.GetPlayerSpineTextureEyeHalf(eyeHalfResource);
            controller.UpdateEye(eyeOpenSprite, eyeHalfSprite);
        }

        public void UpdateTail(int index)
        {
            UpdateTail($"tail_{index + 1:d4}");
        }

        public void UpdateTail(string tailResource)
        {
            if (string.IsNullOrEmpty(tailResource))
            {
                tailResource = $"tail_{Model.Value.tailIndex + 1:d4}";
            }

            var controller = GetComponentInChildren<SkeletonAnimationController>();
            if (!controller)
                return;

            var sprite = SpriteHelper.GetPlayerSpineTextureTail(tailResource);
            controller.UpdateTail(sprite);
        }

        public IEnumerator CoGetExp(long exp)
        {
            if (exp <= 0)
            {
                yield break;
            }

            var level = Level;
            Model.Value.GetExp(exp);
            EXP += exp;

            if (Level != level)
            {
                AnalyticsManager.Instance.OnEvent(AnalyticsManager.EventName.ActionStatusLevelUp, level);
                AudioController.instance.PlaySfx(AudioController.SfxCode.LevelUp);
                VFXController.instance.Create<BattleLevelUp01VFX>(transform, HUDOffset);
                InitStats(Model.Value);
            }
            UpdateHpBar();
        }

        private void InitStats(Model.Player character)
        {
            EXP = character.Exp.Current;
            EXPMax = character.Exp.Max;
            Inventory = character.Inventory;
        }

        private void OnAnimatorEvent(string eventName)
        {
            switch (eventName)
            {
                case "attackStart":
                    AudioController.PlaySwing();
                    break;
                case "attackPoint":
                    Event.OnAttackEnd.Invoke(this);
                    break;
                case "footstep":
                    AudioController.PlayFootStep();
                    break;
            }
        }

        protected override bool CanRun()
        {
            var canRun = base.CanRun();
            var enemy = GetComponentsInChildren<CharacterBase>()
                .Where(c => c.gameObject.CompareTag(TargetTag))
                .OrderBy(c => c.transform.position.x).FirstOrDefault();
            if (enemy != null)
            {
                return canRun && !TargetInRange(enemy);
            }

            return canRun;
        }

        protected override void ProcessAttack(CharacterBase target, Model.Skill.SkillInfo skill, bool isLastHit,
            bool isConsiderElementalType)
        {
            ShowSpeech("PLAYER_SKILL", (int) skill.ElementalType, (int) skill.SkillCategory);
            base.ProcessAttack(target, skill, isLastHit, isConsiderElementalType);
            ShowSpeech("PLAYER_ATTACK");
        }

        protected override IEnumerator CoAnimationCast(Model.Skill.SkillInfo info)
        {
            ShowSpeech("PLAYER_SKILL", (int) info.ElementalType, (int) info.SkillCategory);
            yield return StartCoroutine(base.CoAnimationCast(info));
        }

        public void DoFade(float endValue, float sec)
        {
            var controller = GetComponentInChildren<SkeletonAnimationController>();
            DOTween.Sequence()
                .Append(DOTween.To(
                    () => controller.SkeletonAnimation.skeleton.A,
                    co => controller.SkeletonAnimation.skeleton.A = co, 0, 0f
                ))
                .Append(DOTween.To(
                    () => controller.SkeletonAnimation.skeleton.A,
                    co => controller.SkeletonAnimation.skeleton.A = co, endValue, sec
                ))
                .Play();
        }
    }
}
