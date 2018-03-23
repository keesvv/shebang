#!/usr/bin/env bash
echo -e "\e[92mInstalling Shebang\e[0m..."

# Install all dependencies
sudo apt install -y git jq nano toilet lolcat

# Check if a copy of Shebang is installed on the system
if [[ -d "/usr/share/shebang" ]]; then
  echo -e "\e[93mOther version of Shebang detected, deleting old one...\e[0m..."
  sudo rm -R "/usr/share/shebang"
fi

# Clone Shebang to the local system
sudo git clone "https://github.com/keesvv/shebang" "/usr/share/shebang"

# Modify executable permissions and make symlink
cd "/usr/share/shebang"
sudo chmod -R +x *.sh
sudo ln -s "/usr/share/shebang/shebang.sh" "/usr/bin/shebang"

# Print the install message
printf "\n\a" && echo "INSTALLED!" | toilet -f pagga --filter border
