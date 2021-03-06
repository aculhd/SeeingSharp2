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

using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public interface IModelExporter
    {
        /// <summary>
        /// Exports the model(s) defined in the given model container to the given model file.
        /// </summary>
        /// <param name="modelContainer">The model(s) to export.</param>
        /// <param name="targetFile">The path to the target file.</param>
        /// <param name="exportOptions">Some configuration for the exporter.</param>
        void ExportModelAsync(ExportModelContainer modelContainer, ResourceLink targetFile, ExportOptions exportOptions);
    }
}
