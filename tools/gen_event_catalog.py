#!/usr/bin/env python3
"""Generiert unity/Assets/Scripts/Data/EventCatalog.cs aus lib/models/event_model.dart.

Parst die `kAllEvents`-Const-Liste (GameEvent/EventChoice/EventEffect/
EventRequirements/Enums) und emittiert C#-Objekt-Initializer. Einmalig genutzt
für den Unity-Port; bei Änderungen am Dart-Katalog erneut ausführen.
"""
import re
import sys
import pathlib

ROOT = pathlib.Path(__file__).resolve().parent.parent
SRC = ROOT / "lib" / "models" / "event_model.dart"
OUT = ROOT / "unity" / "Assets" / "Scripts" / "Data" / "EventCatalog.cs"

text = SRC.read_text(encoding="utf-8")

# ── Liste-Body zwischen 'kAllEvents = [' und der zugehörigen schließenden '];'
start = text.index("kAllEvents")
lb = text.index("[", start)
# passende schließende Klammer finden
depth = 0
i = lb
while i < len(text):
    ch = text[i]
    if ch == "'":  # String überspringen
        i += 1
        while i < len(text):
            if text[i] == "\\":
                i += 2
                continue
            if text[i] == "'":
                break
            i += 1
    elif ch == "[":
        depth += 1
    elif ch == "]":
        depth -= 1
        if depth == 0:
            break
    i += 1
body = text[lb + 1 : i]

# ── Tokenizer ──────────────────────────────────────────────────────────────
TOKEN_RE = re.compile(r"""
    (?P<ws>\s+|//[^\n]*)
  | (?P<str>'(?:\\.|[^'\\])*')
  | (?P<num>-?\d+(?:\.\d+)?)
  | (?P<id>[A-Za-z_][A-Za-z0-9_]*)
  | (?P<punc>[(){}\[\]:,.])
""", re.VERBOSE)

def tokenize(s):
    toks = []
    pos = 0
    while pos < len(s):
        m = TOKEN_RE.match(s, pos)
        if not m:
            raise SystemExit(f"Tokenizer stuck at: {s[pos:pos+40]!r}")
        pos = m.end()
        kind = m.lastgroup
        if kind == "ws":
            continue
        toks.append((kind, m.group()))
    return toks

toks = tokenize(body)

# ── Parser (rekursiver Abstieg) ──────────────────────────────────────────────
class P:
    def __init__(self, toks):
        self.toks = toks
        self.i = 0
    def peek(self):
        return self.toks[self.i] if self.i < len(self.toks) else (None, None)
    def next(self):
        t = self.toks[self.i]
        self.i += 1
        return t
    def expect(self, val):
        k, v = self.next()
        if v != val:
            raise SystemExit(f"expected {val!r}, got {v!r}")

def parse_string(p):
    # Dart adjacent-string concatenation: 'a' 'b'
    parts = []
    while p.peek()[0] == "str":
        parts.append(p.next()[1][1:-1])  # ohne umschließende '
    return "".join(parts)

def parse_value(p):
    kind, val = p.peek()
    if kind == "str":
        return ("str", parse_string(p))
    if kind == "num":
        p.next()
        return ("num", val)
    if kind == "punc" and val == "[":
        p.next()
        items = []
        while p.peek()[1] != "]":
            items.append(parse_value(p))
            if p.peek()[1] == ",":
                p.next()
        p.expect("]")
        return ("list", items)
    if kind == "id":
        p.next()
        # Enum-Wert  Foo.bar
        if p.peek()[1] == ".":
            p.next()
            member = p.next()[1]
            return ("enum", f"{val}.{member}")
        # Konstruktor  Foo( ... )
        if p.peek()[1] == "(":
            p.next()
            args = {}
            while p.peek()[1] != ")":
                name = p.next()[1]
                p.expect(":")
                args[name] = parse_value(p)
                if p.peek()[1] == ",":
                    p.next()
            p.expect(")")
            return ("ctor", val, args)
        return ("id", val)
    raise SystemExit(f"unexpected token {kind}:{val}")

def parse_top(p):
    events = []
    while p.peek()[0] is not None:
        events.append(parse_value(p))
        if p.peek()[1] == ",":
            p.next()
    return events

events = parse_top(P(toks))

