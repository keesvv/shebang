#!/usr/bin/env bash
function print_syntax {
  echo "Error: You must specify a GitHub username and repository."
  echo -e "Format: \e[90mshebang \e[0m<\e[96musername\e[0m>/<\e[96mrepository\e[0m> [\e[96mbranch\e[0m]"
  echo "Example:"
  echo -e "    \e[90mshebang \e[96mkeesvv\e[0m/\e[96mshebang\e[0m\n"
  echo -e "If you would like to create a package, type \e[90mshebang \e[96mcreate\e[0m."
  echo -e "You can also easily update Shebang by typing \e[90mshebang \e[96mupdate\e[0m."
  exit
}

function log_info {
  echo -e "\e[96m>\e[0m $1"
}

function log_err {
  echo -e "\e[91m[!]\e[0m $1"
}

function install_shebang {
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
  log_info "Getting install descriptor..."
  descriptor=$(curl -fs "$install_script")

  # Get the status code
  headers=$(curl -i -o - -s "$install_script")
  status_code=$(echo "$headers" | grep HTTP | awk '{print $2}')

  # Check if a common error code is returned
  if [[ "$status_code" -ne 200 ]]; then
    if [[ "$status_code" = 404 ]]; then
      log_err "The shebang.json file could not be found in this repository (error $status_code)."
    elif [[ "$status_code" = 503 ]]; then
      log_err "The GitHub services are currently unavailable or you are sending too much requests (error $status_code)."
    elif [[ "$status_code" = "" ]]; then
      log_err "An unknown error has occured, without reporting an error code."
    else
      log_err "An unknown error has occured (error $status_code)."
    fi

    log_err "Please report this to the author of the repository."
    log_err "If you are the author, see github.com/keesvv/shebang for more info."
  else
    # Extract all entries from the JSON descriptor
    id=$(echo $descriptor | jq -r ".id")
    name=$(echo $descriptor | jq -r ".name")
    version=$(echo $descriptor | jq -r ".version")

    # Print the package information
    log_info "Installing $name (v-$version) ..."

    # Store home directory into a variable
    home_dir=$(eval echo ~)

    # Clone repository to local files
    clone_path="$home_dir/shebang/packages/$id"
    log_info "Cloning repository to \e[92m$clone_path\e[0m ..."
    git clone -q -b "$branch" "https://github.com/$repository" "$clone_path"
    log_info "Done cloning repository."
  fi
}

function create_shebang {
  echo -e "\e[90mNOTE: A package id is always in the format \e[95mpackage-name\e[90m,"
  echo -e "\e[90mevery character must be lower-case and only dashes and alphanumeric characters"
  echo -e "\e[90mare allowed.\e[0m"

  read -p "Package id: " package_id
  read -p "Package name: " package_name
  read -p "Package version: " package_version

  # Build a JSON skeleton
  json_skeleton=$(echo "{}" |
    jq ".id = \"$package_id\"" |
    jq ".name = \"$package_name\"" |
    jq ".version = \"$package_version\"")

  # Store the home directory into a local variable
  home_dir=$(eval echo ~)

  # Check if the descriptors directory exists
  if [[ ! -d "$home_dir/shebang/descriptors" ]]; then
    mkdir "$home_dir/shebang/descriptors"
  fi

  descriptor_path="$home_dir/shebang/descriptors/$package_id.json"
  echo "$json_skeleton" > "$descriptor_path"
  printf "\n" && echo "$json_skeleton" | jq -C . && printf "\n"

  # Ask user if descriptor information is correct
  read -r -p "Is this package descriptor correct? [y/N]" is_correct
  is_correct=${is_correct,,}
  if [[ ! "$is_correct" =~ ^(yes|y)$ ]]; then
    nano -E "$package_id.json"
  fi

  echo -e "Package descriptor saved to \e[92m$descriptor_path\e[0m."
}

# Check if arguments are not null
if [[ "$1" = "" ]]; then
  print_syntax
  exit 0
elif [[ "$1" = "install" ]]; then
  install_shebang "$2" "$3"
  exit 0
elif [[ "$1" = "create" ]]; then
  create_shebang
  exit 0
elif [[ "$1" = "update" ]]; then
  /usr/share/shebang/install.sh
  exit 0
fi
