# CS-AdvancedVehicleOptions 1.9.12
Tested & Updated for Hotel&Resorts DLC
* Fixed - Fix for freezing the game at startup (no longer modal windows, when having a window prompt - especially upon start, when the mod informed user, that a specific service does not have a vehicle assigned)
* Fixed - Hotkey was not remembered, now it is
* Change - Importing the configuration will ask now for confirmation

# CS-AdvancedVehicleOptions 1.9.11
Tested & Updated for Hubs&Transports DLC
* Fixed - Minor code fix, e.g. trailer information for public transport is now shown correct, all special capacities have now an indicator*
* Change - Added a notification for vehicles, when it will be required to set the service building vehicle selector to "Random" with linking to the Wiki instructions
* New Italian translation is now available

# CS-AdvancedVehicleOptions 1.9.10
Tested & updated for Financial Districts DLC
* New - New Section for Bank Service / Cash Delivery Van vehicles

# CS-AdvancedVehicleOptions 1.9.9
Service Release only, no new features, tested & updated for Roads & Vehicles DLC
* Bug Fix for Modal Windows supplied by MacSergey

# CS-AdvancedVehicleOptions 1.9.8
* New - Stand Size selection for passenger planes. All passenger planes can now be re-configured to use Small/Medium/Large stands. Be patient, it will take several minutes until the change is applied, as the setting will be active only for new spawned planes. Existing planes in transit will use the already calculated travel path and will ignore the change. At one point in time re-configured planes will dock to the selected stands. Non Airport-DLC users will not see this option. The selection can be reverted anytime to Default value, settings will be stored in the vehicle config.xml and savegame. Note: Service Vehicle Selector (SVS2) will not show the Passenger Planes in the new selected Stand Type sizes.
* New - Engine/Trailer synchronization for trains. Trains and theirs trailers can now be synchronized when data options are changed on the engine. Data values supported: Speed, Acceleration, Braking, Turning, Springs, Dampers, Lean and Nod. Choose in Mod Options, if AVO shall remember the setting or if it is one-time change only. Engine/trailer sync is done, as long the checkbox in the mainwindow is ticked.
* Change - New logic to sort trucks/busses/service vehicles and their trailers. C:S has not supported bus trailers upto Airport-DLC. Asset Creators used truck trailer templates, with result that AVO sorts e.g. a bus vehicle to the "Bus" section and the bus trailer to "Industrial Farming" - as the bus trailer template was a "farming trailer". To overcome AVO had a comprehenive list in the past checking SteamIDs. This list is removed and AVO now sorts the trailer depending on the engine classification (eg. Public transport Bus or Fire Service). This will make all vehicle trailers sort 100% correct along with the engine vehicle. Works for all road vehicles.

# CS-AdvancedVehicleOptions 1.9.7
* Fixed - More Vehicle compatibility issue which leads to a simulation freeze by infinite loop
* New - Support for Airports DLCs new Aircraft types
* New - Aircrafts will show the required parking stand type
* New - Hotkey for GUI (ALT+SHIFT+O)
* New - Autosave for Vehicle data to XML (save to XML on each change in any data field), added button in Mod Options
* New - Added button in the Mod Options for easy access to AVO config files (vehicles, settings)
* New - Multilanguage support with algernon's Translation Framework with support for Crowdin (EN and DE language available) 
* Change - New configuration settings file (XML format, new storage path)
* Change - Renamed vehicle settings file 
* Change - Tooltips and label descriptions overhaul

# CS-AdvancedVehicleOptions 1.9.6
* Fixed - incorporated bug fixes by Kryzchu124

# CS-AdvancedVehicleOptions 1.9.5
* Updated Bus Trailer List with +80 more vehicles
* Compability towards NoBigTuck Beta testing (looks for Steam ID)

# CS-AdvancedVehicleOptions 1.9.4

Passenger Helicopter Fix:
The Passenger Helicopter accepts again changes to the passenger capacity (was "read only" before)

# CS-AdvancedVehicleOptions 1.9.3

Vehicle Configuration:
* New - Button for Reset full configuration / all customized settings.
* Enhanced - Export now reports back, where the configuration file is located.
* Fixed - Savegames created with older versions of AVO will no longer fail with error message when loading the vehicle configuration

Vehicle Settings
* Changed - Section for Configuration now called "Actions for Vehicle Configuration".
* Changed - Section "Remove Vehicles" now called "Actions For + Vehicle Type".
* Changed - "Restore Default" is now "Default Values" for a vehicle type. Moved down to Remove Driving & Parked section.
* Changed - Capacity description label will be adapted to what kind of vehicle it is, eg. Public transport will show Passenger while Industry shows Cargo and Postal Service reflects Mail.
* New - Option for display speed units Miles per Hour or Kilometer per Hour.
* Fixed - Full support for Fishing Industry.
* Enhanced - Support for enhanced vehicle properties (eg. pumping rate, travel distance, criminal rate, fishing rate and more).
* Changed - AVO will no longer allow to disable "Bus trailers" from the trailer itself. Reason for that is that the bus trailer is a re-assigned industrial vehicle. However, disabling the main vehicle will disable also the trailer.

