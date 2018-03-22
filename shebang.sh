#!/usr/bin/env bash
function printSyntax {
  echo "Error: You must specify a GitHub username and repository."
  echo -e "Format: \e[96mshebang <username>/<repository> [branch]\e[0m"
  echo "Example:"
  echo -e "    \e[96mshebang keesvv/shebang\e[0m\n"
  echo -e "If you would like to create a package, type \e[96mshebang\e[0m"
  exit
}

function log {
  echo -e "\e[96m>\e[0m $1"
}

function err {
  echo -e "\e[91m[!]\e[0m $1"
}

if [[ "$1" = "" ]]; then
  printSyntax
fi

# Check if branch argument was given
if [[ "$2" = "" ]]; then
  branch="master"
else
  branch="$2"
fi

# Set local variables
repository="$1"
install_script="https://raw.githubusercontent.com/$repository/$branch/shebang.json"

# Clear the screen
clear

# Get the install descriptor
echo SHEBANG | toilet -f pagga | lolcat -F 0.25 && printf "\n"
log "Getting install descriptor..."
descriptor=$(curl -fs "$install_script")

# Get the status code
headers=$(curl -i -o - -s "$install_script")
status_code=$(echo "$headers" | grep HTTP | awk '{print $2}')

# Check if a common error code is returned
if [[ "$status_code" -ne 200 ]]; then
  if [[ "$status_code" = 404 ]]; then
    err "The shebang.json file could not be found in this repository (error $status_code)."
  elif [[ "$status_code" = 503 ]]; then
    err "The GitHub services are currently unavailable or you are sending too much requests (error $status_code)."
  elif [[ "$status_code" = "" ]]; then
    err "An unknown error has occured, without reporting an error code."
  else
    err "An unknown error has occured (error $status_code)."
  fi

  err "Please report this to the author of the repository."
  err "If you are the author, see github.com/keesvv/shebang for more info."
else

  # Extract all entries from the JSON descriptor
  name=$(echo $descriptor | jq -r ".name")
  id=$(echo $descriptor | jq -r ".id")
  version=$(echo $descriptor | jq -r ".version")

  # Print the package information
  log "Installing $name (v-$version) ..."

  # Store home directory into a variable
  home_dir=$(eval echo ~)

  # Clone repository to local files
  log "Cloning repository to \e[92m$home_dir/shebang/$id\e[0m ..."
  git clone -q -b "$branch" "https://github.com/$repository" "$home_dir/shebang/$id"
  log "Done cloning repository."

fi
