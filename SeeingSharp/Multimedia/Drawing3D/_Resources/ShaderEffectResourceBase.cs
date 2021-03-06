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

using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public abstract class ShaderEffectResourceBase : Resource
    {
        private static readonly NamedOrGenericKey s_resKeyVertexShader = GraphicsCore.GetNextGenericResourceKey();

        // Resources
        private VertexShaderResource _vertexShader;
        private DefaultResources _defaultResources;

        /// <summary>
        /// Applies alpha based sprite rendering.
        /// </summary>
        /// <param name="deviceContext">The target device context.</param>
        protected void ApplySpriteRendering(D3D11.DeviceContext deviceContext)
        {
            deviceContext.VertexShader.Set(_vertexShader.VertexShader);

            deviceContext.OutputMerger.DepthStencilState = _defaultResources.DepthStencilStateAlwaysPassDepth;
        }

        /// <summary>
        /// Discards alpha based sprite rendering.
        /// </summary>
        /// <param name="deviceContext">The target device context.</param>
        protected void DiscardSpriteRendering(D3D11.DeviceContext deviceContext)
        {
            deviceContext.OutputMerger.DepthStencilState = _defaultResources.DepthStencilStateDefault;
        }

        /// <summary>
        /// Applies alpha based sprite rendering.
        /// </summary>
        /// <param name="deviceContext">The target device context.</param>
        protected void ApplyAlphaBasedSpriteRendering(D3D11.DeviceContext deviceContext)
        {
            deviceContext.VertexShader.Set(_vertexShader.VertexShader);

            deviceContext.OutputMerger.DepthStencilState = _defaultResources.DepthStencilStateAlwaysPassDepth;
            deviceContext.OutputMerger.BlendState = _defaultResources.AlphaBlendingBlendState;
        }

        /// <summary>
        /// Discards alpha based sprite rendering.
        /// </summary>
        /// <param name="deviceContext">The target device context.</param>
        protected void DiscardAlphaBasedSpriteRendering(D3D11.DeviceContext deviceContext)
        {
            deviceContext.OutputMerger.BlendState = _defaultResources.DefaultBlendState;
            deviceContext.OutputMerger.DepthStencilState = _defaultResources.DepthStencilStateDefault;
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _defaultResources = resources.DefaultResources;
            _vertexShader = resources.GetResourceAndEnsureLoaded(
                s_resKeyVertexShader,
                () => GraphicsHelper.Internals.GetVertexShaderResource(device, "Postprocessing", "PostprocessVertexShader"));
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="resources">Parent ResourceDictionary.</param>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _defaultResources = null;
            _vertexShader = null;
        }
    }
}
