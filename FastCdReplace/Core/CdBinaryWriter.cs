using System;
using System.Collections.Generic;
using System.IO;

namespace FastCdReplace.Core
{
    /// <summary>
    /// Записує *.bin для перетвореного блока.
    /// 
    /// Формат:
    /// 1. Спочатку 24 біти - кількість чисел у блоці.
    /// 2. Далі для кожного числа n:
    ///      (n - 1) бітів '1'
    ///      1 біт '0'
    /// 
    /// Приклади:
    /// 1 -> 0
    /// 2 -> 10
    /// 3 -> 110
    /// 4 -> 1110
    /// </summary>
    public sealed class CdBinaryWriter
    {
        public void WriteBlock(string outputBinPath, IReadOnlyList<int> numbers)
        {
            if (string.IsNullOrWhiteSpace(outputBinPath))
                throw new ArgumentException("Output .bin path is empty.", nameof(outputBinPath));

            if (numbers == null)
                throw new ArgumentNullException(nameof(numbers));

            using var fs = new FileStream(
                outputBinPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 1024 * 1024,
                options: FileOptions.SequentialScan);

            using var bitWriter = new BitWriter(fs);

            // 24 біти на кількість чисел у блоці
            Write24BitUnsigned(bitWriter, numbers.Count);

            // Унарний код кожного числа
            for (int i = 0; i < numbers.Count; i++)
            {
                int value = numbers[i];

                if (value < 1)
                    throw new InvalidDataException($"Invalid number for CD binary encoding: {value}. Expected >= 1.");

                for (int b = 0; b < value - 1; b++)
                    bitWriter.WriteBit(true);

                bitWriter.WriteBit(false);
            }

            bitWriter.Flush();
        }

        private static void Write24BitUnsigned(BitWriter writer, int value)
        {
            if (value < 0 || value > 0xFFFFFF)
                throw new ArgumentOutOfRangeException(nameof(value), "24-bit value out of range.");

            for (int bit = 23; bit >= 0; bit--)
            {
                bool current = ((value >> bit) & 1) != 0;
                writer.WriteBit(current);
            }
        }

        private sealed class BitWriter : IDisposable
        {
            private readonly Stream _stream;
            private byte _currentByte;
            private int _bitPosition;
            private bool _disposed;

            public BitWriter(Stream stream)
            {
                _stream = stream ?? throw new ArgumentNullException(nameof(stream));
                _currentByte = 0;
                _bitPosition = 0;
            }

            /// <summary>
            /// Запис одного біта зліва направо всередині байта.
            /// Перший записаний біт стає старшим бітом байта.
            /// </summary>
            public void WriteBit(bool bit)
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(BitWriter));

                if (bit)
                {
                    _currentByte |= (byte)(1 << (7 - _bitPosition));
                }

                _bitPosition++;

                if (_bitPosition == 8)
                    FlushCurrentByte();
            }

            public void Flush()
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(BitWriter));

                if (_bitPosition > 0)
                    FlushCurrentByte();

                _stream.Flush();
            }

            private void FlushCurrentByte()
            {
                _stream.WriteByte(_currentByte);
                _currentByte = 0;
                _bitPosition = 0;
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                Flush();
                _disposed = true;
            }
        }
    }
}