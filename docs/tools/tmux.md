# Tmux Guide for Core Ledger Development

This guide covers the tmux configuration and workflow for developing Core Ledger on macOS.

## Overview

The Core Ledger tmux setup provides a pre-configured development environment that automatically starts all required services in separate windows with monitoring enabled.

**Configuration File:** `~/.tmux.conf`

## Quick Start

### Starting Your Development Environment

```bash
# Start tmux and attach to core-ledger session
tmux attach -t core-ledger
```

This command will automatically:
1. Start Docker infrastructure (PostgreSQL, Redis, RabbitMQ)
2. Start the Angular UI dev server (window 0)
3. Start the .NET API with hot reload (window 1)
4. Start the .NET Worker with hot reload (window 2)
5. Display Docker logs (window 3)

### Stopping Your Environment

```bash
# Detach from tmux (services keep running in background)
Ctrl-a d

# Or kill the entire session (stops all services)
tmux kill-session -t core-ledger
```

## Window Layout

The core-ledger session consists of 4 windows:

| Window | Name | Command | URL |
|--------|------|---------|-----|
| 0 | `ui` | `nx serve core-ledger-ui` | http://localhost:4200 |
| 1 | `api` | `nx serve core-ledger-api` | https://localhost:7109 |
| 2 | `worker` | `nx serve core-ledger-worker` | - |
| 3 | `logs` | `npm run docker:up && docker compose logs -f` | - |

All windows start in `/Users/jlagedo/Developer/core-ledger` with activity monitoring enabled.

## Essential Key Bindings

**Note:** The prefix key is `Ctrl-a` (not the default `Ctrl-b`). All commands start with this prefix.

### Window Management

```bash
Ctrl-a 0-3          # Switch to window 0, 1, 2, or 3
Ctrl-a c            # Create new window
Ctrl-a ,            # Rename current window
Ctrl-a &            # Kill current window (with confirmation)
Ctrl-a n            # Next window
Ctrl-a p            # Previous window
Ctrl-a Ctrl-h       # Previous window (alternative)
Ctrl-a Ctrl-l       # Next window (alternative)
```

### Pane Management

```bash
Ctrl-a |            # Split pane vertically
Ctrl-a -            # Split pane horizontally
Ctrl-a h            # Navigate to left pane
Ctrl-a j            # Navigate to down pane
Ctrl-a k            # Navigate to up pane
Ctrl-a l            # Navigate to right pane
Ctrl-a H            # Resize pane left
Ctrl-a J            # Resize pane down
Ctrl-a K            # Resize pane up
Ctrl-a L            # Resize pane right
Ctrl-a x            # Kill current pane (with confirmation)
Ctrl-a z            # Toggle pane zoom (fullscreen)
```

### Session Management

```bash
Ctrl-a d            # Detach from session
Ctrl-a (            # Switch to previous session
Ctrl-a )            # Switch to next session
Ctrl-a s            # List all sessions (interactive)
```

### Copy Mode & Clipboard

```bash
Ctrl-a [            # Enter copy mode (Vi-style navigation)
Ctrl-a ]            # Paste from tmux buffer

# In copy mode:
v                   # Begin selection
y                   # Copy selection to macOS clipboard (pbcopy)
Enter               # Copy selection to macOS clipboard
q                   # Exit copy mode
```

### Other Useful Commands

```bash
Ctrl-a r            # Reload tmux configuration
Ctrl-a y            # Toggle synchronize-panes (type in all panes)
Ctrl-a C-k          # Clear screen and scrollback history
Ctrl-a ?            # Show all key bindings
Ctrl-a :            # Enter command mode
```

## Common Workflows

### Monitoring All Services

1. Start tmux: `tmux attach -t core-ledger`
2. Window 0 (ui) will show Angular compilation status
3. Window 1 (api) will show .NET API hot reload and requests
4. Window 2 (worker) will show background job processing
5. Window 3 (logs) will show Docker container logs

The status bar will highlight windows with activity (compilation complete, errors, etc.)

### Running Additional Commands

```bash
# Option 1: Create a new window
Ctrl-a c
# Run your command (e.g., npm test, git status)

# Option 2: Split an existing window
Ctrl-a |            # Split vertically
# Run your command in the new pane
```

### Restarting a Service

```bash
# Navigate to the service window (e.g., Ctrl-a 1 for API)
Ctrl-c              # Stop the current process
# Press Up arrow to get last command
Enter               # Restart the service
```

### Viewing Different Log Streams

```bash
# Navigate to logs window
Ctrl-a 3

# Stop current logs
Ctrl-c

# View specific service logs
docker compose logs -f postgres
docker compose logs -f rabbitmq
docker compose logs -f redis

# Or view all again
docker compose logs -f
```

### Working with Multiple Panes

