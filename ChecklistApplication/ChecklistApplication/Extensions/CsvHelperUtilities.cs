using CsvHelper;
using System.Threading.Tasks;

namespace ChecklistApplication.Extensions
{
    public static class CsvHelperUtilities
    {
        public static async Task<bool> ReadAsync(this ICsvReader reader) =>
            await Task.Run(() => reader.Read());

        public static async Task<T> GetRecordAsync<T>(this ICsvReaderRow readerRow) =>
            await Task.Run(() => readerRow.GetRecord<T>());

        public static async Task<T> GetFieldAsync<T>(this ICsvReaderRow readerRow, int index) =>
            await Task.Run(() => readerRow.GetField<T>(index));

        public static async Task<T> GetFieldAsync<T>(this ICsvReaderRow readerRow, string name) =>
            await Task.Run(() => readerRow.GetField<T>(name));

        public static async Task<T> GetFieldAsync<T>(this ICsvReaderRow readerRow, string name, int index) => 
            await Task.Run(() => readerRow.GetField<T>(name, index));

        public static async Task WriteHeaderAsync<T>(this ICsvWriter writer) =>
            await Task.Run(() => writer.WriteHeader<T>());

        public static async Task WriteRecordAsync<T>(this ICsvWriter writer, T item) =>
            await Task.Run(() => writer.WriteRecord(item));
    }
}
