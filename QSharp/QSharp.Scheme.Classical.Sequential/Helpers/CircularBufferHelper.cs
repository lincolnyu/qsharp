using System.Collections.Generic;

namespace QSharp.Scheme.Classical.Sequential.Helpers
{
    public static class CircularBufferHelper
    {
        public static void PlaceMultipleAt<T>(this IList<T> circularBuffer, int start, IEnumerable<T> items)
        {
            var target = start;
            var bufferLength = circularBuffer.Count;
            foreach (var item in items)
            {
                circularBuffer[target] = item;
                target++;
                if (target == bufferLength) target = 0;
            }
        }

        public static void UncheckedCopy<T>(this IList<T> circularBuffer, int oldStart, int newStart, int count)
        {
            var bufferLength = circularBuffer.Count;
            var diff = newStart - oldStart;
            if (diff < 0) diff += bufferLength;
            if (diff < count)
            {
                var oldPos = (oldStart + count - 1) % bufferLength;
                var newPos = (newStart + count - 1) % bufferLength;
                while (newPos != newStart)
                {
                    circularBuffer[newPos] = circularBuffer[oldPos];
                    if (--oldPos < 0) oldPos = bufferLength - 1;
                    if (--newPos < 0) newPos = bufferLength - 1;
                }
                circularBuffer[newStart] = circularBuffer[oldStart];
            }
            else
            {
                var oldPos = oldStart;
                var newPos = newStart;
                for (var i = 0; i < count; i++)
                {
                    circularBuffer[newPos] = circularBuffer[oldPos];
                    if (++oldPos == bufferLength) oldPos = 0;
                    if (++newPos == bufferLength) newPos = 0;
                }
            }
        }

        public static void UncheckedMove<T>(this IList<T> circularBuffer, int oldStart, int newStart, int count)
        {
            var bufferLength = circularBuffer.Count;
            var diff = newStart - oldStart;
            if (diff < 0) diff += bufferLength;
            if (diff < count)
            {
                var oldPos = (oldStart + count - 1) % bufferLength;
                var newPos = (newStart + count - 1) % bufferLength;
                while (newPos != newStart)
                {
                    circularBuffer[newPos] = circularBuffer[oldPos];
                    circularBuffer[oldPos] = default(T);
                    if (--oldPos < 0) oldPos = bufferLength - 1;
                    if (--newPos < 0) newPos = bufferLength - 1;
                }
                circularBuffer[newStart] = circularBuffer[oldStart];
                circularBuffer[oldStart] = default(T);
            }
            else
            {
                var oldPos = oldStart;
                var newPos = newStart;
                for (var i = 0; i < count; i++)
                {
                    circularBuffer[newPos] = circularBuffer[oldPos];
                    circularBuffer[oldPos] = default(T);
                    if (++oldPos == bufferLength) oldPos = 0;
                    if (++newPos == bufferLength) newPos = 0;
                }
            }
        }
    }
}
