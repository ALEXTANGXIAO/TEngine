// #if UNITY_5_3_OR_NEWER
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Diagnostics.CodeAnalysis;
// using System.Linq;
// using System.Runtime.CompilerServices;
// #pragma warning disable CS8600
//
// namespace System.Collections.Generic
// {
//     public class PriorityQueue<TElement, TPriority>
//     {
//         private const int DefaultCapacity = 4;
//
//         private readonly IComparer<TPriority> _priorityComparer;
//
//         private HeapEntry[] _heap;
//         private int _count;
//         private int _version;
//
//         private UnorderedItemsCollection? _unorderedItemsCollection;
//
//         #region Constructors
//         public PriorityQueue() : this(0, null)
//         {
//
//         }
//
//         // public PriorityQueue(int initialCapacity) : this(initialCapacity, null)
//         // {
//         //
//         // }
//
//         // public PriorityQueue(IComparer<TPriority>? comparer) : this(0, comparer)
//         // {
//         //
//         // }
//
//         // public PriorityQueue(int initialCapacity, IComparer<TPriority>? comparer)
//         // {
//         //     if (initialCapacity < 0)
//         //     {
//         //         throw new ArgumentOutOfRangeException(nameof(initialCapacity));
//         //     }
//         //
//         //     if (initialCapacity == 0)
//         //     {
//         //         _heap = Array.Empty<HeapEntry>();
//         //     }
//         //     else
//         //     {
//         //         _heap = new HeapEntry[initialCapacity];
//         //     }
//         //
//         //     _priorityComparer = comparer ?? Comparer<TPriority>.Default;
//         // }
//
//         public PriorityQueue(IEnumerable<(TElement Element, TPriority Priority)> values) : this(values, null)
//         {
//
//         }
//
//         public PriorityQueue(IEnumerable<(TElement Element, TPriority Priority)> values, IComparer<TPriority>? comparer)
//         {
//             _priorityComparer = comparer ?? Comparer<TPriority>.Default;
//             _heap = Array.Empty<HeapEntry>();
//             _count = 0;
//
//             AppendRaw(values);
//             Heapify();
//         }
//         #endregion
//
//         public int Count => _count;
//         public IComparer<TPriority> Comparer => _priorityComparer;
//
//         public void Enqueue(TElement element, TPriority priority)
//         {
//             _version++;
//             if (_count == _heap.Length)
//             {
//                 Resize(ref _heap);
//             }
//
//             SiftUp(index: _count++, in element, in priority);
//         }
//
//         public void EnqueueRange(IEnumerable<(TElement Element, TPriority Priority)> values)
//         {
//             _version++;
//             if (_count == 0)
//             {
//                 AppendRaw(values);
//                 Heapify();
//             }
//             else
//             {
//                 foreach ((TElement element, TPriority priority) in values)
//                 {
//                     if (_count == _heap.Length)
//                     {
//                         Resize(ref _heap);
//                     }
//
//                     SiftUp(index: _count++, in element, in priority);
//                 }
//             }
//         }
//
//         // TODO optimize
//         public void EnqueueRange(IEnumerable<TElement> elements, TPriority priority) => EnqueueRange(elements.Select(e => (e, priority)));
//
//         public TElement Peek()
//         {
//             if (_count == 0)
//             {
//                 throw new InvalidOperationException();
//             }
//
//             return _heap[0].Element;
//         }
//
//         public bool TryPeek([MaybeNullWhen(false)] out TElement element, [MaybeNullWhen(false)] out TPriority priority)
//         {
//             if (_count == 0)
//             {
//                 element = default;
//                 priority = default;
//                 return false;
//             }
//
//             (element, priority) = _heap[0];
//             return true;
//         }
//
//         public TElement Dequeue()
//         {
//             if (_count == 0)
//             {
//                 throw new InvalidOperationException();
//             }
//
//             _version++;
//             RemoveIndex(index: 0, out TElement result, out _);
//             return result;
//         }
//
//         public bool TryDequeue([MaybeNullWhen(false)] out TElement element, [MaybeNullWhen(false)] out TPriority priority)
//         {
//             if (_count == 0)
//             {
//                 element = default;
//                 priority = default;
//                 return false;
//             }
//
//             _version++;
//             RemoveIndex(index: 0, out element, out priority);
//             return true;
//         }
//
//         public TElement EnqueueDequeue(TElement element, TPriority priority)
//         {
//             if (_count == 0)
//             {
//                 return element;
//             }
//
//             ref HeapEntry minEntry = ref _heap[0];
//             if (_priorityComparer.Compare(priority, minEntry.Priority) <= 0)
//             {
//                 return element;
//             }
//
//             _version++;
//             TElement minElement = minEntry.Element;
// #if SIFTDOWN_EMPTY_NODES
//             SiftDownHeapPropertyRequired(index: 0, in element, in priority);
// #else
//             SiftDown(index: 0, in element, in priority);
// #endif
//             return minElement;
//         }
//
//         public void Clear()
//         {
//             _version++;
//             if (_count > 0)
//             {
//                 //if (RuntimeHelpers.IsReferenceOrContainsReferences<HeapEntry>())
//                 {
//                     Array.Clear(_heap, 0, _count);
//                 }
//
//                 _count = 0;
//             }
//         }
//
//         public void TrimExcess()
//         {
//             int count = _count;
//             int threshold = (int)(((double)_heap.Length) * 0.9);
//             if (count < threshold)
//             {
//                 Array.Resize(ref _heap, count);
//             }
//         }
//
//         public void EnsureCapacity(int capacity)
//         {
//             if (capacity < 0)
//             {
//                 throw new ArgumentOutOfRangeException();
//             }
//
//             if (capacity > _heap.Length)
//             {
//                 Array.Resize(ref _heap, capacity);
//             }
//         }
//
//         public UnorderedItemsCollection UnorderedItems => _unorderedItemsCollection ??= new UnorderedItemsCollection(this);
//
//         public class UnorderedItemsCollection : IReadOnlyCollection<(TElement Element, TPriority Priority)>, ICollection
//         {
//             private readonly PriorityQueue<TElement, TPriority> _priorityQueue;
//
//             internal UnorderedItemsCollection(PriorityQueue<TElement, TPriority> priorityQueue)
//             {
//                 _priorityQueue = priorityQueue;
//             }
//
//             public int Count => _priorityQueue.Count;
//             public bool IsSynchronized => false;
//             public object SyncRoot => _priorityQueue;
//
//             public Enumerator GetEnumerator() => new Enumerator(_priorityQueue);
//             IEnumerator<(TElement Element, TPriority Priority)> IEnumerable<(TElement Element, TPriority Priority)>.GetEnumerator() => new Enumerator(_priorityQueue);
//             IEnumerator IEnumerable.GetEnumerator() => new Enumerator(_priorityQueue);
//
//             bool ICollection.IsSynchronized => false;
//             object ICollection.SyncRoot => this;
//             void ICollection.CopyTo(Array array, int index)
//             {
//                 if (array == null)
//                     throw new ArgumentNullException(nameof(array));
//                 if (array.Rank != 1)
//                     throw new ArgumentException("SR.Arg_RankMultiDimNotSupported", nameof(array));
//                 if (index < 0)
//                     throw new ArgumentOutOfRangeException(nameof(index), "SR.ArgumentOutOfRange_Index");
//
//                 int arrayLen = array.Length;
//                 if (arrayLen - index < _priorityQueue._count)
//                     throw new ArgumentException("SR.Argument_InvalidOffLen");
//
//                 int numToCopy = _priorityQueue._count;
//                 HeapEntry[] heap = _priorityQueue._heap;
//
//                 for (int i = 0; i < numToCopy; i++)
//                 {
//                     ref HeapEntry entry = ref heap[i];
//                     array.SetValue((entry.Element, entry.Priority), index + i);
//                 }
//             }
//
//             public struct Enumerator : IEnumerator<(TElement Element, TPriority Priority)>, IEnumerator
//             {
//                 private readonly PriorityQueue<TElement, TPriority> _queue;
//                 private readonly int _version;
//                 private int _index;
//                 private (TElement Element, TPriority Priority) _current;
//
//                 internal Enumerator(PriorityQueue<TElement, TPriority> queue)
//                 {
//                     _version = queue._version;
//                     _queue = queue;
//                     _index = 0;
//                     _current = default;
//                 }
//
//                 public bool MoveNext()
//                 {
//                     PriorityQueue<TElement, TPriority> queue = _queue;
//
//                     if (queue._version == _version && _index < queue._count)
//                     {
//                         ref HeapEntry entry = ref queue._heap[_index];
//                         _current = (entry.Element, entry.Priority);
//                         _index++;
//                         return true;
//                     }
//
//                     if (queue._version != _version)
//                     {
//                         throw new InvalidOperationException("collection was modified");
//                     }
//
//                     return false;
//                 }
//
//                 public (TElement Element, TPriority Priority) Current => _current;
//                 object IEnumerator.Current => _current;
//
//                 public void Reset()
//                 {
//                     if (_queue._version != _version)
//                     {
//                         throw new InvalidOperationException("collection was modified");
//                     }
//
//                     _index = 0;
//                     _current = default;
//                 }
//
//                 public void Dispose()
//                 {
//                 }
//             }
//         }
//
//         #region Private Methods
//         private void Heapify()
//         {
//             HeapEntry[] heap = _heap;
//
//             for (int i = (_count - 1) >> 2; i >= 0; i--)
//             {
//                 HeapEntry entry = heap[i]; // ensure struct is copied before sifting
//                 SiftDown(i, in entry.Element, in entry.Priority);
//             }
//         }
//
//         private void AppendRaw(IEnumerable<(TElement Element, TPriority Priority)> values)
//         {
//             // TODO: specialize on ICollection types
//             var heap = _heap;
//             int count = _count;
//
//             foreach ((TElement element, TPriority priority) in values)
//             {
//                 if (count == heap.Length)
//                 {
//                     Resize(ref heap);
//                 }
//
//                 ref HeapEntry entry = ref heap[count];
//                 entry.Element = element;
//                 entry.Priority = priority;
//                 count++;
//             }
//
//             _heap = heap;
//             _count = count;
//         }
//
//         private void RemoveIndex(int index, out TElement element, out TPriority priority)
//         {
//             Debug.Assert(index < _count);
//
//             (element, priority) = _heap[index];
//
//             int lastElementPos = --_count;
//             ref HeapEntry lastElement = ref _heap[lastElementPos];
//
//             if (lastElementPos > 0)
//             {
// #if SIFTDOWN_EMPTY_NODES
//                 SiftDownHeapPropertyRequired(index, in lastElement.Element, in lastElement.Priority);
// #else
//                 SiftDown(index, in lastElement.Element, in lastElement.Priority);
// #endif
//             }
//
//             //if (RuntimeHelpers.IsReferenceOrContainsReferences<HeapEntry>())
//             {
//                 lastElement = default;
//             }
//         }
//
//         private void SiftUp(int index, in TElement element, in TPriority priority)
//         {
//             while (index > 0)
//             {
//                 int parentIndex = (index - 1) >> 2;
//                 ref HeapEntry parent = ref _heap[parentIndex];
//
//                 if (_priorityComparer.Compare(parent.Priority, priority) <= 0)
//                 {
//                     // parentPriority <= priority, heap property is satisfed
//                     break;
//                 }
//
//                 _heap[index] = parent;
//                 index = parentIndex;
//             }
//
//             ref HeapEntry entry = ref _heap[index];
//             entry.Element = element;
//             entry.Priority = priority;
//         }
//
//         private void SiftDown(int index, in TElement element, in TPriority priority)
//         {
//             int minChildIndex;
//             int count = _count;
//             HeapEntry[] heap = _heap;
//
//             while ((minChildIndex = (index << 2) + 1) < count)
//             {
//                 // find the child with the minimal priority
//                 ref HeapEntry minChild = ref heap[minChildIndex];
//                 int childUpperBound = Math.Min(count, minChildIndex + 4);
//
//                 for (int nextChildIndex = minChildIndex + 1; nextChildIndex < childUpperBound; nextChildIndex++)
//                 {
//                     ref HeapEntry nextChild = ref heap[nextChildIndex];
//                     if (_priorityComparer.Compare(nextChild.Priority, minChild.Priority) < 0)
//                     {
//                         minChildIndex = nextChildIndex;
//                         minChild = ref nextChild;
//                     }
//                 }
//
//                 // compare with inserted priority
//                 if (_priorityComparer.Compare(priority, minChild.Priority) <= 0)
//                 {
//                     // priority <= minChild, heap property is satisfied
//                     break;
//                 }
//
//                 heap[index] = minChild;
//                 index = minChildIndex;
//             }
//
//             ref HeapEntry entry = ref heap[index];
//             entry.Element = element;
//             entry.Priority = priority;
//         }
//
// #if SIFTDOWN_EMPTY_NODES
//         private void SiftDownHeapPropertyRequired(int index, in TElement element, in TPriority priority)
//         {
//             int emptyNodeIndex = SiftDownEmptyNode(index);
//             SiftUp(emptyNodeIndex, in element, in priority);
//         }
//
//         private int SiftDownEmptyNode(int emptyNodeIndex)
//         {
//             int count = _count;
//             int minChildIndex;
//             HeapEntry[] heap = _heap;
//
//             while ((minChildIndex = (emptyNodeIndex << 2) + 1) < count)
//             {
//                 // find the child with the minimal priority
//                 ref HeapEntry minChild = ref heap[minChildIndex];
//                 int childUpperBound = Math.Min(count, minChildIndex + 4);
//
//                 for (int nextChildIndex = minChildIndex + 1; nextChildIndex < childUpperBound; nextChildIndex++)
//                 {
//                     ref HeapEntry nextChild = ref heap[nextChildIndex];
//                     if (_priorityComparer.Compare(nextChild.Priority, minChild.Priority) < 0)
//                     {
//                         minChildIndex = nextChildIndex;
//                         minChild = ref nextChild;
//                     }
//                 }
//
//                 heap[emptyNodeIndex] = minChild;
//                 emptyNodeIndex = minChildIndex;
//             }
//
//             return emptyNodeIndex;
//         }
// #endif
//
//         private void Resize(ref HeapEntry[] heap)
//         {
//             int newSize = heap.Length == 0 ? DefaultCapacity : 2 * heap.Length;
//             Array.Resize(ref heap, newSize);
//         }
//
//         private struct HeapEntry
//         {
//             public TElement Element;
//             public TPriority Priority;
//
//             public void Deconstruct(out TElement element, out TPriority priority)
//             {
//                 element = Element;
//                 priority = Priority;
//             }
//         }
//
// #if DEBUG
//         public void ValidateInternalState()
//         {
//             if (_heap.Length < _count)
//             {
//                 throw new Exception("invalid elements array length");
//             }
//
//             foreach ((var element, var idx) in _heap.Select((x, i) => (x.Element, i)).Skip(_count))
//             {
//                 if (!IsDefault(element))
//                 {
//                     throw new Exception($"Non-zero element '{element}' at index {idx}.");
//                 }
//             }
//
//             foreach ((var priority, var idx) in _heap.Select((x, i) => (x.Priority, i)).Skip(_count))
//             {
//                 if (!IsDefault(priority))
//                 {
//                     throw new Exception($"Non-zero priority '{priority}' at index {idx}.");
//                 }
//             }
//
//             static bool IsDefault<T>(T value)
//             {
//                 T defaultVal = default;
//
//                 if (defaultVal is null)
//                 {
//                     return value is null;
//                 }
//
//                 return value!.Equals(defaultVal);
//             }
//         }
// #endif
//         #endregion
//     }
// }
// #endif