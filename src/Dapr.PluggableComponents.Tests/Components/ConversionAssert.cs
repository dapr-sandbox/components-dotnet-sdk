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

namespace Dapr.PluggableComponents.Components;

internal static class ConversionAssert
{
    public static void ContentTypeEqual<TSource, TResult>(
        Func<string?, TSource> sourceFactory,
        Func<TSource, TResult> converter,
        Func<TResult, string> contentTypeAccessor)
    {
        Equal(
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

    public static void DataEqual<TSource, TResult>(
        Func<byte[], TSource> sourceFactory,
        Func<TSource, TResult> converter,
        Func<TResult, IEnumerable<byte>> dataAccessor)
    {
        var empty = new byte[] { };
        var nonEmpty = new byte[] { 0x01, 0x02, 0x03 };

        Equal(
            sourceFactory,
            (_, source) => converter(source),
            dataAccessor,
            new (byte[], IEnumerable<byte>)[]
            {
                (empty, empty),
                (nonEmpty, nonEmpty)
            });
    }

    public static void ETagEqual<TSource, TResult>(
        Func<string?, TSource> sourceFactory,
        Func<TSource, TResult> converter,
        Func<TResult, Dapr.Proto.Components.V1.Etag> contentTypeAccessor)
    {
        Equal(
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

    public static void MetadataEqual<TSource, TResult>(
        Func<IReadOnlyDictionary<string, string>, TSource> sourceFactory,
        Func<TSource, TResult> converter,
        Func<TResult, IEnumerable<KeyValuePair<string, string>>> metadataAccessor)
    {
        var empty = new Dictionary<string, string>();

        var nonEmpty =
            new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" }
            };

        Equal(
            sourceFactory,
            (_, source) => converter(source),
            metadataAccessor,
            new (IReadOnlyDictionary<string, string>, IEnumerable<KeyValuePair<string, string>>)[]
            {
                (empty, empty),
                (nonEmpty, nonEmpty)
            });
    }

    public static void StringEqual<TSource, TResult>(
        Func<string, TSource> sourceFactory,
        Func<TSource, TResult> converter,
        Func<TResult, string> contentTypeAccessor)
    {
        Equal(
            sourceFactory,
            (_, source) => converter(source),
            contentTypeAccessor,
            new[]
            {
                ("", ""),
                ("value", "value")
            });
    }

    public static void NullableStringEqual<TSource, TResult>(
        Func<string?, TSource> sourceFactory,
        Func<TSource, TResult> converter,
        Func<TResult, string> contentTypeAccessor)
    {
        Equal(
            sourceFactory,
            (_, source) => converter(source),
            contentTypeAccessor,
            new[]
            {
                (null, ""),
                ("", ""),
                ("value", "value")
            });
    }

    public static void Equal<TSourceProperty, TSource, TResult, TResultProperty>(
        Func<TSourceProperty, TSource> sourceFactory,
        Func<TSourceProperty, TSource, TResult> converter,
        Func<TResult, TResultProperty> resultPropertyAccessor,
        IEnumerable<(TSourceProperty Source, TResultProperty Expected)> tests)
    {
        Equal(
            sourceFactory,
            converter,
            resultPropertyAccessor,
            tests,
            (expected, actual) => Assert.Equal(expected, actual));
    }

    public static void Equal<TSourceProperty, TSource, TResult, TResultProperty, TExpected>(
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
