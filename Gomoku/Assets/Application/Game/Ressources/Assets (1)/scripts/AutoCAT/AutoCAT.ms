--***************************************************************
--* AutoCAT.ms
--* Version 2.5.2
--* This file is part of the Mixamo Auto-CAT script.
--* Copyright Mixamo www.mixamo.com 2011-2014
--* Concept by Mixamo, script created by Ofer Zelichover for Mixamo www.mixamo.com
--* Script updates by Dan Babcock
--*
--* This is the main script file.
--* 
--* 
--***************************************************************
--* Changes in 2.5.2
--*     -Refined CAT mapping orientation to better align with CAT standards
--*	    -Script handles arm behavior change for 3ds Max 2015+
--*     -Now works with CAT Digit Modifiers
--***************************************************************
--* Changes in 2.5.1
--*     -Added Mapping preset file for animations
--*     -Refined pre-2012 popup behavior
--***************************************************************
--* Changes in 2.5
--*     -Blendshapes now persist through creation
--*     -Script names no longer conflict with AutoBiped scripts
--*     -Script now matches both normal autorig and SkeLOD naming conventions
--*     -Changed Handling of Skin Modifiers to get around Max2014 bug
--***************************************************************

try(destroyDialog ro_mixamoAutoCAT)catch()

rollout ro_mixamoAutoCAT "Auto-CAT 2.5.2"
(
	
	-- Forward declerations
	---------------------------------------------------------------------
	local nameMapping
	local isSkinnedMesh, isCATObj
	
	
	
	
	-- Local struct declerations
	-------------------------------------------------------------------
	local thisScriptPath = getFilenamePath (getThisScriptFilename())
	local s_AutoCAT_NameMapping = fileIn (thisScriptPath + "AutoCAT_NameMapping.ms")
	local s_AutoCAT_TransferSkinWeights = fileIn (thisScriptPath + "AutoCAT_transferSkinWeights.ms")
	local s_AutoCAT_Manipulation = fileIn (thisScriptPath + "AutoCAT_Manipulation.ms")
	
	
	

	-- Local variable declerations
	---------------------------------------------------------------------
	local nameMapping = s_AutoCAT_NameMapping()
	local CATManip = s_AutoCAT_Manipulation()
	local transSkinWeights = s_AutoCAT_TransferSkinWeights()
	local mappingFileName = thisScriptPath + "AutoCAT_name_mapping.csv"
	local mappingFileName0Fingers = thisScriptPath + "AutoCAT_name_mapping_0fingers.csv"
	local mappingFileName1Fingers = thisScriptPath + "AutoCAT_name_mapping_1fingers.csv"
	local mappingFileName2Fingers = thisScriptPath + "AutoCAT_name_mapping_2fingers.csv"
	local mappingFileName3FingersAutorig = thisScriptPath + "AutoCAT_name_mapping_3fingers_Autorig.csv"
	local mappingFileName3FingersSkelod = thisScriptPath + "AutoCAT_name_mapping_3fingers_Skelod.csv"
	local useMappingFile = mappingFileName
	
	

	
	
	-- User Interface
	---------------------------------------------------------------------
	group "" (
		label lblAbout01 "Mixamo - AutoCAT" align:#center offset:[0,-5]
		hyperLink lnkAbout01 "www.mixamo.com" color:Black hoverColor:Blue visitedColor:Black address:"http://www.mixamo.com" align:#center offset:[0,5]
		label lblAbout02 "Only works with Characters" align:#center 
		label lblAbout03 "Auto-rigged using Mixamo" align:#center 
		label lblBlank "" align:#center 
		label lblMaxVer "Please check for updates" align:#center
		hyperLink lnkUpdate01 "3ds Max Update Link" color:Black hoverColor:Blue visitedColor:Black address:"http://usa.autodesk.com/adsk/servlet/ps/dl/index?id=2334435&linkID=9241178&siteID=123112" align:#center
		hyperLink lnkUpdate02 "FBX Plugin Update Link" color:Black hoverColor:Blue visitedColor:Black address:"http://usa.autodesk.com/adsk/servlet/pc/item?siteID=123112&id=22694994" align:#center
	)
	button bnCreateCAT "Create CAT" width:154 height:23 offset:[0,5]
	
	
	
	
	-- Functions
	---------------------------------------------------------------------
	
	-- Filter functions
	fn isCATObj obj = isKindOf obj CATParent
	fn isSkinnedMesh obj = 
	(
		local returnVal = false
		if isKindOf obj GeometryClass then (
			for m in obj.modifiers while not returnVal where isKindOf m skin do
				returnVal = true
		)
		returnVal
	)
	
	
	-- Returns true if this is an auto-rigged character, false otherwise.
	fn isAutoRiggedCharacter =
	(
-- 		return (rbCharType.state == 1)
		return true
	)
	
	
	-- Returns a negative value of the given euler angles
	fn getNegativeAngle ang =
	(
		ang.x = -ang.x
		ang.y = -ang.y
		ang.z = -ang.z
		
		ang
	)

	
	-- Returns the lowest z pos of the skinned geometry objects.
	fn getGeometryZOffset =
	(
		local minZPos = undefined
		-- Get all skin modifiers
		local skinMods = getClassInstances skin
		-- Loop through the skin modifier
		for s in skinMods do (
			-- Get the objects using the skin modifier
			local objs = refs.dependentNodes s
			-- Loop objs and find the object with the lowest z pos
			for o in objs do (
				if minZPos == undefined or o.min.z < minZPos then
					minZPos = o.min.z
			)
		)
		-- Return the minimum value
		return minZPos
	)
	
	
	
	-- Aligns the CAT bone given in the mappedName to the matching mixamo bone.
	mapped fn alignCATBoneToMixamoBone mappedName CATParentObj mixamoCharName =
	(
		-- Get the CAT bone object
		local CATNode = CATParentObj.GetBoneByAddress mappedName.CATName
		
		if isValidNode CATNode then (
			-- Get the mixamo bone object
			local targetBone = nameMapping.getMixamoNodeByObj CATNode mixamoCharName
			
			if isValidNode targetBone then (
				print (mixamoCharName + "        " + mappedName.CATName)
				if mappedName.doDirection and targetBone.children.count == 1 then (
					local dir = normalize (targetBone.transform.pos - targetBone.children[1].transform.pos)
					local tm = CATManip.getMatrixFromDir dir
					
					tm.pos = targetBone.transform.pos
					
					local doRotation = true
				) else (
					-- Get the target bone's transform matrix, and transform it using the conversionTM of the mapped item
					local tm = mappedName.conversionTM * targetBone.transform
					local doRotation = mappedName.doRotation
				)
				
				--The behavior of arms changed in 2015, need to set the length of the forearms manually
				if (mappedName.CATName == "SceneRootNode.Hub.Spine[0].Hub.Limb[0].LimbBone[1].BoneSeg[0]") or (mappedName.CATName == "SceneRootNode.Hub.Spine[0].Hub.Limb[1].LimbBone[1].BoneSeg[0]") or  (mappedName.CATName == "SceneRootNode.Hub.Spine[0].Hub.Limb[0].LimbBone[0].BoneSeg[0]")  or (mappedName.CATName == "SceneRootNode.Hub.Spine[0].Hub.Limb[1].LimbBone[0].BoneSeg[0]")  then (
					pos1 = targetBone.children[1].transform.pos
					pos2 = targetBone.transform.pos
					dist = distance pos1 pos2
					unitsRatio = CATParentObj.CATUnits
					cat_bone = CATParentObj.GetBoneByAddress mappedName.CATName
					cat_bone.controller.length = (dist/unitsRatio)
					forceCompleteRedraw()
				)
				-- Set the new transform matrix to the CAT bone
				CATManip.setCATBoneTM CATNode tm mappedName.doPosition doRotation
			) else
format "Could not find a mixamo rig match for %\n" CATNode
		) else
format "Could not find a CAT rig match for %\n" mappedName.CATName
	)
	
	
	-- Aligns the the CAT and mixamo character bones.
	-- It first moves the mixamo character to the same position as the CAT,
	-- then tries to match the CAT bones as closly as it can to the mixamo
	-- character's bones. Finally, it matches the mixamo character bones to 
	-- the CAT bones.
	fn alignBones CATParentObj mixamoCharName =
	(
		-- Get CAT Pelvis
		local catPelvis = CATParentObj.GetBoneByAddress "SceneRootNode.Hub"
		-- Get the mixamo root
		local mixamoRoot = nameMapping.getMixamoNodeByObj catPelvis mixamoCharName
		
		-- Move the mixamo root so that the lowest verts are on z=0 and the character is centered
		-- on x=0 and y=0
		mixamoRoot.pos = [0,0,0]
		forceCompleteRedraw()
		local zPosDiff = getGeometryZOffset()
		mixamoRoot.pos.z -= zPosDiff
		
		-- Rotate the cat parent to match the mixamo rig
		rotate CATParentObj (eulerAngles 0 0 180)
		-- Move the cat parent to match the mixamo rig
		CATParentObj.pos = mixamoRoot.transform.pos
		CATParentObj.pos.z = 0
		
		
		-- Prepare the CAT rig for manipulation
		CATManip.prepareCATForManipulation CATParentObj
		
		-- Make the legs and arms very long.This solves an issue when the parent bone moves when 
		-- the child bone is moved if it is not sufficiently long.
		CATManip.prepareCATLimbsForManipulation CATParentObj
		
		
		-- Loop through all the CAT rig bones in the mapping list and align them.
		alignCATBoneToMixamoBone nameMapping.mappingList CATParentObj mixamoCharName
		
		/*
		-- Set the length of palm objects (hands and feet)
		for o in CATParentObj.catRigNodes where isKindOf o.controller PalmTrans do
			CATManip.changeFootLengthByToes o
		
		-- Get the CAT out of set mode
-- 		CATManip.finishCATManipulation CATParentObj */
	)
	
	
	
	-- Returns an approximate height of the mixamo character (it is only approximate because that character is not in T-pose).
	fn getCharacterHeight mixamoCharName =
	(
		-- Get the character objects
		local objs = execute ("$'" + mixamoCharName + "*'")
		
		-- Set the boxsize of bones to a small value to get a the right size
		-- This is a hack so solve the issue of really small characters where a box size
		-- of [1,1,1] (the default) is large enough compared to the character height to offset
		-- the height and give a false result.
		for o in objs where isKindOf o dummy and isProperty o #boxSize do
			o.boxSize = [0.01, 0.01, 0.01]
		
		-- Get the bounding box of objs
		local bbox = objs.max - objs.min
		
		-- Get the biggest value between the y and z axes of the bounding box
		local height = amax bbox.y bbox.z
		
		-- Return the height
		return height
	)
	
	
	
	-- Returns the number of fingers in the mixamo character
	fn getNumberFingers mixamoCharName hand=
	(
		-- Get a hand object
		if hand == "Left" then(
			local handObj = getNodeByName (mixamoCharName + ":LeftHand")
			if isValidNode handObj then (
				return handObj.children.count
			)
			else 	-- otherwise, just return 0 (fingers)
				return 0
		)
		if hand == "Right" then(
			local handObj = getNodeByName (mixamoCharName + ":RightHand")
			if isValidNode handObj then
				return handObj.children.count
			else 	-- otherwise, just return 0 (fingers)
				return 0
		)
	)
	
	
	-- Returns the number of neck bones in the mixamo character
	fn getNumberNeckBones mixamoCharName =
	(
		-- Get the first neck link object
		local neckObj = execute ("$'" + mixamoCharName + ":Neck'")
		
		-- init a neck links counter variable
		local neckLinksCnt = 0
		-- If neckObj is a valid node, then count the neck links. Otherwise, do nothing more.
		if isValidNode neckObj then (
			-- Init a variable the will go through the children.
			local obj = neckObj
			
			-- Define the name match pattern
			local pattern = mixamoCharName + ":Neck*"
			
			-- Init the stopLoop condition
			local stopLoop = false
			
			-- Repeat until the stopLoop conditions are met.
			do (
				-- Increase the links counter by 1
				neckLinksCnt += 1
				
				-- Set the stopLoop var to true
				stopLoop = true
				
				-- If one of obj's children matches the name pattern then set stopLoop to false to keep on going
				for c in obj.children while stopLoop where matchPattern c.name pattern:pattern do (
					stopLoop = false
					obj = c
				)
				
			) while not stopLoop
		)
		
		-- Return the number of neck links
		return neckLinksCnt
	)
	
	
	mapped fn setEndBoneSize chainBone =
	(
		local numBones = chainBone.controller.numBones
		local endBone = chainBone.controller.digit.controller.Bones[numBones]
		endBone = endBone.node
		endBone.length = endBone.width = endBone.depth = 0.001
	)
	
	mapped fn setNumDigitBones firstBoneObj newNumBones =
	(
		firstBoneObj.controller.numBones = newNumBones
		setEndBoneSize firstBoneObj
	)
	
	
	-- Creates a CAT system with the right bones.
	fn CATCreator mixamoCharName =
	(
		-- Create a new CAT rig parent.
		local newCAT = CATParent()
		newCAT.CATUnits = 1.0
		
		--rotate if 2013+
		local versinfo = dotnetclass "System.Diagnostics.FileVersionInfo"
		local myFI = versinfo.GetVersionInfo (pathConfig.appendPath (pathConfig.GetDir #maxroot) "3dsmax.exe")
		local versions = filterString myFI.FileVersion "."
		local maxVersionNum = (versions[1] as integer)
		if maxVersionNum >= 15 then
		(
			newCAT.rotation = eulertoquat (eulerAngles 0 90 0)
		)
		
		-- Load the default base human rig.
		local CATRigPresetsDir = (try(getDir #plugcfg_ln) catch(getDir #plugcfg)) + @"\CAT\CATRigs\"
		newCAT.LoadRig (CATRigPresetsDir + "Base Human.rig")
		
		
		-- Add fingers. We add 4 finger bones (instead of 3) for easy aligning, and to use as end bone.
		local numFingersLeft = getNumberFingers mixamoCharName "Left"
		local numFingersRight = getNumberFingers mixamoCharName "Right"
		local palm1 = newCAT.GetBoneByAddress "SceneRootNode.Hub.Spine[0].Hub.Limb[0].Palm"
		palm1.controller.numDigits = numFingersLeft
		setNumDigitBones palm1.children 4
		local palm2 = newCAT.GetBoneByAddress "SceneRootNode.Hub.Spine[0].Hub.Limb[1].Palm"
		palm2.controller.numDigits = numFingersRight
		setNumDigitBones palm2.children 4
		
		-- Add toes. We add 2 toe bones (instead of 1) for easy aligning, and to use as end bone
		local foot1 = newCAT.GetBoneByAddress "SceneRootNode.Hub.Limb[0].Palm"
		foot1.controller.numDigits = 1
		setNumDigitBones foot1.children 2
		if isProperty foot1.controller.limb.Palm #LayerTargetAlign then
			foot1.controller.limb.Palm.LayerTargetAlign = 1.0
		local foot2 = newCAT.GetBoneByAddress "SceneRootNode.Hub.Limb[1].Palm"
		foot2.controller.numDigits = 1
		setNumDigitBones foot2.children 2
		if isProperty foot2.controller.limb.Palm #LayerTargetAlign then
			foot2.controller.limb.Palm.LayerTargetAlign = 1.0
		
		-- Set number of spine bones
		local spine1 = newCAT.GetBoneByAddress "SceneRootNode.Hub.Spine[0].SpineLink[0]"
		spine1.controller.SpineData.controller.NumBones = 3
		
		-- Set number of neck bones
		local numNeckBones = getNumberNeckBones mixamoCharName
		local neck1 = newCAT.GetBoneByAddress "SceneRootNode.Hub.Spine[0].Hub.Spine[0].SpineLink[0]"
		neck1.controller.SpineData.controller.NumBones = numNeckBones
		
		-- Add end bone for the head
		local head = newCAT.GetBoneByAddress "SceneRootNode.Hub.Spine[0].Hub.Spine[0].Hub"
		local headEndBone = head.controller.addArbBone()
		headEndBone = headEndBone.node
		headEndBone.length = headEndBone.width = headEndBone.depth = 0.001
		
		
		-- Set the CAT rig units
		local mixamoCharHeight = getCharacterHeight mixamoCharName
		local CATHeight = getCharacterHeight newCAT.CATName
		newCAT.CATUnits = mixamoCharHeight / CATHeight
		
		-- Rename fingers and toes
		for o in newCAT.CATRigNodes where isProperty o.controller #Digit and matchPattern o.controller.Digit.controller.name pattern:"Digit*" do (
			local cont = o.controller.Digit.controller
			if cont.Palm.Limb.isLeg then
				cont.name = substituteString cont.name "Digit" "Toe"
			else
				cont.name = substituteString cont.name "Digit" "Finger"
		)

		
		-- Return the new CAT.
		return newCAT
	)
	
	
	
	-- Removes the extra fingers and toes segments.
	fn setSizeOfEndBones catParentObj =
	(
		-- Remove fingers segments
		local palm1 = catParentObj.GetBoneByAddress "SceneRootNode.Hub.Spine[0].Hub.Limb[0].Palm"
		setEndBoneSize palm1.children
		local palm2 = catParentObj.GetBoneByAddress "SceneRootNode.Hub.Spine[0].Hub.Limb[1].Palm"
		setEndBoneSize palm2.children
		
		-- Remove toes segments
		local foot1 = catParentObj.GetBoneByAddress "SceneRootNode.Hub.Limb[0].Palm"
		setEndBoneSize foot1.children
		local foot2 = catParentObj.GetBoneByAddress "SceneRootNode.Hub.Limb[1].Palm"
		setEndBoneSize foot2.children
	)
	
	
	-- Return the first mixamo character name found in the scene.
	fn getMixamoCharName =
	(
		local charName = undefined
		for o in helpers while charName == undefined where matchPattern o.name pattern:"*:*" do (
			local parts = filterString o.name ":" splitEmptyTokens:true
			local tempName = subString o.name 1 (o.name.count - (parts[parts.count].count + 1))
			if tempName != "" then
				charName = tempName
		)
		return charName
	)
	
	
	
	-- Returns the first hips object that has more than 0 children found in the scene.
	fn getHipsObject =
	(
		-- Get all objects named "hips"
		local objs = getNodeByName "hips" all:true
		-- Loop through the objects and return the first one that has children
		for o in objs where o.children.count > 0 do
			return o
		
		-- If no hips object with children was found, return undefined
		return undefined
	)
	
	
	-- Adds a prefix recursively to all children of baseObj.
	fn addNamespacePrefix baseObj prefix =
	(
		-- Add a prefix to the baseObj name
		if isValidNode baseObj then
			baseObj.name = prefix + baseObj.name
		
		-- Loop through baseObj's children and run addNamespacePrefix for each one
		for c in baseObj.children do
			addNamespacePrefix c prefix
	)
	
	
	-- Removes a prefix from all objects with that prefix.
	fn removeNamespacePrefix prefix =
	(
		-- Define a pattern to match
		local pattern = prefix + "*"
		-- Loop through all the objects that match the name pattern
		for o in objects where matchPattern o.name pattern:pattern do (
			-- Remove prefix
			o.name = subString o.name (prefix.count + 1) -1
		)
	)
	
	
	-- Bends the character's knee in the given amount.
	fn bendKnee upperLegObj angDiff axis =
	(
		-- Get the upper leg child object (the knee object)
		local childObj = upperLegObj.children[1]
		-- Get the current position of the knee object
		local childOldPos = childObj.transform.pos
		-- Create an angle for rotation.
		local angOffset = eulerAngles 0 0 0
		setProperty angOffset axis angDiff
		-- Rotate the upper leg 
		upperLegObj.transform = preRotate upperLegObj.transform angOffset
		-- Rotate the knee bone in a negative total angle.
		angOffset = eulerAngles 0 0 0
		setProperty angOffset axis -angDiff
		childObj.transform = preRotate childObj.transform angOffset
		
		-- Return the total rotation angle.
		angDiff
	)
	
	
	
	-- Bends the character's arm in the given amount.
	-- Returns the angle that was rotated
	fn bendArm upperArmObj ang axis =
	(
		local lowerArmObj = upperArmObj.children[1]
		
		-- Check if the angle beween the upper and lower arm is smaller than ang
		local angDiff = (upperArmObj.transform.rotation - lowerArmObj.transform.rotation).angle
		if angDiff > 180 then
			angDiff = 360 - angDiff
		if (abs angDiff) < (abs ang) then (
			-- Set the rotation angle on the right axis
			local rotAng = eulerAngles 0 0 0
			if ang < 0 then
				setProperty rotAng axis (ang + (abs angDiff))
			else
				setProperty rotAng axis (ang - (abs angDiff))
			
			-- Rotate the lower arm
			lowerArmObj.transform = preRotate lowerArmObj.transform rotAng
			
			return rotAng
		) else
			return (eulerAngles 0 0 0)
	)
	
	
	-- Select the right name mapping file for the character.
	fn getMappingFileNameToUse mixamoCharName =
	(
		-- Get the number of fingers
		local numFingers = getNumberFingers mixamoCharName "Left"
		
		useMappingFile = mappingFileName
		if(numFingers == 0) then
			useMappingFile = mappingFileName0Fingers
		else if(numFingers == 1) then
			useMappingFile = mappingFileName1Fingers
		else if(numFingers == 2) then
			useMappingFile = mappingFileName2Fingers
		else if(numFingers == 3) then(
			--check if mid or ring
			midFinger = getnodebyname (mixamoCharName + ":LeftHandMiddle1")
			if (midFinger == undefined) then (
				useMappingFile = mappingFileName3FingersSkelod
			)
			else (
				useMappingFile = mappingFileName3FingersAutorig
			)
		)
	)
	
	fn hasMorpher obj = 
	(
		local returnVal = false
		if isKindOf obj GeometryClass then (
			for m in obj.modifiers while not returnVal where ((isKindOf m morpher) and (m.name != "AutoCAT Mesh Baker")) do
				returnVal = true
		)
		return returnVal
	)
	
	-- Transfers the skin bone weights
	fn transferSkinBoneWeights CATParentObj altNameMapping: =
	(
		-- Init a variable to determine whether or not to use the alternate name mapping
		local useAltMapping = altNameMapping != unsupplied
		-- Determine what name mapping to use.
		local nameMappingToUse = if useAltMapping then altNameMapping else nameMapping
		-- Loop through the scene objects, and for each one that has a skin modifier, swap the bones.
		for o in objects where isSkinnedMesh o do(
format "Transferring skin weights for %" o
			transSkinWeights.swapSkinBones o CATParentObj nameMapping:nameMappingToUse reparseMapping:(not useAltMapping))
	)
	
	-- rebuild blendshapes after skinning has changed
	fn rebuildBlendshapes meshObj =
	(
		objHasMorpher = hasMorpher meshObj 
		if (objHasMorpher == true) then (
			morpherNode = meshObj.morpher
			curIndex = 1
			isValid = true
			while ((curIndex <= 100) and (isValid == true)) do (
				isValid = WM3_MC_HasTarget morpherNode curIndex
				if (isValid == true) then (
					WM3_MC_SetValue morpherNode curIndex 100.0
					newObj = snapshot meshObj
					newObj.name = WM3_MC_GetName morpherNode curIndex
					WM3_MC_SetValue morpherNode curIndex 0.0
					WM3_MC_Delete morpherNode curIndex
					WM3_MC_BuildFromNode morpherNode curIndex newObj
					delete newObj
				)
				curIndex = curIndex +1
			)
		)
	)
	
	-- Deletes the mixamo rig
	fn deleteMixamoRig hipsObj =
	(
		local rigBones = CATManip.getAllChildren hipsObj
		delete rigBones
	)
	
	
	-- Auto create CAT, align the bones and transfer skin weights.
	fn autoCAT CATObj: =
	(
		-- Set a variable to indicate if we need to remove the namespace prefix from the object 
		-- once we're done.
		local needNamespacePrefixRemoval = false
		
		-- Get the mixamo character name
		local mixamoCharName = getMixamoCharName()
		
		-- If the mixamoCharName is undefined, that means the either there's no character in
		-- the scene, or that the character has no namespace prefix. In this case we'll look
		-- for the hips object of a character.
		if mixamoCharName == undefined then (
			-- Try to get the hips object
			local hipsObj = getHipsObject()
			
			-- If no hips object was found, there's probably no character in the scene,
			-- so, notify the user and exit.
			if not isValidNode hipsObj then (
				messageBox "Couldn't find any character in the scene." title:"AutoCAT"
				return undefined
			)
			
			-- Since there is a hips object, but no namespace prefix, we'll add a temporary
			-- namespace prefix, and also set needNamespacePrefixRemoval to true so we know that
			-- we need to remove this prefix at the end.
			mixamoCharName = "mixamoChar"
			addNamespacePrefix hipsObj (mixamoCharName + ":")
			needNamespacePrefixRemoval = true
		)
		
		
		--Go back to bind pose
		select (for l in geometry collect l)
		meshObjs = selection as array
		for obj in meshObjs do
		(
			obj.assumeSkinPose()
		)
		select (for l in helpers collect l)
		helperObjs = selection as array
		for obj in helperObjs do
		(
			obj.assumeSkinPose()
		)
		
		
		-- Parse the mapping file
		getMappingFileNameToUse mixamoCharName
		nameMapping.mappingFilename = useMappingFile
		nameMapping.parseFile()
		
		
		if CATObj == unsupplied then (
			-- Create the CAT.
			local newCAT = CATCreator mixamoCharName
			
			CATObj = newCAT
		)
		
		-- Bend the mixamo character's knees to avoid issues with CAT's IK
		local mixamoHips = nameMapping.getMixamoNodeByObj CATObj.RootHub.Node mixamoCharName
		local leftCATPlatform = CATObj.GetBoneByAddress "SceneRootNode.Hub.Limb[0].Platform"
		local rightCATPlatform = CATObj.GetBoneByAddress "SceneRootNode.Hub.Limb[1].Platform"
		local leftFoot = nameMapping.getMixamoNodeByObj leftCATPlatform mixamoCharName
		local rightFoot = nameMapping.getMixamoNodeByObj rightCATPlatform mixamoCharName
		-- Store old positions to restore them later
		local oldHipsPos = mixamoHips.transform.pos
		local oldLeftFootPos = leftFoot.transform.pos
		local oldRightFootPos = rightFoot.transform.pos
		-- Bend the knees
		local minAngForIKFix = 15.0
		local ang = if isAutoRiggedCharacter() then minAngForIKFix else -minAngForIKFix
		local legRotAngles = #()
		for leg in mixamoHips.children where matchPattern leg.name pattern:"*Leg*" do (
			local rotAng = bendKnee leg ang #X
			append legRotAngles (dataPair obj:leg ang:rotAng)
		)
		
		-- Bend the mixamo character's arms to avoid issues with CAT's IK
		local armBendAxis = if isAutoRiggedCharacter() then #Z else #Y
		local armBendAng = if isAutoRiggedCharacter() then -25 else 25
		local rightArm = getNodeByName (mixamoCharName + ":RightArm")
		if isValidNode rightArm then
			local rightArmRot = bendArm rightArm armBendAng armBendAxis
		local leftArm = getNodeByName (mixamoCharName + ":LeftArm")
		if isValidNode leftArm then
			local leftArmRot = bendArm leftArm -armBendAng armBendAxis
		
		
		-- Align the bones.
		alignBones CATObj mixamoCharName
		
		-- Set the size of the end bones to be very small
		setSizeOfEndBones CATObj
		
		-- Rebuild Blendshapes
		---------------------------
		select (for m in geometry collect m)
		meshObjs = selection as array
		for obj in meshObjs do
		(
			rebuildBlendshapes obj
		)
		
		
		-- Transfer skin weights
		transferSkinBoneWeights CATObj
		forceCompleteRedraw()
		
		--swap any Mesh Bakers with morphers
		for o in meshObjs do
		(
			--collect all modifiers in the current object
			theMods = for m in o.modifiers collect m     
			for m in theMods where classof m == morpher do 
			(
				if (m.name == "AutoCAT Mesh Baker") then (
					addModifier o m before:theMods.count
					deleteModifier o m
				)
			)
		)
		--Delete any Mesh Bakers
		for o in meshObjs do
		(
			for m = 1 to o.modifiers.count do(
				if ((classof o.modifiers[m] == morpher) and (o.modifiers[m].name == "AutoCAT Mesh Baker")) then (
					modPanel.setCurrentObject o.modifiers[m] 
					maxOps.CollapseNodeTo $ m true
				)
			)
		)
		e
		-- Move the character back to the position before bending the knees and arms
		CATManip.setCATBoneTM CATObj.RootHub.node oldHipsPos true false
		CATManip.setCATBoneTM leftCATPlatform oldLeftFootPos true false
		CATManip.setCATBoneTM rightCATPlatform oldRightFootPos true false
		
		-- Get the CAT out of set mode
		CATManip.finishCATManipulation CATObj
		
		
		-- If needNamespacePrefixRemoval is true, remove the namespace prefix from the objects
		if needNamespacePrefixRemoval then
			removeNamespacePrefix (mixamoCharName + ":")
		
		-- Delete the mixamo rig
		deleteMixamoRig mixamoHips
		
		-- Return the new CAT
		return newCAT 
	)

	
	
	
	
	
	
	-- Event Handlers
	------------------------------------------------------------------------
	on bnCreateCAT pressed do (
		autoCAT()
	)
	
) -- End of ro_mixamoAutoCAT rollout



--Warning mesage about CoCreateInstance Fails
try (destroyDialog ::pre2012popupUI) catch()
rollout pre2012popupUI "AutoCAT VERSION WARNING!!"
(	
	label lblAbout00 ""
	label lblAbout01 "The version of 3ds Max you are using may have issues with portions of this script." align:#center offset:[0,0]
	label lblAbout02"You must either run 3ds Max as Administrator or follow the directions in the link below" align:#center offset:[0,0]
	label lblAbout03"for a one-time permanent fix." align:#center offset:[0,0]
	hyperLink lnkAbout01 "Link to CoCreateInstance fix" color:Black hoverColor:Blue visitedColor:Black address:"http://area.autodesk.com/blogs/maxstation/a7_cocreateinstance_failed" align:#center offset:[0,5]
	button btn1 "Don't Show Again" width:100 height:18
     
    on btn1 pressed do
    (
        skipFileName = getFilenamePath (getThisScriptFilename()) + "DontShowWarning.txt"		
		skipFile = createfile skipFileName
		try(destroyDialog pre2012popupUI)catch()
    )
)

fn existFile fname = (getfiles fname).count != 0

fn pre2012popup =
(
	skipFileName = getFilenamePath (getThisScriptFilename()) + "DontShowWarning.txt"
	if not (existFile skipFileName) then (
		local maxVersionNum = ((maxVersion())[1] / 1000)
		if maxVersionNum <= 14 then
		(
			createDialog pre2012popupUI height:135 width:450
		)
	)
)

createDialog ro_mixamoAutoCAT
pre2012popup()