# DexBot Changelog
## v1.1.2
### Commands
* `move` command now displays a type icon next to the type and a category icon next to the move category

## v1.1.1
### Logging
* Rewrote the Logger to use a queue system to avoid throwing errors when trying to write multiple lines at once to the file. The solution's kinda weird but it works now and that's what matters.

## v1.1
### Commands
* Added `AdminModule`
* Added `stop` command to `AdminModule`. Takes the bot offline; can only be used by bot owner
* `GenericModule` removed
* Moved `help` out of `GenericModule` and into its own module
* Added a `help` subcommand to most commands which gives usage information on the command
* Any alias of any command can be used as a subcommand for `help`. This has the same effect as using `help` as a subcommand for the command

### Logging
* Usernames now include discriminator in logs
* Command errors now log the server and channel they occured in in addition to the user who tried to run the command
* Unknown command errors now also log the message that was sent
* Added a Logger to allow for logging console output to a file
* Logs can be found in `Resources/logs`

## v1.0.1
### Commands
* Added GitHub link to help command

### Cleanup
* Fixed a typo in the searching message (`Serching...` → `Searching...`)
* Minor code readbility change in `help` command

## v1.0
### Commands
* Added `pokemon`, `ability`, `move`, `item`, `type`, `data`, `weakness`, `levelset`, and `help` commands