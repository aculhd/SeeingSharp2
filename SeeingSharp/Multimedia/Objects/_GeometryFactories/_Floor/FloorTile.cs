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
namespace SeeingSharp.Multimedia.Objects
{
    #region using

    using SeeingSharp.Util;

    #endregion

    public class FloorTile
    {
        private FloorTileInfo m_tileInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="FloorTile"/> class.
        /// </summary>
        /// <param name="xPos">The x pos.</param>
        /// <param name="yPos">The y pos.</param>
        internal FloorTile(int xPos, int yPos)
        {
            XPos = xPos;
            YPos = yPos;
            m_tileInfo = new FloorTileInfo(NamedOrGenericKey.Empty);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FloorTile"/> class.
        /// </summary>
        /// <param name="xPos">The x pos.</param>
        /// <param name="yPos">The y pos.</param>
        /// <param name="tileInfo">Gets some generic information about the tile.</param>
        internal FloorTile(int xPos, int yPos, FloorTileInfo tileInfo)
        {
            XPos = xPos;
            YPos = yPos;
            m_tileInfo = tileInfo;
        }

        /// <summary>
        /// Gets the material used by this tile.
        /// </summary>
        public NamedOrGenericKey Material
        {
            get { return m_tileInfo.Material; }
        }

        /// <summary>
        /// Gets the x-position of the tile.
        /// </summary>
        public int XPos { get; }

        /// <summary>
        /// Gets the y-position of the tile.
        /// </summary>
        public int YPos { get; }
    }
}