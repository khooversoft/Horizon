using System.Collections.Immutable;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.test.Data;

public class DataETagTests
{
    [Fact]
    public void Empty()
    {
        var blobData = new DataETag(Array.Empty<byte>());
        blobData.Data.NotNull();
        blobData.Data.Length.Be(0);
        blobData.ETag.BeNull();
    }

    [Fact]
    public void ConstructorWithByteArray_ShouldCreateBlobData()
    {
        byte[] data = [1, 2, 3, 4, 5];

        var blobData = new DataETag(data);

        blobData.Data.Length.Be(5);
        blobData.Data.SequenceEqual(data).BeTrue();
        blobData.ETag.BeNull();
    }

    [Fact]
    public void ConstructorWithImmutableArray_ShouldCreateBlobData()
    {
        var immutableData = ImmutableArray.Create<byte>(1, 2, 3, 4, 5);

        var blobData = new DataETag(immutableData, null);

        blobData.Data.Length.Be(5);
        blobData.Data.SequenceEqual(immutableData).BeTrue();
        blobData.ETag.BeNull();
    }

    [Fact]
    public void ETag_ShouldBeConsistent_ForSameData()
    {
        byte[] data = [10, 20, 30];

        var blobData1 = new DataETag(data);
        var blobData2 = new DataETag(data);

        blobData1.ETag.Be(blobData2.ETag);
    }

    [Fact]
    public void ETag_ShouldBeDifferent_ForDifferentData()
    {
        byte[] data1 = [1, 2, 3];
        byte[] data2 = [4, 5, 6];
        var blobData1 = new DataETag(data1).WithHash();
        var blobData2 = new DataETag(data2).WithHash();

        (blobData1.ETag != blobData2.ETag).BeTrue();
    }

    [Fact]
    public void Append_ShouldCombineTwoBlobData()
    {
        byte[] data1 = [1, 2, 3];
        byte[] data2 = [4, 5, 6];
        var blobData1 = new DataETag(data1);
        var blobData2 = new DataETag(data2);

        var result = blobData1.Append(blobData2);

        byte[] expected = [1, 2, 3, 4, 5, 6];
        result.Data.Length.Be(6);
        result.Data.SequenceEqual(expected).BeTrue();
    }

    [Fact]
    public void PlusOperator_ShouldCombineTwoBlobData()
    {
        byte[] data1 = [1, 2, 3];
        byte[] data2 = [4, 5, 6];
        var blobData1 = new DataETag(data1);
        var blobData2 = new DataETag(data2);

        var result = blobData1 + blobData2;

        byte[] expected = [1, 2, 3, 4, 5, 6];
        result.Data.Length.Be(6);
        result.Data.SequenceEqual(expected).BeTrue();
    }

    [Fact]
    public void ImplicitConversion_FromByteArray_ShouldCreateBlobData()
    {
        byte[] data = [7, 8, 9];

        DataETag blobData = data;

        blobData.Data.Length.Be(3);
        blobData.Data.SequenceEqual(data).BeTrue();
    }

