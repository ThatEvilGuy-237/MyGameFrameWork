import collections
import json
import os
import re
import subprocess
import sys

BASELINE = "scripts/clean-code-baseline.json"
ALLOWED = {
    ("Check.cs", "delegate"): "Check.Throws takes the call it expects to fail: the one delegate the tests need, always given a named method",
}
EXACT_WINDOW = 5
SHAPE_WINDOW = 8
LONGEST_METHOD = 30
METHOD_START = re.compile(r"^\s+(?:(?:public|private|protected|internal|static|override|virtual|abstract|sealed|async|new|partial|unsafe|extern)\s+)+[\w<>\[\],.?() ]+?\s+\w+(?:<[^>]*>)?\(")
TABLE_SHARE = 6

RULES = {
    "lambda": "logic in an anonymous function: give it a name (a method or a class)",
    "linq": "a LINQ call (Select, Where, Any, ...): write the loop or a named method",
    "tuple": "a tuple used as data: give it a named type (record or class)",
    "value switch": "a switch on strings or numbers: use types (polymorphism) or an enum",
    "delegate": "a Func or Action passed around: use an interface or a class",
    "types per file": "more than one type in a file: one type per file, named after it",
    "duplicate": "the same code in two places: give it one home",
    "long method": "a method over 30 lines: cut it into named steps",
}

LINQ = re.compile(r"(?<!\bMath)(?<!\bMathF)(?<!\bVector2)(?<!\bVector3)(?<!\bVector4)\.(Select|SelectMany|Concat|ToList|Where|Any|All|First|FirstOrDefault|Last|LastOrDefault|Single|OrderBy|OrderByDescending|ThenBy|Sum|Min|Max|Count|Aggregate|Cast|OfType|Zip|Skip|Take|Distinct|GroupBy|ToDictionary|ToLookup)\(")
TUPLE_TYPE = re.compile(r"(?<![\w>\]])\(\s*[A-Z_a-z][\w.<>?\[\]]*\s+[A-Z]\w*\s*,\s*[A-Z_a-z][\w.<>?\[\]]*\s+[A-Z]\w*")
TUPLE_LITERAL = re.compile(r"(?<![\w>\]])\(\s*[A-Z]\w*\s*:\s*[^,()]+(\([^()]*\))?[^,()]*,\s*[A-Z]\w*\s*:")
VALUE_CASE = re.compile(r"\bcase\s+(\"|-?\d)|^\s*(\"\"|-?\d[\w.]*)\s*(when\b.*)?=>")
DELEGATE = re.compile(r"\b(Func|Action)\s*<|\bAction\s+\w+\s*[,)=;]|\bAction\[\]")
TYPE_DECLARATION = re.compile(r"^\s*(?:(?:public|internal|private|protected|file|sealed|abstract|static|readonly|partial|unsafe|ref)\s+)*(?:class|record|struct|enum|interface)\b(?:\s+(?:class|struct))?\s+\w+", re.M)
LAMBDA_START = re.compile(r"(?:\(\s*[^()]*\)|\b[A-Za-z_]\w*)\s*=>")
BEFORE_LAMBDA = re.compile(r"(?:[(,=:]|\s\?|\breturn|&&|\|\|)\s*$")
TRIVIAL_ACCESSOR = re.compile(r"^((get|set|init)\s*(=>[^;{}]*)?;|: (this|base)\(.*\)( => .*;)?)$")
IGNORED_LINE = re.compile(r"^(\{|\}|\);?|\}\);?|\]|\[|\]\)?;?|using .*;|global using .*;|namespace .*;|break;|return;|else|default:|\[Test\])$")
KEYWORDS = set(
    "public private protected internal static sealed readonly override virtual abstract class record struct enum interface "
    "new return if else for foreach while switch case default break continue true false null this base void float int bool "
    "string double in is not and or out ref using get set init throw typeof nameof".split())


def project_of(path):
    folder = os.path.dirname(path)
    while folder:
        if any(name.endswith(".csproj") for name in os.listdir(folder)):
            return folder
        folder = os.path.dirname(folder)
    return "."


def key_of(path):
    return f"{project_of(path)}::{os.path.basename(path)}"


def tracked_sources():
    names = subprocess.run(["git", "ls-files", "-z", "--cached", "--others", "--exclude-standard", "*.cs"], capture_output=True, text=True, check=True).stdout.split("\0")
    return [name for name in names if name and os.path.isfile(name)]


def strip_strings_and_comments(text):
    text = re.sub(r'@"(?:[^"]|"")*"', '""', text)
    text = re.sub(r'\$?"(?:[^"\\\n]|\\.)*"', '""', text)
    return re.sub(r"//[^\n]*", "", text)


