--***************************************************************
--* AutoCAT_NameMapping.ms
--* This file is part of the Mixamo Auto CAT script.
--* 
--* This file defines a struct to handle name mapping between
--* the mixamo character objects and the CAT rig objects.
--*
--* Copyright Mixamo www.mixamo.com 2011-2014
--* Concept by Mixamo, script created by Ofer Zelichover for Mixamo www.mixamo.com
--* Script updates by Dan Babcock
--* 
--***************************************************************


(
	-- A struct for holding a mapped name item.
	struct s_AutoCAT_MappedItem (
		mixamoName, 
		CATName, 
		doPosition = false,
		doRotation = false,
		doDirection = false,
		conversionTM = matrix3 1
	) -- End of struct s_AutoCAT_MappedItem
	
	
	-- A struct to handle name mapping between Mixamo rig and CAT rig.
	struct s_AutoCAT_NameMapping (
		
		-- Holds an array of mapping itmes
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
		
		
		
		-- Returns the CAT base name based on the names of all the CAT bones in the mappingList.
		fn getCATBaseName =
		(
			-- Get all the CAT bone names.
			local CATNames = for i in mappingList where i.CATName != "" collect i.CATName
			-- Define a variable to hold the base name
			local baseName = ""
			
			-- Get the shortest bone name as a starting point.
			local shortestName = CATNames[1]
			for n in CATNames where n.count < shortestName.count do
				shortestName = n
			
			-- Loop through the names and find the longest match for all names.
			local matched = true
			for i = 1 to shortestName.count while matched do (
				local pattern = baseName + shortestName[i] + "*"
				for n in CATNames while matched where not matchPattern n pattern:pattern do 
					matched = false
				if matched then
					baseName += shortestName[i]
			)
			return baseName
		),
		
		
		
		-- Parse a single line in the mapping file and return a mapping item.
		fn parseLine str delimiter:";" =
		(
			-- Filter the string into items using the delimiter.
			local names = filterString str delimiter splitEmptyTokens:true
			
			-- Get the names of the CAT bone name
			local CATName = substituteString names[1] "\"" ""
			-- Get the names of the mixamo bone name (ignoring the character name)
			local mixamoBoneNameParts = getMixamoBoneNameParts names[2]
			local mixamoName = mixamoBoneNameParts[mixamoBoneNameParts.count]
			mixamoName = substituteString mixamoName "\"" ""
			
			-- Create a new mapped item
			local newItem = s_AutoCAT_MappedItem mixamoName:mixamoName CATName:CATName
			
			-- Set the transform modes for this bone
			newItem.doPosition = matchPattern names[3] pattern:"*P*"
			newItem.doRotation = matchPattern names[3] pattern:"*R*"
			newItem.doDirection = matchPattern names[3] pattern:"*D*"
			
			-- Set the conversion transform matrix
			newItem.conversionTM = execute names[4]
			
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
			
			-- Return true on success.
			return true
		),
		


		/**** Get mapping item methods **********************************/
		
		-- Returns the mapping item based on the given CAT address
		fn getMappingItemByCatName catName =
		(
			-- Convert the CAT name to a name type for faster handling.
			CATName = CATName as name
			-- Collect all the CAT names
			local CATNames = for i in mappingList collect i.CATName as name
			-- Find the number of the item that has the CAT name.
			local n = findItem CATNames CATName
			
			-- Return the mapping item, or undefined if there was no match for the CAT name.
			if n > 0 then (
				return mappingList[n]
			) else
				return undefined
		),
		
		
		-- Returns the mapping item based on the given CAT object
		fn getMappingItemByCatObj catObj =
		(
			-- Convert the CAT name to a name type for faster handling.
			if isProperty CATObj.controller #Address then (
				local CATName = CATObj.controller.Address as name
				
				return (getMappingItemByCatName CATName)
			) else
				return undefined
		),
		
		
		-- Returns the mapping item based on the given mixamo name
		fn getMappingItemByMixamoName mixamoName  =
		(
			-- Get the mixamo bone name
			local mixamoBoneNameParts = getMixamoBoneNameParts mixamoName
			mixamoName = mixamoBoneNameParts[mixamoBoneNameParts.count] as name
			
			-- Collect all the mixamo names
			local mixamoNames = for i in mappingList collect i.mixamoName as name
			-- Find the number of the item that has the CAT name.
			local n = findItem mixamoNames mixamoName
			
			-- Return the mapping item, or undefined if there was no match for the CAT name.
			if n > 0 then (
				return mappingList[n]
			) else
				return undefined
		),
		
		
		-- Returns the mapping item based on the given mixamo object
		fn getMappingItemByMixamoObj mixamoObj =
		(
			return (getMappingItemByMixamoName mixamoObj.name)
		),
		
		
		
		
		/****  Get name methods *****************************************/
		
		-- Returns the mixamo name based on the CAT name
		fn getMixamoName CATName mixamoCharName: =
		(
			-- Get the mapping item
			local mappingItem = getMappingItemByCatName CATName
			
			-- Return the mixamo name, or undefined if there was no match for the CAT name.
			if mappingItem != undefined then (
				if mixamoCharName != unsupplied then 
					return (mixamoCharName + ":" + mappingItem.mixamoName)
				else
					return mappingItem.mixamoName
			) else
				return undefined
		),
		
		
		-- Returns the mixamo name based on the CAT object
		fn getMixamoNameByObj CATObj mixamoCharName: =
		(
			-- Convert the CAT name to a name type for faster handling.
			if isProperty CATObj.controller #Address then (
				local CATName = CATObj.controller.Address as name
				
				-- Return the mixamo name using the getMixamoName function
				return (getMixamoName CATName mixamoCharName:mixamoCharName)
			) else
				return undefined
		),
		
		
		
		-- Returns the CAT node based on the mixamo bone name
		fn getMixamoNode CATName CATBase mixamoCharName =
		(
			-- Get the mixamo bone name
			local mixamoName = getMixamoName CATName mixamoCharName:mixamoCharName
			
			if mixamoName != undefined then
				return (getNodeByName (mixamoCharName + ":" + mixamoName))
			else
				return undefined
		),
		
		
		-- Returns the CAT node based on the mixamo bone name
		fn getMixamoNodeByObj CATObj mixamoCharName =
		(
			-- Get the mixamo bone name
			local mixamoName = getMixamoNameByObj CATObj mixamoCharName:mixamoCharName
			
			if mixamoName != undefined then
				return (getNodeByName mixamoName)
			else
				return undefined
		),
		
		
		
		-- Returns the CAT name based on the mixamo name
		fn getCATName mixamoName =
		(
			if mixamoName == "" then
				return undefined
			
			-- Make sure the mixamo name doesn't contain the character name
			mixamoName = (getMixamoBoneNameParts mixamoName)[2]
			
			-- Convert the mixamo name to a name type for faster handling.
			mixamoName = (mixamoName as string) as name
			-- Collect all the mixamo names
			local mixamoNames = for i in mappingList collect i.mixamoName as name
			-- Find the number of the item that has the CAT name.
			local n = findItem mixamoNames mixamoName
			
			-- Return the mixamo name, or undefined if there was no match for the CAT name.
			if n > 0 then (
				return mappingList[n].CATName
			) else
				return undefined
		),
		
		
		-- Returns the CAT node based on the mixamo bone name
		fn getCATNode mixamoName CATBase =
		(
			if isValidNode CATBase then (
				if not isKindOf CATBase CATParent then
					CATBase = CATBase.controller.CATParent
			)
			
			-- Get the CAT bone name
			local mixamoBoneNameParts = getMixamoBoneNameParts mixamoName
			mixamoName = mixamoBoneNameParts[mixamoBoneNameParts.count]
			local CATName = getCATName mixamoName
			
			
			if CATName != undefined then
				return CATBase.GetBoneByAddress CATName
			else
				return undefined
		)
		
		
		
	) -- End of s_AutoCAT_NameMapping

)