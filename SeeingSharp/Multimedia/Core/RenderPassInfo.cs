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
using System.Collections.ObjectModel;

namespace SeeingSharp.Multimedia.Core
{
    public class RenderPassInfo
    {
        // All available render passes
        public static readonly RenderPassInfo PASS_PLAIN_RENDER = new RenderPassInfo("DefaultPlainRender");
        public static readonly RenderPassInfo PASS_LINE_RENDER = new RenderPassInfo("LineRender");
        public static readonly RenderPassInfo PASS_TRANSPARENT_RENDER = new RenderPassInfo("DefaultTransparentRender");
        public static readonly RenderPassInfo PASS_SPRITE_BATCH = new RenderPassInfo("SpriteBatchRender");
        public static readonly RenderPassInfo PASS_2D_OVERLAY = new RenderPassInfo("2D-Overlay", true);

        /// <summary>
        /// Gets a collection containing all render passes.
        /// </summary>
        public static ReadOnlyCollection<RenderPassInfo> AllRenderPasses { get; }

        /// <summary>
        /// Gets the name of this pass.
        /// </summary>
        public string Name { get; }

        public bool IsSorted { get; }

        /// <summary>
        /// Initializes the <see cref="RenderPassInfo" /> class.
        /// </summary>
        static RenderPassInfo()
        {
            var renderPasses = new List<RenderPassInfo>();
            AllRenderPasses = new ReadOnlyCollection<RenderPassInfo>(renderPasses);

            renderPasses.Add(PASS_PLAIN_RENDER);
            renderPasses.Add(PASS_LINE_RENDER);
            renderPasses.Add(PASS_TRANSPARENT_RENDER);
            renderPasses.Add(PASS_SPRITE_BATCH);
            renderPasses.Add(PASS_2D_OVERLAY);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderPassInfo" /> class.
        /// </summary>
        internal RenderPassInfo(string name, bool isSorted = false)
        {
            this.Name = name;
            this.IsSorted = isSorted;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}