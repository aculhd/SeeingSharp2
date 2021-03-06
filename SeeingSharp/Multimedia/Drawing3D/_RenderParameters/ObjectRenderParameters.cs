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

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class ObjectRenderParameters : Resource
    {
        // Resource keys
        private readonly NamedOrGenericKey _keyConstantBuffer = GraphicsCore.GetNextGenericResourceKey();

        // Resources
        private TypeSafeConstantBufferResource<CBPerObject> _cbPerObject;

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded => _cbPerObject != null;

        /// <summary>
        /// Does this object needs refreshing?
        /// </summary>
        internal bool NeedsRefresh;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectRenderParameters" /> class.
        /// </summary>
        internal ObjectRenderParameters()
        {
            NeedsRefresh = true;
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _cbPerObject = resources.GetResourceAndEnsureLoaded(
                _keyConstantBuffer,
                () => new TypeSafeConstantBufferResource<CBPerObject>());
            NeedsRefresh = true;
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            _cbPerObject = null;
        }

        /// <summary>
        /// Triggers unloading of this resource.
        /// </summary>
        internal void MarkForUnloading()
        {
            this.Dictionary?.MarkForUnloading(this);
        }

        /// <summary>
        /// Updates all parameters.
        /// </summary>
        /// <param name="renderState">The render state on which to apply.</param>
        /// <param name="cbPerObject">Constant buffer data.</param>
        internal void UpdateValues(RenderState renderState, CBPerObject cbPerObject)
        {
            _cbPerObject.SetData(renderState.Device.DeviceImmediateContextD3D11, cbPerObject);
        }

        /// <summary>
        /// Applies all parameters.
        /// </summary>
        /// <param name="renderState">The render state on which to apply.</param>
        internal void Apply(RenderState renderState)
        {
            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;

            // Apply constant buffer on shaders
            deviceContext.VertexShader.SetConstantBuffer(2, _cbPerObject.ConstantBuffer);
            deviceContext.PixelShader.SetConstantBuffer(2, _cbPerObject.ConstantBuffer);
        }
    }
}
