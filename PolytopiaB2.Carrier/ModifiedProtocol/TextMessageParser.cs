using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PolytopiaA10.Carrier.Hubs.ModifiedProtocol
{
    internal static class TextMessageParser
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseMessage(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> payload)
        {
            if (buffer.IsSingleSegment)
            {
                var span = buffer.First.Span;
                var index = span.IndexOf(TextMessageFormatter.RecordSeparator);
                if (index == -1)
                {
                    payload = default;
                    return false;
                }

                payload = buffer.Slice(0, index);

                buffer = buffer.Slice(index + 1);

                return true;
            }
            else
            {
                return TryParseMessageMultiSegment(ref buffer, out payload);
            }
        }

        private static bool TryParseMessageMultiSegment(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> payload)
        {
            var position = buffer.PositionOf(TextMessageFormatter.RecordSeparator);
            if (position == null)
            {
                payload = default;
                return false;
            }

            payload = buffer.Slice(0, position.Value);

            // Skip record separator
            buffer = buffer.Slice(buffer.GetPosition(1, position.Value));

            return true;
        }
    }
}
