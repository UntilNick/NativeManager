﻿using System.Diagnostics;
using System.WinApi;

namespace System.MemoryInteraction
{
    public unsafe class MemoryManager : ModuleManager, IMemory
    {
        #region Private variables
        private Allocator m_Allocator;
        private Executor m_Executor;
        private PageManager m_PageManager;
        private PatternManager m_PatternManager;
        #endregion

        #region Initialization
        public MemoryManager(Process process) : base(process, null) { }
        #endregion

        #region Indexer
        public ModuleManager this[string moduleName] => new ModuleManager(m_Process, moduleName);

        public ModuleManager this[IntPtr modulePtr] => new ModuleManager(m_Process, modulePtr);
        #endregion

        public virtual bool BlockCopy<TArray>(TArray[] src, int srcIndex, IntPtr dst, int dstOffset, IntPtr count) where TArray : unmanaged
        {
            if (count == IntPtr.Zero) return false;

            if (srcIndex >= src.Length) throw new IndexOutOfRangeException("index is more than the length of the array");

            if (count.ToInt64() > src.Length - srcIndex) throw new IndexOutOfRangeException("count is more than the length of the array");

            fixed (TArray* srcPtr = &src[srcIndex])
            {
                return Kernel32.WriteProcessMemory(m_Process.Handle, dst + dstOffset, (IntPtr)srcPtr, (IntPtr)(count.ToInt64() * sizeof(TArray)), IntPtr.Zero);
            }
        }

        public virtual bool MemoryCopy(IntPtr src, int srcOffset, IntPtr dst, int dstOffset, IntPtr count)
        {
            if (count == IntPtr.Zero) return false;

            return Kernel32.WriteProcessMemory(m_Process.Handle, dst + dstOffset, src + srcOffset, count, IntPtr.Zero);
        }

        public IAllocator GetAllocator()
        {
            if (m_Allocator == null) m_Allocator = new Allocator(m_Process);

            return m_Allocator;
        }

        public Executor GetExecutor()
        {
            if (m_Executor == null) m_Executor = new Executor(m_Process, this);

            return m_Executor;
        }

        public PageManager GetPageManager()
        {
            if (m_PageManager == null) m_PageManager = new PageManager(m_Process);

            return m_PageManager;
        }

        public PatternManager GetPatternManager()
        {
            if (m_PatternManager == null) m_PatternManager = new PatternManager(m_Process, this);

            return m_PatternManager;
        }

        public static MemoryManager GetCurrentProcessMemory() => new MemoryManager(Process.GetCurrentProcess());
    }
}