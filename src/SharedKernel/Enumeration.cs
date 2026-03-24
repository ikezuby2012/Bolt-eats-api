using System.Reflection;

namespace SharedKernel;

#pragma warning disable S4035
public abstract class Enumeration<TEnum> : IEquatable<Enumeration<TEnum>> where TEnum : Enumeration<TEnum>
{
    public int Id { get; protected init; }
    public string Name { get; protected init; }
    private static readonly Dictionary<int, TEnum> Enumerations = CreateEnumerations();

    protected Enumeration(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public static TEnum? FromValue(int Id)
    {
        return Enumerations.TryGetValue(Id, out TEnum? enumeration)
            ? enumeration : default;
    }

    public static TEnum? FromName(string Name)
    {
        return Enumerations.Values.SingleOrDefault(o => o.Name == Name);
    }

    public static List<TEnum> GetValues()
    {
        return Enumerations.Values.ToList();
    }

    public bool Equals(Enumeration<TEnum>? other)
    {
        if (other is null)
        { return false; }

        return GetType() == other.GetType() && Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is Enumeration<TEnum> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
    public override string ToString()
    {
        return Name;
    }

    private static Dictionary<int, TEnum> CreateEnumerations()
    {
        Type enumerationType = typeof(TEnum);

        IEnumerable<TEnum?>? fieldsType = enumerationType.GetFields(
            BindingFlags.Public |
            BindingFlags.Static |
            BindingFlags.FlattenHierarchy)
            .Where(fieldInfo => enumerationType.IsAssignableFrom(fieldInfo.FieldType))
            .Select(FieldInfo => (TEnum)FieldInfo.GetValue(default));

        return fieldsType.Where(x => x != null).ToDictionary(x => x!.Id)!;
    }
}

