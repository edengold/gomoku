--***************************************************************
--* AutoCAT_transferSkinWeights.ms
--* This file is part of the Mixamo Auto CAT script.
--*
--* This file defines a struct to perform skin weight transfer
--* from the original bones to the CAT bones.
--*
--* Copyright Mixamo www.mixamo.com 2011-2014
--* Concept by Mixamo, script created by Ofer Zelichover for Mixamo www.mixamo.com
--* Script updates by Dan Babcock
--* 
--***************************************************************


(

	-- A struct to deal with transferring skin weights from a one set of bones to another.
	struct s_AutoCAT_TransferSkinWeights (
		_nameMapping = undefined, 		-- A property to hold a s_NameMapping instance with the name mapping.
		srcBones = undefined, 			-- Holds the source bones objects
		destBones = undefined, 			-- Holds the destination bones objects
		srcBonesToKeep = undefined, 	-- Holds an array of source bones that don't have matching CAT bones, so we can keep them in the skin modifier.
		
		
		
		-- This function bakes the current mesh pose by making a snapshot of the 
		-- mesh, and placing it in a channel in a morpher modifier.
		fn bakeMesh meshObj =
		(
			-- Create a snapshot of the mesh into a temp mesh
			local tempMesh = snapshot meshObj
			
			-- Add a morpher modifier before the skin modifier
			local morpherMod = morpher name:"AutoCAT Mesh Baker"
			for i = 1 to meshObj.modifiers.count where isKindOf meshObj.modifiers[i] skin do (
				-- Add the modifier
				addModifier meshObj morpherMod before:i
				-- Exit the loop
				exit
			)
			-- Add the tempMesh to the first channel in the morpher.
			WM3_MC_BuildFromNode morpherMod 1 tempMesh
			-- Set the value of the first channel to 100%
			WM3_MC_SetValue morpherMod 1 100.
			-- Delete the temp mesh
			delete tempMesh
		),
		
		
		-- This function solves the issue that the default load envelopes in max
		-- opens a window for bone mappping.
		-- This function automatically "presses" the OK button in that window.
		fn confirmLoadEnvelopes =
		(
			local hwnd = dialogMonitorOps.getWindowHandle()
			if (uiAccessor.getWindowText hwnd == "Load Envelopes") then
			(
				UIAccessor.PressButtonByName hwnd "Match by Name"
				forceCompleteRedraw()
				UIAccessor.PressButtonByName hwnd "OK"
			)
			true
		),

		fn loadEnvelopes skinMod filename=
		(			
			DialogMonitorOps.Enabled = true
			DialogMonitorOps.RegisterNotification (confirmLoadEnvelopes) id:#pressSkinOK
			skinOps.LoadEnvelope skinMod filename
			DialogMonitorOps.unRegisterNotification id:#pressSkinOK
			DialogMonitorOps.Enabled = false
			ok
		),
				
		
		-- Removes the bone objects in boneOjbs from the given skin modifier.
		fn removeOldBones skinMod boneObjs =
		(
			-- Make a temp array with the boneObjs object names.
			local boneNames = for b in boneObjs where (findItem srcBonesToKeep b == 0) collect b.name as name
			
			-- Get the number of bones in the skin modifier.
			local numBones = skinOps.GetNumberBones skinMod
			
			-- Loop through the bones in the skin modifier.
			for i = numBones to 1 by -1 do (
				-- Get the bone name of the i'th bone in the modifier.
				local boneName = (skinOps.GetBoneName skinMod i 0) as name
				
				-- If the name exist in the boneNames array, then remove this bone.
				if findItem boneNames boneName > 0 then
					skinOps.removebone skinMod i
			)
		),
		
		
		
		
		-- Returns array of matching bones (mixamo bones with CAT bones) and bones that have no matching bone.
		fn getBonesLists skinMod CATParentObj reparseMapping:true =
		(
			-- Make sure the skin modifier is selected in the modifier panel.
			max modify mode
			modPanel.setCurrentObject skinMod
			
			-- Init the arrays
			srcBones = #()
			destBones = #()
			srcBonesToKeep = #()
			
			-- Parse the naming file.
			if reparseMapping then
				_nameMapping.parseFile()
			
			
			local numBones = skinOps.GetNumberBones skinMod
			for i = 1 to numBones do (
				local boneName = skinOps.GetBoneName skinMod i 0
				local srcObj = getNodeByName boneName
				local destObj = _nameMapping.getCATNode boneName CATParentObj
				
				if isValidNode srcObj and isValidNode destObj then (
					append srcBones srcObj
					append destBones destObj
				) else (
					append srcBonesToKeep srcObj
					format "No matching CAT bone for %\n" boneName
				)
			)
		),
		
		
		
		-- Reparent mixamo bones that didn't have a matching CAT bone.
		fn reparentNonMatchingBones =
		(
			-- Loop throug the keep bones array
			for b in srcBonesToKeep do (
				-- Find the bone's parent index in the srcBones array
				local n = findItem srcBones b.parent
				
				-- If the bone's parent is in the srcBones array, that means the parent was swapped
				-- with a CAT bone, so reparent the bone to the matching CAT.
				if n > 0 then (
					b.parent = destBones[n]
				)
			)
		),
		
		
		
		-- Swaps the srcBones that are in the skin modifier with the destBones.
		-- srcBones and destBones are arrays of objects. They both must have the same number 
		-- of items, as the bones are replace by replacing the n'th source bone with the n'th dest bone.
		fn swapSkinBones meshObj CATParentObj nameMapping: reparseMapping:true =
		(
			if nameMapping != unsupplied then
				_nameMapping = nameMapping
			
			-- Get the skin modifier from the mesh object.
			local skinMod = undefined
			for m in meshObj.modifiers while skinMod == undefined where isKindOf m skin do
				skinMod = m
			
			-- If no skin modifier was found, exit.
			if skinMod == undefined then
				return false
			
			
			-- Get the srcBones and destBones arrays
			getBonesLists skinMod CATParentObj reparseMapping:reparseMapping
			
			-- Make sure the skin modifier is selected in the modifier panel.
			max modify mode
			modPanel.setCurrentObject skinMod
			subobjectLevel = 0
			
			local oldJointNames = #()
			--instead of using ASCII and switching names in that file, we're going to save as binary and change the joint names!!!
			for i = 1 to srcBones.count do(
				append oldJointNames srcBones[i].name
				srcBones[i].name = destBones[i].name
			)			
			
			-- First, save the current envelopes.
			local filename = sysInfo.tempDir + "~boneReplacer" + timeStamp() as string + ".env"
			skinOps.saveEnvelope skinMod filename
			-- Since we don't have the rig in T-pose, we need to bake the mesh in it's current position
			-- so when we assign new bones to the skin modifier, the skinning won't break.
			bakeMesh meshObj
			-- Turn off always deform in the skin modifier.
			skinMod.alwaysDeform = false
			
			
			-- Now, remove the source bones from the skin modifier
			removeOldBones skinMod srcBones
			
			--restore names of joints for other meshes
			for i = 1 to srcBones.count do(
				srcBones[i].name = oldJointNames[i]
			)	
			
			
			-- ... and add the destination bones to the skin modifier.
			for b in destBones do
				skinOps.addbone skinMod b 0
			
			-- Lastly, load the envelopes from the file saved erlier.
			loadEnvelopes skinMod filename
			
			-- Turn always deform in the skin modifier back on.
			skinMod.alwaysDeform = true
			
			-- Reparent non matching bones
			reparentNonMatchingBones()
			
			-- Clean up
			gc light:true
			deleteFile filename
			
			newFilename
		)
		
	) -- End of s_AutoCAT_TransferSkinWeights struct
	

)