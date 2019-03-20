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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Core
{
    public class AnimationSequence : IAnimation
    {
        private Thread m_currentWorkingThread;

        private int m_currentWorkingThreadCount;
        private ConcurrentQueue<Action> m_preUpdateActions;

        private int m_preUpdateActionsCount;
        private Queue<IAnimation> m_runningAnimations;
        private int m_runningAnimationsCount;
        private Queue<Queue<IAnimation>> m_runningSecondaryAnimations;
        private int m_runningSecondaryAnimationsCount;
        private TimeSpan m_timeTillNextPartFinished;

        /// <summary>
        /// Raises when an animation within this sequence has failed.
        /// </summary>
        public event EventHandler<AnimationFailedEventArgs> AnimationFailed;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimationSequence"/> class.
        /// </summary>
        public AnimationSequence()
        {
            m_preUpdateActions = new ConcurrentQueue<Action>();
            m_runningAnimations = new Queue<IAnimation>();
            m_runningSecondaryAnimations = new Queue<Queue<IAnimation>>();
            m_timeTillNextPartFinished = SeeingSharpConstants.UPDATE_STATE_MAX_TIME;
        }

        /// <summary>
        /// Begins to cancel all animations managed by this AnimationSequence.
        /// </summary>
        public void BeginCancelAnimation()
        {
            m_preUpdateActions.Enqueue(this.CancelAnimations);
            Interlocked.Increment(ref m_preUpdateActionsCount);
        }

        /// <summary>
        /// Begins to cancel all animations managed by this AnimationSequence and animating the given target object.
        /// </summary>
        /// <param name="targetObject">The target object for which to cancel all animations.</param>
        public void BeginCancelAnimation(object targetObject)
        {
            m_preUpdateActions.Enqueue(() =>
            {
                this.CancelAnimations(targetObject);
            });
            Interlocked.Increment(ref m_preUpdateActionsCount);
        }

        /// <summary>
        /// Cancels all animations.
        /// </summary>
        public void CancelAnimations()
        {
            this.PreCheckExecutionInternal();
            try
            {
                // Cancel all primary animations
                if (m_runningAnimationsCount > 0)
                {
                    foreach (var actAnimation in m_runningAnimations)
                    {
                        actAnimation.Canceled = true;
                    }
                }

                // Cancel all secondary animations too
                if (m_runningSecondaryAnimationsCount > 0)
                {
                    foreach (var actSecondaryQueue in m_runningSecondaryAnimations)
                    {
                        foreach (var actAnimation in actSecondaryQueue)
                        {
                            actAnimation.Canceled = true;
                        }
                    }
                }
            }
            finally
            {
                this.PostCheckExecutionInternal();
            }
        }

        /// <summary>
        /// Cancel all animations with the given target object.
        /// </summary>
        /// <param name="targetObject">The target object for which to clear all animations.</param>
        public void CancelAnimations(object targetObject)
        {
            this.PreCheckExecutionInternal();
            try
            {
                // Cancel animations on primary queue
                if (m_runningAnimationsCount > 0)
                {
                    foreach (var actAnimation in m_runningAnimations)
                    {
                        if (actAnimation.IsObjectAnimated(targetObject))
                        {
                            actAnimation.Canceled = true;
                        }
                    }
                }

                // Cancel animations on secondary queue
                if (m_runningSecondaryAnimationsCount > 0)
                {
                    foreach (var actSecondaryQueue in m_runningSecondaryAnimations)
                    {
                        foreach (var actAnimation in actSecondaryQueue)
                        {
                            if (actAnimation.IsObjectAnimated(actAnimation))
                            {
                                actAnimation.Canceled = true;
                            }
                        }
                    }
                }
            }
            finally
            {
                this.PostCheckExecutionInternal();
            }
        }

        /// <summary>
        /// Performs this animation in a completely continuous way.
        /// </summary>
        public int CalculateContinuous(TimeSpan singleUpdateInterval)
        {
            this.PreCheckExecutionInternal();
            try
            {
                var totalStepCount = 0;

                this.PrecalculateAnimations();

                // Create shared UpdateState object
                while (this.CountRunningAnimations > 0)
                {
                    this.Update(new UpdateState(singleUpdateInterval));

                    totalStepCount++;
                }

                return totalStepCount;
            }
            finally
            {
                this.PostCheckExecutionInternal();
            }
        }

        /// <summary>
        /// Performs this animation in a completely event-driven way.
        /// </summary>
        public EventDrivenPassInfo CalculateEventDriven()
        {
            this.PreCheckExecutionInternal();
            try
            {
                this.PrecalculateAnimations();

                // Create shared UpdateState object
                var updateState = new UpdateState(TimeSpan.Zero);

                // Perform whole animation in an event-driven way
                var steps = new List<EventDrivenStepInfo>(12);

                while (this.CountRunningAnimations > 0)
                {
                    // Store some values for measurement
                    var countAnimationsBegin = this.CountRunningAnimations;

                    // Perform animation calculation
                    var timeTillNext = this.TimeTillCurrentAnimationStepFinished;
                    updateState.Reset(timeTillNext);
                    var updateResult = this.Update(updateState);

                    // Generate report data
                    steps.Add(new EventDrivenStepInfo
                    {
                        AnimationCount = updateResult.CountFinishedAnimations,
                        UpdateTime = timeTillNext
                    });
                }

                return new EventDrivenPassInfo(steps);
            }
            finally
            {
                this.PostCheckExecutionInternal();
            }
        }

        /// <summary>
        /// Precalculates previously added animations.
        /// After this call, the TimeTillCurrentAnimationStepFinished property is filled correctly.
        /// </summary>
        public void PrecalculateAnimations()
        {
            this.PreCheckExecutionInternal();
            try
            {
                // Execute all pre-update actions
                if (m_preUpdateActionsCount > 0)
                {
                    this.PerformPreupdateActionsInternal(
                        true);
                }
            }
            finally
            {
                this.PostCheckExecutionInternal();
            }
        }

        /// <summary>
        /// Updates all animations contained by this animation sequence.
        /// </summary>
        /// <param name="updateState">Current state of update process.</param>
        public AnimationUpdateResult Update(IAnimationUpdateState updateState)
        {
            return this.Update(updateState, null);
        }

        /// <summary>
        /// Checks if the given object is animated by this animation.
        /// </summary>
        /// <param name="targetObject">The object to check for.</param>
        /// <returns></returns>
        public bool IsObjectAnimated(object targetObject)
        {
            this.PreCheckExecutionInternal();
            try
            {
                // Check on primary animations
                if (m_runningAnimationsCount > 0)
                {
                    foreach (var actAnimation in m_runningAnimations)
                    {
                        if (actAnimation.IsObjectAnimated(targetObject))
                        {
                            return true;
                        }
                    }
                }

                // Check on all secondary animations too
                if (m_runningSecondaryAnimationsCount > 0)
                {
                    foreach (var actSecondaryQueue in m_runningSecondaryAnimations)
                    {
                        foreach (var actAnimation in actSecondaryQueue)
                        {
                            if (actAnimation.IsObjectAnimated(targetObject))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
            finally
            {
                this.PostCheckExecutionInternal();
            }
        }

        /// <summary>
        /// Called for each update step of this animation.
        /// </summary>
        /// <param name="updateState">The current state of the update pass.</param>
        /// <param name="animationState">The current state of the animation.</param>
        public AnimationUpdateResult Update(IAnimationUpdateState updateState, AnimationState animationState)
        {
            var countAnimationsFinished = 0;
            var prevIgnorePauseState = updateState.IgnorePauseState;

            this.PreCheckExecutionInternal();
            try
            {
                // Execute all pre-update actions
                if (m_preUpdateActionsCount > 0)
                {
                    this.PerformPreupdateActionsInternal();
                }

                // Cancel here if there are no animations at all
                if (m_runningAnimationsCount == 0 &&
                    m_runningSecondaryAnimationsCount == 0)
                {
                    m_timeTillNextPartFinished = SeeingSharpConstants.UPDATE_STATE_MAX_TIME;
                    return AnimationUpdateResult.Empty;
                }

                // Check collection counters for plausibility
                if (m_runningAnimationsCount < 0 ||
                   m_runningSecondaryAnimationsCount < 0 ||
                   m_preUpdateActionsCount < 0)
                {
                    throw new SeeingSharpGraphicsException("Internal error: Invalid animation count errors in Animation handler!");
                }

                var anySubAnimationFinishedOrCanceled = false;

                // Animation update loop for primary animations
                if (m_runningAnimationsCount > 0)
                {
                    anySubAnimationFinishedOrCanceled |= this.UpdateQueueInternal(updateState, animationState, m_runningAnimations);
                }

                // Animation update loop for secondary animations
                if (m_runningSecondaryAnimationsCount > 0)
                {
                    foreach (var actSecondaryQueue in m_runningSecondaryAnimations)
                    {
                        anySubAnimationFinishedOrCanceled |= this.UpdateQueueInternal(updateState, animationState, actSecondaryQueue);
                    }
                }

                // Dequeue all finished animations
                // Do also calculate time till next finished animation here
                while (anySubAnimationFinishedOrCanceled && m_runningAnimationsCount > 0)
                {
                    if (m_runningAnimations.First().IsFinishedOrCanceled())
                    {
                        anySubAnimationFinishedOrCanceled = true;
                        countAnimationsFinished++;
                        var currentAnimation = m_runningAnimations.Dequeue();

                        if (currentAnimation != null)
                        {
                            Interlocked.Decrement(ref m_runningAnimationsCount);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                if (anySubAnimationFinishedOrCanceled && m_runningSecondaryAnimationsCount > 0)
                {
                    foreach (var actSecondaryQueue in m_runningSecondaryAnimations)
                    {
                        while (actSecondaryQueue.Count > 0)
                        {
                            if (actSecondaryQueue.Peek().IsFinishedOrCanceled())
                            {
                                anySubAnimationFinishedOrCanceled = true;
                                countAnimationsFinished++;
                                actSecondaryQueue.Dequeue();
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                // Clear all secondary animations which are finished completely
                while (m_runningSecondaryAnimationsCount > 0 &&
                      m_runningSecondaryAnimations.Peek().Count == 0)
                {
                    var dummy = m_runningSecondaryAnimations.Dequeue();
                    Interlocked.Decrement(ref m_runningSecondaryAnimationsCount);
                }

                // Calculate time till next partial animation step
                if (anySubAnimationFinishedOrCanceled)
                {
                    this.UpdateTimeTillNextPartFinished();
                }
                else
                {
                    m_timeTillNextPartFinished = m_timeTillNextPartFinished - updateState.UpdateTime;
                    if(m_timeTillNextPartFinished < this.DefaultCycleTime)
                    {
                        m_timeTillNextPartFinished = this.DefaultCycleTime;
                    }
                }
            }
            finally
            {
                updateState.IgnorePauseState = prevIgnorePauseState;

                this.PostCheckExecutionInternal();
            }

            // Return some diagnostics about the executed animation
            var result = new AnimationUpdateResult
            {
                CountFinishedAnimations = countAnimationsFinished
            };

            return result;
        }

        /// <summary>
        /// Resets this animation.
        /// </summary>
        public void Reset()
        {
            this.ClearAnimations();
        }

        /// <summary>
        /// Gets the time in milliseconds till this animation is finished.
        /// This method is relevant for event-driven processing and tells the system by what amount the clock is to be increased next.
        /// </summary>
        /// <param name="previousMinFinishTime">The minimum TimeSpan previous animations take.</param>
        /// <param name="previousMaxFinishTime">The maximum TimeSpan previous animations take.</param>
        /// <param name="defaultCycleTime">The default cycle time if we would be in continuous calculation mode.</param>
        public TimeSpan GetTimeTillNextEvent(TimeSpan previousMinFinishTime, TimeSpan previousMaxFinishTime, TimeSpan defaultCycleTime)
        {
            return m_timeTillNextPartFinished;
        }

        /// <summary>
        /// Called when an animation throws an exception during execution.
        /// </summary>
        /// <param name="animation">The failed animation.</param>
        /// <param name="ex">The exception thrown.</param>
        protected virtual AnimationFailedReaction OnAnimationFailed(IAnimation animation, Exception ex)
        {
            return AnimationFailedReaction.ThrowException;
        }

        /// <summary>
        /// Clears all running animations.
        /// </summary>
        internal void ClearAnimations()
        {
            this.CancelAnimations();
        }

        /// <summary>
        /// Begins the given AnimationSequence objects.
        /// </summary>
        /// <param name="animationSequenceList">The list containing all animations to be added.</param>
        internal void BeginAnimation(IEnumerable<IAnimation> animationSequenceList)
        {
            // Queue this call to update logic..
            m_preUpdateActions.Enqueue(() =>
            {
                // Append a WaitFinished sequence if there are currently previous animations executing
                if (m_runningAnimationsCount > 0)
                {
                    m_runningAnimations.Enqueue(new WaitFinishedAnimation());
                    Interlocked.Increment(ref m_runningAnimationsCount);
                }

                // Append all given animations
                foreach (var actChildAnimation in animationSequenceList)
                {
                    m_runningAnimations.Enqueue(actChildAnimation);
                    Interlocked.Increment(ref m_runningAnimationsCount);
                }
            });
            Interlocked.Increment(ref m_preUpdateActionsCount);
        }

        /// <summary>
        /// Begins the given AnimationSequence object in a separate queue.
        /// </summary>
        /// <param name="animationSequenceList">The list containing all animations to be added.</param>
        internal void BeginSecondaryAnimation(IEnumerable<IAnimation> animationSequenceList)
        {
            // Queue this call to update logic..
            m_preUpdateActions.Enqueue(() =>
            {
                var newAnimationQueue = new Queue<IAnimation>(animationSequenceList);
                m_runningSecondaryAnimations.Enqueue(newAnimationQueue);
                Interlocked.Increment(ref m_runningSecondaryAnimationsCount);
            });
            Interlocked.Increment(ref m_preUpdateActionsCount);
        }

        /// <summary>
        /// Performs all preupdate actions.
        /// </summary>
        /// <param name="updateTimeTillNextTime">Update 'TimeTillNext' property?</param>
        private void PerformPreupdateActionsInternal(bool updateTimeTillNextTime = false)
        {
            // Walk through preupdate action queue and perform each
            while (m_preUpdateActions.TryDequeue(out var actPreUpdateAction))
            {
                Interlocked.Decrement(ref m_preUpdateActionsCount);
                actPreUpdateAction();
            }

            // Update the time till next sub-animation finished
            if (updateTimeTillNextTime)
            {
                this.UpdateTimeTillNextPartFinished();
            }
        }

        /// <summary>
        /// Calculates the duration which this AnimationHandler takes till the next event.
        /// </summary>
        private void UpdateTimeTillNextPartFinished()
        {
            var timeTillNextEvent = SeeingSharpConstants.UPDATE_STATE_MAX_TIME;

            // Cancel here if there are no animations at all
            if (m_runningAnimationsCount == 0 &&
                m_runningSecondaryAnimationsCount == 0)
            {
                m_timeTillNextPartFinished = SeeingSharpConstants.UPDATE_STATE_MAX_TIME;
                return;
            }

            // Calculate sub-animation time using main animation
            if (m_runningAnimationsCount > 0)
            {
                var timeTillNextStepMin = SeeingSharpConstants.UPDATE_STATE_MAX_TIME;
                var timeTillNextStepMax = TimeSpan.MinValue;
                var actAnimTime = TimeSpan.MinValue;
                var containedBlockingAnimation = false;

                foreach (var actAnimationStep in m_runningAnimations)
                {
                    if (actAnimationStep.Canceled) { continue; }
                    if (actAnimationStep.Finished) { continue; }

                    // Calculate parameters for GetTimeTillFinished method
                    //  .. Constants.UPDATE_STATE_MAX_TIME oder TimeSpan.MinValue are only internal representations for empty values
                    var mParamMin = timeTillNextStepMin != SeeingSharpConstants.UPDATE_STATE_MAX_TIME ? timeTillNextStepMin : TimeSpan.Zero;
                    var mParamMax = timeTillNextStepMax != TimeSpan.MinValue ? timeTillNextStepMax : TimeSpan.Zero;

                    // Update local time values
                    actAnimTime = actAnimationStep.GetTimeTillNextEvent(mParamMin, mParamMax, this.DefaultCycleTime);

                    if (actAnimTime < timeTillNextStepMin) { timeTillNextStepMin = actAnimTime; }
                    if (actAnimTime > timeTillNextStepMax) { timeTillNextStepMax = actAnimTime; }

                    // Stop calculation if we have reached a blocking animation
                    //  .. an event occurs on the end of each blocking animation
                    if (actAnimationStep.IsBlockingAnimation)
                    {
                        containedBlockingAnimation = true;

                        if (actAnimTime < timeTillNextEvent) { timeTillNextEvent = actAnimTime; }

                        break;
                    }
                }

                // Handle last animation like a blocking one
                if (!containedBlockingAnimation &&
                    actAnimTime >= TimeSpan.Zero)
                {
                    if (actAnimTime < timeTillNextEvent) { timeTillNextEvent = actAnimTime; }
                }
            }

            // Calculate sub-animation time using secondary animations
            if (m_runningSecondaryAnimationsCount > 0)
            {
                foreach (var actSecondaryQueue in m_runningSecondaryAnimations)
                {
                    if (actSecondaryQueue.Count > 0)
                    {
                        var timeTillNextStepMin = SeeingSharpConstants.UPDATE_STATE_MAX_TIME;
                        var timeTillNextStepMax = TimeSpan.MinValue;
                        var actAnimTime = TimeSpan.Zero;
                        var containedBlockingAnimation = false;

                        foreach (var actAnimationStep in actSecondaryQueue)
                        {
                            if (actAnimationStep.Canceled) { continue; }
                            if (actAnimationStep.Finished) { continue; }

                            // Calculate parameters for GetTimeTillFinished method
                            //  .. Constants.UPDATE_STATE_MAX_TIME oder TimeSpan.MinValue are only internal representations for empty values
                            var mParamMin = timeTillNextStepMin != SeeingSharpConstants.UPDATE_STATE_MAX_TIME ? timeTillNextStepMin : TimeSpan.Zero;
                            var mParamMax = timeTillNextStepMax != TimeSpan.MinValue ? timeTillNextStepMax : TimeSpan.Zero;

                            // Update local time values
                            actAnimTime = actAnimationStep.GetTimeTillNextEvent(mParamMin, mParamMax, this.DefaultCycleTime);

                            if (actAnimTime < timeTillNextStepMin) { timeTillNextStepMin = actAnimTime; }
                            if (actAnimTime > timeTillNextStepMax) { timeTillNextStepMax = actAnimTime; }

                            // Stop calculation if we have reached a blocking animation
                            //  .. an event occurs on the end of each blocking animation
                            if (actAnimationStep.IsBlockingAnimation)
                            {
                                containedBlockingAnimation = true;
                                if (actAnimTime < timeTillNextEvent) { timeTillNextEvent = actAnimTime; }
                                break;
                            }
                        }

                        // Handle last animation like a blocking one
                        if (!containedBlockingAnimation &&
                            actAnimTime >= TimeSpan.Zero)
                        {
                            if (actAnimTime < timeTillNextEvent) { timeTillNextEvent = actAnimTime; }
                        }
                    }
                }
            }

            // Apply time till next animation step
            if (timeTillNextEvent == SeeingSharpConstants.UPDATE_STATE_MAX_TIME) { m_timeTillNextPartFinished = SeeingSharpConstants.UPDATE_STATE_MAX_TIME; }
            else if (timeTillNextEvent < TimeSpan.Zero) { throw new SeeingSharpGraphicsException("Invalid calculated value for timeTillNextEvent: " + timeTillNextEvent); }
            else
            {
                // This path is normal if we have animations applied
                m_timeTillNextPartFinished = timeTillNextEvent;
            }
        }

        /// <summary>
        /// Updates all animations within the given animation queue.
        /// Returns true if any animation was finished or canceled.
        /// </summary>
        /// <param name="updateState">Current update state.</param>
        /// <param name="animationState">Current animation state.</param>
        /// <param name="animationQueue">The queue which should be updated.</param>
        private bool UpdateQueueInternal(IAnimationUpdateState updateState, AnimationState animationState, IEnumerable<IAnimation> animationQueue)
        {
            var anyFinishedOrCanceled = false;
            var animationStateInner = new AnimationState();
            var actIndex = 0;
            var actMaxTime = TimeSpan.Zero;

            // Loop over all animations and update them till next blocking animation
            foreach (var actAnimation in animationQueue)
            {
                if (actAnimation.Canceled) { continue; }
                if (actAnimation.Finished) { continue; }

                try
                {
                    animationStateInner.RunningAnimationsIndex = actIndex;
                    actIndex++;

                    // Call update of the animation
                    updateState.IgnorePauseState = actAnimation.IgnorePauseState;
                    actAnimation.Update(updateState, animationStateInner);

                    // Decrement current animation index if the current one is finished now
                    if (actAnimation.Finished || actAnimation.Canceled)
                    {
                        actIndex--;
                        anyFinishedOrCanceled = true;
                    }

                    // Break on blocking animations
                    if (actAnimation.IsBlockingAnimation)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    //Log error
                    var animHandler = this as AnimationHandler;

                    // Raise the animation failed event
                    this.AnimationFailed.Raise(this, new AnimationFailedEventArgs(actAnimation, ex));

                    //Query for reaction
                    var reaction = this.OnAnimationFailed(actAnimation, ex);

                    switch (reaction)
                    {
                        case AnimationFailedReaction.ThrowException:
                            throw;

                        //Remove the animation and
                        case AnimationFailedReaction.RemoveAndContinue:
                            actAnimation.Canceled = true;
                            anyFinishedOrCanceled = true;
                            break;
                    }
                }
            }

            return anyFinishedOrCanceled;
        }

        /// <summary>
        /// A condition method for error checking.
        /// Check executing thread when we are in Debug mode.
        /// </summary>
        [Conditional("DEBUG")]
        private void PreCheckExecutionInternal()
        {
            if (m_currentWorkingThread != null &&
                m_currentWorkingThread != Thread.CurrentThread) { throw new SeeingSharpGraphicsException("Parallel-Call detection on AnimationSequence!"); }
            m_currentWorkingThread = Thread.CurrentThread;

            m_currentWorkingThreadCount++;
        }

        /// <summary>
        /// A condition method for error checking.
        /// Check executing thread when we are in Debug mode.
        /// </summary>
        [Conditional("DEBUG")]
        private void PostCheckExecutionInternal()
        {
            m_currentWorkingThreadCount--;
            if (m_currentWorkingThreadCount == 0)
            {
                m_currentWorkingThread = null;
            }

            if (m_currentWorkingThreadCount < 0) { throw new SeeingSharpGraphicsException("Invalid currentWorkingThreadCount on AnimationSequence!"); }
        }

        /// <summary>
        /// Is this animation finished?
        /// </summary>
        public bool Finished => m_runningAnimationsCount <= 0;

        /// <summary>
        /// Is this animation a blocking animation?
        /// </summary>
        public bool IsBlockingAnimation => false;

        /// <summary>
        /// Is this animation canceled?
        /// </summary>
        public bool Canceled
        {
            get;
            set;
        }

        /// <summary>
        /// Should this animation ignore pause state?
        /// </summary>
        public bool IgnorePauseState
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the total count of running animations
        /// </summary>
        public int CountRunningAnimations => m_runningAnimationsCount + m_runningSecondaryAnimationsCount;

        /// <summary>
        /// Gets the time when the next part of this animation is finished.
        /// </summary>
        public TimeSpan TimeTillCurrentAnimationStepFinished => m_timeTillNextPartFinished;

        /// <summary>
        /// Gets or sets the default cycle time (cycles time in continuous calculation model).
        /// </summary>
        public TimeSpan DefaultCycleTime { get; set; } = SeeingSharpConstants.UPDATE_DEFAULT_CYLCE;
    }
}