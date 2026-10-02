namespace Toolbox.Extensions;

public static class DictionaryExtensions
{
    public static bool DictionaryEquals<TKey, TValue>(this IDictionary<TKey, TValue> left, IDictionary<TKey, TValue> right)
    {
        if (left.Count != right.Count) return false;

        foreach (var kvp in left)
        {
            if (!right.TryGetValue(kvp.Key, out var rightValue)) return false;
            if (!EqualityComparer<TValue>.Default.Equals(kvp.Value, rightValue)) return false;
        }

        return true;
    }
}
