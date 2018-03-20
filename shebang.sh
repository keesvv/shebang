#!/usr/bin/env bash
if [[ "$1" = "" ]]; then
  echo "Error: You must specify a GitHub username and repository."
  echo -e "Format: \e[96mshebang <username>/<repository> [branch]\e[0m"
  echo "Example:"
  echo -e "    \e[96mshebang keesvv/shebang\e[0m"
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

function err {
  echo -e "\e[91m[!]\e[0m $1"
}

# Set local variables
repository="$1"
install_script="https://raw.githubusercontent.com/$repository/$branch/shebang.json"

# Clear the screen
clear

# Get the install descriptor
echo SHEBANG | toilet
log "Getting install descriptor..."
descriptor=$(curl -fs --output /dev/stderr --write-out "%{http_code}" "$install_script")

# Check if a common error code is returned
if [[ "$descriptor" -ne 200 ]]; then
  if [[ "$descriptor" = 404 ]]; then
    err "The shebang.json file could not be found in this repository."
    err "Please report this to the author of the repository."
    err "If you are the author, see github.com/keesvv/shebang for more info."
  fi
else
  log "Install descriptor found!"
  echo "$descriptor"
fi