# ── C#-Emitter ───────────────────────────────────────────────────────────────
ENUM_MAP = {
    "EventCategory.good": "EventCategory.Good",
    "EventCategory.bad": "EventCategory.Bad",
    "EventCategory.neutral": "EventCategory.Neutral",
    "EventCategory.opportunity": "EventCategory.Opportunity",
    "EventWeight.rare": "EventWeight.Rare",
    "EventWeight.normal": "EventWeight.Normal",
    "EventWeight.common": "EventWeight.Common",
}

def cs_str(s):
    s = s.replace("\\", "\\\\").replace('"', '\\"')
    return f'"{s}"'

def emit_effect(args, indent):
    pad = " " * indent
    lines = ["new EventEffect"]
    lines.append(pad + "{")
    body = []
    if "cashDelta" in args:
        body.append(f"CashDelta = {args['cashDelta'][1]}")
    if "reputationDelta" in args:
        body.append(f"ReputationDelta = {args['reputationDelta'][1]}")
    if "brandAwarenessDelta" in args:
        body.append(f"BrandAwarenessDelta = {args['brandAwarenessDelta'][1]}")
    body.append(f"ResultMessage = {cs_str(args['resultMessage'][1])}")
    for b in body:
        lines.append(pad + "    " + b + ",")
    lines.append(pad + "}")
    return ("\n").join(lines)

def emit_choice(choice, indent):
    pad = " " * indent
    assert choice[0] == "ctor" and choice[1] == "EventChoice"
    a = choice[2]
    out = [pad + "new EventChoice"]
    out.append(pad + "{")
    out.append(pad + f"    Label = {cs_str(a['label'][1])},")
    eff = a["effect"][2]
    out.append(pad + "    Effect = " + emit_effect(eff, indent + 4) + ",")
    if "cost" in a:
        out.append(pad + f"    Cost = {a['cost'][1]},")
    out.append(pad + "},")
    return "\n".join(out)

def emit_requirements(args, indent):
    pad = " " * indent
    parts = []
    if "minShops" in args: parts.append(f"MinShops = {args['minShops'][1]}")
    if "minDay" in args: parts.append(f"MinDay = {args['minDay'][1]}")
    if "minCash" in args: parts.append(f"MinCash = {args['minCash'][1]}")
    if "needsMetropolitanShop" in args:
        v = args['needsMetropolitanShop']
        val = "true" if v[1] == "true" else "false"
        parts.append(f"NeedsMetropolitanShop = {val}")
    return "new EventRequirements { " + ", ".join(parts) + " }"

def emit_event(ev, indent):
    pad = " " * indent
    assert ev[0] == "ctor" and ev[1] == "GameEvent", ev[:2]
    a = ev[2]
    out = [pad + "new GameEvent"]
    out.append(pad + "{")
    out.append(pad + f"    Id = {cs_str(a['id'][1])},")
    out.append(pad + f"    Title = {cs_str(a['title'][1])},")
    out.append(pad + f"    Description = {cs_str(a['description'][1])},")
    out.append(pad + f"    Emoji = {cs_str(a['emoji'][1])},")
    out.append(pad + f"    Category = {ENUM_MAP[a['category'][1]]},")
    if "weight" in a:
        out.append(pad + f"    Weight = {ENUM_MAP[a['weight'][1]]},")
    if "requirements" in a:
        out.append(pad + f"    Requirements = {emit_requirements(a['requirements'][2], indent)},")
    out.append(pad + "    Choices = new List<EventChoice>")
    out.append(pad + "    {")
    for ch in a["choices"][1]:
        out.append(emit_choice(ch, indent + 8))
    out.append(pad + "    },")
    out.append(pad + "},")
    return "\n".join(out)

emitted = "\n".join(emit_event(ev, 12) for ev in events)

cs = f"""// Döner Empire 3D — Event-Katalog (Krisen/Chancen-Pool)
// AUTO-GENERIERT aus lib/models/event_model.dart via tools/gen_event_catalog.py.
// Nicht von Hand editieren — bei Änderungen am Dart-Katalog neu generieren.

using System.Collections.Generic;
using System.Linq;
using DoenerEmpire.Models;

namespace DoenerEmpire.Data
{{
    public static class EventCatalog
    {{
        public static readonly IReadOnlyList<GameEvent> All = new List<GameEvent>
        {{
{emitted}
        }};

        public static GameEvent ById(string id) => All.FirstOrDefault(e => e.Id == id);
    }}
}}
"""

OUT.write_text(cs, encoding="utf-8")
print(f"OK: {len(events)} Events -> {OUT.relative_to(ROOT)}")