def count_lambdas(line):
    found = 0
    for match in LAMBDA_START.finditer(line):
        before = line[:match.start()]
        if BEFORE_LAMBDA.search(before) and not re.search(r"\bcase\s*$", before):
            found += 1
    return found


def code_rules(text):
    clean = strip_strings_and_comments(text)
    counts = collections.Counter()
    for line in clean.split("\n"):
        counts["lambda"] += count_lambdas(line)
        counts["linq"] += len(LINQ.findall(line))
        counts["tuple"] += len(TUPLE_TYPE.findall(line)) + len(TUPLE_LITERAL.findall(line))
        counts["value switch"] += len(VALUE_CASE.findall(line))
        counts["delegate"] += len(DELEGATE.findall(line))
    counts["long method"] = len(long_methods(clean))
    types = len(TYPE_DECLARATION.findall(clean))
    counts["types per file"] = max(0, types - 1)
    return counts


def long_methods(clean):
    lines = clean.split("\n")
    found = []
    for start, line in enumerate(lines):
        if not METHOD_START.match(line) or re.search(r"\b(class|record|struct|interface|enum)\b", line):
            continue
        open_at = start
        while open_at < len(lines) and "{" not in lines[open_at] and "=>" not in lines[open_at] and not lines[open_at].rstrip().endswith(";"):
            open_at += 1
        if open_at >= len(lines) or "{" not in lines[open_at] or "=>" in lines[open_at].split("{")[0]:
            continue
        depth = 0
        for end in range(open_at, len(lines)):
            depth += lines[end].count("{") - lines[end].count("}")
            if depth == 0:
                if end - start + 1 > LONGEST_METHOD:
                    found.append(start + 1)
                break
    return found


def outside_enums(raw_lines):
    inside = False
    depth = 0
    for number, line in enumerate(raw_lines, start=1):
        if not inside and re.search(r"\benum\s+\w+", line):
            inside = True
            depth = 0
        if inside:
            depth += line.count("{") - line.count("}")
            if "}" in line and depth <= 0:
                inside = False
            continue
        yield number, line


def normalize_shape(line):
    line = re.sub(r'"(?:[^"\\]|\\.)*"', "S", line)
    line = re.sub(r"\b\d+(\.\d+)?f?\b", "N", line)
    return re.sub(r"\b[A-Za-z_]\w*\b", lambda match: match.group(0) if match.group(0) in KEYWORDS else "I", line)


def meaningful_lines(text):
    lines = []
    for number, raw in outside_enums(text.split("\n")):
        line = re.sub(r"\s+", " ", raw.strip())
        if line and not IGNORED_LINE.match(line):
            lines.append((number, line))
    return lines


def duplicates(sources):
    windows = collections.defaultdict(list)
    numbers = {}
    for path, text in sources.items():
        lines = meaningful_lines(text)
        numbers[path] = [number for number, _ in lines]
        for size, shaped in ((EXACT_WINDOW, False), (SHAPE_WINDOW, True)):
            for start in range(len(lines) - size + 1):
                window = lines[start:start + size]
                if shaped and any(TRIVIAL_ACCESSOR.match(line) for _, line in window):
                    continue
                chunk = tuple(normalize_shape(line) if shaped else line for _, line in window)
                if shaped and collections.Counter(chunk).most_common(1)[0][1] >= TABLE_SHARE:
                    continue
                windows[(size, shaped, chunk)].append((path, start, size))
    hits = collections.Counter()
    places = []
    covered = set()
    repeated = [spots for spots in windows.values() if len(spots) > 1]
    for spots in sorted(repeated, key=lambda group: (-group[0][2], group)):
        fresh = [spot for spot in spots if (spot[0], spot[1]) not in covered]
        for path, start, size in spots:
            covered.update((path, start + offset) for offset in range(size))
        if len(fresh) < 2:
            continue
        for path, _, _ in fresh:
            hits[path] += 1
        places.append([(path, numbers[path][start]) for path, start, _ in fresh])
    return hits, places


def measure():
    sources = {path: open(path, encoding="utf-8").read() for path in tracked_sources()}
    report = {}
    for path, text in sources.items():
        rules = code_rules(text)
        for (name, rule), _ in ALLOWED.items():
            if os.path.basename(path) == name:
                rules[rule] = 0
        report[key_of(path)] = dict(rules)
    hits, places = duplicates(sources)
    for path, count in hits.items():
        report[key_of(path)]["duplicate"] = count
    return {key: {rule: count for rule, count in rules.items() if count} for key, rules in report.items()}, places


