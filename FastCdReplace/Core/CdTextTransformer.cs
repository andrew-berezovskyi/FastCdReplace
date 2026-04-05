using System;
using System.Collections.Generic;

namespace FastCdReplace.Core
{
    /// <summary>
    /// Текстовое преобразование блока чисел для формирования *-CDout.txt.
    ///
    /// Реализация ориентирована на фактический эталонный пример:
    /// 31-2.txt -> 31-2-CDout.txt
    /// </summary>
    public sealed class CdTextTransformer
    {
        public IReadOnlyList<int> Transform(IReadOnlyList<int> inputNumbers)
        {
            if (inputNumbers == null)
                throw new ArgumentNullException(nameof(inputNumbers));

            if (inputNumbers.Count == 0)
                return Array.Empty<int>();

            var result = new List<int>(capacity: inputNumbers.Count + 16);

            int i = 0;
            while (i < inputNumbers.Count)
            {
                if (TryMatchPattern22113(inputNumbers, i))
                {
                    result.Add(1);
                    result.Add(2);
                    result.Add(3);
                    result.Add(3);
                    i += 5;
                    continue;
                }

                if (TryMatchSwap2X1(inputNumbers, i))
                {
                    result.Add(1);
                    result.Add(inputNumbers[i + 1]);
                    result.Add(2);
                    i += 3;
                    continue;
                }

                if (TryMatchDescendingExpand(inputNumbers, i, out int expandValue))
                {
                    for (int k = 0; k < expandValue - 1; k++)
                        result.Add(1);

                    i += 1;
                    continue;
                }

                result.Add(inputNumbers[i]);
                i++;
            }

            return result;
        }

        private static bool TryMatchPattern22113(IReadOnlyList<int> data, int index)
        {
            if (index + 4 >= data.Count)
                return false;

            return data[index] == 2
                && data[index + 1] == 2
                && data[index + 2] == 1
                && data[index + 3] == 1
                && data[index + 4] == 3;
        }

        private static bool TryMatchSwap2X1(IReadOnlyList<int> data, int index)
        {
            if (index + 2 >= data.Count)
                return false;

            if (data[index] != 2)
                return false;

            if (data[index + 2] != 1)
                return false;

            if (index + 3 < data.Count && data[index + 3] == 1)
                return false;

            return true;
        }

        private static bool TryMatchDescendingExpand(IReadOnlyList<int> data, int index, out int valueToExpand)
        {
            valueToExpand = 0;

            if (index + 2 >= data.Count)
                return false;

            int current = data[index];
            int next = data[index + 1];
            int afterNext = data[index + 2];

            if (current < 3)
                return false;

            if (next != current - 1)
                return false;

            if (afterNext >= current - 2)
                return false;

            valueToExpand = current;
            return true;
        }
    }
}