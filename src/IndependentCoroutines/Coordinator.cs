﻿#region Copyright and license information
// Copyright 2011 Jon Skeet
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections;
using System.Collections.Generic;

namespace Eduasync
{
    public sealed class Coordinator : IEnumerable
    {
        private readonly Queue<Action> actions = new Queue<Action>();

        // Used by collection initializer to specify the coroutines to run
        public void Add(Action<Coordinator> coroutine)
        {
            actions.Enqueue(() => coroutine(this));
        }

        // Required for collection initializers, but we don't really want
        // to expose anything.
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException("IEnumerable only supported to enable collection initializers");
        }
        
        // Execute actions in the queue until it's empty. Actions add *more*
        // actions (continuations) to the queue by awaiting this coordinator.
        public void Start()
        {
            while (actions.Count > 0)
            {
                actions.Dequeue().Invoke();
            }
        }

        // Used by await expressions to get an awaiter
        public Coordinator GetAwaiter()
        {
            return this;
        }

        // Force await to yield control
        public bool IsCompleted { get { return false; } }

        public void OnCompleted(Action continuation)
        {
            // Put the continuation at the end of the queue, ready to
            // execute when the other coroutines have had a go.
            actions.Enqueue(continuation);
        }

        public void GetResult()
        {
            // Our await expressions are void, and we never need to throw
            // an exception, so this is a no-op.
        }
    }
}
