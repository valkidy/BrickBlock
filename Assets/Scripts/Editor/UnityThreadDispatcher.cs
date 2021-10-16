using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace UnityThreadDispatcher
{
    public interface IThreadSwitcher : INotifyCompletion
    {
        bool IsCompleted { get; }

        IThreadSwitcher GetAwaiter();

        void GetResult();
    }

    public static class ThreadSwitcher
    {        
        public static IThreadSwitcher ResumeUnityAsync() => new ThreadSwitcherUnity();    
    }

    internal static class UnityThreadDispatcher
    {
#pragma warning disable IDE0032 // Use auto property
        private static SynchronizationContext _context;
#pragma warning restore IDE0032 // Use auto property

        public static SynchronizationContext Context => _context;

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Capture()
        {
            _context = SynchronizationContext.Current;
        }
    }

    internal struct ThreadSwitcherUnity : IThreadSwitcher
    {
        public IThreadSwitcher GetAwaiter()
        {
            return this;
        }

        public bool IsCompleted => SynchronizationContext.Current == UnityThreadDispatcher.Context;

        public void GetResult() {}
        
        public void OnCompleted(System.Action continuation)
        {
            if (continuation == null)
                throw new ArgumentNullException(nameof(continuation));

            UnityThreadDispatcher.Context.Post(s => continuation(), null);
        }
    }
}