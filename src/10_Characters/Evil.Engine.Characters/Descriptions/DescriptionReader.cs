using System.Numerics;
using System.Text.Json;

namespace Evil.Engine.Characters;

internal sealed class DescriptionReader
{
    private const int Format = 1;

    private readonly string _source;

    public DescriptionReader(string source) => _source = source;

    public CharacterDescription Read(string json)
    {
        using JsonDocument document = Parse(json);
        JsonFields told = new JsonFields(document.RootElement, _source);
        if ((int)told.Number("format") != Format)
        {
            throw new InvalidDataException($"Character: {_source} is written in format {told.Number("format")}; this engine reads format {Format}.");
        }

        JsonFields outline = told.Inside("outline");
        return new CharacterDescription(
            told.Text("name"),
            told.Number("height"),
            told.Number("scale"),
            told.Text("skin"),
            new OutlineDescription(outline.Colour("colour"), outline.Number("width")),
            Piece(told.Inside("body")),
            Clothing(told),
            Outfits(told),
            Clips(told),
            Expressions(told),
            Pictures(told));
    }

    private JsonDocument Parse(string json)
    {
        try
        {
            return JsonDocument.Parse(json);
        }
        catch (JsonException error)
        {
            throw new InvalidDataException($"Character: {_source} is not readable ({error.Message}).", error);
        }
    }

    private static PieceDescription Piece(JsonFields told)
    {
        List<PartDescription> parts = new List<PartDescription>();
        foreach (JsonFields part in told.Rows("parts"))
        {
            parts.Add(new PartDescription(part.Text("name"), part.Flag("outline"), part.Word<ShadowRole>("shadow"), Surfaces(part)));
        }

        return new PieceDescription(told.Text("file"), parts);
    }

    private static IReadOnlyList<SurfaceDescription> Surfaces(JsonFields part)
    {
        List<SurfaceDescription> surfaces = new List<SurfaceDescription>();
        foreach (JsonFields surface in part.Rows("surfaces"))
        {
            RimDescription? rim = surface.Has("rim") ? new RimDescription(surface.Inside("rim").Colour("colour"), surface.Inside("rim").Number("facing")) : null;
            Vector3? inside = surface.Has("inside") ? surface.Colour("inside") : null;
            surfaces.Add(new SurfaceDescription(surface.Text("name"), surface.Colour("lit"), surface.Colour("shade"), surface.Number("threshold"), rim, inside, Paints(surface)));
        }

        return surfaces;
    }

    private static IReadOnlyList<PaintDescription> Paints(JsonFields surface)
    {
        List<PaintDescription> paints = new List<PaintDescription>();
        foreach (JsonFields paint in surface.Rows("paints"))
        {
            paints.Add(new PaintDescription(paint.Text("picture"), paint.Word<PaintWeight>("weight"), paint.Colour("shade_tint"), paint.Flag("lit_only")));
        }

        return paints;
    }

    private static Dictionary<string, ClothingDescription> Clothing(JsonFields told)
    {
        Dictionary<string, ClothingDescription> clothing = new Dictionary<string, ClothingDescription>(StringComparer.Ordinal);
        foreach (JsonEntry entry in told.Entries("clothing"))
        {
            clothing[entry.Name] = new ClothingDescription(entry.Fields.Text("slot"), Piece(entry.Fields), entry.Fields.Whole("hides"), entry.Fields.Amounts("shapes"));
        }

        return clothing;
    }

    private static Dictionary<string, IReadOnlyList<string>> Outfits(JsonFields told)
    {
        Dictionary<string, IReadOnlyList<string>> outfits = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        foreach (JsonEntry entry in told.Entries("outfits"))
        {
            outfits[entry.Name] = told.Inside("outfits").Texts(entry.Name);
        }

        return outfits;
    }

    private static Dictionary<string, ClipDescription> Clips(JsonFields told)
    {
        Dictionary<string, ClipDescription> clips = new Dictionary<string, ClipDescription>(StringComparer.Ordinal);
        foreach (JsonEntry entry in told.Entries("clips"))
        {
            List<FaceMoment> faces = new List<FaceMoment>();
            foreach (JsonFields face in entry.Fields.Rows("faces"))
            {
                faces.Add(new FaceMoment(face.Number("second"), face.Text("expression")));
            }

            clips[entry.Name] = new ClipDescription(entry.Fields.Number("seconds"), entry.Fields.Flag("loops"), faces);
        }

        return clips;
    }

    private static Dictionary<string, ExpressionDescription> Expressions(JsonFields told)
    {
        Dictionary<string, ExpressionDescription> expressions = new Dictionary<string, ExpressionDescription>(StringComparer.Ordinal);
        foreach (JsonEntry entry in told.Entries("expressions"))
        {
            expressions[entry.Name] = new ExpressionDescription(entry.Fields.Amounts("shapes"), entry.Fields.Names("pictures"));
        }

        return expressions;
    }

    private static Dictionary<string, PictureDescription> Pictures(JsonFields told)
    {
        Dictionary<string, PictureDescription> pictures = new Dictionary<string, PictureDescription>(StringComparer.Ordinal);
        foreach (JsonEntry entry in told.Entries("pictures"))
        {
            pictures[entry.Name] = new PictureDescription(entry.Fields.Text("file"), entry.Fields.Region("region"));
        }

        return pictures;
    }
}
