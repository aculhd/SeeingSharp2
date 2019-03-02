﻿#region License information
/*
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
#endregion

using System;

namespace SeeingSharp.Multimedia.Core
{
    #region using
    #endregion

    public class AnimationMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationMetadata"/> class.
        /// </summary>
        /// <param name="animation">The animation.</param>
        /// <param name="finishedCallback">The finished callback.</param>
        public AnimationMetadata(IAnimation animation, Action finishedCallback)
        {
            Animation = animation;
            FinishedCallback = finishedCallback;
        }

        /// <summary>
        /// Gets the animation.
        /// </summary>
        public IAnimation Animation
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the finished callback.
        /// </summary>
        public Action FinishedCallback
        {
            get;
            private set;
        }
    }
}
