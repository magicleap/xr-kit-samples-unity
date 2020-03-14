// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// <copyright file = "MLThreadDispatch.cs" company="Magic Leap, Inc">
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
//
// </copyright>
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

#if PLATFORM_LUMIN

namespace UnityEngine.XR.MagicLeap.Native
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Handles dispatching calls from the Magic Leap native thread to the Unity thread
    /// </summary>
    public class MLThreadDispatch
    {
        /// <summary>
        /// A concurrent queue handles the dispatched callbacks in a thread-safe way.
        /// </summary>
        private static System.Collections.Concurrent.ConcurrentQueue<Dispatcher> actionQueue = new System.Collections.Concurrent.ConcurrentQueue<Dispatcher>();

        /// <summary>
        /// A method that queues an action without a payload
        /// </summary>
        /// <param name="callback">A callback function to be called when the delegate is invoked </param>
        public static void Call(System.Delegate callback)
        {
            if (callback != null)
            {
                System.Action call = delegate
                {
                    DispatchNoPayload newDispatch = new DispatchNoPayload(Cast<Action>(callback));
                    actionQueue.Enqueue(newDispatch);
                };

                call();
            }
        }

        /// <summary>
        /// A template method that queues an action with a single payload
        /// </summary>
        /// <typeparam name="A">Payload type</typeparam>
        /// <param name="a">Payload 1</param>
        /// <param name="callback">A callback function to be called when the delegate is invoked </param>
        public static void Call<A>(A a, System.Delegate callback)
        {
            if (callback != null)
            {
                System.Action<A> call = delegate(A arg1)
                {
                    DispatchPayload1<A> newDispatch = new DispatchPayload1<A>(arg1, Cast<Action<A>>(callback));
                    actionQueue.Enqueue(newDispatch);
                };

                call(a);
            }
        }

        /// <summary>
        /// A template method that queues an action with two payloads
        /// </summary>
        /// <typeparam name="A">First payload type</typeparam>
        /// <typeparam name="B">Second payload type</typeparam>
        /// <param name="a">Payload 1</param>
        /// <param name="b">Payload 2</param>
        /// <param name="callback">A callback function to be called when the delegate is invoked </param>
        public static void Call<A, B>(A a, B b, System.Delegate callback)
        {
            if (callback != null)
            {
                System.Action<A, B> call = delegate(A arg1, B arg2)
                {
                    DispatchPayload2<A, B> newDispatch = new DispatchPayload2<A, B>(arg1, arg2, Cast<Action<A, B>>(callback));
                    actionQueue.Enqueue(newDispatch);
                };

                call(a, b);
            }
        }

        /// <summary>
        /// A template method that queues an action with three payloads
        /// </summary>
        /// <typeparam name="A">First payload type</typeparam>
        /// <typeparam name="B">Second payload type</typeparam>
        /// <typeparam name="C">Third payload type</typeparam>
        /// <param name="a">Payload 1</param>
        /// <param name="b">Payload 2</param>
        /// <param name="c">Payload 3</param>
        /// <param name="callback">A callback function to be called when the delegate is invoked </param>
        public static void Call<A, B, C>(A a, B b, C c, System.Delegate callback)
        {
            if (callback != null)
            {
                System.Action<A, B, C> call = delegate(A arg1, B arg2, C arg3)
                {
                    DispatchPayload3<A, B, C> newDispatch = new DispatchPayload3<A, B, C>(arg1, arg2, arg3, Cast<Action<A, B, C>>(callback));
                    actionQueue.Enqueue(newDispatch);
                };

                call(a, b, c);
            }
        }

        /// <summary>
        /// Dispatch all queued items
        /// </summary>
        public static void DispatchAll()
        {
            Dispatcher callbacks;

            while (actionQueue.TryDequeue(out callbacks))
            {
                callbacks.Dispatch();
            }
        }

        /// <summary>
        /// Casts a delegate of unknown type to a delegate of a defined type.
        /// </summary>
        /// <typeparam name="T">Type of delegate to cast to</typeparam>
        /// <param name="source">Original delegate</param>
        /// <returns>Returns a newly constructed delegate of the specified type or null if a conversion doesn't exist</returns>
        private static T Cast<T>(Delegate source) where T : class
        {
            if (source == null)
            {
                return null;
            }

            Delegate[] delegates = source.GetInvocationList();

            if (delegates.Length > 1)
            {
                Delegate[] delegatesDest = new Delegate[delegates.Length];

                for (int nn = 0; nn < delegates.Length; nn++)
                {
                    delegatesDest[nn] = Delegate.CreateDelegate(typeof(T), delegates[nn].Target, delegates[nn].Method);
                }

                return Delegate.Combine(delegatesDest) as T;
            }

            return Delegate.CreateDelegate(typeof(T), delegates[0].Target, delegates[0].Method) as T;
        }

        /// <summary>
        /// Defines a generic dispatching class.
        /// </summary>
        private abstract class Dispatcher
        {
            /// <summary>
            /// Abstract dispatch method to be called when removing callbacks from the queue
            /// </summary>
            public abstract void Dispatch();
        }

        /// <summary>
        /// Overloads the generic dispatcher to call back a method without a payload
        /// </summary>
        private class DispatchNoPayload : Dispatcher
        {
            /// <summary>
            /// Stores the method to be dispatched
            /// </summary>
            private System.Action action;

            /// <summary>
            /// Initializes a new instance of the <see cref="DispatchNoPayload"/> class with the supplied callback
            /// </summary>
            /// <param name="action">Method to call back</param>
            public DispatchNoPayload(System.Action action)
            {
                this.action = action;
            }

            /// <summary>
            /// Dispatches the previously stored callback
            /// </summary>
            public override void Dispatch()
            {
                this.action();
            }
        }

        /// <summary>
        /// Overloads the generic dispatcher to call back a method with a single payload
        /// </summary>
        /// <typeparam name="T">Payload type</typeparam>
        private class DispatchPayload1<T> : Dispatcher
        {
            /// <summary>
            /// Stores the method to be dispatched
            /// </summary>
            private System.Action<T> action;

            /// <summary>
            /// Stores a copy or reference of the payload to dispatch
            /// </summary>
            private T payload;

            /// <summary>
            /// Initializes a new instance of the <see cref="DispatchPayload1{T}"/> class. with the supplied callback and payload
            /// </summary>
            /// <param name="payload">Payload to dispatch</param>
            /// <param name="action">Method to call back</param>
            public DispatchPayload1(T payload, System.Action<T> action)
            {
                this.payload = payload;
                this.action = action;
            }

            /// <summary>
            /// Dispatches the previously stored callback with the supplied payload
            /// </summary>
            public override void Dispatch()
            {
                this.action(this.payload);
            }
        }

        /// <summary>
        /// Overloads the generic dispatcher to call back a method with two payloads
        /// </summary>
        /// <typeparam name="A">First payload type</typeparam>
        /// <typeparam name="B">Second payload type</typeparam>
        private class DispatchPayload2<A, B> : Dispatcher
        {
            /// <summary>
            /// Stores the method to be dispatched
            /// </summary>
            private System.Action<A, B> action;

            /// <summary>
            /// Stores a copy or reference of a payload to dispatch
            /// </summary>
            private A payload1;

            /// <summary>
            /// Stores a copy or reference of a payload to dispatch
            /// </summary>
            private B payload2;

            /// <summary>
            /// Initializes a new instance of the <see cref="DispatchPayload2{A,B}"/> class with the supplied callback and payloads
            /// </summary>
            /// <param name="payload1">First payload</param>
            /// <param name="payload2">Second payload</param>
            /// <param name="action">Method to dispatch</param>
            public DispatchPayload2(A payload1, B payload2, System.Action<A, B> action)
            {
                this.payload1 = payload1;
                this.payload2 = payload2;
                this.action = action;
            }

            /// <summary>
            /// Dispatches the previously stored callback with the supplied payloads
            /// </summary>
            public override void Dispatch()
            {
                this.action(this.payload1, this.payload2);
            }
        }

        /// <summary>
        /// Overloads the generic dispatcher to call back a method with three payloads
        /// </summary>
        /// <typeparam name="A">First payload type</typeparam>
        /// <typeparam name="B">Second payload type</typeparam>
        /// <typeparam name="C">Third payload type</typeparam>
        private class DispatchPayload3<A, B, C> : Dispatcher
        {
            /// <summary>
            /// Stores the method to be dispatched
            /// </summary>
            private System.Action<A, B, C> action;

            /// <summary>
            /// Stores a copy or reference of a payload to dispatch
            /// </summary>
            private A payload1;

            /// <summary>
            /// Stores a copy or reference of a payload to dispatch
            /// </summary>
            private B payload2;

            /// <summary>
            /// Stores a copy or reference of a payload to dispatch
            /// </summary>
            private C payload3;

            /// <summary>
            /// Initializes a new instance of the <see cref="DispatchPayload3{A,B,C}"/> class with the supplied callback and payloads
            /// </summary>
            /// <param name="payload1">First payload</param>
            /// <param name="payload2">Second payload</param>
            /// <param name="payload3">Third payload</param>
            /// <param name="action">Method to dispatch</param>
            public DispatchPayload3(A payload1, B payload2, C payload3, System.Action<A, B, C> action)
            {
                this.payload1 = payload1;
                this.payload2 = payload2;
                this.payload3 = payload3;
                this.action = action;
            }

            /// <summary>
            /// Dispatches the previously stored callback with the supplied payloads
            /// </summary>
            public override void Dispatch()
            {
                this.action(this.payload1, this.payload2, this.payload3);
            }
        }
    }
}

#endif
