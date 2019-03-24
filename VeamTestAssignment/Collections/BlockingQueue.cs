using System;
using System.Collections.Generic;
using System.Threading;

namespace VeeamTestAssignment.Collections
{
    internal class BlockingQueue<T>
    {
        private readonly SemaphoreSlim producerSemaphore;
        private readonly SemaphoreSlim consumerSemaphore;
        private readonly Queue<T> queue;

        private bool completeAdd;

        public BlockingQueue(int capacity)
        {
            this.producerSemaphore = new SemaphoreSlim(capacity, capacity);
            this.consumerSemaphore = new SemaphoreSlim(0, capacity);
            this.queue = new Queue<T>(capacity);
        }

        public bool IsEmpty => this.queue.Count == 0;

        public bool IsCompleted => this.completeAdd && this.IsEmpty;

        public int Count => this.queue.Count;

        public void CompleteAdding()
        {
            this.completeAdd = true;
        }

        public void Add(T value)
        {
            if (this.completeAdd)
            {
                throw new InvalidOperationException("Blocking queue is already complete");
            }

            this.producerSemaphore.Wait();

            lock(this.queue)
            {
                this.queue.Enqueue(value);
            }

            this.consumerSemaphore.Release();
        }

        public bool TryTake(out T value, int millisecondsTimeout)
        {
            value = default(T);
            if (this.IsEmpty)
            {
                return false;
            }

            if (!this.consumerSemaphore.Wait(millisecondsTimeout))
            {
                return false;
            }

            lock (this.queue)
            {
                value = this.queue.Dequeue();
            }
            
            this.producerSemaphore.Release();

            return true;
        }
    }
}
