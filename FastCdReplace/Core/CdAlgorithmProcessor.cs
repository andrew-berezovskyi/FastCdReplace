using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FastCdReplace.Core
{
    public sealed class CdAlgorithmProcessor
    {
        private readonly CdTextTransformer _textTransformer;

        public CdAlgorithmProcessor()
        {
            _textTransformer = new CdTextTransformer();
        }

        public async Task<FileProcessResult> ProcessFileAsync(
            string inputFilePath,
            string? outputFolderPath,
            bool createOutputFiles,
            Encoding encoding,
            IProgress<FileProgressInfo>? progress,
            long totalBytesAllFiles,
            Func<long>? getProcessedBytesAllFiles,
            Action<long>? addProcessedBytesAllFiles,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(inputFilePath))
                throw new ArgumentException("Путь к входному файлу не задан.", nameof(inputFilePath));

            if (!File.Exists(inputFilePath))
                throw new FileNotFoundException("Входной файл не найден.", inputFilePath);

            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));

            var stopwatch = Stopwatch.StartNew();

            var inputFileInfo = new FileInfo(inputFilePath);
            long inputFileSizeBytes = inputFileInfo.Length;
            string inputFileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputFilePath);

            string outputTextFilePath = string.Empty;
            string outputBinFilePath = string.Empty;

            if (createOutputFiles)
            {
                if (string.IsNullOrWhiteSpace(outputFolderPath))
                    throw new ArgumentException("Путь к выходной папке не задан.", nameof(outputFolderPath));

                Directory.CreateDirectory(outputFolderPath);

                outputTextFilePath = Path.Combine(outputFolderPath, $"{inputFileNameWithoutExtension}-CDout.txt");
                outputBinFilePath = Path.Combine(outputFolderPath, $"{inputFileNameWithoutExtension}-CDout.bin");
            }

            int blocksCount = 0;
            int inputNumbersCount = 0;
            int outputNumbersCount = 0;
            long inputNumbersSum = 0;
            long outputNumbersSum = 0;

            using var inputFs = new FileStream(
                inputFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 1024 * 1024,
                options: FileOptions.SequentialScan);

            using var reader = new StreamReader(
                inputFs,
                encoding,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 1024 * 1024);

            using StreamWriter? outputTextWriter = createOutputFiles
                ? new StreamWriter(
                    new FileStream(
                        outputTextFilePath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None,
                        bufferSize: 1024 * 1024,
                        options: FileOptions.SequentialScan),
                    encoding,
                    bufferSize: 1024 * 1024)
                : null;

            using FileStream? outputBinStream = createOutputFiles
                ? new FileStream(
                    outputBinFilePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 1024 * 1024,
                    options: FileOptions.SequentialScan)
                : null;

            using var tokenReader = new NumberTokenReader(
                reader,
                Path.GetFileName(inputFilePath),
                inputFileSizeBytes,
                totalBytesAllFiles,
                getProcessedBytesAllFiles,
                addProcessedBytesAllFiles,
                progress,
                cancellationToken);

            using var bitWriter = createOutputFiles && outputBinStream != null
                ? new BitWriter(outputBinStream)
                : null;

            var inputValueTotals = new Dictionary<int, long>();
            var outputValueTotals = new Dictionary<int, long>();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int? blockCountValue = await tokenReader.ReadNextIntAsync();
                if (blockCountValue == null)
                    break;

                int declaredCount = blockCountValue.Value;
                if (declaredCount < 1)
                    throw new InvalidDataException($"Некорректный размер блока: {declaredCount}. Ожидается число >= 1.");

                var sourceBlock = new List<int>(declaredCount);

                for (int i = 0; i < declaredCount; i++)
                {
                    int? number = await tokenReader.ReadNextIntAsync();
                    if (number == null)
                    {
                        throw new InvalidDataException(
                            $"Файл повреждён: для блока заявлено {declaredCount} чисел, но файл закончился раньше.");
                    }

                    if (number.Value < 1)
                        throw new InvalidDataException($"Некорректное число во входном файле: {number.Value}. Ожидается число >= 1.");

                    sourceBlock.Add(number.Value);
                }

                IReadOnlyList<int> transformedBlock = _textTransformer.Transform(sourceBlock);

                blocksCount++;
                inputNumbersCount += sourceBlock.Count;
                outputNumbersCount += transformedBlock.Count;

                for (int i = 0; i < sourceBlock.Count; i++)
                {
                    int value = sourceBlock[i];
                    inputNumbersSum += value;
                    AddValueContribution(inputValueTotals, value);
                }

                for (int i = 0; i < transformedBlock.Count; i++)
                {
                    int value = transformedBlock[i];
                    outputNumbersSum += value;
                    AddValueContribution(outputValueTotals, value);
                }

                if (createOutputFiles && outputTextWriter != null)
                {
                    await outputTextWriter.WriteLineAsync(transformedBlock.Count.ToString());

                    for (int i = 0; i < transformedBlock.Count; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await outputTextWriter.WriteLineAsync(transformedBlock[i].ToString());
                    }
                }

                if (createOutputFiles && bitWriter != null)
                {
                    Write24BitUnsigned(bitWriter, transformedBlock.Count);

                    for (int i = 0; i < transformedBlock.Count; i++)
                    {
                        WriteUnaryNumber(bitWriter, transformedBlock[i]);
                    }
                }

                sourceBlock.Clear();
            }

            if (blocksCount == 0)
                throw new InvalidDataException("Входной файл пуст или не содержит ни одного корректного блока.");

            if (createOutputFiles && outputTextWriter != null)
                await outputTextWriter.FlushAsync();

            if (createOutputFiles && bitWriter != null)
                bitWriter.Flush();

            progress?.Report(new FileProgressInfo
            {
                FileName = Path.GetFileName(inputFilePath),
                BytesProcessed = inputFileSizeBytes,
                TotalBytes = inputFileSizeBytes,
                TotalProcessedBytesAllFiles = getProcessedBytesAllFiles?.Invoke() ?? inputFileSizeBytes,
                TotalBytesAllFiles = totalBytesAllFiles
            });

            stopwatch.Stop();

            long outputTextSizeBytes = 0;
            long outputBinSizeBytes = 0;

            if (createOutputFiles)
            {
                outputTextSizeBytes = new FileInfo(outputTextFilePath).Length;
                outputBinSizeBytes = new FileInfo(outputBinFilePath).Length;
            }

            return new FileProcessResult
            {
                InputFilePath = inputFilePath,
                OutputTextFilePath = outputTextFilePath,
                OutputBinFilePath = outputBinFilePath,
                InputFileSizeBytes = inputFileSizeBytes,
                OutputTextFileSizeBytes = outputTextSizeBytes,
                OutputBinFileSizeBytes = outputBinSizeBytes,
                InputNumbersCount = inputNumbersCount,
                OutputNumbersCount = outputNumbersCount,
                BlocksCount = blocksCount,
                InputNumbersSum = inputNumbersSum,
                OutputNumbersSum = outputNumbersSum,
                NumberStatistics = BuildNumberStatistics(inputValueTotals, outputValueTotals),
                Elapsed = stopwatch.Elapsed
            };
        }

        private static void AddValueContribution(Dictionary<int, long> totals, int value)
        {
            if (totals.TryGetValue(value, out long current))
                totals[value] = current + value;
            else
                totals[value] = value;
        }

        private static List<FileProcessResult.NumberStatisticRow> BuildNumberStatistics(
            Dictionary<int, long> inputTotals,
            Dictionary<int, long> outputTotals)
        {
            var allNumbers = new SortedSet<int>();

            foreach (int value in inputTotals.Keys)
                allNumbers.Add(value);

            foreach (int value in outputTotals.Keys)
                allNumbers.Add(value);

            var rows = new List<FileProcessResult.NumberStatisticRow>();

            foreach (int number in allNumbers)
            {
                inputTotals.TryGetValue(number, out long inputValue);
                outputTotals.TryGetValue(number, out long outputValue);

                rows.Add(new FileProcessResult.NumberStatisticRow
                {
                    Number = number,
                    InputValue = inputValue,
                    OutputValue = outputValue
                });
            }

            rows.Sort((a, b) => b.Number.CompareTo(a.Number));
            return rows;
        }

        private static void WriteUnaryNumber(BitWriter writer, int value)
        {
            if (value < 1)
                throw new InvalidDataException($"Некорректное число для бинарного кодирования: {value}. Ожидается число >= 1.");

            for (int i = 0; i < value - 1; i++)
                writer.WriteBit(true);

            writer.WriteBit(false);
        }

        private static void Write24BitUnsigned(BitWriter writer, int value)
        {
            if (value < 0 || value > 0xFFFFFF)
                throw new ArgumentOutOfRangeException(nameof(value), "24-битное значение вышло за допустимый диапазон.");

            for (int bit = 23; bit >= 0; bit--)
            {
                bool current = ((value >> bit) & 1) != 0;
                writer.WriteBit(current);
            }
        }

        private sealed class NumberTokenReader : IDisposable
        {
            private readonly StreamReader _reader;
            private readonly string _fileName;
            private readonly long _currentFileTotalBytes;
            private readonly long _totalBytesAllFiles;
            private readonly Func<long>? _getProcessedBytesAllFiles;
            private readonly Action<long>? _addProcessedBytesAllFiles;
            private readonly IProgress<FileProgressInfo>? _progress;
            private readonly CancellationToken _cancellationToken;

            private readonly char[] _buffer;
            private readonly StringBuilder _tokenBuilder;

            private int _bufferLength;
            private int _bufferIndex;
            private long _lastReportedPosition;
            private bool _endOfStream;

            public NumberTokenReader(
                StreamReader reader,
                string fileName,
                long currentFileTotalBytes,
                long totalBytesAllFiles,
                Func<long>? getProcessedBytesAllFiles,
                Action<long>? addProcessedBytesAllFiles,
                IProgress<FileProgressInfo>? progress,
                CancellationToken cancellationToken)
            {
                _reader = reader ?? throw new ArgumentNullException(nameof(reader));
                _fileName = fileName;
                _currentFileTotalBytes = currentFileTotalBytes;
                _totalBytesAllFiles = totalBytesAllFiles;
                _getProcessedBytesAllFiles = getProcessedBytesAllFiles;
                _addProcessedBytesAllFiles = addProcessedBytesAllFiles;
                _progress = progress;
                _cancellationToken = cancellationToken;

                _buffer = new char[8192];
                _tokenBuilder = new StringBuilder(32);
            }

            public async Task<int?> ReadNextIntAsync()
            {
                while (true)
                {
                    _cancellationToken.ThrowIfCancellationRequested();

                    if (_bufferIndex >= _bufferLength)
                    {
                        if (_endOfStream)
                        {
                            if (_tokenBuilder.Length == 0)
                                return null;

                            int valueAtEnd = ParsePositiveInt(_tokenBuilder.ToString());
                            _tokenBuilder.Clear();
                            return valueAtEnd;
                        }

                        _bufferLength = await _reader.ReadAsync(_buffer.AsMemory(0, _buffer.Length), _cancellationToken);
                        _bufferIndex = 0;

                        if (_bufferLength == 0)
                        {
                            _endOfStream = true;
                            continue;
                        }

                        long currentPosition = _reader.BaseStream.Position;
                        long delta = currentPosition - _lastReportedPosition;
                        if (delta < 0)
                            delta = 0;

                        _lastReportedPosition = currentPosition;

                        if (delta > 0)
                            _addProcessedBytesAllFiles?.Invoke(delta);

                        _progress?.Report(new FileProgressInfo
                        {
                            FileName = _fileName,
                            BytesProcessed = currentPosition,
                            TotalBytes = _currentFileTotalBytes,
                            TotalProcessedBytesAllFiles = _getProcessedBytesAllFiles?.Invoke() ?? currentPosition,
                            TotalBytesAllFiles = _totalBytesAllFiles
                        });
                    }

                    while (_bufferIndex < _bufferLength)
                    {
                        char c = _buffer[_bufferIndex++];
                        if (char.IsWhiteSpace(c))
                        {
                            if (_tokenBuilder.Length > 0)
                            {
                                int value = ParsePositiveInt(_tokenBuilder.ToString());
                                _tokenBuilder.Clear();
                                return value;
                            }
                        }
                        else
                        {
                            _tokenBuilder.Append(c);
                        }
                    }
                }
            }

            public void Dispose()
            {
            }

            private static int ParsePositiveInt(string token)
            {
                if (!int.TryParse(token, out int value))
                    throw new InvalidDataException($"Некорректное число во входном файле: '{token}'.");

                if (value < 1)
                    throw new InvalidDataException($"Некорректное число во входном файле: '{token}'. Ожидается целое число >= 1.");

                return value;
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

            public void WriteBit(bool bit)
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(BitWriter));

                if (bit)
                    _currentByte |= (byte)(1 << (7 - _bitPosition));

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