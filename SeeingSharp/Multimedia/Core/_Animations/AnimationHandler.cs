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

namespace SeeingSharp.Multimedia.Core
{
    public class AnimationHandler : AnimationSequence
    {
        /// <summary>
        /// Gets the owner object.
        /// </summary>
        public object Owner { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationHandler"/> class.
        /// </summary>
        /// <param name="owner">The owner object of this AnimationHandler.</param>
        public AnimationHandler(object owner)
        {
            this.Owner = owner;
        }

        /// <summary>
        /// Called when an animation throws an exception during execution.
        /// </summary>
        /// <param name="animation">The failed animation.</param>
        /// <param name="ex">The exception thrown.</param>
        protected override AnimationFailedReaction OnAnimationFailed(IAnimation animation, Exception ex)
        {
            return AnimationFailedReaction.RemoveAndContinue;
        }

        /// <summary>
        /// Starts building an animation sequence for the current target object.
        /// </summary>
        internal IAnimationSequenceBuilder<TTargetObject> BuildAnimationSequence<TTargetObject>()
            where TTargetObject : class, IAnimatableObject
        {
            return new AnimationSequenceBuilder<TTargetObject>(this);
        }

        /// <summary>
        /// Starts building an animation sequence for the current target object.
        /// </summary>
        /// <param name="animatedObject">The target object which is to be animated.</param>
        internal IAnimationSequenceBuilder<TTargetObject> BuildAnimationSequence<TTargetObject>(TTargetObject animatedObject)
            where TTargetObject : class
        {
            return new AnimationSequenceBuilder<TTargetObject>(this, animatedObject);
        }
    }
}