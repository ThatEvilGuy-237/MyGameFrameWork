import re
import subprocess
import sys

TARGET_TYPED_NEW = re.compile(r"(?<![\w.])new\s*\(")
NEW_TYPE = re.compile(r"\bnew\s+[A-Za-z_][\w.]*(?:<[^;{}()]*?>)?(?:\[\])?")
VAR = re.compile(r"(?<![\w.])var\s+[\w(]")
MEMBER_SET = re.compile(r"\s*(?:[A-Za-z_]\w*|\[[^\]]*\])\s*=(?!=)")


def skip_parens(text, index):
    depth = 0
    while index < len(text):
        char = text[index]
        if char == "(":
            depth += 1
        elif char == ")":
            depth -= 1
            if depth == 0:
                return index + 1
        elif char == '"':
            index = text.index('"', index + 1)
        index += 1
    return index


def initializers(text):
    for match in NEW_TYPE.finditer(text):
        index = match.end()
        while index < len(text) and text[index].isspace():
            index += 1
        if index < len(text) and text[index] == "(":
            index = skip_parens(text, index)
            while index < len(text) and text[index].isspace():
                index += 1
        if index < len(text) and text[index] == "{" and MEMBER_SET.match(text, index + 1):
            yield match.start()


files = subprocess.run(["git", "ls-files", "*.cs"], capture_output=True, text=True, check=True).stdout.split()
problems = []
for path in files:
    text = open(path, encoding="utf-8").read()
    lines = text.splitlines()

    def line_of(index):
        return text.count("\n", 0, index) + 1

    for match in TARGET_TYPED_NEW.finditer(text):
        if re.search(r"\bwhere\b.*:.*new\s*\(\s*\)", lines[line_of(match.start()) - 1]):
            continue
        problems.append(f"{path}:{line_of(match.start())}: write the type after new (no target-typed new())")
    for start in initializers(text):
        problems.append(f"{path}:{line_of(start)}: no initializer block; build with the constructor, then set properties as statements")
    for match in VAR.finditer(text):
        problems.append(f"{path}:{line_of(match.start())}: write the type instead of var")

for problem in problems:
    print(problem)
print(f"{len(problems)} style problems")
sys.exit(1 if problems else 0)
