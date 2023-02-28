// ------------------------------------------------------------------------
// Copyright 2023 The Dapr Authors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ------------------------------------------------------------------------

using Xunit;

namespace Dapr.PluggableComponents.Components.StateStore;

public sealed class StateStoreGetResponseTests
{
    [Fact]
    public void ToGetResponseNullTest()
    {
        var grpcResponse = StateStoreGetResponse.ToGetResponse(null);

        Assert.Equal(String.Empty, grpcResponse.ContentType);
        Assert.Equal(new byte[] { }, grpcResponse.Data);
        Assert.Null(grpcResponse.Etag);
        Assert.Empty(grpcResponse.Metadata);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("application/json", "application/json")]
    public void ToGetResponseContentTypeTests(string? contentType, string expectedContentType)
    {
        var response = new StateStoreGetResponse
        {
            ContentType = contentType
        };

        var grpcResponse = StateStoreGetResponse.ToGetResponse(response);

        Assert.Equal(expectedContentType, grpcResponse.ContentType);
    }

    public static IEnumerable<object[]> DataTests =>
        new[]{
            new[]
            {
                new byte[] {}
            },
            new[]
            {
                new byte[] { 0x01, 0x02, 0x03 }
            }
        };

    [Theory]
    [MemberData(nameof(DataTests))]
    public void ToGetResponseDataTests(byte[] data)
    {
        var response = new StateStoreGetResponse
        {
            Data = data
        };

        var grpcResponse = StateStoreGetResponse.ToGetResponse(response);

        Assert.Equal(data, grpcResponse.Data);
    }

    [Fact]
    public void ToGetResponseETagTests()
    {
        var response = new StateStoreGetResponse();

        var grpcResponse = StateStoreGetResponse.ToGetResponse(response);

        Assert.Null(grpcResponse.Etag);

        string etag = "value";

        response = new StateStoreGetResponse
        {
            ETag = etag
        };

        grpcResponse = StateStoreGetResponse.ToGetResponse(response);

        Assert.NotNull(grpcResponse.Etag);
        Assert.Equal(etag, grpcResponse.Etag.Value);
    }

    public static IEnumerable<object[]> MetadataTests =>
        new[]{
            new[]
            {
                new Dictionary<string, string>()
            },
            new[]
            {
                new Dictionary<string, string>
                {
                    { "key1", "value1" },
                    { "key2", "value2" }
                }
            }
        };

    [Theory]
    [MemberData(nameof(MetadataTests))]
    public void ToGetResponseMetadataTests(Dictionary<string, string> metadata)
    {
        var response = new StateStoreGetResponse
        {
            Metadata = metadata
        };

        var grpcResponse = StateStoreGetResponse.ToGetResponse(response);

        Assert.Equal(metadata, grpcResponse.Metadata);
    }

    [Fact]
    public void ToBulkStateItemTests()
    {
        TestConversion(
            key => new StateStoreGetResponse(),
            (source, response) => StateStoreGetResponse.ToBulkStateItem(source, response),
            result => result.Key,
            new[]
            {
                ("", ""),
                ("key", "key")
            });

        TestGrpcContentConversion(
            contentType => new StateStoreGetResponse { ContentType = contentType },
            response => StateStoreGetResponse.ToBulkStateItem("key", response),
            response => response.ContentType);

        TestGrpcDataConversion(
            data => new StateStoreGetResponse { Data = data },
            response => StateStoreGetResponse.ToBulkStateItem("key", response),
            response => response.Data);

        TestGrpcETagConversion(
            etag => new StateStoreGetResponse { ETag = etag },
            response => StateStoreGetResponse.ToBulkStateItem("key", response),
            response => response.Etag);

        TestGrpcMetadataConversion(
            metadata => new StateStoreGetResponse { Metadata = metadata },
            response => StateStoreGetResponse.ToBulkStateItem("key", response),
            response => response.Metadata);

        // TODO: Test other properties.
    }

    private static void TestGrpcContentConversion<TSource, TResult>(
        Func<string?, TSource> sourceFactory,
        Func<TSource, TResult> converter,
        Func<TResult, string> contentTypeAccessor)
    {
        TestConversion(
            sourceFactory,
            (_, source) => converter(source),
            contentTypeAccessor,
            new[]
            {
                (null, ""),
                ("", ""),
                ("application/json", "application/json")
            });
    }

    private static void TestGrpcDataConversion<TSource, TResult>(
        Func<byte[], TSource> sourceFactory,
        Func<TSource, TResult> converter,
        Func<TResult, IEnumerable<byte>> dataAccessor)
    {
        var empty = new byte[] { };
        var nonEmpty = new byte[] { 0x01, 0x02, 0x03 };

        TestConversion(
            sourceFactory,
            (_, source) => converter(source),
            dataAccessor,
            new (byte[], IEnumerable<byte>)[]
            {
                (empty, empty),
                (nonEmpty, nonEmpty)
            });
    }

    private static void TestGrpcETagConversion<TSource, TResult>(
        Func<string?, TSource> sourceFactory,
        Func<TSource, TResult> converter,
        Func<TResult, Dapr.Proto.Components.V1.Etag> contentTypeAccessor)
    {
        TestConversion(
            sourceFactory,
            (_, source) => converter(source),
            contentTypeAccessor,
            new[]
            {
                (null, null),
                ("", ""),
                ("value", "value")
            },
            (expected, actual) =>
            {
                if (expected == null)
                {
                    Assert.Null(actual);
                }
                else
                {
                    Assert.NotNull(actual);
                    Assert.Equal(expected, actual.Value);
                }
            });
    }

    private static void TestGrpcMetadataConversion<TSource, TResult>(
        Func<IReadOnlyDictionary<string, string>, TSource> sourceFactory,
        Func<TSource, TResult> converter,
        Func<TResult, Google.Protobuf.Collections.MapField<string, string>> metadataAccessor)
    {
        var empty = new Dictionary<string, string>();

        var nonEmpty =
            new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" }
            };

        TestConversion(
            sourceFactory,
            (_, source) => converter(source),
            metadataAccessor,
            new (IReadOnlyDictionary<string, string>, IReadOnlyDictionary<string, string>)[]
            {
                (empty, empty),
                (nonEmpty, nonEmpty)
            });
    }

    private static void TestConversion<TSourceProperty, TSource, TResult, TResultProperty>(
        Func<TSourceProperty, TSource> sourceFactory,
        Func<TSourceProperty, TSource, TResult> converter,
        Func<TResult, TResultProperty> resultPropertyAccessor,
        IEnumerable<(TSourceProperty Source, TResultProperty Expected)> tests)
    {
        TestConversion(
            sourceFactory,
            converter,
            resultPropertyAccessor,
            tests,
            (expected, actual) => Assert.Equal(expected, actual));
    }

    private static void TestConversion<TSourceProperty, TSource, TResult, TResultProperty, TExpected>(
        Func<TSourceProperty, TSource> sourceFactory,
        Func<TSourceProperty, TSource, TResult> converter,
        Func<TResult, TResultProperty> resultPropertyAccessor,
        IEnumerable<(TSourceProperty Source, TExpected Expected)> tests,
        Action<TExpected, TResultProperty> assertion)
    {
        foreach (var test in tests)
        {
            var source = sourceFactory(test.Source);

            var result = converter(test.Source, source);

            var resultProperty = resultPropertyAccessor(result);

            assertion(test.Expected, resultProperty);
        }
    }
}
