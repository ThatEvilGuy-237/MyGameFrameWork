import re
import sys
import textwrap

DOC = "documents/Getting Started/Make a Game.md"
TEMPLATE = "templates/Evil.Engine.Templates/evil-game"
SHOWN = [("csharp", "Main.cs"), ("csharp", "MapScene.cs"), ("xml", "MyGame.csproj")]


def blocks(text, language):
    pattern = r"^[ \t]*```" + language + r"\n(.*?)^[ \t]*```"
    return [textwrap.dedent(block) for block in re.findall(pattern, text, re.S | re.M)]


def main():
    text = open(DOC, encoding="utf-8").read()
    shown = {language: blocks(text, language) for language in ("csharp", "xml")}
    drifted = 0
    for language, name in SHOWN:
        template = open(f"{TEMPLATE}/{name}", encoding="utf-8").read()
        if template not in shown[language]:
            drifted += 1
            print(f"Make a Game no longer shows the template's {name} as it is; copy {TEMPLATE}/{name} into the note")
    print(f"Make a Game shows {len(SHOWN) - drifted} of {len(SHOWN)} template files as they are")
    return 1 if drifted else 0


if __name__ == "__main__":
    sys.exit(main())
