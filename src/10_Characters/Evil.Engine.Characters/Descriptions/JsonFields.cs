using System.Numerics;
using System.Text.Json;

namespace Evil.Engine.Characters;

internal sealed class JsonFields
{
    private readonly JsonElement _element;
    private readonly string _where;

    public JsonFields(JsonElement element, string where)
    {
        _element = element;
        _where = where;
    }

    public bool Has(string name) => _element.TryGetProperty(name, out JsonElement _);

    public string Text(string name) => Field(name).GetString() ?? throw Wrong(name, "a text");

    public float Number(string name) => Field(name).GetSingle();

    public bool Flag(string name) => Field(name).GetBoolean();

    public TWord Word<TWord>(string name)
        where TWord : struct, Enum =>
        Enum.TryParse(Text(name), true, out TWord word) ? word : throw Wrong(name, $"one of {string.Join(", ", Enum.GetNames<TWord>()).ToLowerInvariant()}");

    public Vector3 Colour(string name)
    {
        IReadOnlyList<float> parts = Numbers(name, 3);
        return new Vector3(parts[0], parts[1], parts[2]);
    }

    public Vector4 Region(string name)
    {
        IReadOnlyList<float> parts = Numbers(name, 4);
        return new Vector4(parts[0], parts[1], parts[2], parts[3]);
    }

    public IReadOnlyList<int> Whole(string name)
    {
        List<int> found = new List<int>();
        foreach (JsonElement item in Field(name).EnumerateArray())
        {
            found.Add(item.GetInt32());
        }

        return found;
    }

    public IReadOnlyList<string> Texts(string name)
    {
        List<string> found = new List<string>();
        foreach (JsonElement item in Field(name).EnumerateArray())
        {
            found.Add(item.GetString() ?? throw Wrong(name, "a list of texts"));
        }

        return found;
    }

    public JsonFields Inside(string name) => new JsonFields(Field(name), $"{_where} > {name}");

    public IReadOnlyList<JsonFields> Rows(string name)
    {
        List<JsonFields> found = new List<JsonFields>();
        foreach (JsonElement item in Field(name).EnumerateArray())
        {
            found.Add(new JsonFields(item, $"{_where} > {name}[{found.Count}]"));
        }

        return found;
    }

    public IReadOnlyList<JsonEntry> Entries(string name)
    {
        List<JsonEntry> found = new List<JsonEntry>();
        foreach (JsonProperty property in Field(name).EnumerateObject())
        {
            found.Add(new JsonEntry(property.Name, new JsonFields(property.Value, $"{_where} > {name} > {property.Name}")));
        }

        return found;
    }

    public IReadOnlyDictionary<string, float> Amounts(string name)
    {
        Dictionary<string, float> found = new Dictionary<string, float>(StringComparer.Ordinal);
        foreach (JsonProperty property in Field(name).EnumerateObject())
        {
            found[property.Name] = property.Value.GetSingle();
        }

        return found;
    }

    public IReadOnlyDictionary<string, string> Names(string name)
    {
        Dictionary<string, string> found = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (JsonProperty property in Field(name).EnumerateObject())
        {
            found[property.Name] = property.Value.GetString() ?? throw Wrong(name, "names");
        }

        return found;
    }

    private IReadOnlyList<float> Numbers(string name, int count)
    {
        List<float> found = new List<float>();
        foreach (JsonElement item in Field(name).EnumerateArray())
        {
            found.Add(item.GetSingle());
        }

        return found.Count == count ? found : throw Wrong(name, $"{count} numbers");
    }

    private JsonElement Field(string name) =>
        _element.TryGetProperty(name, out JsonElement found) ? found : throw new InvalidDataException($"Character: {_where} has no '{name}'.");

    private InvalidDataException Wrong(string name, string wanted) =>
        new InvalidDataException($"Character: '{name}' of {_where} is not {wanted}.");
}