Vehicle Locator & Remover
* Enhanced - When pressing SHIFT on the location tag, AVO will directly zoom to the next available vehicle. If SHIFT is not pressed, it will only center to the next available vehicle.
* New - Remove the previously located vehicle.

Mod Compatibility
* Change - AVO will no longer block setting changes, if Improved Public Transport or Transport Lines Manager are present. Instead it will color shared fields in red to warn the user, that this may result in compatibility issues.
* Change - Settings for warnings (triggered by Improved Public Transport and Transport Lines Manager) activate/deactivate the red color coding in shared data fields.
* Removed - Checkbox Options for Improved Public Transport and Transport Lines Manager have been removed.

Vehicle Spawning
* New - Button for accessing directly the Cities Skylines Transport Line Overview panel instead informing the user that "Spawning is controlled by Cities Skylines, IPT or TLM". User can now directly open the Line Overview and change the desired vehicle.
* New - Small help button sending the user to a new section for Spawning Explanation.

Mod Option Panel
* Changed - Compatibility settings in the Mod Option Panel will be only displayed, if the related mod is installed and active (Vehicle Color Extender, IsBigVehicle).

Experimental Mod Support
* New - Mod support NoBigTruck created by MacSergey using the "IsLargeVehicle" flag. You need the mod NoBigTruck installed to make this work. If a vehicle is flagged as "Large Vehicle" the IsBigTruck mod should not send this vehicle to small buildings. Please contact MacSergey for any issues, AVO only sets the flag. Subscribe to the NoBigTruck mod [here](https://steamcommunity.com/sharedfiles/filedetails/?id=2069057130).

Trailer Reference List
* References updated 

# CS-AdvancedVehicleOptions 1.9.2

Change the Mod Option Panel
- Compatibility : If TLM is present and active, disable some vehicle parameters; can be turned off, default setting: ON
- Compatibility : If IPT is present and active, disable some vehicle parameters; can be turned off, default setting: ON

Note: Disabling the option will make the values for editing available, however, the Spawn Control for Bus, Trolley Bus, Metro, Tram and Monorail is still under control of the game, TLM or IPT. The vehicles must be configured in the respective transport line managers.

Lean Modifier and Nod Mulltiplier are not saved if zero or negative value. Now remembers and loads values properly (eg. for trains or helicopters).

Trolley Bus to show next to Bus and Intercity Bus

# CS-AdvancedVehicleOptions 1.9.1
This version is 1.9.1, tested in a limited environment only. Adds compatibility patches to the game (vehicle spawning), ITP/TLM (vehicle spawning, vehicle parameters), Vehicle Color Expander (coloring parameters).

Adds compatibility patches to the game (vehicle spawning), ITP/TLM (vehicle spawning, vehicle parameters), Vehicle Color Expander (coloring parameters).

Adds compatibility for game controlled vehicle spawning (Hide Spawn option for game controlled vehicles (eg bus); if vehicle spawning is controlled by game, the vehicle will always be turned on (even if it was disabled before)

New Options are visible in the Mod Option panel of Cities Skylines.
 - Game Balancing : Hide capacity value for vehicles without passenger/cargo capacity; can be turned off, default setting: ON
 - Compatibility : If Color Vehicle Expander is present and active, disable Coloring; cannot be turned off, default setting: ON
 - Compatibility : If TLM is present and active, disable some vehicle parameters; cannot be turned off, default setting: ON
 - Compatibility : If IPT is present and active, disable some vehicle parameters; cannot be turned off, default setting: ON

New Category for Intercity Bus

Support for categorizing properly Bus trailers. SteamID config file 09-Apr-2020 for 1.9.1.

Hiding GUI works again

Added Mod Option Panel links to Wiki and Log directory 

# CS-AdvancedVehicleOptions 1.9.0

This version is 1.9.0, tested in a limited environment only. Last changes include a new logo (yeah) und the change of the namespace for the mod. This should stop annyoing messages when user are still subscribed to the original one. 

- Issue on load: Still error messages when opening an existing savegame (one time only due to exception on loading values)
- Added new vehicle categories for new DLC Sunset Harbor
- Compatibility towards More Vehicles, no more cargo train issues

# CS-AdvancedVehicleOptions

This version is 1.8.7, incorporating airatin's updates and additional changes intended to make this mod compatible with More Vechicles by @dymanoid, most importantly using ```Array16<T>::m_buffer.Length``` instead of ```Array16<T>::m_size``` to iterate over the filled length of ```VehicleManager.instance.m_vehicles``` and ```VehicleManager.instance.m_parkedVehicles``` to circumvent the use of preset vehicle limits.

# Testing

I've seen a normal frequency of functional cargo trains in multiple cities of different sizes while using this version locally with More Vehicles and many other mods (to be updated), but please test further before incorporating into the workshop version.
