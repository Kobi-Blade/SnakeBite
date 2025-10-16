# SnakeBite Mod Manager
SnakeBite is a mod manager and launcher for Metal Gear Solid V: The Phantom Pain (PC/Steam). This detached fork introduces critical updates and refactors to improve stability, security, and maintainability.

## 🔧 About This Fork
This version includes:

- Refactored code for improved readability and maintainability.
- Updated framework dependencies and package bindings.
- Patches for high security vulnerabilities present in the upstream source, and TinManTex fork.
- This project requires a full rewrite in .NET to meet modern development standards, but that is currently outside the scope of this fork.

## 🚀 Getting Started
Before running MakeBite, ensure that the `00.dat` and `01.dat` files in `MGSV_TPP\master\0` folder are unmodified. Use Steam “Verify integrity of games files” option if needed.

Upon launching SnakeBite, the setup wizard will guide you through:

1. Selecting your MGSV installation directory.
2. Creating a backup of game data (optional).
3. Preparing game data for modding.

## 📦 Installing Mods

- Mods can be installed/uninstalled via the `Mods` tab.
- Presets allow you to save and restore mod configurations.

## 🛠 Troubleshooting

- **Do not** run SnakeBite as administrator.
- Verify integrity of game files via Steam if mod conflicts arise.
- Check `Log.txt` in the Log directory for error messages.

## 🧪 Developers
Use MakeBite to create .MGSV mod files.