```bash
# Split logs window to monitor multiple things
Ctrl-a 3            # Go to logs window
Ctrl-a |            # Split vertically
# Left pane: docker compose logs -f postgres
Ctrl-a l            # Move to right pane
# Right pane: docker compose logs -f rabbitmq

# Navigate between panes
Ctrl-a h/j/k/l      # Vim-style navigation
```

### Synchronizing Commands Across Panes

```bash
# Enable synchronize-panes
Ctrl-a y

# Now typing will appear in all panes simultaneously
# Useful for running the same command in multiple directories

# Toggle off when done
Ctrl-a y
```

## macOS-Specific Features

### Clipboard Integration

Copy-paste between tmux and macOS works seamlessly:

```bash
# Copy in tmux copy mode
Ctrl-a [            # Enter copy mode
v                   # Start selection
y                   # Copy to macOS clipboard

# Paste in tmux
Ctrl-a ]            # Paste

# Or paste anywhere in macOS with Cmd-v
```

### Mouse Support

Mouse support is enabled:
- Click to switch panes
- Scroll to navigate history
- Drag pane borders to resize (not implemented for copy-drag)

### Activity Notifications

Visual notifications are enabled in the status bar when:
- Background windows have activity
- Processes complete or encounter errors
- Services restart

## Troubleshooting

### Session Already Exists

If you get "session already exists" when trying to start tmux:

```bash
# List existing sessions
tmux list-sessions

# Attach to existing session
tmux attach -t core-ledger

# Or kill and restart
tmux kill-session -t core-ledger
tmux attach -t core-ledger
```

### Services Not Starting

If services fail to start automatically:

```bash
# Navigate to the problematic window
Ctrl-a 0-3

# Check for error messages
# Manually restart the service
nx serve core-ledger-ui    # or api, worker
```

### Docker Infrastructure Issues

```bash
# Navigate to logs window
Ctrl-a 3

# Stop docker compose logs
Ctrl-c

# Restart infrastructure
npm run docker:down
npm run docker:up
docker compose logs -f
```

### Pane Too Small Errors

If you see "pane too small" errors:

```bash
# Zoom the current pane
Ctrl-a z

# Or resize the pane
Ctrl-a H/J/K/L
```

### Configuration Not Loading

```bash
# Reload configuration
Ctrl-a r

# Or restart tmux entirely
tmux kill-session -t core-ledger
tmux attach -t core-ledger
```

## Advanced Usage

### Creating Custom Windows

```bash
# Create a new window for testing
Ctrl-a c
Ctrl-a ,            # Rename to "tests"
npm run test

# Create a window for git operations
Ctrl-a c
Ctrl-a ,            # Rename to "git"
git status
```

### Using Sessions for Different Tasks

```bash
# Create a new session for ETL work
tmux new-session -s etl -c ~/Developer/core-ledger/tools/etl

# Switch between sessions
Ctrl-a s            # Interactive session list
# Or use Ctrl-a ( and Ctrl-a )

# List all sessions from outside tmux
tmux list-sessions
```

### Customizing Your Setup

Edit `~/.tmux.conf` to customize:
- Status bar colors and format
- Key bindings
- Auto-start commands
- Default window layout

After editing, reload with `Ctrl-a r`

## Reference

### Command Line Utilities

```bash
# Session management
tmux list-sessions              # List all sessions
tmux attach -t core-ledger      # Attach to session
tmux kill-session -t core-ledger # Kill session
tmux new-session -s name        # Create new session

# Window management (from outside tmux)
tmux list-windows -t core-ledger
tmux select-window -t core-ledger:0

# Pane management (from outside tmux)
tmux list-panes -t core-ledger:0
```

### Configuration File Location

```bash
~/.tmux.conf                    # User configuration
```

### Useful Links

- [Tmux Cheat Sheet](https://tmuxcheatsheet.com/)
- [Tmux Manual](https://man.openbsd.org/tmux.1)
- [Core Ledger Documentation](../README.md)

## Tips & Best Practices

1. **Use `Ctrl-a d` to detach** instead of closing the terminal - services keep running
2. **Monitor the status bar** for activity notifications in other windows
3. **Use copy mode** (`Ctrl-a [`) for scrolling through long output
4. **Create splits** for related tasks (e.g., logs + shell in same window)
5. **Name your windows** (`Ctrl-a ,`) when creating custom ones
6. **Use `Ctrl-a z`** to zoom a pane temporarily for better visibility
7. **Clear scrollback** (`Ctrl-a C-k`) when output gets too long

## Quick Reference Card

```
Prefix: Ctrl-a

Windows:        Panes:          Copy Mode:      Other:
0-3  Switch     |    Split V    [    Enter      d    Detach
c    Create     -    Split H    v    Select     r    Reload
,    Rename     h/j/k/l Nav     y    Copy       y    Sync panes
&    Kill       H/J/K/L Resize  q    Exit       z    Zoom
n/p  Next/Prev  x    Kill       ]    Paste      ?    Help

Services:
0: UI (http://localhost:4200)
1: API (https://localhost:7109)
2: Worker
3: Logs (Docker)
```
