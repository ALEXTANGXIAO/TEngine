// ---------------------------------------------------------------------
// Copyright (c) 2015-2016 Microsoft
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// ---------------------------------------------------------------------

#pragma warning disable CS8625
#pragma warning disable CS8604
#pragma warning disable CS8600
#pragma warning disable CS8618
namespace TEngine.IO
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;

    /// <summary>
    /// Manages pools of <see cref="RecyclableMemoryStream"/> objects.
    /// </summary>
    /// <remarks>
    /// <para>
    /// There are two pools managed in here. The small pool contains same-sized buffers that are handed to streams
    /// as they write more data.
    ///</para>
    ///<para>
    /// For scenarios that need to call <see cref="RecyclableMemoryStream.GetBuffer"/>, the large pool contains buffers of various sizes, all
    /// multiples/exponentials of <see cref="LargeBufferMultiple"/> (1 MB by default). They are split by size to avoid overly-wasteful buffer
    /// usage. There should be far fewer 8 MB buffers than 1 MB buffers, for example.
    /// </para>
    /// </remarks>
    public partial class RecyclableMemoryStreamManager
    {
        /// <summary>
        /// Maximum length of a single array.
        /// </summary>
        /// <remarks>See documentation at https://docs.microsoft.com/dotnet/api/system.array?view=netcore-3.1
        /// </remarks>
        internal const int MaxArrayLength = 0X7FFFFFC7;

        /// <summary>
        /// Default block size, in bytes.
        /// </summary>
        public const int DefaultBlockSize = 128 * 1024;

        /// <summary>
        /// Default large buffer multiple, in bytes.
        /// </summary>
        public const int DefaultLargeBufferMultiple = 1024 * 1024;

        /// <summary>
        /// Default maximum buffer size, in bytes.
        /// </summary>
        public const int DefaultMaximumBufferSize = 128 * 1024 * 1024;

        // 0 to indicate unbounded
        private const long DefaultMaxSmallPoolFreeBytes = 0L;
        private const long DefaultMaxLargePoolFreeBytes = 0L;

        private readonly long[] largeBufferFreeSize;
        private readonly long[] largeBufferInUseSize;

        private readonly ConcurrentStack<byte[]>[] largePools;

        private readonly ConcurrentStack<byte[]> smallPool;

        private long smallPoolFreeSize;
        private long smallPoolInUseSize;

        /// <summary>
        /// Initializes the memory manager with the default block/buffer specifications. This pool may have unbounded growth unless you modify <see cref="MaximumFreeSmallPoolBytes"/> and <see cref="MaximumFreeLargePoolBytes"/>.
        /// </summary>
        public RecyclableMemoryStreamManager()
            : this(DefaultBlockSize, DefaultLargeBufferMultiple, DefaultMaximumBufferSize, false, DefaultMaxSmallPoolFreeBytes, DefaultMaxLargePoolFreeBytes) { }

        /// <summary>
        /// Initializes the memory manager with the default block/buffer specifications and maximum free bytes specifications.
        /// </summary>
        /// <param name="maximumSmallPoolFreeBytes">Maximum number of bytes to keep available in the small pool before future buffers get dropped for garbage collection</param>
        /// <param name="maximumLargePoolFreeBytes">Maximum number of bytes to keep available in the large pool before future buffers get dropped for garbage collection</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maximumSmallPoolFreeBytes"/> is negative, or <paramref name="maximumLargePoolFreeBytes"/> is negative.</exception>
        public RecyclableMemoryStreamManager(long maximumSmallPoolFreeBytes, long maximumLargePoolFreeBytes)
            :this(DefaultBlockSize, DefaultLargeBufferMultiple, DefaultMaximumBufferSize, useExponentialLargeBuffer:false, maximumSmallPoolFreeBytes, maximumLargePoolFreeBytes)
        {
        }

        /// <summary>
        /// Initializes the memory manager with the given block requiredSize. This pool may have unbounded growth unless you modify <see cref="MaximumFreeSmallPoolBytes"/> and <see cref="MaximumFreeLargePoolBytes"/>.
        /// </summary>
        /// <param name="blockSize">Size of each block that is pooled. Must be > 0.</param>
        /// <param name="largeBufferMultiple">Each large buffer will be a multiple of this value.</param>
        /// <param name="maximumBufferSize">Buffers larger than this are not pooled</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="blockSize"/> is not a positive number,
        /// or <paramref name="largeBufferMultiple"/> is not a positive number,
        /// or <paramref name="maximumBufferSize"/> is less than <paramref name="blockSize"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="maximumBufferSize"/> is not a multiple of <paramref name="largeBufferMultiple"/>.</exception>
        public RecyclableMemoryStreamManager(int blockSize, int largeBufferMultiple, int maximumBufferSize)
            : this(blockSize, largeBufferMultiple, maximumBufferSize, false, DefaultMaxSmallPoolFreeBytes, DefaultMaxLargePoolFreeBytes) { }

        /// <summary>
        /// Initializes the memory manager with the given block requiredSize.
        /// </summary>
        /// <param name="blockSize">Size of each block that is pooled. Must be > 0.</param>
        /// <param name="largeBufferMultiple">Each large buffer will be a multiple of this value.</param>
        /// <param name="maximumBufferSize">Buffers larger than this are not pooled</param>
        /// <param name="maximumSmallPoolFreeBytes">Maximum number of bytes to keep available in the small pool before future buffers get dropped for garbage collection</param>
        /// <param name="maximumLargePoolFreeBytes">Maximum number of bytes to keep available in the large pool before future buffers get dropped for garbage collection</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="blockSize"/> is not a positive number,
        /// or <paramref name="largeBufferMultiple"/> is not a positive number,
        /// or <paramref name="maximumBufferSize"/> is less than <paramref name="blockSize"/>,
        /// or <paramref name="maximumSmallPoolFreeBytes"/> is negative,
        /// or <paramref name="maximumLargePoolFreeBytes"/> is negative.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="maximumBufferSize"/> is not a multiple of <paramref name="largeBufferMultiple"/>.</exception>
        public RecyclableMemoryStreamManager(int blockSize, int largeBufferMultiple, int maximumBufferSize, long maximumSmallPoolFreeBytes, long maximumLargePoolFreeBytes)
            : this(blockSize, largeBufferMultiple, maximumBufferSize, false, maximumSmallPoolFreeBytes, maximumLargePoolFreeBytes) { }


        /// <summary>
        /// Initializes the memory manager with the given block requiredSize. This pool may have unbounded growth unless you modify <see cref="MaximumFreeSmallPoolBytes"/> and <see cref="MaximumFreeLargePoolBytes"/>.
        /// </summary>
        /// <param name="blockSize">Size of each block that is pooled. Must be > 0.</param>
        /// <param name="largeBufferMultiple">Each large buffer will be a multiple/exponential of this value.</param>
        /// <param name="maximumBufferSize">Buffers larger than this are not pooled</param>
        /// <param name="useExponentialLargeBuffer">Switch to exponential large buffer allocation strategy</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="blockSize"/> is not a positive number,
        /// or <paramref name="largeBufferMultiple"/> is not a positive number,
        /// or <paramref name="maximumBufferSize"/> is less than <paramref name="blockSize"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="maximumBufferSize"/> is not a multiple/exponential of <paramref name="largeBufferMultiple"/>.</exception>
        public RecyclableMemoryStreamManager(int blockSize, int largeBufferMultiple, int maximumBufferSize, bool useExponentialLargeBuffer)
            :this(blockSize, largeBufferMultiple, maximumBufferSize, useExponentialLargeBuffer, DefaultMaxSmallPoolFreeBytes, DefaultMaxLargePoolFreeBytes)
        {
        }

        /// <summary>
        /// Initializes the memory manager with the given block requiredSize.
        /// </summary>
        /// <param name="blockSize">Size of each block that is pooled. Must be > 0.</param>
        /// <param name="largeBufferMultiple">Each large buffer will be a multiple/exponential of this value.</param>
        /// <param name="maximumBufferSize">Buffers larger than this are not pooled.</param>
        /// <param name="useExponentialLargeBuffer">Switch to exponential large buffer allocation strategy.</param>
        /// <param name="maximumSmallPoolFreeBytes">Maximum number of bytes to keep available in the small pool before future buffers get dropped for garbage collection.</param>
        /// <param name="maximumLargePoolFreeBytes">Maximum number of bytes to keep available in the large pool before future buffers get dropped for garbage collection.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="blockSize"/> is not a positive number,
        /// or <paramref name="largeBufferMultiple"/> is not a positive number,
        /// or <paramref name="maximumBufferSize"/> is less than <paramref name="blockSize"/>,
        /// or <paramref name="maximumSmallPoolFreeBytes"/> is negative,
        /// or <paramref name="maximumLargePoolFreeBytes"/> is negative.
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="maximumBufferSize"/> is not a multiple/exponential of <paramref name="largeBufferMultiple"/>.</exception>
        public RecyclableMemoryStreamManager(int blockSize, int largeBufferMultiple, int maximumBufferSize, bool useExponentialLargeBuffer, long maximumSmallPoolFreeBytes, long maximumLargePoolFreeBytes)
        {
            if (blockSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(blockSize), blockSize, $"{nameof(blockSize)} must be a positive number");
            }

            if (largeBufferMultiple <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(largeBufferMultiple), $"{nameof(largeBufferMultiple)} must be a positive number");
            }

            if (maximumBufferSize < blockSize)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumBufferSize), $"{nameof(maximumBufferSize)} must be at least {nameof(blockSize)}");
            }

            if (maximumSmallPoolFreeBytes < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumSmallPoolFreeBytes), $"{nameof(maximumSmallPoolFreeBytes)} must be non-negative");
            }

            if (maximumLargePoolFreeBytes < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumLargePoolFreeBytes), $"{nameof(maximumLargePoolFreeBytes)} must be non-negative");
            }

            this.BlockSize = blockSize;
            this.LargeBufferMultiple = largeBufferMultiple;
            this.MaximumBufferSize = maximumBufferSize;
            this.UseExponentialLargeBuffer = useExponentialLargeBuffer;
            this.MaximumFreeSmallPoolBytes = maximumSmallPoolFreeBytes;
            this.MaximumFreeLargePoolBytes = maximumLargePoolFreeBytes;

            if (!this.IsLargeBufferSize(maximumBufferSize))
            {
                throw new ArgumentException(
                    $"{nameof(maximumBufferSize)} is not {(this.UseExponentialLargeBuffer ? "an exponential" : "a multiple")} of {nameof(largeBufferMultiple)}.",
                    nameof(maximumBufferSize));
            }

            this.smallPool = new ConcurrentStack<byte[]>();
            var numLargePools = useExponentialLargeBuffer
                                    ? ((int)Math.Log(maximumBufferSize / largeBufferMultiple, 2) + 1)
                                    : (maximumBufferSize / largeBufferMultiple);

            // +1 to store size of bytes in use that are too large to be pooled
            this.largeBufferInUseSize = new long[numLargePools + 1];
            this.largeBufferFreeSize = new long[numLargePools];

            this.largePools = new ConcurrentStack<byte[]>[numLargePools];

            for (var i = 0; i < this.largePools.Length; ++i)
            {
                this.largePools[i] = new ConcurrentStack<byte[]>();
            }

            Events.Writer.MemoryStreamManagerInitialized(blockSize, largeBufferMultiple, maximumBufferSize);
        }

        /// <summary>
        /// The size of each block. It must be set at creation and cannot be changed.
        /// </summary>
        public int BlockSize { get; }

        /// <summary>
        /// All buffers are multiples/exponentials of this number. It must be set at creation and cannot be changed.
        /// </summary>
        public int LargeBufferMultiple { get; }

        /// <summary>
        /// Use multiple large buffer allocation strategy. It must be set at creation and cannot be changed.
        /// </summary>
        public bool UseMultipleLargeBuffer => !this.UseExponentialLargeBuffer;

        /// <summary>
        /// Use exponential large buffer allocation strategy. It must be set at creation and cannot be changed.
        /// </summary>
        public bool UseExponentialLargeBuffer { get; }

        /// <summary>
        /// Gets the maximum buffer size.
        /// </summary>
        /// <remarks>Any buffer that is returned to the pool that is larger than this will be
        /// discarded and garbage collected.</remarks>
        public int MaximumBufferSize { get; }

        /// <summary>
        /// Number of bytes in small pool not currently in use.
        /// </summary>
        public long SmallPoolFreeSize => this.smallPoolFreeSize;

        /// <summary>
        /// Number of bytes currently in use by stream from the small pool.
        /// </summary>
        public long SmallPoolInUseSize => this.smallPoolInUseSize;

        /// <summary>
        /// Number of bytes in large pool not currently in use.
        /// </summary>
        public long LargePoolFreeSize
        {
            get
            {
                long sum = 0;
                foreach (long freeSize in this.largeBufferFreeSize)
                {
                    sum += freeSize;
                }

                return sum;
            }
        }

        /// <summary>
        /// Number of bytes currently in use by streams from the large pool.
        /// </summary>
        public long LargePoolInUseSize
        {
            get
            {
                long sum = 0;
                foreach (long inUseSize in this.largeBufferInUseSize)
                {
                    sum += inUseSize;
                }

                return sum;
            }
        }

        /// <summary>
        /// How many blocks are in the small pool.
        /// </summary>
        public long SmallBlocksFree => this.smallPool.Count;

        /// <summary>
        /// How many buffers are in the large pool.
        /// </summary>
        public long LargeBuffersFree
        {
            get
            {
                long free = 0;
                foreach (var pool in this.largePools)
                {
                    free += pool.Count;
                }
                return free;
            }
        }

        /// <summary>
        /// How many bytes of small free blocks to allow before we start dropping
        /// those returned to us.
        /// </summary>
        /// <remarks>The default value is 0, meaning the pool is unbounded.</remarks>
        public long MaximumFreeSmallPoolBytes { get; set; }

        /// <summary>
        /// How many bytes of large free buffers to allow before we start dropping
        /// those returned to us.
        /// </summary>
        /// <remarks>The default value is 0, meaning the pool is unbounded.</remarks>
        public long MaximumFreeLargePoolBytes { get; set; }

        /// <summary>
        /// Maximum stream capacity in bytes. Attempts to set a larger capacity will
        /// result in an exception.
        /// </summary>
        /// <remarks>A value of 0 indicates no limit.</remarks>
        public long MaximumStreamCapacity { get; set; }

        /// <summary>
        /// Whether to save callstacks for stream allocations. This can help in debugging.
        /// It should NEVER be turned on generally in production.
        /// </summary>
        public bool GenerateCallStacks { get; set; }

        /// <summary>
        /// Whether dirty buffers can be immediately returned to the buffer pool.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When <see cref="RecyclableMemoryStream.GetBuffer"/> is called on a stream and creates a single large buffer, if this setting is enabled, the other blocks will be returned
        /// to the buffer pool immediately.
        /// </para>
        /// <para>
        /// Note when enabling this setting that the user is responsible for ensuring that any buffer previously
        /// retrieved from a stream which is subsequently modified is not used after modification (as it may no longer
        /// be valid).
        /// </para>
        /// </remarks>
        public bool AggressiveBufferReturn { get; set; }

        /// <summary>
        /// Causes an exception to be thrown if <see cref="RecyclableMemoryStream.ToArray"/> is ever called.
        /// </summary>
        /// <remarks>Calling <see cref="RecyclableMemoryStream.ToArray"/> defeats the purpose of a pooled buffer. Use this property to discover code that is calling <see cref="RecyclableMemoryStream.ToArray"/>. If this is
        /// set and <see cref="RecyclableMemoryStream.ToArray"/> is called, a <c>NotSupportedException</c> will be thrown.</remarks>
        public bool ThrowExceptionOnToArray { get; set; }

        /// <summary>
        /// Removes and returns a single block from the pool.
        /// </summary>
        /// <returns>A <c>byte[]</c> array.</returns>
        internal byte[] GetBlock()
        {
            Interlocked.Add(ref this.smallPoolInUseSize, this.BlockSize);

            if (!this.smallPool.TryPop(out byte[] block))
            {
                // We'll add this back to the pool when the stream is disposed
                // (unless our free pool is too large)
#if NET5_0_OR_GREATER
                block = GC.AllocateUninitializedArray<byte>(this.BlockSize);
#else
                block = new byte[this.BlockSize];
#endif
                ReportBlockCreated();
            }
            else
            {
                Interlocked.Add(ref this.smallPoolFreeSize, -this.BlockSize);
            }

            return block;
        }

        /// <summary>
        /// Returns a buffer of arbitrary size from the large buffer pool. This buffer
        /// will be at least the requiredSize and always be a multiple/exponential of largeBufferMultiple.
        /// </summary>
        /// <param name="requiredSize">The minimum length of the buffer.</param>
        /// <param name="id">Unique ID for the stream.</param>
        /// <param name="tag">The tag of the stream returning this buffer, for logging if necessary.</param>
        /// <returns>A buffer of at least the required size.</returns>
        /// <exception cref="OutOfMemoryException">Requested array size is larger than the maximum allowed.</exception>
        internal byte[] GetLargeBuffer(long requiredSize, Guid id, string tag)
        {
            if (requiredSize > MaxArrayLength)
            {
                throw new OutOfMemoryException($"Requested size exceeds maximum array length of {MaxArrayLength}.");
            }

            requiredSize = this.RoundToLargeBufferSize(requiredSize);

            var poolIndex = this.GetPoolIndex(requiredSize);

            bool createdNew = false;
            bool pooled = true;
            string callStack = null;

            byte[] buffer;
            if (poolIndex < this.largePools.Length)
            {
                if (!this.largePools[poolIndex].TryPop(out buffer))
                {
                    buffer = AllocateArray(requiredSize);
                    createdNew = true;
                }
                else
                {
                    Interlocked.Add(ref this.largeBufferFreeSize[poolIndex], -buffer.Length);
                }
            }
            else
            {
                // Buffer is too large to pool. They get a new buffer.

                // We still want to track the size, though, and we've reserved a slot
                // in the end of the inuse array for nonpooled bytes in use.
                poolIndex = this.largeBufferInUseSize.Length - 1;

                // We still want to round up to reduce heap fragmentation.
                buffer = AllocateArray(requiredSize);
                if (this.GenerateCallStacks)
                {
                    // Grab the stack -- we want to know who requires such large buffers
                    callStack = Environment.StackTrace;
                }
                createdNew = true;
                pooled = false;
            }

            Interlocked.Add(ref this.largeBufferInUseSize[poolIndex], buffer.Length);
            if (createdNew)
            {
                ReportLargeBufferCreated(id, tag, requiredSize, pooled: pooled, callStack);
            }

            return buffer;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static byte[] AllocateArray(long requiredSize) =>
#if NET5_0_OR_GREATER
                GC.AllocateUninitializedArray<byte>((int)requiredSize);
#else
                new byte[requiredSize];
#endif
        }

        private long RoundToLargeBufferSize(long requiredSize)
        {
            if (this.UseExponentialLargeBuffer)
            {
                long pow = 1;
                while (this.LargeBufferMultiple * pow < requiredSize)
                {
                    pow <<= 1;
                }
                return this.LargeBufferMultiple * pow;
            }
            else
            {
                return ((requiredSize + this.LargeBufferMultiple - 1) / this.LargeBufferMultiple) * this.LargeBufferMultiple;
            }
        }

        private bool IsLargeBufferSize(int value)
        {
            return (value != 0) && (this.UseExponentialLargeBuffer
                                        ? (value == RoundToLargeBufferSize(value))
                                        : (value % this.LargeBufferMultiple) == 0);
        }

        private int GetPoolIndex(long length)
        {
            if (this.UseExponentialLargeBuffer)
            {
                int index = 0;
                while ((this.LargeBufferMultiple << index) < length)
                {
                    ++index;
                }
                return index;
            }
            else
            {
                return (int)(length / this.LargeBufferMultiple - 1);
            }
        }

        /// <summary>
        /// Returns the buffer to the large pool.
        /// </summary>
        /// <param name="buffer">The buffer to return.</param>
        /// <param name="id">Unique stream ID.</param>
        /// <param name="tag">The tag of the stream returning this buffer, for logging if necessary.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentException"><c>buffer.Length</c> is not a multiple/exponential of <see cref="LargeBufferMultiple"/> (it did not originate from this pool).</exception>
        internal void ReturnLargeBuffer(byte[] buffer, Guid id, string tag)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (!this.IsLargeBufferSize(buffer.Length))
            {
                throw new ArgumentException($"{nameof(buffer)} did not originate from this memory manager. The size is not " +
                                            $"{(this.UseExponentialLargeBuffer ? "an exponential" : "a multiple")} of {this.LargeBufferMultiple}.");
            }

            var poolIndex = this.GetPoolIndex(buffer.Length);

            if (poolIndex < this.largePools.Length)
            {
                if ((this.largePools[poolIndex].Count + 1) * buffer.Length <= this.MaximumFreeLargePoolBytes ||
                    this.MaximumFreeLargePoolBytes == 0)
                {
                    this.largePools[poolIndex].Push(buffer);
                    Interlocked.Add(ref this.largeBufferFreeSize[poolIndex], buffer.Length);
                }
                else
                {
                    ReportBufferDiscarded(id, tag, Events.MemoryStreamBufferType.Large, Events.MemoryStreamDiscardReason.EnoughFree);
                }
            }
            else
            {
                // This is a non-poolable buffer, but we still want to track its size for inuse
                // analysis. We have space in the inuse array for this.
                poolIndex = this.largeBufferInUseSize.Length - 1;

                ReportBufferDiscarded(id, tag, Events.MemoryStreamBufferType.Large, Events.MemoryStreamDiscardReason.TooLarge);
            }

            Interlocked.Add(ref this.largeBufferInUseSize[poolIndex], -buffer.Length);
        }

        /// <summary>
        /// Returns the blocks to the pool.
        /// </summary>
        /// <param name="blocks">Collection of blocks to return to the pool.</param>
        /// <param name="id">Unique Stream ID.</param>
        /// <param name="tag">The tag of the stream returning these blocks, for logging if necessary.</param>
        /// <exception cref="ArgumentNullException"><paramref name="blocks"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="blocks"/> contains buffers that are the wrong size (or null) for this memory manager.</exception>
        internal void ReturnBlocks(List<byte[]> blocks, Guid id, string tag)
        {
            if (blocks == null)
            {
                throw new ArgumentNullException(nameof(blocks));
            }

            long bytesToReturn = (long)blocks.Count * (long)this.BlockSize;
            Interlocked.Add(ref this.smallPoolInUseSize, -bytesToReturn);

            foreach (var block in blocks)
            {
                if (block == null || block.Length != this.BlockSize)
                {
                    throw new ArgumentException($"{nameof(blocks)} contains buffers that are not {nameof(BlockSize)} in length.", nameof(blocks));
                }
            }

            foreach (var block in blocks)
            {
                if (this.MaximumFreeSmallPoolBytes == 0 || this.SmallPoolFreeSize < this.MaximumFreeSmallPoolBytes)
                {
                    Interlocked.Add(ref this.smallPoolFreeSize, this.BlockSize);
                    this.smallPool.Push(block);
                }
                else
                {
                    ReportBufferDiscarded(id, tag, Events.MemoryStreamBufferType.Small, Events.MemoryStreamDiscardReason.EnoughFree);
                    break;
                }
            }
        }

        /// <summary>
        /// Returns a block to the pool.
        /// </summary>
        /// <param name="block">Block to return to the pool.</param>
        /// <param name="id">Unique Stream ID.</param>
        /// <param name="tag">The tag of the stream returning this, for logging if necessary.</param>
        /// <exception cref="ArgumentNullException"><paramref name="block"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="block"/> is the wrong size for this memory manager.</exception>
        internal void ReturnBlock(byte[] block, Guid id, string tag)
        {
            var bytesToReturn = this.BlockSize;
            Interlocked.Add(ref this.smallPoolInUseSize, -bytesToReturn);

            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            if (block.Length != this.BlockSize)
            {
                throw new ArgumentException($"{nameof(block)} is not not {nameof(BlockSize)} in length.");
            }

            if (this.MaximumFreeSmallPoolBytes == 0 || this.SmallPoolFreeSize < this.MaximumFreeSmallPoolBytes)
            {
                Interlocked.Add(ref this.smallPoolFreeSize, this.BlockSize);
                this.smallPool.Push(block);
            }
            else
            {
                ReportBufferDiscarded(id, tag, Events.MemoryStreamBufferType.Small, Events.MemoryStreamDiscardReason.EnoughFree);
            }
        }

        internal void ReportBlockCreated()
        {
            Events.Writer.MemoryStreamNewBlockCreated(this.smallPoolInUseSize);
            this.BlockCreated?.Invoke(this, new BlockCreatedEventArgs(this.smallPoolInUseSize));
        }

        internal void ReportLargeBufferCreated(Guid id, string tag, long requiredSize, bool pooled, string callStack)
        {
            if (pooled)
            {
                Events.Writer.MemoryStreamNewLargeBufferCreated(requiredSize, this.LargePoolInUseSize);
            }
            else
            {
                Events.Writer.MemoryStreamNonPooledLargeBufferCreated(id, tag, requiredSize, callStack);
            }
            this.LargeBufferCreated?.Invoke(this, new LargeBufferCreatedEventArgs(id, tag, requiredSize, this.LargePoolInUseSize, pooled, callStack));
        }

        internal void ReportBufferDiscarded(Guid id, string tag, Events.MemoryStreamBufferType bufferType, Events.MemoryStreamDiscardReason reason)
        {
            Events.Writer.MemoryStreamDiscardBuffer(id, tag, bufferType, reason,
                this.SmallBlocksFree, this.smallPoolFreeSize, this.smallPoolInUseSize,
                this.LargeBuffersFree, this.LargePoolFreeSize, this.LargePoolInUseSize);
            this.BufferDiscarded?.Invoke(this, new BufferDiscardedEventArgs(id, tag, bufferType, reason));
        }

        internal void ReportStreamCreated(Guid id, string tag, long requestedSize, long actualSize)
        {
            Events.Writer.MemoryStreamCreated(id, tag, requestedSize, actualSize);
            this.StreamCreated?.Invoke(this, new StreamCreatedEventArgs(id, tag, requestedSize, actualSize));
        }

        internal void ReportStreamDisposed(Guid id, string tag, TimeSpan lifetime, string allocationStack, string disposeStack)
        {
            Events.Writer.MemoryStreamDisposed(id, tag, (long)lifetime.TotalMilliseconds, allocationStack, disposeStack);
            this.StreamDisposed?.Invoke(this, new StreamDisposedEventArgs(id, tag, lifetime, allocationStack, disposeStack));
        }

        internal void ReportStreamDoubleDisposed(Guid id, string tag, string allocationStack, string disposeStack1, string disposeStack2)
        {
            Events.Writer.MemoryStreamDoubleDispose(id, tag, allocationStack, disposeStack1, disposeStack2);
            this.StreamDoubleDisposed?.Invoke(this, new StreamDoubleDisposedEventArgs(id, tag, allocationStack,disposeStack1, disposeStack2));
        }

        internal void ReportStreamFinalized(Guid id, string tag, string allocationStack)
        {
            Events.Writer.MemoryStreamFinalized(id, tag, allocationStack);
            this.StreamFinalized?.Invoke(this, new StreamFinalizedEventArgs(id, tag, allocationStack));
        }

        internal void ReportStreamLength(long bytes)
        {
            this.StreamLength?.Invoke(this, new StreamLengthEventArgs(bytes));
        }

        internal void ReportStreamToArray(Guid id, string tag, string stack, long length)
        {
            Events.Writer.MemoryStreamToArray(id, tag, stack, length);
            this.StreamConvertedToArray?.Invoke(this, new StreamConvertedToArrayEventArgs(id, tag, stack, length));
        }

        internal void ReportStreamOverCapacity(Guid id, string tag, long requestedCapacity, string allocationStack)
        {
            Events.Writer.MemoryStreamOverCapacity(id, tag, requestedCapacity, this.MaximumStreamCapacity, allocationStack);
            this.StreamOverCapacity?.Invoke(this, new StreamOverCapacityEventArgs(id, tag, requestedCapacity, this.MaximumStreamCapacity, allocationStack));
        }

        internal void ReportUsageReport()
        {
            this.UsageReport?.Invoke(this, new UsageReportEventArgs(this.smallPoolInUseSize, this.smallPoolFreeSize, this.LargePoolInUseSize, this.LargePoolFreeSize));
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with no tag and a default initial capacity.
        /// </summary>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream()
        {
            return new RecyclableMemoryStream(this);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with no tag and a default initial capacity.
        /// </summary>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(Guid id)
        {
            return new RecyclableMemoryStream(this, id);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and a default initial capacity.
        /// </summary>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(string tag)
        {
            return new RecyclableMemoryStream(this, tag);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and a default initial capacity.
        /// </summary>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(Guid id, string tag)
        {
            return new RecyclableMemoryStream(this, id, tag);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and at least the given capacity.
        /// </summary>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(string tag, int requiredSize)
        {
            return new RecyclableMemoryStream(this, tag, requiredSize);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and at least the given capacity.
        /// </summary>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(Guid id, string tag, int requiredSize)
        {
            return new RecyclableMemoryStream(this, id, tag, requiredSize);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and at least the given capacity.
        /// </summary>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(Guid id, string tag, long requiredSize)
        {
            return new RecyclableMemoryStream(this, id, tag, requiredSize);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and at least the given capacity, possibly using
        /// a single contiguous underlying buffer.
        /// </summary>
        /// <remarks>Retrieving a <c>MemoryStream</c> which provides a single contiguous buffer can be useful in situations
        /// where the initial size is known and it is desirable to avoid copying data between the smaller underlying
        /// buffers to a single large one. This is most helpful when you know that you will always call <see cref="RecyclableMemoryStream.GetBuffer"/>
        /// on the underlying stream.</remarks>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <param name="asContiguousBuffer">Whether to attempt to use a single contiguous buffer.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(Guid id, string tag, int requiredSize, bool asContiguousBuffer)
        {
            return this.GetStream(id, tag, (long)requiredSize, asContiguousBuffer);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and at least the given capacity, possibly using
        /// a single contiguous underlying buffer.
        /// </summary>
        /// <remarks>Retrieving a <c>MemoryStream</c> which provides a single contiguous buffer can be useful in situations
        /// where the initial size is known and it is desirable to avoid copying data between the smaller underlying
        /// buffers to a single large one. This is most helpful when you know that you will always call <see cref="RecyclableMemoryStream.GetBuffer"/>
        /// on the underlying stream.</remarks>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <param name="asContiguousBuffer">Whether to attempt to use a single contiguous buffer.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(Guid id, string tag, long requiredSize, bool asContiguousBuffer)
        {
            if (!asContiguousBuffer || requiredSize <= this.BlockSize)
            {
                return this.GetStream(id, tag, requiredSize);
            }

            return new RecyclableMemoryStream(this, id, tag, requiredSize, this.GetLargeBuffer(requiredSize, id, tag));
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and at least the given capacity, possibly using
        /// a single contiguous underlying buffer.
        /// </summary>
        /// <remarks>Retrieving a MemoryStream which provides a single contiguous buffer can be useful in situations
        /// where the initial size is known and it is desirable to avoid copying data between the smaller underlying
        /// buffers to a single large one. This is most helpful when you know that you will always call <see cref="RecyclableMemoryStream.GetBuffer"/>
        /// on the underlying stream.</remarks>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <param name="asContiguousBuffer">Whether to attempt to use a single contiguous buffer.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(string tag, int requiredSize, bool asContiguousBuffer)
        {
            return GetStream(tag, (long)requiredSize, asContiguousBuffer);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and at least the given capacity, possibly using
        /// a single contiguous underlying buffer.
        /// </summary>
        /// <remarks>Retrieving a MemoryStream which provides a single contiguous buffer can be useful in situations
        /// where the initial size is known and it is desirable to avoid copying data between the smaller underlying
        /// buffers to a single large one. This is most helpful when you know that you will always call <see cref="RecyclableMemoryStream.GetBuffer"/>
        /// on the underlying stream.</remarks>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <param name="asContiguousBuffer">Whether to attempt to use a single contiguous buffer.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(string tag, long requiredSize, bool asContiguousBuffer)
        {
            return GetStream(Guid.NewGuid(), tag, requiredSize, asContiguousBuffer);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and with contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <param name="offset">The offset from the start of the buffer to copy from.</param>
        /// <param name="count">The number of bytes to copy from the buffer.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(Guid id, string tag, byte[] buffer, int offset, int count)
        {
            RecyclableMemoryStream stream = null;
            try
            {
                stream = new RecyclableMemoryStream(this, id, tag, count);
                stream.Write(buffer, offset, count);
                stream.Position = 0;
                return stream;
            }
            catch
            {
                stream?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(byte[] buffer)
        {
            return GetStream(null, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and with contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <param name="offset">The offset from the start of the buffer to copy from.</param>
        /// <param name="count">The number of bytes to copy from the buffer.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(string tag, byte[] buffer, int offset, int count)
        {
            return GetStream(Guid.NewGuid(), tag, buffer, offset, count);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and with contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        [Obsolete("Use the ReadOnlySpan<byte> version of this method instead.")]
        public MemoryStream GetStream(Guid id, string tag, Memory<byte> buffer)
        {
            RecyclableMemoryStream stream = null;
            try
            {
                stream = new RecyclableMemoryStream(this, id, tag, buffer.Length);
                stream.Write(buffer.Span);
                stream.Position = 0;
                return stream;
            }
            catch
            {
                stream?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and with contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="id">A unique identifier which can be used to trace usages of the stream.</param>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(Guid id, string tag, ReadOnlySpan<byte> buffer)
        {
            RecyclableMemoryStream stream = null;
            try
            {
                stream = new RecyclableMemoryStream(this, id, tag, buffer.Length);
                stream.Write(buffer);
                stream.Position = 0;
                return stream;
            }
            catch
            {
                stream?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        [Obsolete("Use the ReadOnlySpan<byte> version of this method instead.")]
        public MemoryStream GetStream(Memory<byte> buffer)
        {
            return GetStream(null, buffer);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(ReadOnlySpan<byte> buffer)
        {
            return GetStream(null, buffer);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and with contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        [Obsolete("Use the ReadOnlySpan<byte> version of this method instead.")]
        public MemoryStream GetStream(string tag, Memory<byte> buffer)
        {
            return GetStream(Guid.NewGuid(), tag, buffer);
        }

        /// <summary>
        /// Retrieve a new <c>MemoryStream</c> object with the given tag and with contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="tag">A tag which can be used to track the source of the stream.</param>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <returns>A <c>MemoryStream</c>.</returns>
        public MemoryStream GetStream(string tag, ReadOnlySpan<byte> buffer)
        {
            return GetStream(Guid.NewGuid(), tag, buffer);
        }

        /// <summary>
        /// Triggered when a new block is created.
        /// </summary>
        public event EventHandler<BlockCreatedEventArgs> BlockCreated;

        /// <summary>
        /// Triggered when a new large buffer is created.
        /// </summary>
        public event EventHandler<LargeBufferCreatedEventArgs> LargeBufferCreated;

        /// <summary>
        /// Triggered when a new stream is created.
        /// </summary>
        public event EventHandler<StreamCreatedEventArgs> StreamCreated;

        /// <summary>
        /// Triggered when a stream is disposed.
        /// </summary>
        public event EventHandler<StreamDisposedEventArgs> StreamDisposed;

        /// <summary>
        /// Triggered when a stream is disposed of twice (an error).
        /// </summary>
        public event EventHandler<StreamDoubleDisposedEventArgs> StreamDoubleDisposed;

        /// <summary>
        /// Triggered when a stream is finalized.
        /// </summary>
        public event EventHandler<StreamFinalizedEventArgs> StreamFinalized;

        /// <summary>
        /// Triggered when a stream is disposed to report the stream's length.
        /// </summary>
        public event EventHandler<StreamLengthEventArgs> StreamLength;

        /// <summary>
        /// Triggered when a user converts a stream to array.
        /// </summary>
        public event EventHandler<StreamConvertedToArrayEventArgs> StreamConvertedToArray;

        /// <summary>
        /// Triggered when a stream is requested to expand beyond the maximum length specified by the responsible RecyclableMemoryStreamManager.
        /// </summary>
        public event EventHandler<StreamOverCapacityEventArgs> StreamOverCapacity;

        /// <summary>
        /// Triggered when a buffer of either type is discarded, along with the reason for the discard.
        /// </summary>
        public event EventHandler<BufferDiscardedEventArgs> BufferDiscarded;

        /// <summary>
        /// Periodically triggered to report usage statistics.
        /// </summary>
        public event EventHandler<UsageReportEventArgs> UsageReport;
    }
}
