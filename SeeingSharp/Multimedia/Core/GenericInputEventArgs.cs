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
using System.Collections.Generic;
using SeeingSharp.Multimedia.Input;

namespace SeeingSharp.Multimedia.Core
{
    public class GenericInputEventArgs : EventArgs
    {
        private List<InputFrame> _inputFrames;

        /// <summary>
        /// Gets a list containing all InputFrames.
        /// </summary>
        public IReadOnlyList<InputFrame> InputFrames => _inputFrames;

        internal GenericInputEventArgs()
        {
            _inputFrames = new List<InputFrame>(16);
        }

        internal void NotifyNewPass(List<InputFrame> inputFrames)
        {
            _inputFrames.Clear();
            _inputFrames.AddRange(inputFrames);
        }
    }
}