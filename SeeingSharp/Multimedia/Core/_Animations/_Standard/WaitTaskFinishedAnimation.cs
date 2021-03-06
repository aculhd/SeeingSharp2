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

using System.Threading.Tasks;

namespace SeeingSharp.Multimedia.Core
{
    public class WaitTaskFinishedAnimation : AnimationBase
    {
        private Task _taskToWaitFor;

        /// <summary>
        /// Is this animation a blocking animation?
        /// If true, all following animation have to wait for finish-event.
        /// </summary>
        public override bool IsBlockingAnimation => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="WaitTaskFinishedAnimation" /> class.
        /// </summary>
        public WaitTaskFinishedAnimation(Task taskToWaitFor)
            : base(null, AnimationType.FinishedByEvent)
        {
            _taskToWaitFor = taskToWaitFor;
        }

        protected override void OnCurrentTimeUpdated(IAnimationUpdateState updateState, AnimationState animationState)
        {
            base.OnCurrentTimeUpdated(updateState, animationState);

            if (_taskToWaitFor.IsCanceled ||
               _taskToWaitFor.IsCompleted ||
               _taskToWaitFor.IsFaulted)
            {
                this.NotifyAnimationFinished();
            }
        }
    }
}