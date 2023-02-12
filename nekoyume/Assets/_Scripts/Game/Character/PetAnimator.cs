﻿using UnityEngine;

namespace Nekoyume.Game.Character
{
    public class PetAnimator : SkeletonAnimator
    {
        public PetAnimator(GameObject root) : base(root)
        {
        }

        public void Play(PetAnimation.Type type, float normalizedTime = 0f)
        {
            if (!ValidateAnimator())
            {
                return;
            }

            Animator.Play(type.ToString(), BaseLayerIndex, normalizedTime);
        }
    }
}
