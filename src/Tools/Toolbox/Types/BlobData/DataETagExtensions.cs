using System.Diagnostics;

using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

public static class DataETagExtensions
{
    extension(DataETag subject)
    {
        public Option Validate() => DataETag.Validator.Validate(subject).ToOptionStatus();

        [DebuggerStepThrough]
        public string DataToString() => subject.Data.BytesToString();

        public DataETag StripETag() => new DataETag([.. subject.Data]);
        public string ToHash() => subject.Data.ToHexHash();
        public DataETag WithHash() => new DataETag(subject.NotNull().Data, subject.ToHash());

        public DataETag WithETag(string eTag) => new DataETag(subject.Data, eTag.NotEmpty());

    }

    extension<T>(T value)
    {
        [DebuggerStepThrough]
        public DataETag ToDataETag(string? currentETag = null)
        {
            value.NotNull();

            switch (value)
            {
                case DataETag dataTag: return dataTag;

                case Stream stream:
                    {
                        using var memoryStream = new MemoryStream();
                        stream.CopyTo(memoryStream);

                        return new DataETag(memoryStream.ToArray(), currentETag);
                    }

                case byte[] b1:
                    return new DataETag(b1, currentETag);

                default:
                    var bytes = value.ConvertToBytes();
                    return new DataETag(bytes, currentETag);
            }
        }

        [DebuggerStepThrough]
        public DataETag ToDataETagWithHash()
        {
            value.NotNull();
            if (value is DataETag dataTag) return dataTag;

            var bytes = value.ConvertToBytes();
            return new DataETag(bytes, bytes.ToHexHash());
        }

        [DebuggerStepThrough]
        private byte[] ConvertToBytes()
        {
            value.NotNull();

            return value switch
            {
                null => throw new ArgumentNullException("value"),
                IEnumerable<DataETag> => throw new ArgumentException("No array are allowed"),
                IEnumerable<byte> v => v.ToArray(),
                string v => v.ToBytes(),
                Memory<byte> v => v.ToArray(),
                var v => v.ToJson().ToBytes(),
            };
        }
    }

    extension<T>(Task<Option<DataETag>> option)
    {
        public async Task<Option<T>> MapAsync()
        {
            var result = await option;
            if (result.IsError()) return result.ToOptionStatus<T>();
            return result.Return().ToObject<T>();
        }
    }

}