    [Fact]
    public void Equals_ShouldReturnTrue_ForSameData()
    {
        byte[] data1 = [1, 2, 3];
        byte[] data2 = [1, 2, 3];
        var blobData1 = new DataETag(data1);
        var blobData2 = new DataETag(data2);

        blobData1.Equals(blobData2).BeTrue();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_ForDifferentData()
    {
        byte[] data1 = [1, 2, 3];
        byte[] data2 = [4, 5, 6];
        var blobData1 = new DataETag(data1);
        var blobData2 = new DataETag(data2);

        blobData1.Equals(blobData2).BeFalse();
    }

    [Fact]
    public void EqualOperator_ShouldReturnTrue_ForSameData()
    {
        byte[] data1 = [1, 2, 3];
        byte[] data2 = [1, 2, 3];
        var blobData1 = new DataETag(data1);
        var blobData2 = new DataETag(data2);

        (blobData1 == blobData2).BeTrue();
    }

    [Fact]
    public void EqualOperator_ShouldReturnFalse_ForDifferentData()
    {
        byte[] data1 = [1, 2, 3];
        byte[] data2 = [4, 5, 6];
        var blobData1 = new DataETag(data1);
        var blobData2 = new DataETag(data2);

        (blobData1 == blobData2).BeFalse();
    }

    [Fact]
    public void EqualOperator_ShouldHandleNullComparisons()
    {
        byte[] data = [1, 2, 3];
        var blobData = new DataETag(data);
        DataETag? nullBlobData = null;

        (blobData == null).BeFalse();
        (null == blobData).BeFalse();
        (nullBlobData == null).BeTrue();
    }

    [Fact]
    public void NotEqualOperator_ShouldReturnFalse_ForSameData()
    {
        byte[] data1 = [1, 2, 3];
        byte[] data2 = [1, 2, 3];
        var blobData1 = new DataETag(data1);
        var blobData2 = new DataETag(data2);

        (blobData1 != blobData2).BeFalse();
    }

    [Fact]
    public void NotEqualOperator_ShouldReturnTrue_ForDifferentData()
    {
        byte[] data1 = [1, 2, 3];
        byte[] data2 = [4, 5, 6];
        var blobData1 = new DataETag(data1);
        var blobData2 = new DataETag(data2);

        (blobData1 != blobData2).BeTrue();
    }

    [Fact]
    public void NotEqualOperator_ShouldHandleNullComparisons()
    {
        byte[] data = [1, 2, 3];
        var blobData = new DataETag(data);
        DataETag? nullBlobData = null;

        (blobData != null).BeTrue();
        (null != blobData).BeTrue();
        (nullBlobData != null).BeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_ForNull()
    {
        byte[] data = [1, 2, 3];
        var blobData = new DataETag(data);

        blobData.Equals(null).BeFalse();
    }

    [Fact]
    public void GetHashCode_ShouldBeConsistent_ForSameData()
    {
        byte[] data1 = [1, 2, 3];
        byte[] data2 = [1, 2, 3];
        var blobData1 = new DataETag(data1);
        var blobData2 = new DataETag(data2);

        blobData1.GetHashCode().Be(blobData2.GetHashCode());
    }

    [Fact]
    public void ToBlobData_FromByteEnumerable_ShouldCreateBlobData()
    {
        IEnumerable<byte> data = new byte[] { 1, 2, 3 };

        var blobData = data.ToDataETag();

        byte[] expected = [1, 2, 3];
        blobData.Data.Length.Be(3);
        blobData.Data.SequenceEqual(expected).BeTrue();
    }

    [Fact]
    public void ToBlobData_FromString_ShouldCreateBlobData()
    {
        string text = "Hello";

        var blobData = text.ToDataETag();

        (blobData.Data.Length > 0).BeTrue();
        blobData.ETag.BeNull();
    }

    [Fact]
    public void ToBlobData_FromBlobData_ShouldReturnSameInstance()
    {
        byte[] data = [1, 2, 3];
        var original = new DataETag(data);

        var result = original.ToDataETag();

        ReferenceEquals(original, result).BeTrue();
    }

    [Fact]
    public void ToBlobData_FromMemory_ShouldCreateBlobData()
    {
        Memory<byte> memory = new byte[] { 1, 2, 3 };

        var blobData = memory.ToDataETag();

        byte[] expected = [1, 2, 3];
        blobData.Data.Length.Be(3);
        blobData.Data.SequenceEqual(expected).BeTrue();
    }

    [Fact]
    public void Append_WithEmptyBlobData_ShouldReturnOriginalData()
    {
        byte[] data = [1, 2, 3];
        var blobData = new DataETag(data);
        var empty = new DataETag(Array.Empty<byte>());

        var result = blobData.Append(empty);

        byte[] expected = [1, 2, 3];
        result.Data.Length.Be(3);
        result.Data.SequenceEqual(expected).BeTrue();
    }

    [Fact]
    public void PlusOperator_ChainedAppends_ShouldCombineAll()
    {
        byte[] d1 = [1];
        byte[] d2 = [2];
        byte[] d3 = [3];
        var blob1 = new DataETag(d1);
        var blob2 = new DataETag(d2);
        var blob3 = new DataETag(d3);

        var result = blob1 + blob2 + blob3;

        byte[] expected = [1, 2, 3];
        result.Data.Length.Be(3);
        result.Data.SequenceEqual(expected).BeTrue();
    }

    private record TestPayload(string Name, int Count);

    [Fact]
    public void TestSerializerRegistery()
    {
        JsonSerializerContextRegistered.Find<DataETag>().BeOk();
    }

    [Fact]
    public void EmptyDataETagEqual()
    {
        var d1 = Array.Empty<byte>();
        DataETag e1 = new DataETag(d1);
        DataETag e2 = new DataETag(d1);

        (e1 == e2).BeTrue();

        string data = e1.ToJson();
        data.NotEmpty();
        DataETag e3 = data.ToObject<DataETag>().NotNull();

        (e1 == e3).BeTrue();
        Enumerable.SequenceEqual(e3.Data, e1.Data).BeTrue();
        e3.ETag.Be(e1.ETag);
    }

    [Fact]
    public void EqualityWithSameDataPreservesETag()
    {
        var d1 = "hello".ToBytes();
        DataETag e1 = new DataETag(d1, "FF");

        string data = e1.ToJson();
        DataETag e2 = data.ToObject<DataETag>().NotNull();

        (e1 == e2).BeTrue();
        e2.ETag.Be("FF");
    }

    [Fact]
    public void EqualityDiffersWhenOneIsEmpty()
    {
        DataETag empty = new DataETag(Array.Empty<byte>());
        DataETag populated = new DataETag("simple".ToBytes());

        (empty == populated).BeFalse();
    }

    [Fact]
    public void EqualityIgnoresETag()
    {
        var data = "same".ToBytes();
        DataETag e1 = new DataETag(data, "etag-1");
        DataETag e2 = new DataETag(data, "etag-2");

        (e1 == e2).BeTrue();
    }

    [Fact]
    public void ValidateEmptyDataFails()
    {
        var tag = new DataETag(Array.Empty<byte>());
        var result = tag.Validate();

        result.IsBadRequest().BeTrue();
        (result.Error?.Contains("Data 0 is invalid") ?? false).BeTrue();
    }

    [Fact]
    public void ValidatePopulatedDataSucceeds()
    {
        var tag = new DataETag("ok".ToBytes());

        tag.Validate().IsOk().BeTrue();
    }

    [Fact]
    public void AppendConcatenatesDataAndDropsETag()
    {
        DataETag left = new DataETag("left".ToBytes(), "etag-left");
        DataETag right = new DataETag("right".ToBytes(), "etag-right");

        DataETag combined = left + right;

        combined.DataToString().Be("leftright");
        (combined.ETag == null).BeTrue();
    }

    [Fact]
    public void StripETagRemovesOnlyETag()
    {
        DataETag tag = new DataETag("payload".ToBytes(), "etag-value");

        DataETag stripped = tag.StripETag();

        stripped.DataToString().Be("payload");
        (stripped.ETag == null).BeTrue();
    }

    [Fact]
    public void WithETagOverridesValue()
    {
        DataETag tag = new DataETag("payload".ToBytes(), "old");

        DataETag updated = tag.WithETag("new");

        updated.DataToString().Be("payload");
        updated.ETag.Be("new");
    }

    [Fact]
    public void WithHashSetsHashAsETag()
    {
        var data = "hash-me".ToBytes();
        var expectedHash = data.ToHexHash();

        DataETag tagged = new DataETag(data).WithHash();

        tagged.ETag.Be(expectedHash);
        tagged.DataToString().Be("hash-me");
    }

    [Fact]
    public void ToDataETagWithHashAddsHash()
    {
        var expectedHash = "abc".ToBytes().ToHexHash();

        DataETag tagged = "abc".ToDataETagWithHash();

        tagged.DataToString().Be("abc");
        tagged.ETag.Be(expectedHash);
    }

    [Fact]
    public void ToObjectRoundTripsPayload()
    {
        var payload = new TestPayload("alpha", 42);

        DataETag data = payload.ToDataETag();
        TestPayload roundtrip = data.ToObject<TestPayload>();

        (roundtrip == payload).BeTrue();
    }

    [Fact]
    public void JsonSerializationRoundTripPreservesETag()
    {
        var original = new DataETag("payload".ToBytes(), "etag-value");

        string json = original.ToJson();
        DataETag roundtrip = json.ToObject<DataETag>();
        roundtrip.NotNull();

        roundtrip.DataToString().Be("payload");
        roundtrip.ETag.Be("etag-value");
        (roundtrip == original).BeTrue();
    }
}
