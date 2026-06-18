import json, urllib.request, sys

code1 = open(r'C:\Users\Kaan\Documents\GitHub\Doener-Empire-3D\lib\core\theme.dart', encoding='utf-8').read()
code2 = open(r'C:\Users\Kaan\Documents\GitHub\Doener-Empire-3D\lib\ui\widgets\building_styles.dart', encoding='utf-8').read()
code3 = open(r'C:\Users\Kaan\Documents\GitHub\Doener-Empire-3D\lib\ui\screens\city_map_screen.dart', encoding='utf-8').read()

prompt = f"""Führe Code-Review für diese 3 Flutter-Dateien durch. Finde:
1. Ungenutzte Imports
2. Duplizierte Konfigurationen/Farbdefinitionen
3. Veraltete Kommentare
4. Fehlende const-Konstruktoren
5. Inkonsistenzen zwischen den Dateien (z.B. MapPalette vs AppColors)

Datei theme.dart:
{code1[:8000]}

Datei building_styles.dart:
{code2[:5000]}

Datei city_map_screen.dart:
{code3[:8000]}

Antworte als Markdown-Liste mit Dateipfad, Zeilennummer und Beschreibung."""

body = json.dumps({
    'model': 'qwen2.5-coder-14b-instruct',
    'messages': [{'role': 'user', 'content': prompt[:32000]}],
    'max_tokens': 2000,
    'temperature': 0.1
}).encode('utf-8')

req = urllib.request.Request('http://localhost:1234/v1/chat/completions', data=body,
    headers={'Content-Type': 'application/json'}, method='POST')
resp = urllib.request.urlopen(req, timeout=120)
result = json.loads(resp.read())
with open(r'C:\Users\Kaan\Documents\GitHub\Doener-Empire-3D\.agent-control\lm_review_task1_result.md', 'w', encoding='utf-8') as f:
    f.write(result['choices'][0]['message']['content'])
print("Done - result written to lm_review_task1_result.md")
