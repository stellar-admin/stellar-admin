# Shared by setup.sh, run.sh and grade.sh.

evals="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
repo="$(cd "$evals/../.." && pwd)"
mcp_version="1.13.0"

tool_dir() { dirname "$(readlink -f "$(mise which "$1" 2>/dev/null || command -v "$1")")"; }
dotnet_root="$(tool_dir dotnet)"
sandbox_path="$(tool_dir claude):$(tool_dir node):$dotnet_root:/usr/local/bin:/usr/bin:/bin"

# The run root must be outside the repository, so agents can't find StellarAdmin's source through it.
check_root() {
  case "$1/" in "$repo/"*) echo "the run root must be outside the repository" >&2; exit 1 ;; esac
}

# The agent's credentials: CLAUDE_CODE_OAUTH_TOKEN (from \`claude setup-token\`) or ANTHROPIC_API_KEY,
# from the environment or from ~/.config/stellaradmin-agent-evals/claude-token.
credentials() {
  if [ -n "${CLAUDE_CODE_OAUTH_TOKEN:-}" ]; then echo "CLAUDE_CODE_OAUTH_TOKEN=$CLAUDE_CODE_OAUTH_TOKEN"
  elif [ -n "${ANTHROPIC_API_KEY:-}" ]; then echo "ANTHROPIC_API_KEY=$ANTHROPIC_API_KEY"
  elif [ -s "$HOME/.config/stellaradmin-agent-evals/claude-token" ]; then echo "CLAUDE_CODE_OAUTH_TOKEN=$(tr -d '[:space:]' < "$HOME/.config/stellaradmin-agent-evals/claude-token")"
  else echo "no credentials: run \`claude setup-token\` and save the token to ~/.config/stellaradmin-agent-evals/claude-token" >&2; return 1
  fi
}

# Runs a command in a sandbox that sees the system and the tools but not the home directory: not this
# repository, other checkouts, or Claude's user configuration, memory and skills. Only <rw-dir> is writable,
# plus an empty home and /tmp that vanish afterwards; directories in the sandbox_ro array are added read-only.
# The network is shared, so the sandbox reaches the internet and apps on localhost.
# Usage: sandbox <rw-dir> <cwd> <command>...
sandbox_ro=()
sandbox() {
  local rw="$1" cwd="$2"; shift 2
  local args=(--die-with-parent --unshare-pid
    --ro-bind / / --dev /dev --proc /proc --tmpfs /tmp --tmpfs "$HOME"
    --ro-bind "$HOME/.local/share/mise" "$HOME/.local/share/mise")
  local path
  for path in "${sandbox_ro[@]}"; do args+=(--ro-bind "$path" "$path"); done
  args+=(--bind "$rw" "$rw" --chdir "$cwd")
  # The environment is replaced in a subshell rather than with --setenv, so the credentials never appear in a
  # command line that ps shows to other users.
  (
    home="$HOME"
    while IFS= read -r name; do unset "$name" 2> /dev/null; done < <(compgen -e)
    export HOME="$home" PATH="$sandbox_path" LANG=en_US.UTF-8 DOTNET_ROOT="$dotnet_root"
    export DOTNET_SYSTEM_NET_DISABLEIPV6=1 DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
    export CHROME_DEVTOOLS_MCP_NO_USAGE_STATISTICS=1
    if [ -n "${sandbox_credentials:-}" ]; then export "$sandbox_credentials"; fi
    exec bwrap "${args[@]}" "$@"
  )
}
