import json, urllib.request

# Task 2: Spec-Extraktion aus MAP_DESIGN_SPEC.md + Vergleich mit Ist-Code
spec = open(r'C:\Users\Kaan\Documents\GitHub\Doener-Empire-3D\docs\MAP_DESIGN_SPEC.md', encoding='utf-8').read()
code1 = open(r'C:\Users\Kaan\Documents\GitHub\Doener-Empire-3D\lib\ui\widgets\building_styles.dart', encoding='utf-8').read()
code2 = open(r'C:\Users\Kaan\Documents\GitHub\Doener-Empire-3D\lib\ui\screens\city_map_screen.dart', encoding='utf-8').read()

prompt = f"""Vergleiche die Design-Spec mit dem tatsächlichen Code. Finde Diskrepanzen.

DESIGN SPEC (MAP_DESIGN_SPEC.md):
{spec[:10000]}

AKTUELLER CODE (building_styles.dart + city_map_screen.dart):
{code1[:4000]}
{code2[:4000]}

Erstelle 3 Tabellen:
1. ✅ Matches - Was stimmt zwischen Spec und Code überein
2. ⚠️ Abweichungen - Was Spec sagt vs. was Code tut
3. ❌ Fehlend - Was in Spec erwähnt wird, aber im Code fehlt (Dateien, Klassen, Funktionen)

Gib auch an, ob die Doku generell aktuell oder veraltet ist."""

body = json.dumps({
    'model': 'qwen2.5-coder-14b-instruct',
    'messages': [{'role': 'user', 'content': prompt[:32000]}],
    'max_tokens': 2500,
    'temperature': 0.1
}).encode('utf-8')

req = urllib.request.Request('http://localhost:1234/v1/chat/completions', data=body,
    headers={'Content-Type': 'application/json'}, method='POST')
resp = urllib.request.urlopen(req, timeout=120)
result = json.loads(resp.read())
with open(r'C:\Users\Kaan\Documents\GitHub\Doener-Empire-3D\.agent-control\lm_review_task3_consistency.md', 'w', encoding='utf-8') as f:
    f.write("# Konsistenz-Prüfung\n\n")
    f.write(result['choices'][0]['message']['content'])
print("Task 3 done")
