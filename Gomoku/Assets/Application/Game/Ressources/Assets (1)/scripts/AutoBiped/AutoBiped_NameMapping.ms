--***************************************************************
--* AutoBiped_NameMapping.ms
--* This file is part of the Mixamo Auto Biped script.
--* 
--* This file defines a struct to handle name mapping between
--* the mixamo character objects and the biped objects.
--*
--* Copyright Mixamo www.mixamo.com 2011
--* Concept by Mixamo, script created by Ofer Zelichover for Mixamo www.mixamo.com
--* 
--***************************************************************


(
	global s_AutoBiped_NameMapping
	-- A struct to handle name mapping between Mixamo rig and biped.
	struct s_AutoBiped_NameMapping (
		
		-- Holds an array of mapping items
		mappingList = #(),
		
		mappingFilename = undefined,
		
		
		/**** Parsing methods *****************************************/
		
		-- Returns a 2 item array containg the mixamo character name in the first item
		-- and the mixamo bone name in the second item.
		fn getMixamoBoneNameParts mixamoName =
		(
			-- If the name contains the delimiter character (:), then parse it.
			if matchPattern mixamoName pattern:"*:*" then (
				local nameParts = filterString mixamoName ":" splitEmptyTokens:true
				while nameParts.count < 2 do
					append nameParts ""
				nameParts
			) else ( -- the name doesn't contain the character name.
				#("", mixamoName)
			)
		),
		
		
		
		-- Returns the biped base name based on the names of all the biped bones in the mappingList.
		fn getBipedBaseName =
		(
			-- Get all the biped bone names.
			local bipedNames = for i in mappingList where i.bipedName != "" collect i.bipedName
			-- Define a variable to hold the base name
			local baseName = ""
			
			-- Get the shortest bone name as a starting point.
			local shortestName = bipedNames[1]
			for n in bipedNames where n.count < shortestName.count do
				shortestName = n
			
			-- Loop through the names and find the longest match for all names.
			local matched = true
			for i = 1 to shortestName.count while matched do (
				local pattern = baseName + shortestName[i] + "*"
				for n in bipedNames while matched where not matchPattern n pattern:pattern do
					matched = false
				if matched then
					baseName += shortestName[i]
			)
			baseName
			
		),
		
		
		
		-- Parse a single line in the mapping file and return a mapping item.
		fn parseLine str delimiter:";" =
		(
			-- Filter the string into items using the delimiter.
			local names = filterString str delimiter splitEmptyTokens:true
			
			-- Get the names of the biped bone name
			local bipedName = substituteString names[1] "\"" ""
			-- Get the names of the mixamo bone name (ignoring the character name)
			local mixamoBoneNameParts = getMixamoBoneNameParts names[2]
			local mixamoName = mixamoBoneNameParts[mixamoBoneNameParts.count]
			mixamoName = substituteString mixamoName "\"" ""
			
			-- Create a new datePair item
			local newItem = DataPair mixamoName:mixamoName bipedName:bipedName
			
			-- Return the new item
			return newItem
		),
		
		
		-- Parse the name mapping file and populate the mappingList array.
		fn parseFile filename: =
		(
			if filename != unsupplied then
				mappingFilename = filename
			
			-- Init the mapping list
			mappingList = #()
			
			-- Open the file for reading.
			local f = openFile mappingFilename mode:"r"
			
			-- If f is undefined (meaning the file open failed) then return false.
			if f == undefined then
				return false
			
			-- Read the lines one at a time and parse them.
			while not eof f do (
				-- Read a single line.
				local str = readLine f
				-- If this line is a comment line (starting with the # sign) or empty then skip parsing it.
				if str == "" or matchPattern str pattern:"#*" then
					continue
				-- Parse the string
				local newItem = parseLine str
				
				-- Add the new item to the array
				append mappingList newItem
			)
			
			-- Close the file
			close f
			
			-- Substitute the biped base name with a special name.
			-- Fisrt, find the biped base name.
			local baseName = getBipedBaseName()
			-- Then loop therough the mappingList and replace the base name with a special name ({base_name})
			for i in mappingList do
				i.bipedName = substituteString i.bipedName baseName "{base_name}"
			
			-- Return true on success.
			return true
		),
		
		
		/****  Get name methods *****************************************/
		
		-- Returns the mixamo name based on the biped name
		fn getMixamoName bipedName baseName mixamoCharName: =
		(
			-- Convert the biped name to a name type for faster handling.
-- 			bipedName = (substituteString (toLower bipedName) (toLower baseName) "{base_name}") as name
			-- Collect all the biped names
			local bipedNames = for i in mappingList collect i.bipedName as name
			-- Find the number of the item that has the biped name.
			local n = findItem bipedNames bipedName
			
			-- Return the mixamo name, or undefined if there was no match for the biped name.
			if n > 0 then (
				if mixamoCharName != unsupplied then 
					return (mixamoCharName + ":" + mappingList[n].mixamoName)
				else
					return mappingList[n].mixamoName
			) else
				return undefined
		),
		
		
		-- Returns the mixamo name based on the biped object
		fn getMixamoNameByObj bipedObj mixamoCharName: =
		(
			-- Get the biped base name
			local baseName = bipedObj.controller.rootName
			
			-- Convert the biped name to a name type for faster handling.
			local bipedName = (substituteString (toLower bipedObj.name) (toLower baseName) "{base_name}") as name
			
			-- Return the mixamo name using the getMixamoName function
			return (getMixamoName bipedName baseName mixamoCharName:mixamoCharName)
		),
		
		
		
		-- Returns the biped node based on the mixamo bone name
		fn getMixamoNode bipedName bipedBase mixamoCharName =
		(
			-- Get the mixamo bone name
			local mixamoName = getMixamoName bipedName bipedBase mixamoCharName:mixamoCharName
			
			if mixamoName != undefined then
				return (getNodeByName (mixamoCharName + ":" + mixamoName))
			else
				return undefined
		),
		
		
		-- Returns the biped node based on the mixamo bone name
		fn getMixamoNodeByObj bipedObj mixamoCharName =
		(
			-- Get the mixamo bone name
			local mixamoName = getMixamoNameByObj bipedObj mixamoCharName:mixamoCharName
			
			if mixamoName != undefined then
				return (getNodeByName mixamoName)
			else
				return undefined
		),
		
		
		
		-- Returns the biped name based on the mixamo name
		fn getBipedName mixamoName bipedBaseName: =
		(
			if mixamoName == "" then
				return undefined
			
			-- Make sure the mixamo name doesn't contain the character name
			local mixamoNameParts = getMixamoBoneNameParts mixamoName
			mixamoName = mixamoNameParts[mixamoNameParts.count]
			
			-- Convert the biped name to a name type for faster handling.
			mixamoName = (mixamoName as string) as name
			-- Collect all the biped names
			local mixamoNames = for i in mappingList collect i.mixamoName as name
			-- Find the number of the item that has the biped name.
			local n = findItem mixamoNames mixamoName
			
			-- Return the mixamo name, or undefined if there was no match for the biped name.
			if n > 0 then (
				if isKindOf bipedBaseName string then
					return (substituteString mappingList[n].bipedName "{base_name}" bipedBaseName)
				else
					return mappingList[n].bipedName
			) else
				return undefined
		),
		
		
		-- Returns the biped node based on the mixamo bone name
		fn getBipedNode mixamoName bipedBase =
		(
			if isValidNode bipedBase then
				bipedBase = bipedBase.controller.rootName
			
			-- Get the biped bone name
			local bipedName = getBipedName mixamoName bipedBaseName:bipedBase
			
			if bipedName != undefined then
				return (getNodeByName bipedName)
			else
				return undefined
		)
		
	) -- End of s_AutoBiped_NameMapping

)