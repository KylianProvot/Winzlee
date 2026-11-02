#!/usr/bin/env python3
"""Rebuild complete Filee 2.2.0 locales, preserving Winzlee's own v0.6 strings."""
from __future__ import annotations
import json
import os
from pathlib import Path
import subprocess

ROOT = Path.cwd()
UPSTREAM = Path(os.environ["UPSTREAM_FILEE_ROOT"])
WINZLEE_BASE = "a1b53842040f2b7f99a393fafd66e435243e7032"

def blob(workdir: Path, revision: str, path: str) -> bytes | None:
    result = subprocess.run(["git", "-C", str(workdir), "show", f"{revision}:{path}"],
                            capture_output=True)
    return result.stdout if result.returncode == 0 else None

def locale(blob_bytes: bytes | None) -> dict[str, str]:
    return json.loads(blob_bytes) if blob_bytes else {}

def main() -> None:
    directory = ROOT / "app/src/Filee.App/Assets/i18n"
    counts = {}
    for file in sorted(directory.glob("*.json")):
        src = f"src/Filee.App/Assets/i18n/{file.name}"
        original = locale(blob(UPSTREAM, "v2.1.0", src))
        updated = locale(blob(UPSTREAM, "v2.2.0", src))
        winzlee = locale(blob(ROOT, WINZLEE_BASE, "app/" + src))
        if not updated:
            continue
        overrides = {key: value for key, value in winzlee.items()
                     if key not in original or original[key] != value}
        updated.update(overrides)
        file.write_text(json.dumps(updated, ensure_ascii=False, indent=2) + "\n",
                        encoding="utf-8")
        counts[file.name] = {
            "official_2_2_keys": len(updated) - len([k for k in overrides if k not in original]),
            "winzlee_overrides": len(overrides),
            "result": len(updated),
        }
        if "installer.error.incomplete" not in updated:
            raise ValueError(f"Missing upstream key installer.error.incomplete: {file.name}")
    # Ensure every locale has the complete keys, including German's Winzlee-only ones.
    # Preserve translated strings unless their format placeholders differ from English.
    import re
    english = json.loads((directory / "en.json").read_text(encoding="utf-8"))
    harmonization = {}
    for path in sorted(directory.glob("*.json")):
        if path.name == "en.json":
            continue
        strings = json.loads(path.read_text(encoding="utf-8"))
        filled = 0
        placeholders_fixed = 0
        for key, master in english.items():
            if key not in strings:
                strings[key] = master
                filled += 1
            elif set(re.findall(r"\{\d+\}", strings[key])) != set(re.findall(r"\{\d+\}", master)):
                strings[key] = master
                placeholders_fixed += 1
        path.write_text(json.dumps(strings, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        counts[path.name]["result"] = len(strings)
        harmonization[path.name] = {"missing_keys_filled": filled, "placeholder_repairs": placeholders_fixed}

    # Winzlee's Avalonia assembly is named Winzlee, not Filee, including new 2.2 windows.
    fixed = []
    for path in (ROOT / "app/src/Filee.App").rglob("*"):
        if path.suffix not in (".axaml", ".cs") or not path.is_file():
            continue
        current = path.read_text(encoding="utf-8-sig")
        desired = current.replace("avares://Filee/", "avares://Winzlee/")
        desired = desired.replace("fonts:Filee#", "fonts:Winzlee#")
        if desired != current:
            path.write_text(desired, encoding="utf-8")
            fixed.append(str(path.relative_to(ROOT)))
    (ROOT / "docs/FILEE_220_LOCALE_REPORT.json").write_text(
        json.dumps({"locales": counts, "harmonization": harmonization, "fixed_avalonia_references": fixed},
                   ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"Reconciled {len(counts)} locales, fixed {len(fixed)} app resource paths")

if __name__ == "__main__":
    main()
