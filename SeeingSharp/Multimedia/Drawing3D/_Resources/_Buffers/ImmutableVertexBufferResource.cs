﻿using System;
using System.Collections.Generic;
using System.Text;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class ImmutableVertexBufferResource<T> : Resource
        where T : struct
    {
        // Direct3D resources
        private D3D11.Buffer m_buffer;

        // Configuration
        private Func<T[]> m_bufferDataFactory;

        public ImmutableVertexBufferResource(Func<T[]> bufferDataFactory)
        {
            m_bufferDataFactory = bufferDataFactory;
        }

        protected override void LoadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            m_buffer = GraphicsHelper.CreateImmutableVertexBuffer(
                device, m_bufferDataFactory());
        }

        protected override void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources)
        {
            SeeingSharpUtil.SafeDispose(ref m_buffer);
        }

        internal D3D11.Buffer Buffer => m_buffer;

        public override bool IsLoaded => m_buffer != null;
    }
}
