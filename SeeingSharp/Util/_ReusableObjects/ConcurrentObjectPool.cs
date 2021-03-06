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
using System.Threading;

namespace SeeingSharp.Util
{
    // Original code from book "Pro .Net Memory Management" by Konrad Kokosa (sample sourcecode chapter 6)
    // Changes:
    //  - Only some namings to meet coding style of Seeing#
    //
    // Based on ObjectPool<T> from Roslyn source code (with comments reused):
    // https://github.com/dotnet/roslyn/blob/d4dab355b96955aca5b4b0ebf6282575fad78ba8/src/Dependencies/PooledObjects/ObjectPool%601.cs

    /// <summary>
    /// Helper class for reusing objects.
    /// 
    /// </summary>
    public class ConcurrentObjectPool<T> 
        where T : class
    {
        private readonly T[] _items;
        private readonly Func<T> _generator;
        private T _firstItem;

        public ConcurrentObjectPool(Func<T> generator, int size)
        {
            _generator = generator ?? throw new ArgumentNullException(nameof(generator));
            _items = new T[size - 1];
        }

        public T Rent()
        {
            // PERF: Examine the first element. If that fails, RentSlow will look at the remaining elements.
            // Note that the initial read is optimistically not synchronized. That is intentional. 
            // We will interlock only when we have a candidate. in a worst case we may miss some
            // recently returned objects. Not a big deal.
            var inst = _firstItem;
            if (inst == null || inst != Interlocked.CompareExchange(ref _firstItem, null, inst))
            {
                inst = this.RentSlow();
            }
            return inst;
        }

        public void Return(T item)
        {
            if (_firstItem == null)
            {
                // Intentionally not using interlocked here. 
                // In a worst case scenario two objects may be stored into same slot.
                // It is very unlikely to happen and will only mean that one of the objects will get collected.
                _firstItem = item;
            }
            else
            {
                this.ReturnSlow(item);
            }
        }

        private T RentSlow()
        {
            for (var i = 0; i < _items.Length; i++)
            {
                // Note that the initial read is optimistically not synchronized. That is intentional. 
                // We will interlock only when we have a candidate. in a worst case we may miss some
                // recently returned objects. Not a big deal.
                var inst = _items[i];
                if (inst != null)
                {
                    if (inst == Interlocked.CompareExchange(ref _items[i], null, inst))
                    {
                        return inst;
                    }
                }
            }
            return _generator();
        }

        private void ReturnSlow(T obj)
        {
            for (var i = 0; i < _items.Length; i++)
            {
                if (_items[i] == null)
                {
                    // Intentionally not using interlocked here. 
                    // In a worst case scenario two objects may be stored into same slot.
                    // It is very unlikely to happen and will only mean that one of the objects will get collected.
                    _items[i] = obj;
                    break;
                }
            }
        }
    }
}
