# DexBot 
A C# Discord bot for searching for information about pokémon and things related to pokémon (i.e. abilities, moves, items, etc. This is a pretty amatuer project, so don't expect anything crazy, but I like to think I somewhat know what I'm doing so take that how you will.

## Usage
**If you simply want to add the bot to your discord server**, follow [this link](https://discord.com/api/oauth2/authorize?client_id=832840741931843614&permissions=8&scope=bot) and select the server you want to add it to.
*NOTE: You must have the `Manage Server` permission in order to add bots to a server*

**If you want to run it yourself** follow these steps.
1. Download the latest release and extract the .zip folder
1. Go to the [Discord Developer Portal](https://discord.com/developers/applications), and make sure you are ogged in
1. Click the `New Application` button, and name it whatever you want
1. Go to the `Bot` category under `Settings` and click `Add Bot`
1. Click the link that says `Click to Reveal Token` or just click the `Copy` button under the token heading. *IMPORTANT: This token lets an application log in as a specific account. DO NOT SHARE THIS TOKEN. If you do, anyone with access to the token can log in as the bot account from any application, and can take control of the bot. If you ever lose access to this token, be sure to go back to the bot page and click `Regenerate`*
1. In the `Resources` folder, edit the `config.json` file and replace `BOT_TOKEN_GOES_HERE` with your bot token
1. Run the `dexbot.exe` file, and you should be good to go!

To add the bot you're running to your server:
1. Go back to the [developer portal](https://discord.com/developers/applications)
1. Go to the page for your bot, and go to the `OAuth2` category under `Settings`
1. Under the `Scopes` heading, check the `bot` checkbox
1. Under `Bot Permissions` check the `Administrator` box
1. Copy and follow the link that has been generated under `Scopes` and select the server you want to add your bot to

## Commands list
You can find a list of commands [here](https://docs.google.com/document/d/1CvEa6Fp4mHlToc9yQRZRLk2hBGOj5TWleEEyx73gxCU/edit?usp=sharing).

## Issues/Feature Requests
If you have any bugs to report or features to request, post them on the GitHub issue tracker and I might just get to them!

## Changelog
See the [CHANGELOG.md file](https://github.com/Jolkert/DexBot/blob/main/CHANGELOG.md) for verion history.

## License
Released under the [GNU General Public License v3.0](https://www.gnu.org/licenses/gpl-3.0.en.html)