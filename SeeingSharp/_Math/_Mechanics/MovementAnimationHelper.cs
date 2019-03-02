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

using System;
using SharpDX;

namespace SeeingSharp
{
    #region using
    #endregion

    /// <summary>
    /// This class is responsible for standard movement calculation depending by
    /// maximum speed, acceleration and deceleration.
    /// 
    /// see: http://www.frustfrei-lernen.de/mechanik/gleichfoermige-bewegung-physik.html
    /// see: http://www.frustfrei-lernen.de/mechanik/gleichmaessig-beschleunigte-bewegung-physik.html
    /// </summary>
    public class MovementAnimationHelper
    {
        /// <summary>
        /// Gets the move distance by the given timespan.
        /// </summary>
        /// <param name="elapsedTime"></param>
        public Vector3 GetPartialMoveDistance(TimeSpan elapsedTime)
        {
            var elapsedSeconds = elapsedTime.TotalSeconds;

            var movedLength = 0.0;
            if (elapsedSeconds < m_accelerationSeconds)
            {
                // We are in acceleration phase
                movedLength = 0.5 * m_speed.Acceleration * Math.Pow(elapsedSeconds, 2.0);
            }
            else if (elapsedSeconds < m_accelerationSeconds + m_fullSpeedSeconds)
            {
                // We are in full-speed phase
                var elapsedSecondsFullSpeed = elapsedSeconds - m_accelerationSeconds;
                movedLength = m_accelerationLength + m_speed.MaximumSpeed * elapsedSecondsFullSpeed;
            }
            else if (elapsedSeconds < m_accelerationSeconds + m_fullSpeedSeconds + m_decelerationSeconds)
            {
                // We are in deceleration phase
                var elapsedSecondsDeceleration = elapsedSeconds - (m_accelerationSeconds + m_fullSpeedSeconds);
                movedLength =
                    m_accelerationLength + m_fullSpeedLength +
                    0.5 * m_speed.Decelration * Math.Pow(elapsedSecondsDeceleration, 2.0) + m_reachedMaxSpeed * elapsedSecondsDeceleration;
            }
            else
            {
                // Movement is finished
                return MovementVector;
            }

            // Generate the full movement vector
            return m_movementNormal * (float)movedLength;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MovementAnimationHelper" /> class.
        /// </summary>
        /// <param name="speed">The speed data.</param>
        /// <param name="movementDistance">The full distance for the movement.</param>
        public MovementAnimationHelper(MovementSpeed speed, Vector3 movementDistance)
        {
            // Store main parameters
            MovementVector = movementDistance;
            m_speed = speed;
            m_speed.ValidateWithException();

            // Calculate length and normal
            var length = 0f;
            m_movementNormal = Vector3.Normalize(movementDistance);
            length = movementDistance.Length();
            if (length <= EngineMath.TOLERANCE_DOUBLE_POSITIVE)
            {
                // No movement.. leave all values on defaults
                return;
            }

            // Calculate acceleration values
            m_accelerationLength = 0f;
            m_accelerationSeconds = 0f;
            if (m_speed.Acceleration > EngineMath.TOLERANCE_DOUBLE_POSITIVE)
            {
                m_accelerationSeconds = m_speed.MaximumSpeed / m_speed.Acceleration;
                m_accelerationLength = 0.5 * m_speed.Acceleration * Math.Pow(m_accelerationSeconds, 2.0);
            }

            // Calculate deceleration values
            m_decelerationLength = 0f;
            m_decelerationSeconds = 0f;
            if (m_speed.Decelration < EngineMath.TOLERANCE_DOUBLE_NEGATIVE)
            {
                m_decelerationSeconds = m_speed.MaximumSpeed / -m_speed.Decelration;
                m_decelerationLength = 0.5f * -m_speed.Decelration * Math.Pow(m_decelerationSeconds, 2.0);
            }

            // Handle the case where we don't reach full speed
            // => Change length values depending on percentage of these phases on whole movement
            var fullAccDecLength = m_accelerationLength + m_decelerationLength;
            m_fullSpeedLength = length;
            if (length < fullAccDecLength)
            {
                var accWeight = m_accelerationLength / fullAccDecLength;
                var decWeight = m_decelerationLength / fullAccDecLength;
                m_accelerationLength = length * accWeight;
                m_decelerationLength = length * decWeight;
                m_fullSpeedLength = 0.0;
                m_accelerationSeconds = m_speed.Acceleration > EngineMath.TOLERANCE_DOUBLE_POSITIVE ? Math.Pow(m_accelerationLength / (0.5 * speed.Acceleration), 0.5) : 0.0;
                m_decelerationSeconds = m_speed.Decelration < EngineMath.TOLERANCE_DOUBLE_NEGATIVE ? Math.Pow(m_decelerationLength / (0.5 * -speed.Decelration), 0.5) : 0.0;
                m_reachedMaxSpeed = m_speed.Acceleration * m_accelerationSeconds;
            }
            else
            {
                m_reachedMaxSpeed = m_speed.MaximumSpeed;
                m_fullSpeedLength = m_fullSpeedLength - fullAccDecLength;
            }
            m_fullSpeedSeconds = m_fullSpeedLength / m_speed.MaximumSpeed;
        }

        /// <summary>
        /// Gets the full movement time.
        /// </summary>
        public TimeSpan MovementTime => TimeSpan.FromSeconds(m_accelerationSeconds + m_fullSpeedSeconds + m_decelerationSeconds);

        /// <summary>
        /// Gets the time which is needed for acceleration phase.
        /// </summary>
        public TimeSpan AccelerationTime => TimeSpan.FromSeconds(m_accelerationSeconds);

        /// <summary>
        /// Gets the time which is needed for full-speed phase.
        /// </summary>
        public TimeSpan FullSpeedTime => TimeSpan.FromSeconds(m_fullSpeedSeconds);

        /// <summary>
        /// Gets the time which is needed for deceleration phase.
        /// </summary>
        public TimeSpan DecelerationTime => TimeSpan.FromSeconds(m_decelerationSeconds);

        /// <summary>
        /// Gets the full movement vector.
        /// </summary>
        public Vector3 MovementVector { get; }

        #region Outer parameters
        private MovementSpeed m_speed;
        private Vector3 m_movementNormal;
        #endregion

        #region All values needed location calculation
        private double m_accelerationLength;
        private double m_accelerationSeconds;
        private double m_decelerationLength;
        private double m_decelerationSeconds;
        private double m_fullSpeedLength;
        private double m_fullSpeedSeconds;
        private double m_reachedMaxSpeed;
        #endregion
    }
}