def totals(report):
    summed = collections.Counter()
    for rules in report.values():
        summed.update(rules)
    return summed


def self_test():
    cases = [
        ("list.Where(item => item.Alive)", "lambda", 1),
        ("Run(() => Fire())", "lambda", 1),
        ("Sort((a, b) => a - b)", "lambda", 1),
        ("public float Area => Width * Height;", "lambda", 0),
        ("public void Exit() => IsExiting = true;", "lambda", 0),
        ("    Easing.InQuad => t * t,", "lambda", 0),
        ("    _ => t,", "lambda", 0),
        ("get => _value;", "lambda", 0),
        ("internal WorldObject? Root => _world;", "lambda", 0),
        ("public Scene? Current => _scenes.Count > 0 ? _scenes[^1] : null;", "lambda", 0),
        ("Shape pick = useAll ? (a => a) : Pick;", "lambda", 1),
        ("foreach (object key in _queues.Keys.ToList())", "linq", 1),
        ("Action<int> done = value => Print(value);", "lambda", 1),
        ("Action<int> done = value => Print(value);", "delegate", 1),
        ("List<int> alive = units.Where(IsAlive).ToList();", "linq", 2),
        ("(int Kind, Vector2 Value)[] steps;", "tuple", 1),
        ("random => (Easing: Pick(random), T: Next(random))", "tuple", 1),
        ("Vector3 middle = (a + b) / 2f;", "tuple", 0),
        ("Call(Name: value)", "tuple", 0),
        ("new OrbitLimits(MinDistance: 6f, MaxDistance: 30f)", "tuple", 0),
        ("Show((Name: a, Size: b))", "tuple", 1),
        ("public readonly record struct Color(float R, float G, float B, float A = 1f)", "tuple", 0),
        ("List<(string Name, int Count)> rows = Load();", "tuple", 1),
        ("float top = MathF.Max(a, b) + Vector3.Min(c, d).X + Math.Min(e, f);", "linq", 0),
        ("float lowest = points.Min(point => point.X);", "linq", 1),
        ("Capsule enemy = _pawns.First(IsEnemy);", "linq", 1),
        ("case \"Move\":", "value switch", 1),
        ("case 0:", "value switch", 1),
        ("case Key.Escape:", "value switch", 0),
        ("    \".wav\" => AudioStreamWav.LoadFromFile(full),", "value switch", 1),
        ("    0 => new MoveStep(point),", "value switch", 1),
        ("    Key.Control => \"Ctrl\",", "value switch", 0),
        ("internal sealed class A { }\ninternal sealed record B(int X);", "types per file", 1),
        ("public enum Key\n{\n    A,\n}", "types per file", 0),
        ("Check.Throws<ArgumentException>(() => new Ray3(o, d), \"=> no\");", "lambda", 1),
    ]
    body = "".join(f"        Step{n}();\n" for n in range(26))
    cases.append(("class L\n{\n    public void Long()\n    {\n" + body + "        Last();\n        More();\n    }\n}\n", "long method", 1))
    cases.append(("class E\n{\n    public void Exactly()\n    {\n" + body + "        Last();\n    }\n}\n", "long method", 0))
    cases.append(("class S\n{\n    public void Short()\n    {\n" + body + "    }\n}\n", "long method", 0))
    cases.append(("class P\n{\n    public bool Busy\n    {\n        get\n        {\n" + body + "            return true;\n        }\n    }\n}\n", "long method", 0))
    failed = 0
    for code, rule, expected in cases:
        got = code_rules(code)[rule]
        if got != expected:
            failed += 1
            print(f"self-test: {rule} counted {got}, expected {expected}: {code!r}")
    planted = {
        "a.cs": "class A\n{\n    void One()\n    {\n        int alpha = 1;\n        int beta = alpha + 2;\n        int gamma = beta * 3;\n        Print(gamma);\n        Save(gamma, beta);\n    }\n}\n",
        "b.cs": "class B\n{\n    void Two()\n    {\n        int alpha = 1;\n        int beta = alpha + 2;\n        int gamma = beta * 3;\n        Print(gamma);\n        Save(gamma, beta);\n    }\n}\n",
        "c.cs": "class C\n{\n    void Three()\n    {\n        int x = 5;\n        int y = x + 7;\n        int z = y * 9;\n        Show(z);\n        Keep(z, y);\n        Done(z);\n        Last(y);\n        End(x);\n    }\n}\n",
        "d.cs": "class D\n{\n    void Four()\n    {\n        int p = 5;\n        int q = p + 7;\n        int r = q * 9;\n        Show(r);\n        Keep(r, q);\n        Done(r);\n        Last(q);\n        End(p);\n    }\n}\n",
        "g.cs": "class G\n{\n    public float Radius\n    {\n        get => _mesh.Radius;\n        set => _mesh.Radius = value;\n    }\n\n    public float Height\n    {\n        get => _mesh.Height;\n        set => _mesh.Height = value;\n    }\n}\n",
        "h.cs": "class H\n{\n    public float Inner\n    {\n        get => _ring.Inner;\n        set => _ring.Inner = value;\n    }\n\n    public float Outer\n    {\n        get => _ring.Outer;\n        set => _ring.Outer = value;\n    }\n}\n",
        "t.cs": "class T\n{\n    void Keys()\n    {\n" + "".join(f"        CheckMapped(GodotKey.K{n}, Key.K{n});\n" for n in range(9)) + "    }\n}\n",
        "u.cs": "class U\n{\n    void Codes()\n    {\n" + "".join(f"        Expect(Code.C{n}, Name.N{n});\n" for n in range(9)) + "    }\n}\n",
        "s.cs": "public sealed class S : Shape\n{\n    public S()\n        : this(0.5f, 2f, Color.White)\n    {\n    }\n\n    public S(float radius, float height, Color color)\n        : base(Checked(radius, height), color)\n    {\n    }\n}\n",
        "r.cs": "public sealed class R : Shape\n{\n    public R()\n        : this(0.4f, 0.6f, Color.White)\n    {\n    }\n\n    public R(float inner, float outer, Color color)\n        : base(Checked(inner, outer), color)\n    {\n    }\n}\n",
        "e.cs": "enum E\n{\n    A,\n    B,\n    C,\n    D,\n    F,\n    G,\n    H,\n    I,\n}\n",
        "f.cs": "enum F\n{\n    A,\n    B,\n    C,\n    D,\n    F,\n    G,\n    H,\n    I,\n}\n",
    }
    hits, _ = duplicates(planted)
    for path, expected in (("a.cs", 1), ("b.cs", 1), ("c.cs", 1), ("d.cs", 1), ("e.cs", 0), ("f.cs", 0), ("g.cs", 0), ("h.cs", 0), ("t.cs", 0), ("u.cs", 0), ("s.cs", 0), ("r.cs", 0)):
        if hits[path] != expected:
            failed += 1
            print(f"self-test: duplicate in {path} counted {hits[path]}, expected {expected}")
    moved = "src/03_World/Evil.Engine.World/Models/Model.cs"
    if key_of(moved) != "src/03_World/Evil.Engine.World::Model.cs":
        failed += 1
        print(f"self-test: a file moved inside its project must keep its key, got {key_of(moved)}")
    return failed


