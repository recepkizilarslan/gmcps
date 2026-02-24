using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Gmcps.Tools.Core;

internal static class GenericOutputMapper
{
    public static Result<TOutput> Map<TOutput>(object? raw)
    {
        try
        {
            var mapped = MapToType(raw, typeof(TOutput));
            if (mapped is null)
            {
                return Result<TOutput>.Failure("Output mapping produced null value.");
            }

            return Result<TOutput>.Success((TOutput)mapped);
        }
        catch (Exception ex)
        {
            return Result<TOutput>.Failure($"Output mapping failed: {ex.Message}");
        }
    }

    private static object? MapToType(object? source, Type targetType)
    {
        var nonNullableTarget = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (source is null)
        {
            return targetType.IsValueType && Nullable.GetUnderlyingType(targetType) is null
                ? Activator.CreateInstance(targetType)
                : null;
        }

        var sourceType = source.GetType();
        if (nonNullableTarget.IsAssignableFrom(sourceType))
        {
            return source;
        }

        if (nonNullableTarget == typeof(string))
        {
            return source switch
            {
                DateTime dt => dt == DateTime.MinValue ? string.Empty : dt.ToString("O", CultureInfo.InvariantCulture),
                Enum e => e.ToString(),
                _ => source.ToString() ?? string.Empty
            };
        }

        if (source is string sourceString && nonNullableTarget.IsEnum)
        {
            return Enum.Parse(nonNullableTarget, sourceString, ignoreCase: true);
        }

        if (nonNullableTarget.IsEnum)
        {
            return Enum.ToObject(nonNullableTarget, source);
        }

        if (IsSimpleType(nonNullableTarget) && source is IConvertible)
        {
            return Convert.ChangeType(source, nonNullableTarget, CultureInfo.InvariantCulture);
        }

        if (TryMapCollection(source, nonNullableTarget, out var mappedCollection))
        {
            return mappedCollection;
        }

        return MapObject(source, nonNullableTarget);
    }

    private static bool TryMapCollection(object source, Type targetType, out object? mapped)
    {
        mapped = null;

        if (targetType == typeof(string) || source is not IEnumerable enumerable)
        {
            return false;
        }

        var elementType = GetCollectionElementType(targetType);
        if (elementType is null)
        {
            return false;
        }

        var mappedListType = typeof(List<>).MakeGenericType(elementType);
        var mappedList = (IList)Activator.CreateInstance(mappedListType)!;

        foreach (var item in enumerable)
        {
            mappedList.Add(MapToType(item, elementType));
        }

        if (targetType.IsArray)
        {
            var array = Array.CreateInstance(elementType, mappedList.Count);
            mappedList.CopyTo(array, 0);
            mapped = array;
            return true;
        }

        if (targetType.IsAssignableFrom(mappedListType))
        {
            mapped = mappedList;
            return true;
        }

        if (targetType.IsInterface)
        {
            mapped = mappedList;
            return true;
        }

        var ctor = targetType.GetConstructor(new[] { mappedListType })
                   ?? targetType.GetConstructor(new[] { typeof(IEnumerable<>).MakeGenericType(elementType) });
        if (ctor is not null)
        {
            mapped = ctor.Invoke(new object[] { mappedList });
            return true;
        }

        return false;
    }

    private static object MapObject(object source, Type targetType)
    {
        var sourceProps = source.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        foreach (var ctor in targetType.GetConstructors().OrderByDescending(c => c.GetParameters().Length))
        {
            var parameters = ctor.GetParameters();
            var args = new object?[parameters.Length];
            var canConstruct = true;

            if (parameters.Length == 1)
            {
                args[0] = MapToType(source, parameters[0].ParameterType);
                return ctor.Invoke(args);
            }

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                if (TryGetSourceValue(source, sourceProps, parameter, i, out var value))
                {
                    args[i] = MapToType(value, parameter.ParameterType);
                    continue;
                }

                if (parameter.HasDefaultValue)
                {
                    args[i] = parameter.DefaultValue;
                    continue;
                }

                if (targetType.Name == "ListCriticalPackagesOutput" &&
                    string.Equals(parameter.Name, "support", StringComparison.OrdinalIgnoreCase))
                {
                    args[i] = "bestEffort";
                    continue;
                }

                canConstruct = false;
                break;
            }

            if (canConstruct)
            {
                return ctor.Invoke(args);
            }
        }

        throw new InvalidOperationException($"Cannot map {source.GetType().Name} to {targetType.Name}");
    }

    private static bool TryGetSourceValue(
        object source,
        IReadOnlyDictionary<string, PropertyInfo> sourceProps,
        ParameterInfo parameter,
        int parameterIndex,
        out object? value)
    {
        value = null;

        if (sourceProps.TryGetValue(parameter.Name ?? string.Empty, out var directMatch))
        {
            value = directMatch.GetValue(source);
            return true;
        }

        if (!string.IsNullOrWhiteSpace(parameter.Name) &&
            parameter.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) &&
            sourceProps.TryGetValue("Id", out var idMatch))
        {
            value = idMatch.GetValue(source);
            return true;
        }

        if (source is ITuple tuple && parameterIndex < tuple.Length)
        {
            value = tuple[parameterIndex];
            return true;
        }

        return false;
    }

    private static Type? GetCollectionElementType(Type collectionType)
    {
        if (collectionType.IsArray)
        {
            return collectionType.GetElementType();
        }

        if (collectionType.IsGenericType)
        {
            var genericDef = collectionType.GetGenericTypeDefinition();
            if (genericDef == typeof(IEnumerable<>) ||
                genericDef == typeof(IReadOnlyList<>) ||
                genericDef == typeof(IList<>) ||
                genericDef == typeof(List<>))
            {
                return collectionType.GetGenericArguments()[0];
            }
        }

        var enumerableInterface = collectionType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        return enumerableInterface?.GetGenericArguments()[0];
    }

    private static bool IsSimpleType(Type type) =>
        type.IsPrimitive ||
        type == typeof(decimal) ||
        type == typeof(DateTime) ||
        type == typeof(Guid);
}
