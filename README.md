# Shebang

[![Join the chat at https://gitter.im/shebang-package-manager/Lobby](https://badges.gitter.im/shebang-package-manager/Lobby.svg)](https://gitter.im/shebang-package-manager/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

[![Build Status](https://travis-ci.org/keesvv/shebang.svg?branch=master)](https://travis-ci.org/keesvv/shebang)
[![codecov](https://codecov.io/gh/keesvv/shebang/branch/master/graph/badge.svg)](https://codecov.io/gh/keesvv/shebang)

📦 A package manager for GitHub repositories.

# Installation
> Note: You need to have root privileges for installing Shebang.

Open your terminal application and paste in the following:

`bash <(curl -fsSL "https://raw.githubusercontent.com/keesvv/shebang/master/install.sh")`

That's it! Now you're ready to roll. Type `shebang` in your terminal to see the syntax and all subcommands.
If you have any issues with installing Shebang, please create a new issue on the Issues page. Before you submit an issue, please check if a similar question have been asked before you ask the same question.

# Creating a package

If you would like to create your own package, first you need to have the latest version of Shebang installed on your system. To create a package, open your terminal and type `shebang create`. You will be prompted for which package information to use and when the process completes, you can find your created package under `/home/USERNAME/shebang/descriptors`.

If you want to manually create a package descriptor, you can download the skeleton file at the base of the repository, called `shebang.json`. After you've downloaded the skeleton file, modify it as you like and upload it to your GitHub repository.

# Publishing a package
If you have successfully followed the steps from **Creating a package**, you can publish your package by following these simple steps. First off, navigate to `/home/USERNAME/shebang/descriptors`. Pick the JSON file you've created with `shebang create` or by creating it manually and push it to the top of your GitHub repository. Other branches than `master` are supported, so you can also upload the file to the top of another branch. If you choose to push the file to a branch that differs from the `master` branch, you need to instruct the users that want to install your package to use `shebang install your/repository <branch>` instead of just `shebang install your/repository`.
