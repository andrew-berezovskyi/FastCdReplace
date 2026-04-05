using System.Text;

namespace FastCdReplace.Core
{
    public sealed class ProcessingOptions
    {
        public string InputFolderPath { get; set; } = string.Empty;

        public bool CreateOutputFolder { get; set; } = true;

        public bool ShowStatistics { get; set; } = false;

        /// <summary>
        /// Розмір блока читання в байтах.
        /// Для великих txt-файлів можна ставити 4-8 МБ.
        /// </summary>
        public int BufferSizeBytes { get; set; } = 4 * 1024 * 1024;

        /// <summary>
        /// Кодування текстових файлів.
        /// Поки лишаємо UTF8 за замовчуванням.
        /// Якщо в реальних файлах буде інше кодування — змінимо.
        /// </summary>
        public Encoding TextEncoding { get; set; } = new UTF8Encoding(false);

        /// <summary>
        /// Дозволити паралельну обробку двох файлів.
        /// За умовою завдання: 1 файл - 1 потік, 2 файли - 2 потоки.
        /// </summary>
        public bool EnableParallelProcessing { get; set; } = true;
    }
}