using System;
using System.Collections.Generic;

namespace FastCdReplace.Core
{
    public sealed class FileProcessResult
    {
        public sealed class NumberStatisticRow
        {
            public int Number { get; set; }

            public long InputValue { get; set; }

            public long OutputValue { get; set; }

            public long Difference => InputValue - OutputValue;
        }

        public string InputFilePath { get; set; } = string.Empty;

        public string OutputTextFilePath { get; set; } = string.Empty;

        public string OutputBinFilePath { get; set; } = string.Empty;

        public long InputFileSizeBytes { get; set; }

        public long OutputTextFileSizeBytes { get; set; }

        public long OutputBinFileSizeBytes { get; set; }

        public int InputNumbersCount { get; set; }

        public int OutputNumbersCount { get; set; }

        public int BlocksCount { get; set; }

        public long InputNumbersSum { get; set; }

        public long OutputNumbersSum { get; set; }

        public List<NumberStatisticRow> NumberStatistics { get; set; } = new List<NumberStatisticRow>();

        public TimeSpan Elapsed { get; set; }

        public long TextSizeDeltaBytes => OutputTextFileSizeBytes - InputFileSizeBytes;

        public int NumbersDelta => OutputNumbersCount - InputNumbersCount;

        public long NumbersSumDelta => OutputNumbersSum - InputNumbersSum;

        public double OutputNumbersPercentOfInput =>
            InputNumbersCount == 0 ? 0d : OutputNumbersCount * 100.0 / InputNumbersCount;

        public double OutputNumbersSumPercentOfInput =>
            InputNumbersSum == 0 ? 0d : OutputNumbersSum * 100.0 / InputNumbersSum;

        public double NumbersPercentDelta =>
            InputNumbersCount == 0 ? 0d : (OutputNumbersCount - InputNumbersCount) * 100.0 / InputNumbersCount;

        public double NumbersSumPercentDelta =>
            InputNumbersSum == 0 ? 0d : (OutputNumbersSum - InputNumbersSum) * 100.0 / InputNumbersSum;
    }
}