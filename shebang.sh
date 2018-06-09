#!/usr/bin/env bash
home_dir=$(eval echo ~)

function print_syntax {
  echo "Error: You must specify a GitHub username and repository."
  echo -e "Format: \e[90mshebang install \e[0m<\e[96musername\e[0m>/<\e[96mrepository\e[0m> [\e[96mbranch\e[0m]"
  echo "Example:"
  echo -e "    \e[90mshebang install \e[96mkeesvv\e[0m/\e[96mshebang\e[0m"
  echo "To remove a package, you can use the following command:"
  echo -e "    \e[90mshebang remove \e[96mpackage-id\e[0m\n"
  echo -e "If you would like to create a package, type \e[90mshebang \e[96mcreate\e[0m."
  echo -e "You can also easily update Shebang by typing \e[90mshebang \e[96mupdate\e[0m."
  exit
}

function print_splash {
  echo SHEBANG | toilet -f pagga | lolcat -F 0.25 && printf "\n"
}

function log_info {
  echo -e "\e[96m>\e[0m $1"
}

function log_err {
  echo -e "\e[91m[!]\e[0m $1"
}

function log_warn {
  echo -e "\e[93m[~]\e[0m $1"
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

  # Print the splash text
  print_splash

  # Get the install descriptor
  log_info "Getting install descriptor..."

  # Check if repository is not null
  if [[ "$repository" = "" ]]; then
    log_err "Please specify a repository to install."
    log_err "To view the syntax, type \e[90mshebang\e[0m."
    exit 0
  fi

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
    executable=$(echo $descriptor | jq -r ".properties.executable")
    symlink_name=$(echo $descriptor | jq -r ".properties.symlink_name")
    before_install_script=$(echo $descriptor | jq -r ".properties.commands.before_install")
    after_install_script=$(echo $descriptor | jq -r ".properties.commands.after_install")

    # Print the package information
    log_info "Installing $name (v-$version) ..."

    # Execute the 'before install' command script
    bash -c "$before_install_script"

    # Define the clone path
    clone_path="$home_dir/shebang/packages/$id"

    # Check if package is already installed
    if [[ -d "$clone_path" ]]; then
      log_warn "This package is already installed on your system."
      log_warn "To remove this package, type \e[90mshebang remove \e[92m$id\e[0m."
      exit 0
    fi

    # Clone repository to local files
    log_info "Cloning repository to \e[92m$clone_path\e[0m ..."
    git clone -q -b "$branch" "https://github.com/$repository" "$clone_path"
    log_info "Done cloning repository."

    # Set executable permissions to all files in package
    old_dir=$(pwd)
    cd "$clone_path"
    sudo chmod -R +x ./*.*
    cd "$old_dir"

    # Create a symlink to the main executable
    log_info "Creating symlink..."
    sudo ln -s "$clone_path/$executable" "/usr/bin/$symlink_name"
    log_info "Done creating symlink."

    # Execute the 'after install' command script
    bash -c "$after_install_script"
  fi
}

function create_shebang {
  echo -e "\e[90mNOTE: A package id is always in the format \e[95mpackage-name\e[90m,"
  echo -e "\e[90mevery character must be lower-case and only dashes and alphanumeric characters"
  echo -e "\e[90mare allowed.\e[0m"

  read -p -r "Package id: " package_id
  read -p -r "Package name: " package_name
  read -p -r "Package version: " package_version

  # Build a JSON skeleton
  json_skeleton=$(echo "{}" |
    jq ".id = \"$package_id\"" |
    jq ".name = \"$package_name\"" |
    jq ".version = \"$package_version\"" |
    jq ".properties.executable = \"\"" |
    jq ".properties.symlink_name = \"\"" |
    jq ".properties.commands.before_install = \"\"" |
    jq ".properties.commands.after_install = \"\""
  )

  # Check if the descriptors directory exists
  if [[ ! -d "$home_dir/shebang/descriptors" ]]; then
    mkdir "$home_dir/shebang/descriptors"
  fi

  descriptor_path="$home_dir/shebang/descriptors/$package_id.json"
  echo "$json_skeleton" > "$descriptor_path"

  # Open a new NANO editor for the descriptor file
  nano -E "$descriptor_path"

  # Print the final package descriptor
  printf "\n" && < "$descriptor_path" jq -C . && printf "\n"
  echo -e "Package descriptor saved to \e[92m$descriptor_path\e[0m."
}

function remove_shebang {
  if [[ "$1" = "" ]]; then
    log_err "You need to specify a package to remove."
    log_err "To remove this package, type \e[90mshebang remove \e[92mpackage-id\e[0m."
    exit 0
  fi

  # Clear the screen
  clear

  # Print the splash text
  print_splash
  log_info "Removing package \e[92m$1\e[0m..."

  # Check if package directory exists
  if [[ ! -d "$home_dir/shebang/packages/$1" ]]; then
    log_err "Package \e[92m$1\e[0m not found, have you spelled it correctly?"
    log_err "There is no package to remove."
    exit 0
  fi

  # Remove the package
  sudo rm -R "$home_dir/shebang/packages/$1"

  # Print success message
  log_info "Package \e[92m$1\e[0m has been removed successfully!"
}

# Check arguments
if [[ "$1" = "install" ]]; then
  install_shebang "$2" "$3"
  exit 0

elif [[ "$1" = "create" ]]; then
  create_shebang
  exit 0

elif [[ "$1" = "remove" ]]; then
  remove_shebang "$2"
  exit 0

elif [[ "$1" = "update" ]]; then
  /usr/share/shebang/install.sh
  exit 0

else
  print_syntax
  exit 0
fi
