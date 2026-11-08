import re
import sys
import textwrap

doc, out, feed = sys.argv[1:4]
text = open(doc, encoding="utf-8").read()


def blocks(lang):
    pattern = r"^[ \t]*```" + lang + r"\n(.*?)^[ \t]*```"
    return [textwrap.dedent(block) for block in re.findall(pattern, text, re.S | re.M)]


ini, xml, cs = blocks("ini"), blocks("xml"), blocks("csharp")
if (len(ini), len(xml), len(cs)) != (2, 2, 1):
    sys.exit(f"Make a Game should have 2 ini, 2 xml and 1 csharp block, found {len(ini)}, {len(xml)}, {len(cs)}")

documented_feed = "/mnt/hdd/projects/MyGameFrameWork-data/feed"
if xml[1].count(documented_feed) != 1:
    sys.exit("the nuget.config in Make a Game no longer names the feed folder")

usings = re.search(r"Put the usings in one `GlobalUsings.cs`: (.*?)\.\n\n", text, re.S)
if usings is None:
    sys.exit("Make a Game no longer lists the usings")
names = re.findall(r"`([A-Za-z.]+)`", usings.group(1))

files = {
    "project.godot": ini[0],
    "main.tscn": ini[1],
    "MyGame.csproj": xml[0],
    "nuget.config": xml[1].replace(documented_feed, feed),
    "Main.cs": cs[0],
    "GlobalUsings.cs": "".join(f"global using {name};\n" for name in names),
}
for name, content in files.items():
    with open(f"{out}/{name}", "w", encoding="utf-8") as handle:
        handle.write(content)
print(f"made {out} from the note: {len(files)} files, {len(names)} usings")
