# Vault-admin-tools
Admin and Migration Tools for Autodesk® Vault

## What the tools are
The tools provided here are command line utilities that are useful when setting up an Autodesk® Vault, administering it or migrating data into a Vault.

| Utility Name | Purpose |
| ------- | ------- |
| ImportFolderStructure | Import a folder structure into Autodesk Vault and apply categories, lifecycles, permissions and properties |
| ImportPropertyList | Populate existing User Defined Properties with lists of values |
| ImportNamingSchemeList | Import a list of naming scheme strings into a Vault naming scheme including codes and optionally description|
| ImportObjectProperties | Import Properties of Items, Files or Custom Entities into Autodesk Vault. Includes possibility to set Category, Revision and Lifecycle |
| VaultUserAccounts | Imports or Exports users and groups to or from Autodesk Vault |

## How to use
See ./doc/Vault Admin Tools.rtf

**Note that in order to run the tools you will need to copy the following files from the Vault SDK into the directory containing your executable**

* Autodesk.Connectivity.WebServices.WCF.dll
* clmloader.dll

## Versioning
The version numbers for the tool executables correspond to the major version number of Vault they will run against

* Vault 2017 - Version 22
* Vault 2018 - Version 23
* Vault 2019 - Version 24

## License
See [LICENSE.md](LICENSE.md)

## Contributions
Read our contribution guidelines here: [CONTRIBUTE.md](CONTRIBUTE.md)
