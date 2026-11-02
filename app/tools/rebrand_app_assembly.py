#!/usr/bin/env python3
"""One-time, repeatable Winzlee app-host rename.

Keep Filee.* namespaces, project filenames, and original third-party notices for now.
The only managed assembly renamed here is Filee.App, which owns the Avalonia assets.
"""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
APP = ROOT / "src" / "Filee.App"
CSPROJ = APP / "Filee.App.csproj"

def main() -> None:
    project = CSPROJ.read_text(encoding="utf-8")
    old = "<AssemblyName>Filee</AssemblyName>"
    new = "<AssemblyName>Winzlee</AssemblyName>"
    if old in project:
        CSPROJ.write_text(project.replace(old, new), encoding="utf-8")
        print(f"Renamed output assembly: {CSPROJ.relative_to(ROOT)}")
    elif new not in project:
        raise RuntimeError("Cannot locate the app's AssemblyName element")

    # Filee.Engines exposes internal converter helpers to the UI assembly.
    # Preserve that friend relationship when the app is named Winzlee.
    engines = ROOT / "src" / "Filee.Engines" / "Filee.Engines.csproj"
    engines_xml = engines.read_text(encoding="utf-8")
    friend = '    <InternalsVisibleTo Include="Winzlee" />'
    if friend not in engines_xml:
        anchor = '    <InternalsVisibleTo Include="Filee" />'
        if anchor not in engines_xml:
            raise RuntimeError("Cannot locate Filee.Engines friend assembly list")
        engines.write_text(engines_xml.replace(anchor, anchor + "\n" + friend), encoding="utf-8")
        print("Granted Winzlee access to Filee.Engines internal helpers")

    count = 0
    # Explicit Avalonia URIs must match the new assembly resource name.
    # Do NOT rename Filee.Core or other libraries, namespaces, or original licenses.
    replacements = (
        ("avares://Filee/", "avares://Winzlee/"),
        ("fonts:Filee", "fonts:Winzlee"),
    )
    for folder in [ROOT / "src", ROOT / "tests"]:
        for path in folder.rglob("*"):
            if not path.is_file() or path.suffix.lower() not in {".cs", ".axaml", ".xaml", ".xml", ".csproj", ".json"}:
                continue
            old_content = path.read_text(encoding="utf-8-sig")
            content = old_content
            for before, after in replacements:
                content = content.replace(before, after)
            if content != old_content:
                path.write_text(content, encoding="utf-8")
                count += 1
                print(f"Updated resource URI: {path.relative_to(ROOT)}")
    print(f"Winzlee assembly resource rewrite complete ({count} files).")

if __name__ == "__main__":
    main()
