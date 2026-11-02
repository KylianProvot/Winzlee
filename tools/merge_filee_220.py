#!/usr/bin/env python3
"""Import Filee 2.2.0 into Winzlee while retaining Winzlee changes by 3-way merge."""
from __future__ import annotations
import json
import os
from pathlib import Path
import subprocess
import sys
import tempfile

UPSTREAM = Path(os.environ["UPSTREAM_FILEE_ROOT"])
ROOT = Path.cwd()
APP = ROOT / "app"
BASE, NEW = "v2.1.0", "v2.2.0"
# These files are part of Winzlee's identity or the Windows Ctrl+right-click hook.
PROTECTED = {
    "src/Filee.App/Services/Triggers/TriggerService.cs",
    "src/Filee.App/Services/Triggers/GestureDetector.cs",
    "src/Filee.App/Services/SoftPalette.cs",
    "src/Filee.App/Styles/WinzleeShell.axaml",
    "src/Filee.App/App.axaml",
    "src/Filee.App/Views/MainWindow.axaml",
    "src/Filee.App/ViewModels/MainWindowViewModel.cs",
    "src/Filee.App/Services/TrayService.cs",
    "src/Filee.App/Services/SingleInstance.cs",
    "src/Filee.App/Services/SystemIntegrationService.cs",
    "src/Filee.App/Services/StartupDiagnostics.cs",
    "src/Filee.App/Views/Pages/AboutPage.axaml",
    "src/Filee.App/ViewModels/Pages/AboutPageViewModel.cs",
    "src/Filee.App/Filee.App.csproj",
    "src/Filee.Setup/Filee.Setup.csproj",
    "src/Filee.Setup/Views/SetupWindow.axaml",
    "src/Filee.Setup/Views/StateViews.axaml",
    "src/Filee.Core/Settings/TriggerGesture.cs",
}
def git(*args, check=True):
    return subprocess.run(["git", "-C", str(UPSTREAM), *args], check=check, capture_output=True)
def file_at(ref, path):
    result = git("show", f"{ref}:{path}", check=False)
    return result.stdout if result.returncode == 0 else None
def merge_text(ours, base, theirs):
    with tempfile.TemporaryDirectory() as temp:
        d = Path(temp)
        for name, content in (("ours", ours), ("base", base), ("theirs", theirs)):
            (d / name).write_bytes(content)
        result = subprocess.run(["git", "merge-file", "--ours", "-p", "ours", "base", "theirs"],
                                cwd=d, capture_output=True)
        if result.returncode not in (0, 1):
            raise RuntimeError(result.stderr.decode(errors="replace"))
        return result.stdout, result.returncode != 0
def main():
    assert APP.is_dir(), "Run from Winzlee repository root"
    if (ROOT / "docs" / "UPSTREAM_FILEE_220_MERGE.json").exists():
        print("Filee 2.2.0 is already integrated; skip re-import")
        return
    for ref in (BASE, NEW):
        git("rev-parse", "--verify", f"refs/tags/{ref}")
    changed = git("diff", "--name-status", "--no-renames", BASE, NEW,
                  "--", "src", "tests", "THIRD-PARTY-NOTICES.md").stdout.decode()
    report, stats = [], dict(imported=0, merged=0, protected=0, additions=0,
                             deleted=0, conflicts=0, binary_kept=0)
    for row in changed.splitlines():
        status, path = row.split("\t", 1)
        if path in PROTECTED or path.startswith("src/Filee.App/Assets/Icons/"):
            stats["protected"] += 1
            report.append({"file": path, "result": "preserved Winzlee"})
            continue
        if status not in ("M", "A", "D"):
            raise ValueError(f"Unknown upstream change: {row}")
        target = APP / path
        base, theirs = file_at(BASE, path), file_at(NEW, path)
        ours = target.read_bytes() if target.exists() else None
        if status == "D":
            if ours is not None and ours == base:
                target.unlink()
                stats["deleted"] += 1
            else:
                stats["protected"] += 1
            continue
        assert theirs is not None
        if ours == theirs:
            continue
        if ours is None or ours == base:
            final = theirs
            stats["additions" if ours is None else "imported"] += 1
            state = "upstream copied"
        elif base is None:
            final = ours
            stats["conflicts"] += 1
            state = "independent addition: kept Winzlee"
        elif any(b"\x00" in data for data in (ours, base, theirs)):
            final = ours
            stats["binary_kept"] += 1
            state = "binary conflict: kept Winzlee"
        else:
            final, conflict = merge_text(ours, base, theirs)
            stats["merged"] += 1
            stats["conflicts"] += int(conflict)
            state = "three-way merged" + (" (Winzlee favored)" if conflict else "")
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_bytes(final)
        if state != "upstream copied":
            report.append({"file": path, "result": state})
    log = ROOT / "docs" / "UPSTREAM_FILEE_220_MERGE.json"
    log.parent.mkdir(exist_ok=True)
    log.write_text(json.dumps({"from": BASE, "to": NEW, "statistics": stats,
                               "exceptions": report}, ensure_ascii=False, indent=2) + "\n")
    print("Upstream merge summary:", json.dumps(stats), flush=True)
    if stats["imported"] + stats["merged"] + stats["additions"] < 60:
        sys.exit("Too few upstream changes imported; refusing to commit")
if __name__ == "__main__":
    main()
