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
using System.Threading.Tasks;

namespace SeeingSharp.Multimedia.Core
{
    public static class Extensions
    {
        /// <summary>
        /// Waits some time before continuing with next animation sequence.
        /// </summary>
        /// <param name="builder">The AnimationSequenceBuilder object.</param>
        /// <param name="milliseconds">The total milliseconds to wait.</param>
        public static IAnimationSequenceBuilder<ObjectType> Delay<ObjectType>(this IAnimationSequenceBuilder<ObjectType> builder, int milliseconds)
            where ObjectType : class
        {
            builder.Add(new DelayAnimation(TimeSpan.FromMilliseconds(milliseconds)));
            return builder;
        }

        /// <summary>
        /// Waits some time before continuing with next animation sequence.
        /// </summary>
        /// <param name="builder">The AnimationSequenceBuilder object.</param>
        /// <param name="duration">Total duration to wait.</param>
        public static IAnimationSequenceBuilder<ObjectType> Delay<ObjectType>(this IAnimationSequenceBuilder<ObjectType> builder, TimeSpan duration)
            where ObjectType : class
        {
            builder.Add(new DelayAnimation(duration));
            return builder;
        }

        /// <summary>
        /// Waits until given task has finished executing.
        /// </summary>
        /// <param name="builder">The AnimationSequenceBuilder object.</param>
        /// <param name="blockingTask">The Task for which we have to wait.</param>
        public static IAnimationSequenceBuilder<ObjectType> WaitTaskFinished<ObjectType>(this IAnimationSequenceBuilder<ObjectType> builder, Task blockingTask)
            where ObjectType : class
        {
            builder.Add(new WaitTaskFinishedAnimation(blockingTask));
            return builder;
        }

        /// <summary>
        /// Waits until the given condition returns true.
        /// </summary>
        /// <param name="builder">The AnimationSequenceBuilder object.</param>
        /// <param name="checkFunction">Return true to continue with the animation.</param>
        public static IAnimationSequenceBuilder<ObjectType> WaitForCondition<ObjectType>(this IAnimationSequenceBuilder<ObjectType> builder, Func<bool> checkFunction)
            where ObjectType : class
        {
            builder.Add(new WaitForConditionPassedAnimation(checkFunction));
            return builder;
        }

        /// <summary>
        /// Waits until previous animation steps are finished.
        /// </summary>
        /// <param name="builder">The AnimationSequenceBuilder object.</param>
        public static IAnimationSequenceBuilder<ObjectType> WaitFinished<ObjectType>(this IAnimationSequenceBuilder<ObjectType> builder)
            where ObjectType : class
        {
            builder.Add(new WaitFinishedAnimation());
            return builder;
        }

        /// <summary>
        /// Wait until given time has passed.
        /// </summary>
        /// <typeparam name="ObjectType">The type of the object to be animated.</typeparam>
        /// <param name="builder">The AnimationSequenceBuilder object.</param>
        /// <param name="waittime">The total time to wait.</param>
        public static IAnimationSequenceBuilder<ObjectType> WaitUntilTimePassed<ObjectType>(this IAnimationSequenceBuilder<ObjectType> builder, TimeSpan waittime)
            where ObjectType : class
        {
            builder.Add(new WaitTimePassedAnimation(waittime));
            return builder;
        }

        /// <summary>
        /// Adds a lazy animation object.
        /// </summary>
        /// <param name="builder">The AnimationSequenceBuilder object.</param>
        /// <param name="animationCreator">A lambda that creates the animation.</param>
        public static IAnimationSequenceBuilder<ObjectType> Lazy<ObjectType>(this IAnimationSequenceBuilder<ObjectType> builder, Func<IAnimation> animationCreator)
            where ObjectType : class
        {
            builder.Add(new LazyAnimation(animationCreator));
            return builder;
        }

