﻿/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/

using System;
using SeeingSharp.Checking;

namespace SeeingSharp.Multimedia.Core
{
    public class ChangeAccentuationToAnimation : AnimationBase
    {
        // Parameters
        private float _startAccentuation;
        private float _moveAccentuation;
        private float _targetAccentuation;
        private IAnimatableObjectAccentuation _targetObject;

        /// <summary>
        /// Initialize a new Instance of the <see cref="ChangeAccentuationToAnimation" /> class.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="targetAccentuation">The target accentuation.</param>
        /// <param name="duration">The duration.</param>
        /// <exception cref="System.Exception">Accentuation value can be between 0 and 1, not greater than 1 and not lower than 0!</exception>
        public ChangeAccentuationToAnimation(IAnimatableObjectAccentuation targetObject, float targetAccentuation, TimeSpan duration)
            : base(targetObject, AnimationType.FixedTime, duration)
        {
            targetObject.EnsureNotNull(nameof(targetObject));
            targetAccentuation.EnsureInRange(0f, 1f, nameof(targetAccentuation));
            duration.EnsureLongerThanZero(nameof(duration));

            _targetObject = targetObject;
            _targetAccentuation = targetAccentuation;

            if (targetAccentuation < 0f || targetAccentuation > 1f)
            {
                throw new Exception("Accentuation value can be between 0 and 1, not greater than 1 and not lower than 0!");
            }
        }

        /// <summary>
        /// Called when animation starts.
        /// </summary>
        protected override void OnStartAnimation()
        {
            _startAccentuation = _targetObject.AccentuationFactor;
            _moveAccentuation = _targetAccentuation - _startAccentuation;
        }

        /// <summary>
        /// Called each time the CurrentTime value gets updated.
        /// </summary>
        protected override void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
            var changeFactor = this.CurrentTime.Ticks / (float)this.FixedTime.Ticks;
            _targetObject.AccentuationFactor = _startAccentuation + _moveAccentuation * changeFactor;
        }

        /// <summary>
        /// Called when the FixedTime animation has finished.
        /// (Sets final state to the target object and clears all runtime values).
        /// </summary>
        protected override void OnFixedTimeAnimationFinished()
        {
            _targetObject.AccentuationFactor = _targetAccentuation;

            _moveAccentuation = 0;
            _startAccentuation = 1;
        }
    }
}