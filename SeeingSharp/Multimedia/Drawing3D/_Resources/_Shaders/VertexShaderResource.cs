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
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class VertexShaderResource : ShaderResource
    {
        // Resources for Direct3D 11 rendering
        private D3D11.VertexShader _vertexShader;
        private Dictionary<D3D11.InputElement[], D3D11.InputLayout> _inputLayouts;

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public override bool IsLoaded => _vertexShader != null;

        /// <summary>
        /// Gets the loaded VertexShader object.
        /// </summary>
        internal D3D11.VertexShader VertexShader => _vertexShader;

        /// <summary>
        /// Initializes a new instance of the <see cref="VertexShaderResource" /> class.
        /// </summary>
        /// <param name="shaderProfile">Shader profile used for compiling.</param>
        /// <param name="resourceLink">The resourceLink.</param>
        public VertexShaderResource(string shaderProfile, ResourceLink resourceLink)
            : base(shaderProfile, resourceLink, ShaderResourceKind.HlsFile)
        {
            _inputLayouts = new Dictionary<D3D11.InputElement[], D3D11.InputLayout>();
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        protected internal override void LoadShader(EngineDevice device, byte[] shaderBytecode)
        {
            if (_vertexShader == null)
            {
                _vertexShader = new D3D11.VertexShader(device.DeviceD3D11_1, shaderBytecode);
            }
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        protected internal override void UnloadShader()
        {
            foreach (var actInputLayout in _inputLayouts.Values)
            {
                SeeingSharpUtil.DisposeObject(actInputLayout);
            }
            _inputLayouts.Clear();

            _vertexShader = SeeingSharpUtil.DisposeObject(_vertexShader);
        }

        /// <summary>
        /// Generates the requested input layout.
        /// </summary>
        /// <param name="device">The device on which to create the input layout.</param>
        /// <param name="inputElements">An array of InputElements describing vertex input structure.</param>
        internal D3D11.InputLayout GetInputLayout(EngineDevice device, D3D11.InputElement[] inputElements)
        {
            if (_inputLayouts.TryGetValue(inputElements, out var inputLayout))
            {
                return inputLayout;
            }

            inputLayout = new D3D11.InputLayout(device.DeviceD3D11_1, this.ShaderBytecode, inputElements);
            _inputLayouts.Add(inputElements, inputLayout);
            return inputLayout;
        }
    }
}