        /// <summary>
        /// Builds a lazy animation object using the given child sequence.
        /// </summary>
        /// <param name="builder">The AnimationSequenceBuilder object.</param>
        /// <param name="childSequenceBuilder">A SequenceBuilder building a child sequence.</param>
        public static IAnimationSequenceBuilder<ObjectType> Lazy<ObjectType>(this IAnimationSequenceBuilder<ObjectType> builder, Action<IAnimationSequenceBuilder<ObjectType>> childSequenceBuilder)
            where ObjectType : class
        {
            return builder.Lazy(() =>
            {
                var result = new AnimationHandler(builder.AnimationHandler.Owner);
                IAnimationSequenceBuilder<ObjectType> childBuilder = new AnimationSequenceBuilder<ObjectType>(result);
                childSequenceBuilder(childBuilder);

                if (childBuilder.ItemCount == 0 && !childBuilder.Applied)
                {
                    childBuilder.Apply();
                }

                if (!childBuilder.Applied)
                {
                    throw new InvalidOperationException("Child sequence was not correctly applied (call to Apply is missing!)");
                }

                return result;
            });
        }

        /// <summary>
        /// Calls the given action.
        /// </summary>
        /// <param name="builder">The AnimationSequenceBuilder object.</param>
        /// <param name="actionToCall">The action to call on this step of the animation.</param>
        public static IAnimationSequenceBuilder<ObjectType> CallAction<ObjectType>(this IAnimationSequenceBuilder<ObjectType> builder, Action actionToCall)
            where ObjectType : class
        {
            if (actionToCall == null) { throw new ArgumentNullException("actionToCall"); }

            builder.Add(new CallActionAnimation(actionToCall));
            return builder;
        }

        /// <summary>
        /// Calls the given action.
        /// </summary>
        /// <param name="builder">The AnimationSequenceBuilder object.</param>
        /// <param name="actionToCall">The action to call on this step of the animation.</param>
        /// <param name="cancelAction">The action to be called when this animation would be canceled.</param>
        public static IAnimationSequenceBuilder<ObjectType> CallAction<ObjectType>(this IAnimationSequenceBuilder<ObjectType> builder, Action actionToCall, Action cancelAction)
            where ObjectType : class
        {
            if (actionToCall == null) { throw new ArgumentNullException("actionToCall"); }

            builder.Add(new CallActionAnimation(actionToCall, cancelAction));
            return builder;
        }

        /// <summary>
        /// Increases a float value by a given total increase value over the given duration.
        /// </summary>
        /// <param name="builder">The AnimationSequenceBuilder object.</param>
        /// <param name="valueGetter">The value getter.</param>
        /// <param name="valueSetter">The value setter.</param>
        /// <param name="totalIncrease">The value to increase in total.</param>
        /// <param name="duration">Total duration to wait.</param>
        public static IAnimationSequenceBuilder<ObjectType> ChangeFloatBy<ObjectType>(this IAnimationSequenceBuilder<ObjectType> builder, Func<float> valueGetter, Action<float> valueSetter, float totalIncrease, TimeSpan duration)
            where ObjectType : class
        {
            builder.Add(new ChangeFloatByAnimation(builder.TargetObject, valueGetter, valueSetter, totalIncrease, duration));
            return builder;
        }

        /// <summary>
        /// Increases a int value by a given total increase value over the given duration.
        /// </summary>
        /// <param name="builder">The AnimationSequenceBuilder object.</param>
        /// <param name="valueGetter">The value getter.</param>
        /// <param name="valueSetter">The value setter.</param>
        /// <param name="totalIncrease">The value to increase in total.</param>
        /// <param name="duration">Total duration to wait.</param>
        public static IAnimationSequenceBuilder<ObjectType> ChangeIntBy<ObjectType>(this IAnimationSequenceBuilder<ObjectType> builder, Func<int> valueGetter, Action<int> valueSetter, int totalIncrease, TimeSpan duration)
            where ObjectType : class
        {
            builder.Add(new ChangeIntByAnimation(builder.TargetObject, valueGetter, valueSetter, totalIncrease, duration));
            return builder;
        }
    }
}