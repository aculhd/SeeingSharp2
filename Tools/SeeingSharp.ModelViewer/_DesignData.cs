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

using System.Collections.Generic;
using System.Threading;
using FakeItEasy;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;

namespace SeeingSharp.ModelViewer
{
    public static class DesignData
    {
        public static MainWindowVM MainWindowVM
        {
            get
            {
                var renderLoopHost = A.Fake<IRenderLoopHost>();

                var result = new MainWindowVM(new RenderLoop(
                    new SynchronizationContext(),
                    renderLoopHost,
                    true));
                result.IsLoading = true;
                return result;
            }
        }

        public static SceneBrowserViewModel SceneBrowserViewModel
        {
            get
            {
                return new SceneBrowserViewModel( () =>
                {
                    var sceneObjects = new List<SceneObjectInfo>();
                    sceneObjects.Add(new SceneObjectInfo(new Mesh(NamedOrGenericKey.Empty)));
                    sceneObjects.Add(new SceneObjectInfo(new Mesh(NamedOrGenericKey.Empty)));
                    sceneObjects.Add(new SceneObjectInfo(new Mesh(NamedOrGenericKey.Empty)));

                    return sceneObjects;
                });
            }
        }
    }
}
