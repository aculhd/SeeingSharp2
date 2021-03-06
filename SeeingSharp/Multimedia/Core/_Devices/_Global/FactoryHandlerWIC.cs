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
using SeeingSharp.Util;
using SharpDX.WIC;

namespace SeeingSharp.Multimedia.Core
{
    public class FactoryHandlerWIC : IDisposable, ICheckDisposed
    {
        private ImagingFactory _wicFactory;

        public bool IsDisposed => this.Factory == null;

        /// <summary>
        /// Gets the WIC factory object.
        /// </summary>
        internal ImagingFactory Factory
        {
            get
            {
                if(_wicFactory == null){ throw new ObjectDisposedException(nameof(FactoryHandlerWIC)); }
                return _wicFactory;
            }
            private set => _wicFactory = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FactoryHandlerWIC" /> class.
        /// </summary>
        internal FactoryHandlerWIC(GraphicsCoreConfiguration coreConfiguration)
        {
            this.Factory = new ImagingFactory();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Factory = SeeingSharpUtil.DisposeObject(this.Factory);
        }
    }
}