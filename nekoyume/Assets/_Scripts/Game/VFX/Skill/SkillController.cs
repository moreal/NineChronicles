using Nekoyume.EnumType;
using Nekoyume.Game.Character;
using Nekoyume.Game.Util;
using UnityEngine;

namespace Nekoyume.Game.VFX.Skill
{
    public class SkillController : MonoBehaviour
    {
        private const int InitCount = 5;

        public SkillVFX[] skills;
        private ObjectPool _pool;

        private void Awake()
        {
            _pool = FindObjectOfType<ObjectPool>();
            skills = Resources.LoadAll<SkillVFX>("VFX/Prefabs");
            foreach (var skill in skills)
            {
                _pool.Add(skill.gameObject, InitCount);
            }
        }

        public T Get<T>(CharacterBase target, Model.Skill.SkillInfo skillInfo) where T : SkillVFX
        {
            var position = target.transform.position;
            position.x -= 0.2f;
            position.y += 0.32f;
            var size = target.characterSize == "xs" ? "s" : "m";
            var elemental = skillInfo.Elemental;
            if (skillInfo.skillCategory == SkillCategory.Area)
            {
                size = "l";
                //FIXME 현재 무속성 범위공격 이펙트는 존재하지 않기때문에 임시처리.
                if (elemental == ElementalType.Normal)
                    elemental = ElementalType.Fire;
                var pos = ActionCamera.instance.Cam.ScreenToWorldPoint(
                    new Vector2((float) Screen.width / 2, 0));
                position.x = pos.x + 0.5f;
                position.y = Stage.StageStartPosition;
            }
            var skillName = $"{skillInfo.skillCategory}_{size}_{elemental}".ToLower();
            var go = _pool.Get(skillName, false, position);
            if (go == null)
            {
                go = _pool.Get(skillName, true, position);
            }
            var effect = go.GetComponent<T>();
            if (effect == null)
            {
                Debug.LogError(skillName);
            }
            effect.target = target;
            effect.Stop();
            return effect;
        }

        public SkillCastingVFX Get(Vector3 position, Model.Skill.SkillInfo skillInfo)
        {
            var elemental = skillInfo.Elemental ?? ElementalType.Normal;
            var skillName = $"casting_{elemental}".ToLower();
            var go = _pool.Get(skillName, false, position);
            var effect = go.GetComponent<SkillCastingVFX>();
            effect.Stop();
            return effect;
        }
    }
}
