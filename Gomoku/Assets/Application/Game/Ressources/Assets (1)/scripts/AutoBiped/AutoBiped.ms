--***************************************************************
--* AutoBiped.ms 
--* Version 2.3.1
--* This file is part of the Mixamo Auto-Biped script.
--* Copyright Mixamo www.mixamo.com 2011-2014
--* Concept by Mixamo, script created by Ofer Zelichover for Mixamo www.mixamo.com
--* Script updates by Dan Babcock
--*
--* This is the main script file.
--* 
--* 
--***************************************************************
--* Changes in 2.3.1
--*     -Fix for flipped fingers on .bip animation files
--* Changes in 2.3
--*     -Blendshapes now persist through creation
--*     -Some cases that caused bad skin transfer have been fixed
--*     -Joints should be placed more accurately now
--*     -Script names no longer conflict with AutoCAT scripts
--*     -Script now matches both normal autorig and SkeLOD naming conventions
--*     -Changed Handling of Skin Modifiers to get around Max2014 bug
--***************************************************************

try(destroyDialog ro_mixamoAutoBiped)catch()

rollout ro_mixamoAutoBiped "Auto-Biped 2.3.1"
(
	
	-- Forward declerations
	---------------------------------------------------------------------
	local nameMapping
	local isSkinnedMesh, isBipedObj
	
	
	
	
	-- Local struct declerations
	-------------------------------------------------------------------
	local thisScriptPath = getFilenamePath (getThisScriptFilename())
	local s_AutoBiped_NameMapping = fileIn (thisScriptPath + "AutoBiped_NameMapping.ms")
	local s_AutoBiped_TransferSkinWeights = fileIn (thisScriptPath + "AutoBiped_transferSkinWeights.ms")
	
	
	

	-- Local variable declerations
	---------------------------------------------------------------------
	local nameMapping = s_AutoBiped_NameMapping()
	local transSkinWeights = s_AutoBiped_TransferSkinWeights()
	local mappingFileName = thisScriptPath + "AutoBiped_name_mapping.csv"
	local mappingFileName0Fingers = thisScriptPath + "AutoBiped_name_mapping_0fingers.csv"
	local mappingFileName1Fingers = thisScriptPath + "AutoBiped_name_mapping_1fingers.csv"
	local mappingFileName2Fingers = thisScriptPath + "AutoBiped_name_mapping_2fingers.csv"
	local mappingFileName3FingersAutorig = thisScriptPath + "AutoBiped_name_mapping_3fingers_Autorig.csv"
	local mappingFileName3FingersSkelod = thisScriptPath + "AutoBiped_name_mapping_3fingers_Skelod.csv"
	local useMappingFile = mappingFileName
	
	
	local isAutoRig = true
	
	

	-- These are the biped body parts id's.
	local KEY_LARM 		= 1
	local KEY_RARM 		= 2
	local KEY_LHAND 	= 3
	local KEY_RHAND 	= 4
	local KEY_LLEG 		= 5
	local KEY_RLEG 		= 6
	local KEY_LTOES 	= 7
	local KEY_RTOES 	= 8
	local KEY_SPINE 	= 9
	local KEY_TAIL 		= 10 
	local KEY_HEAD 		= 11					
	local KEY_PELVIS 	= 12		
	local KEY_VERTICAL 	= 13
	local KEY_HORIZONTAL = 14
	local KEY_TURN 		= 15
	local KEY_FOOTPRINTS = 16
	local KEY_NECK 		= 17
	local KEY_PONY1 	= 18
	local KEY_PONY2 	= 19
	local KEY_PROP1 	= 20
	local KEY_PROP2 	= 21
	local KEY_PROP3 	= 22
	local KEY_LFARM 	= 23
	local KEY_RFARM 	= 24


	
	
	
	
	-- User Interface
	---------------------------------------------------------------------
	group "" (
		label lblAbout01 "Mixamo - AutoBiped" align:#center offset:[0,-5]
		hyperLink lnkAbout01 "www.mixamo.com" color:Black hoverColor:Blue visitedColor:Black address:"http://www.mixamo.com/c/auto-rigger" align:#center offset:[0,5]
		label lblAbout02 "Only works with Characters" align:#center 
		label lblAbout03 "Auto-rigged using Mixamo" align:#center 
		label lblBlank "" align:#center 
		label lblMaxVer "Please check for updates" align:#center
		hyperLink lnkUpdate01 "3ds Max Update Link" color:Black hoverColor:Blue visitedColor:Black address:"http://usa.autodesk.com/adsk/servlet/ps/dl/index?id=2334435&linkID=9241178&siteID=123112" align:#center
		hyperLink lnkUpdate02 "FBX Plugin Update Link" color:Black hoverColor:Blue visitedColor:Black address:"http://usa.autodesk.com/adsk/servlet/pc/item?siteID=123112&id=22694994" align:#center
	)
	button bnCreateBiped "Create Biped" width:154 height:23 offset:[0,5]
	
	
	-- Functions
	---------------------------------------------------------------------
	
	-- Filter functions
	fn isBipedObj obj = isKindOf obj Biped_Object
	fn isSkinnedMesh obj = 
	(
		local returnVal = false
		if isKindOf obj GeometryClass then (
			for m in obj.modifiers while not returnVal where isKindOf m skin do
				returnVal = true
		)
		returnVal
	)
	
	-- Converts a direction (vector) to a matrix3 value
	fn getMatrixFromDir dir =
	(
		local upVector = [0,0,1]
		local y = normalize dir
		local z = normalize (cross y upVector)
		local x = normalize (cross y z)
		local tm = matrix3 x y z [0,0,0]
		
		return tm
	)
	
	
	-- Returns an array of all the object's children (and children's children, etc.)
	fn getAllChildren obj =
	(
		local allChildren = #(obj)
		for c in obj.children do
			join allChildren (getAllChildren c)
		return allChildren
	)
	
	
	
	fn createBipedXtra boneNode bipedRoot mixamoCharName =
	(
		local newXtraName = trimLeft (substituteString boneNode.name mixamoCharName "") ":"
		local newXtra = biped.createXtra bipedRoot.controller 1 newXtraName
	 	biped.attachXtra newXtra boneNode.parent
		biped.setTransform newXtra #pos boneNode.transform.pos false
		local tm = (preRotateY boneNode.transform -90)
		biped.setTransform newXtra #rotation tm.rotation false
		
		local scaleFactor = (biped.getTransform bipedRoot #scale) * 10
		scaleFactor.x /= 10.
		biped.setTransform newXtra #scale scaleFactor false
		
		for c in boneNode.children do
			c.parent = newXtra
		
		return (dataPair origBone:boneNode newXtra:newXtra)
	)
	
	
	fn replaceNonBipedBonesWithXtras baseNode bipedRoot mixamoCharName =
	(
		local matchingList = #()
		if not isProperty baseNode.controller #rootNode then
		(
			local newMatchingItem = createBipedXtra baseNode bipedRoot mixamoCharName
			append matchingList newMatchingItem
			baseNode = newMatchingItem.newXtra
		)
		
		for c in baseNode.children do
			join matchingList (replaceNonBipedBonesWithXtras c bipedRoot mixamoCharName)
		
		return matchingList
	)
	
	
	
	
	
	-- Positions the biped thighs by scaling, rotating and positioning the pelvis,
	-- because the thighs cannot be moved directly.
	fn positionBipedThighs bpRoot lThighPos rThighPos =
	(
		-- Get the relevant nodes
		local pelvis = biped.getNode bpRoot #Pelvis
		local lThigh = biped.getNode bpRoot #LLeg link:1
		local rThigh = biped.getNode bpRoot #RLeg link:1
		
		-- Then move the pelvis to the middle between the 2 positions
		biped.setTransform bpRoot #pos ((lThighPos + rThighPos) / 2.) true
		
		-- First, scale the pelvis to the right size.
		local curScale = biped.getTransform pelvis #scale
		local curDist = distance lThigh rThigh
		local newDist = distance lThighPos rThighPos
		local mult = newDist / curDist
		biped.setTransform pelvis #scale (curScale * [1,1,mult]) true
	)
	
	
	
	-- Scale biped bone to the right length by moving the parent bone.
	fn scaleBipedBone bpBone targetBone =
	(
		local bipedRoot = bpBone.controller.RootNode
		
		local bipChild = bpBone.children[1]
		local targetChild = targetBone.children[1]
		
		-- Get the biped bone id
		local boneId = biped.getIdLink bpBone
		
		if isValidNode bipChild and isValidNode targetChild then (
			-- Deal with special case: feet
			if (boneId.x == KEY_LLEG or boneId.x == KEY_RLEG) and boneId.y == 3 then (
				-- Get the position difference between the bone and it's child. This is the scale that needs to be set.
				local diff = in coordsys bpBone (targetChild.pos - targetBone.pos)
				-- Get the current scale, we'll need that for the z axis.
				local oldScale = biped.getTransform bpBone #scale
				-- Calculate the right y axis scale using the ankleAttach value
				local yScale = diff.y / (1.0 - bipedRoot.controller.ankleAttach)
				-- Make sure that the height of the foot is not negative
				if diff.x <= 0 then
					diff.x = 0.1
				-- Set the new scale
				local newScale = [diff.x, yScale, oldScale.z]
				biped.setTransform bpBone #scale newScale false
			)
			-- Deal with special case: hands and neck
			else if boneId.x ==  KEY_NECK or ((boneId.x == KEY_LARM or boneId.x == KEY_RARM) and boneId.y == 4) then (
				-- Get the current scale, we'll need that for the y and z axes.
				local oldScale = biped.getTransform bpBone #scale
				-- Get the distance to the last child (pinky)
				local dist = distance targetBone targetBone.children[targetBone.children.count]
				-- Set the new scale
				local newScale = [dist, oldScale.y, oldScale.z]
				biped.setTransform bpBone #scale newScale false
			) 
			-- Generic case
			else (
				-- Get the current length
				local currentLength = distance bpBone.transform.pos bipChild.transform.pos
				local newLength = distance targetBone.transform.pos targetChild.transform.pos
				-- Calculate a multiplier to get from the current length to the new one
				local mult = newLength / currentLength
				
				-- Get the current biped bone's parent scale
				local currentScale = biped.getTransform bpBone #scale
				
				-- Set the biped bone's parent scale
				biped.setTransform bpBone #scale (currentScale * [mult,1,1]) true
			)
		)
	)
	
	
	
	
	
	-- Returns the transform matrix needed to match a mixamo character bone's
	-- transformation the given biped bone. 
	-- There are different rotation matrices for different bones, due to differences
	-- in the local coordinate systems of biped and the mixamo character.
	fn getRotationMatrix bipedBone =
	(
		-- Init some variables
		local boneId = biped.getIdLink bipedBone
		local tm = bipedBone.transform
		local rotMatrix = matrix3 1
		
		
		-- Define rotation matrix based on the bone's id.
		if boneID != undefined then (
			
			if boneID.x == KEY_PELVIS then
				rotMatrix = matrix3 [1,0,0] [0,0,1] [0,-1,0] [0,0,0]
			else if boneID.x == KEY_SPINE then
				rotMatrix = (matrix3 [0,0,1] [1,0,0] [0,1,0] [0,0,0])
			else if boneID.x == KEY_NECK then
				rotMatrix = (matrix3 [0,0,1] [1,0,0] [0,1,0] [0,0,0])
			else if boneID.x == KEY_HEAD then
				rotMatrix = (matrix3 [0,0,1] [1,0,0] [0,1,0] [0,0,0])
			
			else if boneID.x == KEY_LARM and boneID.y == 1 then 		-- Left shoulder
				rotMatrix = (matrix3 [0,1,0] [1,0,0] [0,0,-1] [0,0,0])
			else if boneID.x == KEY_LARM and (boneID.y == 2 or boneID.y == 3) then 		-- Left arm
				rotMatrix = (matrix3 [0,1,0] [1,0,0] [0,0,-1] [0,0,0])
			else if boneID.x == KEY_LARM and boneID.y == 4 then 		-- Left hand
				rotMatrix = (matrix3 [0,0,1] [1,0,0] [0,1,0] [0,0,0])
			else if boneID.x == KEY_LHAND then (						-- Left Fingers
				return undefined
			)
			
			else if boneID.x == KEY_RARM and boneID.y == 1 then 		-- Right shoulder
				rotMatrix = (matrix3 [0,-1,0] [1,0,0] [0,0,1] [0,0,0])
			else if boneID.x == KEY_RARM and (boneID.y == 2 or boneID.y == 3) then 		-- Right arm
				rotMatrix = (matrix3 [0,-1,0] [1,0,0] [0,0,1] [0,0,0])
			else if boneID.x == KEY_RARM and boneID.y == 4 then 		-- Right hand
				rotMatrix = (matrix3 [0,0,1] [1,0,0] [0,1,0] [0,0,0])
			else if boneID.x == KEY_RHAND then (						-- Right Fingers
				return undefined
			)
			
			else if boneID.x == KEY_LLEG then (
				if boneID.y == 3 then 	-- Left Foot
					return undefined
				else 					-- Left Thigh and Shin
					rotMatrix = (matrix3 [0,0,1] [1,0,0] [0,1,0] [0,0,0])
			) else if boneID.x == KEY_LTOES then 			-- Left Toes
				return undefined
			
			else if boneID.x == KEY_RLEG then (
				if boneID.y == 3 then 	-- Right Foot
					return undefined
				else 					-- Right Thigh and Shin
					rotMatrix = (matrix3 [0,0,1] [1,0,0] [0,1,0] [0,0,0])
			) else if boneID.x == KEY_RTOES then 			-- Right Toes
				return undefined
			
			else 	-- Default matrix
				rotMatrix = matrix3 1
			
			
			-- Calculate the transform matrix
			tm = rotMatrix * bipedBone.transform
			tm.pos = bipedBone.transform.pos
			
		) else ( -- if boneId == undefined - it's not a normal biped bone (e.g. a toe nub), so return only the position.
			tm = translate (matrix3 1) bipedBone.transform.pos
		)
		
		return tm
	)

	
	
	-- Sets the biped finger rotation based on the mixamo bone direction,
	-- rather than on it's rotation, since the some of the fingers' bones
	-- are not pointing along one of their axes.
	fn setBipedFingerRotation bipedBone mixamoCharName preRotateAng:[-90,0,0] =
	(
		local boneId = biped.getIdLink bipedBone
		local bipedRoot = bipedBone.controller.RootNode
		local numFingerLinks = bipedRoot.controller.fingerLinks
		-- Get the target (mixamo) bone.
		local targetBone = nameMapping.getMixamoNodeByObj bipedBone mixamoCharName
		-- If this is the last link in the finger there might be a need for special
		-- treatment.
		if mod boneID.y numFingerLinks == 0 then (
			-- Use the position of the first child object to derermine the fingertip's direction.
			local dir = normalize (targetBone.children[1].transform.pos - targetBone.transform.pos)
		) else (
			-- Calculate the bone's direction using it's first child.
			local dir = normalize (targetBone.children[1].transform.pos - targetBone.transform.pos)
		)
		-- Get the up vector
		local upVector = targetBone.dir --[0,0,1]
		-- Create a transform matrix
		local x = normalize (cross dir upVector)
		local y = normalize (cross dir x)
		local tm = matrix3 dir x y [0,0,0]
		-- Rotate the matrix to the right orientation
		tm = preRotateX tm preRotateAng.x
		tm = preRotateY tm preRotateAng.y
		tm = preRotateZ tm preRotateAng.z
		-- Set the bone's rotation
		biped.setTransform bipedBone #rotation tm false
	)
	
	
	-- Set the given object's rotation to the given values
	fn SetObjectRotation obj rx ry rz =
	(
		local translateMat = transMatrix obj.transform.pos
		local scaleMat = scaleMatrix obj.transform.scale
		obj.transform = scaleMat * translateMat
		rotate obj (angleaxis rx [1,0,0])
		rotate obj (angleaxis ry [0,1,0])
		rotate obj (angleaxis rz [0,0,1])
	)
	
	
	
	fn wiggleRotationCloser bipedBone bipedBoneChild targetBoneChild testValue axis numTestsLeft = 
	(	
		bipedBoneTm = bipedBone.transform
		startDist = (distance bipedBoneChild.transform.position targetBoneChild.transform.position)
		rotate bipedBone (angleaxis testValue axis)
		posTest = (distance bipedBoneChild.transform.position targetBoneChild.transform.position)
		rotate bipedBone (angleaxis (-2*testValue) axis)
		negTest = (distance bipedBoneChild.transform.position targetBoneChild.transform.position)
		rotate bipedBone (angleaxis testValue axis)
		if (posTest < negTest) then (
			if (startDist < posTest) then (
				testValue = testValue/2
			)
			else(
				rotate bipedBone (angleaxis testValue axis)
			)
		)
		else (
			if (startDist < negTest) then (
				testValue = testValue/2
			)
			else(
				rotate bipedBone (angleaxis (-1*testValue) axis)
			)
		)
		if (numTestsLeft > 1) then (
			wiggleRotationCloser bipedBone bipedBoneChild targetBoneChild testValue axis (numTestsLeft-1)
		)
	)

	
	
	
	-- Recursively align the bones.
	fn recurseAlignBones bipedBone mixamoCharName mode: =
	(
		-- Get the matching mixamo bone object
		local targetBone = nameMapping.getMixamoNodeByObj bipedBone mixamoCharName
		
		-- If the mixamo bone is valid, then align the biped bone to it.
		if isValidNode targetBone then (
			-- Scale mode
			if mode == #Scale then (
				scaleBipedBone bipedBone targetBone
			)
			-- position biped bones
			else if mode == #BipedPos then (
				biped.setTransform bipedBone #pos targetBone.transform.pos false
				
				-- Auto-rig adjustments
				if isAutoRig then (
					local boneID = biped.getIdLink bipedBone
					
					if boneID != undefined then (
						-- Deal with feet rotation in auto-rig
						if (boneID.x == KEY_LLEG or boneID.x == KEY_RLEG) and boneID.y == 3 then (
							local posDiff = (targetBone.children[1].transform.pos - targetBone.transform.pos)
							posDiff.z = abs posDiff.z
							local dir = normalize posDiff
							local tm = getMatrixFromDir dir
							local ang = eulerAngles (tm.rotation as eulerAngles).x 90 -90
							biped.setTransform bipedBone #rotation ang false
						)
						
						-- Deal with hand rotations in auto-rig
						else if (boneID.x == KEY_LARM or boneID.x == KEY_RARM) and boneID.y == 4 then (
							-- Get the rotation matrix
							local newTM = getRotationMatrix bipedBone
							newTM *= inverse bipedBone.transform
							newTM = preRotateX newTM -90
							newTM = preRotateZ newTM -90
							newTM = newTM * targetBone.transform
							-- Assign the new rotation matrix.
							biped.setTransform bipedBone #rotation newTM.rotation false
						)
						
						-- Deal with neck rotation in auto-rig
						else if (boneID.x == KEY_NECK) then (
							setBipedFingerRotation bipedBone mixamoCharName preRotateAng:[-90,0,0]
						)
						
						--deal with spine rotation in auto-rig
						else if (boneID.x == KEY_SPINE) then (
							local tm = targetBone.transform
							local ang = eulerAngles 90 ((tm.rotation as eulerAngles).x*-1) 90
							biped.setTransform bipedBone #rotation ang false							
						)
						
						--deal with shoulder rotation in auto-rig
						else if ((boneID.x == KEY_LARM or boneID.x == KEY_RARM) and (boneID.y == 1)) then(
							bipedBoneChild = bipedBone.children[1]
							targetBoneChild = targetBone.children[1]
							--wiggle in Y and Z, 2x because they can play off each other
							wiggleRotationCloser bipedBone bipedBoneChild targetBoneChild 8 ([0,1,0]) 20
							wiggleRotationCloser bipedBone bipedBoneChild targetBoneChild 8 ([0,0,1]) 20
							wiggleRotationCloser bipedBone bipedBoneChild targetBoneChild 8 ([0,1,0]) 20
							wiggleRotationCloser bipedBone bipedBoneChild targetBoneChild 8 ([0,0,1]) 20
						)
					)
				)
				
				-- Set fingers rotation
				local boneId = biped.getIdLink bipedBone
				if boneId != undefined and (boneId.x == KEY_RHAND or boneId.x == KEY_LHAND) then (
					setBipedFingerRotation bipedBone mixamoCharName
				)
			) 
			-- Align mode (align mixamo char bones to biped.)
			else if mode == #Align then (
				-- Get the rotation matrix
				local newTM = getRotationMatrix bipedBone
				-- Assign the new rotation matrix, if it's not undefined.
				if newTM != undefined then (
					preScale newTM targetBone.transform.scale
					targetBone.transform = newTM
				)
			)
		)
		forceCompleteRedraw()
		-- Loop through the bone's children and run this function on them as well.
		for c in bipedBone.children do
			recurseAlignBones c mixamoCharName mode:mode
	)
	
	
	
	-- Aligns the mixamo character root (hips) to the biped root.
	fn alignRigRoot mixamoRoot bipedRoot =
	(
		-- Multiply the biped transform by a matrix to fix the y-up/z-up difference.
		local tm = bipedRoot.transform * (matrix3 [0,0,1] [-1,0,0] [0,-1,0] [0,0,0])
		-- Move the matrix to the position of the biped root.
		tm.pos = bipedRoot.transform.pos
		-- Set the scale according to the mixamoRoot
		preScale tm mixamoRoot.transform.scale false
		-- Assign the transform to the mixamo character root.
		mixamoRoot.transform = tm
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
	
	
	
	-- Aligns the the biped and mixamo character bones.
	-- It first moves the mixamo character to the same position as the biped,
	-- then tries to match the biped bones as closly as it can to the mixamo
	-- character's bones. Finally, it matches the mixamo character bones to 
	-- the biped bones.
	fn alignBones bipedObj mixamoCharName =
	(
		-- Get the mixamo root
		local mixamoRoot = nameMapping.getMixamoNodeByObj bipedObj mixamoCharName
		-- Move and rotate the Mixamo root
		alignRigRoot mixamoRoot bipedObj
		-- Move the biped so that lowest verices are on z=0
		local zPosDiff = getGeometryZOffset()
		biped.setTransform bipedObj #pos (bipedObj.transform.pos - [0,0,zPosDiff]) false
		alignRigRoot mixamoRoot bipedObj
		
		-- Set the biped to figure mode
		bipedObj.controller.figureMode = true
		
		-- Position the thighs and rotate the pelvis.
		local lThigh = nameMapping.getMixamoNodeByObj (biped.getNode bipedObj #LLeg link:1) mixamoCharName
		local rThigh = nameMapping.getMixamoNodeByObj (biped.getNode bipedObj #RLeg link:1) mixamoCharName
		positionBipedThighs bipedObj lThigh.transform.pos rThigh.transform.pos
		
		-- Set the lengths of the biped bones.
		local startBone = biped.getNode bipedObj #pelvis link:1 	-- Start with the pelvis.
		recurseAlignBones startBone mixamoCharName mode:#Scale
		
		-- Try to place the biped bones on the mixamo character bones.
		recurseAlignBones startBone mixamoCharName mode:#BipedPos
		
		
		-- Store feet rotation to restore them later
		if isAutoRig then (
			local LFootBiped = biped.getNode bipedObj #LLeg link:3
			local LFootNode = nameMapping.getMixamoNodeByObj LFootBiped mixamoCharName
			local LFootTM = LFootNode.transform
			LFootTM.pos = LFootBiped.transform.pos
			
			local RFootBiped = biped.getNode bipedObj #RLeg link:3
			local RFootNode = nameMapping.getMixamoNodeByObj RFootBiped mixamoCharName
			local RFootTM = RFootNode.transform
			RFootTM.pos = RFootBiped.transform.pos
		)
		
		
		-- Align the mixamo character bones to the biped bones, to mache the biped bones that couldn't 
		-- be matched exactly to the mixamo character due to IK and other constraints.
		--This does not do well for certain areas, but is needed for others.
		recurseAlignBones startBone mixamoCharName mode:#Align
		
		
		-- Restore feet rotation.
		if isAutoRig then (
			LFootNode.transform = LFootTM
			RFootNode.transform = RFootTM
		)
		
		-- To fix fingers issues, re-align the biped fingers based on the mixamo finger directions.
		local LHand = biped.getNode bipedObj #LArm link:4
		for finger in LHand.children do
			recurseAlignBones finger mixamoCharName mode:#BipedPos
		local RHand = biped.getNode bipedObj #RArm link:4
		for c in RHand.children do
			recurseAlignBones c mixamoCharName mode:#BipedPos
		
		-- Get the biped out of figure mode
		bipedObj.controller.figureMode = false
	)
	
	
	
	-- Returns an approximate height of the mixamo character (it is only approximate because that character is not in T-pose).
	fn getCharacterHeight mixamoCharName =
	(
		-- Get the character objects
		local objs = execute ("$'" + mixamoCharName + "*'")
		
		-- Get the bounding box of objs
		local bbox = objs.max - objs.min
		
		-- Get the biggest value between the y and z axes of the bounding box
		local height = amax bbox.y bbox.z
		
		-- Return the height
		return height
	)
	
	
	
	-- Returns the number of fingers in the mixamo character 
	--If the hands have different numbers of fingers, takes the higher number 
	fn getNumberFingers mixamoCharName = 
	( 
		numFingers = 0 
		-- Get a hand object 
		local leftHandObj = getNodeByName (mixamoCharName + ":LeftHand") 
		local rightHandObj = getNodeByName (mixamoCharName + ":RightHand") 
		if isValidNode leftHandObj then 
		numFingers = leftHandObj.children.count 
		if isValidNode rightHandObj then( 
		temp = rightHandObj.children.count 
		if temp > numFingers then 
		numFingers = temp 
		) 
		return numFingers
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
	
	
	
	
	-- Creates a biped system with the right bones.
	fn bipedCreator mixamoCharName =
	(
		-- Create a variable to hold the new biped.
		local newBip
		
		-- Create the new biped object
		local height = getCharacterHeight mixamoCharName --heightAxis:ddlHeightAxis.selected 
		local pos = try((getNodeByName (mixamoCharName + ":Hips")).transform.pos)catch([0,0,0])
		local numFingers = getNumberFingers mixamoCharName
		local numNeckBones = getNumberNeckBones mixamoCharName
		newBip = biped.createNew height -90 pos \
			arms:True 				\ -- Specifies whether or not arms will be generated for the current biped. 
			neckLinks:numNeckBones 	\ -- Specifies the number of links in the biped neck. 
			spineLinks:3 			\ -- Specifies the number of links in the biped spine. 
			legLinks:3 				\ -- Specifies the number of links in the biped legs. 
			tailLinks:0 			\ -- Specifies the number of links in the biped tail. A value of 0 specifies no tail. 
			ponytail1Links:0 		\ -- Specifies the number of Ponytail links.
			ponytail2Links:0 		\ -- Specifies the number of Ponytail links. 
			fingers:numFingers 		\ -- Specifies the number of biped fingers. 
			fingerLinks:3 			\ -- Sets the number of links per finger. 
			toes:1 					\ -- Specifies the number of biped toes. 
			toeLinks:1 				\ -- Specifies the number of links per toe. 
			ankleAttach:0.2  		\ -- Specifies the right and left ankles' point of attachment along the corresponding foot block. A value of 0 places the ankle attachment point at the heel. A value of 1 places the ankle attachment point at the toes.
			trianglePelvis:True 	\ -- Select this control to create links from the upper legs to the lowest biped spine object when Physique is applied.
			triangleNeck:false 		\ -- If true, the clavicles are parented to the top spine link. If false, the clavicles are parented to the neck base.
			prop1Exists:False 		\ -- Specifies whether prop 1 exists.
			prop2Exists:False 		\ -- Specifies whether prop 2 exists.
			prop3Exists:False 		\ -- Specifies whether prop 3 exists.
			forearmTwistLinks:0 	\ -- Specifies the number of forearm links.
		
		
		-- Return the new biped.
		return newBip
	)
	
	-- Returns the hips joint from the original fbx rig if it exists.
	fn getHipsJoint =
	(
		-- Get all objects named "Hips"
		local objs = $*Hips
		-- Loop through the objects and return the first one that has children
		for o in objs where o.children.count > 0 do
			return o
		
		-- If no hips object with children was found, return undefined
		return undefined
	)
	
	
	-- Return the first mixamo character name found in the scene.
	fn getMixamoCharName =
	(
		local charName = undefined
		local hipsJoint = getHipsJoint()
		if isValidNode hipsJoint and matchPattern hipsJoint.name pattern:"*:*" do(
			local parts = filterString hipsJoint.name ":" splitEmptyTokens:true
			local tempName = subString hipsJoint.name 1 (hipsJoint.name.count - (parts[parts.count].count + 1))
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
	
	
	
	-- Transfers the skin bone weights
	fn transferSkinBoneWeights bipedObj altNameMapping: =
	(
		if altNameMapping == unsupplied then	
			nameMapping.mappingFilename = useMappingFile
		-- Init a variable to determine whether or not to use the alternate name mapping
		local useAltMapping = altNameMapping != unsupplied
		-- Determine what name mapping to use.
		local nameMappingToUse = if useAltMapping then altNameMapping else nameMapping
		-- Loop through the scene objects, and for each one that has a skin modifier, swap the bones.
		for o in objects where isSkinnedMesh o do (
			transSkinWeights.swapSkinBones o bipedObj nameMapping:nameMappingToUse reparseMapping:(not useAltMapping)
		)
	)
	
	
	
	fn convertRemainingMixamoBonesToBipedXtras bipObj mixamoCharName =
	(
		-- Set the biped to figure mode
		bipObj.controller.figureMode = true
		
		-- Replace remaining mixamo bones with biped Xtras
		local matchingList = replaceNonBipedBonesWithXtras bipObj bipObj mixamoCharName
		local tmpNameMapping = s_AutoBiped_NameMapping()
		tmpNameMapping.mappingList = for matchingItem in matchingList collect (
			local bipedPart = substituteString matchingItem.newXtra.name matchingItem.newXtra.controller.rootName "{base_name}"
			local mixamoPart = substituteString matchingItem.origBone.name mixamoCharName "CharName"
			local strToParse = bipedPart + ";" + mixamoPart
			tmpNameMapping.parseLine strToParse
		)
		
		-- Transfer skin weights from the old mixamo bones to the new Xtras.
		for o in objects where isSkinnedMesh o do
			transSkinWeights.swapSkinBones o bipObj nameMapping:tmpNameMapping reparseMapping:false 
		
		-- Set the biped to animation mode
		bipObj.controller.figureMode = false
		
		-- Delete the old mixamo bones.
		local pattern = mixamoCharName + "*"
		local objsToDelete = for h in helpers where (refs.dependentNodes h).count == 0 and matchPattern h.name pattern:pattern collect h
		delete objsToDelete
	)		
	
	
	fn hasMorpher obj = 
	(
		local returnVal = false
		if isKindOf obj GeometryClass then (
			for m in obj.modifiers while not returnVal where isKindOf m morpher do
				returnVal = true
		)
		return returnVal
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
		
	
	
	-- Auto create biped, align the bones and transfer skin weights.
	fn autoBiped bipObj:  =
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
				messageBox "Couldn't find any character in the scene." title:"AutoBiped"
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
		
		
		if bipObj == unsupplied then (
			-- Create the biped.
			local newBip = bipedCreator mixamoCharName
			
			bipObj = newBip
		)
		
		-- Parse the mapping file
		useMappingFile = mappingFileName
		hand = biped.getNode bipObj #lArm link:4
		children = hand.children
		numFingers = 0
		if (children != undefined) then (
			numFingers = children.count
		)
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
		
		nameMapping.mappingFilename = useMappingFile
		nameMapping.parseFile()
		
		
		-- Align the bones.
		alignBones bipObj mixamoCharName 
		
		-- Rebuild Blendshapes
		for obj in meshObjs do
		(
			rebuildBlendshapes obj
		)
		
		-- Transfer skin weights
		transferSkinBoneWeights bipObj
		
		-- Delete old mixamo rig
		local hipsObj = nameMapping.getMixamoNodeByObj bipObj mixamoCharName
		local objs = getAllChildren hipsObj
		delete objs
		
		-- Convert the remaining Mixamo bones to Biped Xtras bones.
		convertRemainingMixamoBonesToBipedXtras bipObj mixamoCharName
		
		--swap any Mesh Bakers with morphers
		for o in meshObjs do
		(
			--collect all modifiers in the current object
			theMods = for m in o.modifiers collect m     
			for m in theMods where classof m == morpher do 
			(
				if (m.name == "AutoBiped Mesh Baker") then (
					addModifier o m before:theMods.count
					deleteModifier o m
				)
			)
		)
		--Delete any Mesh Bakers
		for o in meshObjs do
		(
			for m = 1 to o.modifiers.count do(
				if ((classof o.modifiers[m] == morpher) and (o.modifiers[m].name == "AutoBiped Mesh Baker")) then (
					modPanel.setCurrentObject o.modifiers[m] 
					maxOps.CollapseNodeTo $ m true
				)
			)
		)
		
		-- If needNamespacePrefixRemoval is true, remove the namespace prefix from the objects
		if needNamespacePrefixRemoval then
			removeNamespacePrefix (mixamoCharName + ":")
		
		-- Return the new biped 
		return newBip 
	)

	
	-- Event Handlers
	------------------------------------------------------------------------
	on bnCreateBiped pressed do (
		autoBiped()
	)
	
) -- End of ro_mixamoAutoBiped rollout



--Warning mesage about CoCreateInstance Fails
try (destroyDialog ::pre2012popupUI) catch()
rollout pre2012popupUI "AutoBiped VERSION WARNING!!"
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

createDialog ro_mixamoAutoBiped
pre2012popup()