def main():
    if self_test():
        print("the clean-code check itself is wrong; fix scripts/clean-code-check.py first")
        return 2

    report, places = measure()
    if "--write-baseline" in sys.argv:
        with open(BASELINE, "w", encoding="utf-8") as handle:
            json.dump(dict(sorted(report.items())), handle, indent=1, sort_keys=True)
            handle.write("\n")
        print(f"baseline written: {sum(totals(report).values())} findings")
        return 0

    try:
        with open(BASELINE, encoding="utf-8") as handle:
            baseline = json.load(handle)
    except FileNotFoundError:
        baseline = {}

    worse = []
    better = 0
    for path, rules in sorted(report.items()):
        allowed = baseline.get(path, {})
        for rule, count in rules.items():
            if count > allowed.get(rule, 0):
                worse.append(f"{path}: {count} x {rule} (baseline {allowed.get(rule, 0)}): {RULES[rule]}")
    for path, rules in baseline.items():
        for rule, count in rules.items():
            better += max(0, count - report.get(path, {}).get(rule, 0))

    if "--show" in sys.argv:
        for (name, rule), reason in ALLOWED.items():
            print(f"allowed: {rule} in {name}: {reason}")
        for spots in places:
            print("duplicate: " + ", ".join(f"{path}:{line}" for path, line in spots))
        for path, rules in sorted(report.items()):
            print(f"{path}: " + ", ".join(f"{rule} {count}" for rule, count in sorted(rules.items())))

    left = totals(report)
    summary = ", ".join(f"{rule} {left[rule]}" for rule in RULES if left[rule])
    print(f"clean code: {sum(left.values())} left ({summary or 'none'}); {better} fixed since the baseline")
    for line in worse:
        print(f"worse: {line}")
    return 1 if worse else 0


if __name__ == "__main__":
    sys.exit(main())
