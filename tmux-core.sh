#!/bin/bash
#######################################
# Script Name: tmux-core.sh
# Description: Core Ledger Development Environment - Creates a 2x2 tmux layout
# Requirements: tmux, OrbStack (orb), nx
# Author: Core Ledger Team
# Date: 2026-01-18
#######################################

# Configuration
readonly SESSION_NAME="core-ledger"
readonly SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
readonly PROJECT_DIR="$SCRIPT_DIR"
readonly DOCKER_WAIT_TIMEOUT=30

# Logging function
log() {
    echo "[$(date +'%Y-%m-%d %H:%M:%S')] $*"
}

# Error handler
error_exit() {
    echo "ERROR: $1" >&2
    exit 1
}

# Check dependencies
check_dependencies() {
    local deps=("tmux" "orb" "nx")
    for cmd in "${deps[@]}"; do
        command -v "$cmd" >/dev/null 2>&1 || error_exit "$cmd is not installed"
    done
}

# Ensure OrbStack is running
ensure_orbstack_running() {
    log "Checking OrbStack status..."

    # Check if docker context is available (OrbStack is running)
    if docker ps >/dev/null 2>&1; then
        log "OrbStack is already running"
        return 0
    fi

    log "Starting OrbStack..."
    orb start

    # Wait for OrbStack to be ready
    local elapsed=0
    while [[ $elapsed -lt $DOCKER_WAIT_TIMEOUT ]]; do
        if docker ps >/dev/null 2>&1; then
            log "OrbStack is ready"
            return 0
        fi
        sleep 1
        ((elapsed++))
    done

    error_exit "Failed to start OrbStack after ${DOCKER_WAIT_TIMEOUT}s"
}

# Wait for Docker services to be healthy
wait_for_docker() {
    local elapsed=0
    local required_containers=("core-ledger-db" "rabbitmq" "redis")
    local required_count=${#required_containers[@]}

    log "Checking Docker services (${required_count} containers required)..."

    while [[ $elapsed -lt $DOCKER_WAIT_TIMEOUT ]]; do
        local running_count=$(docker ps --format '{{.Names}}' 2>/dev/null | grep -E '(core-ledger-db|rabbitmq|redis)' | wc -l | tr -d ' ')

        if [[ $running_count -eq $required_count ]]; then
            log "Docker services are ready (${running_count}/${required_count} containers running)"
            return 0
        fi

        sleep 1
        ((elapsed++))
    done

    local final_count=$(docker ps --format '{{.Names}}' 2>/dev/null | grep -E '(core-ledger-db|rabbitmq|redis)' | wc -l | tr -d ' ')
    log "Warning: Only ${final_count}/${required_count} containers running after ${DOCKER_WAIT_TIMEOUT}s. Proceeding anyway..."
    return 1
}

# Setup session if it doesn't exist
setup_session() {
    # Create 3 more panes (total 4)
    tmux split-window -h -t "$SESSION_NAME:dev" -c "$PROJECT_DIR"
    tmux split-window -h -t "$SESSION_NAME:dev" -c "$PROJECT_DIR"
    tmux split-window -h -t "$SESSION_NAME:dev" -c "$PROJECT_DIR"

    # Apply tiled layout for perfect 2x2 grid
    tmux select-layout -t "$SESSION_NAME:dev" tiled

    # Set pane titles (displayed by Catppuccin theme)
    tmux select-pane -t "$SESSION_NAME:dev.1" -T "󰚲 Angular UI"
    tmux select-pane -t "$SESSION_NAME:dev.2" -T "󰒍 .NET API"
    tmux select-pane -t "$SESSION_NAME:dev.3" -T "󰓁 .NET Worker"
    tmux select-pane -t "$SESSION_NAME:dev.4" -T "󰆼 PostgreSQL"

    # Pane 1 (top-left): Angular UI
    tmux send-keys -t "$SESSION_NAME:dev.1" "clear" C-m
    tmux send-keys -t "$SESSION_NAME:dev.1" "echo '🎨 Starting Angular UI...'" C-m
    tmux send-keys -t "$SESSION_NAME:dev.1" "nx serve core-ledger-ui || echo '❌ Failed to start Angular UI'" C-m

    # Pane 2 (top-right): .NET API
    # Wait for Docker infrastructure before starting
    tmux send-keys -t "$SESSION_NAME:dev.2" "clear" C-m
    tmux send-keys -t "$SESSION_NAME:dev.2" "echo '🚀 Waiting for Docker services...'" C-m
    tmux send-keys -t "$SESSION_NAME:dev.2" "until [ \$(docker ps --format '{{.Names}}' 2>/dev/null | grep -E '(core-ledger-db|rabbitmq|redis)' | wc -l | tr -d ' ') -eq 3 ]; do sleep 1; done && echo '🚀 Starting .NET API...'" C-m
    tmux send-keys -t "$SESSION_NAME:dev.2" "nx serve core-ledger-api || echo '❌ Failed to start .NET API'" C-m

    # Pane 3 (bottom-left): .NET Worker
    # Wait for Docker infrastructure before starting
    tmux send-keys -t "$SESSION_NAME:dev.3" "clear" C-m
    tmux send-keys -t "$SESSION_NAME:dev.3" "echo '⚙️  Waiting for Docker services...'" C-m
    tmux send-keys -t "$SESSION_NAME:dev.3" "until [ \$(docker ps --format '{{.Names}}' 2>/dev/null | grep -E '(core-ledger-db|rabbitmq|redis)' | wc -l | tr -d ' ') -eq 3 ]; do sleep 1; done && echo '⚙️  Starting .NET Worker...'" C-m
    tmux send-keys -t "$SESSION_NAME:dev.3" "nx serve core-ledger-worker || echo '❌ Failed to start .NET Worker'" C-m

    # Pane 4 (bottom-right): PostgreSQL logs
    tmux send-keys -t "$SESSION_NAME:dev.4" "clear" C-m
    tmux send-keys -t "$SESSION_NAME:dev.4" "echo '📋 PostgreSQL Database Logs'" C-m
    tmux send-keys -t "$SESSION_NAME:dev.4" "echo ''" C-m
    tmux send-keys -t "$SESSION_NAME:dev.4" "docker logs -f core-ledger-db" C-m

    # Select the first pane (Angular UI)
    tmux select-pane -t "$SESSION_NAME:dev.1"

    log "Session '$SESSION_NAME' created and configured"
}

# Main function
main() {
    log "Starting Core Ledger Development Environment"

    # Check dependencies
    check_dependencies

    # Ensure OrbStack is running
    ensure_orbstack_running

    # Wait for Docker services (non-blocking - just checks)
    wait_for_docker

    # Check if session already exists
    if tmux has-session -t "$SESSION_NAME" 2>/dev/null; then
        log "Session '$SESSION_NAME' already exists, attaching..."
        tmux attach-session -t "$SESSION_NAME"
    else
        log "Creating new session '$SESSION_NAME'..."
        # Create new session with first pane
        tmux new-session -d -s "$SESSION_NAME" -n "dev" -c "$PROJECT_DIR"

        # Setup the session
        setup_session

        # Attach to session
        tmux attach-session -t "$SESSION_NAME"
    fi
}

# Run main function
main "$@"
