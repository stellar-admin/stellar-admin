#!/usr/bin/env bash
# Read-only triage of generated website changes against HEAD. Never restores files.
# Usage: bash classify-generator-output.sh path/to/website
set -eu
if [ "$#" -ne 1 ]; then
  echo 'Usage: classify-generator-output.sh path/to/website' >&2
  exit 2
fi
python3 - "$1" <<'PY'
import os
from pathlib import Path
import re
import subprocess
import sys

repo = Path(sys.argv[1]).resolve()
def git(*args):
    return subprocess.check_output(['git', '-C', str(repo), *args])

try:
    if Path(os.fsdecode(git('rev-parse', '--show-toplevel')).strip()).resolve() != repo:
        raise ValueError('Pass the website repository root, not a subdirectory.')
    paths = ['public/demo', 'content/docs/tag-helpers/components/_include']
    changed = git('diff', '--name-only', '--no-renames', '-z', 'HEAD', '--', *paths)
    untracked = git('ls-files', '--others', '--exclude-standard', '-z', '--', *paths)
    names = sorted(set(os.fsdecode(p) for p in (changed + untracked).split(b'\0') if p))
    def normalize(data):
        data = re.sub(rb'<svg[^>]*>', b'<svg>', data)
        return re.sub(rb'(__RequestVerificationToken"[^>]*value=")[^"]*', rb'\1TOKEN', data)
    for name in names:
        path = repo / name
        # Links, additions, deletions, and mode changes always need review.
        old = subprocess.run(['git', '-C', str(repo), 'show', 'HEAD:' + name], capture_output=True)
        mode_change = bool(git('diff', '--summary', 'HEAD', '--', name).strip())
        equal = (old.returncode == 0 and path.is_file() and not path.is_symlink()
                 and not mode_change and normalize(old.stdout) == normalize(path.read_bytes()))
        label = 'normalized-equal (review SVG attributes/tokens)' if equal else 'changed (review)'
        print(f'{label}: {name!r}')
    print(f'{len(names)} changed path(s). Read-only: no files restored. Normalized equality does not prove a change is disposable.')
except (OSError, subprocess.CalledProcessError, ValueError) as error:
    print(str(error), file=sys.stderr)
    raise SystemExit(2)
PY
