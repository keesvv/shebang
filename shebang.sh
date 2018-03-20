#!/usr/bin/env bash
if [[ "$1" = "" ]]; then
  echo "Error: You must specify a GitHub username and repository."
  echo -e "Format: \e[96mshebang <username>/<repository> [branch]\e[0m"
  echo "Example:"
  echo -e "    \e[96mshebang keesvv/plutonium-syntax\e[0m"
  exit
fi

# Check if branch argument was given
if [[ "$2" = "" ]]; then
  branch="master"
else
  branch="$2"
fi

function log {
  echo -e "\e[96m>\e[0m $1"
}

# Set local variables
repository="$1"
install_script="https://raw.githubusercontent.com/$repository/$branch/shebang/"

# Clear the screen
clear

# Get the install descriptor
echo SHEBANG | toilet
log "Getting install descriptor..."
descriptor=$(curl -fsSL "$install_script")