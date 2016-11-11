--***************************************************************
--* AutoBiped_transferSkinWeights.ms
--* This file is part of the Mixamo Auto Biped script.
--*
--* This file defines a struct to perform skin weight transfer
--* from the original bones to the biped bones.
--*
--* Copyright Mixamo www.mixamo.com 2011
--* Concept by Mixamo, script created by Ofer Zelichover for Mixamo www.mixamo.com
--* Updated by Dan Babcock
--* 
--***************************************************************



(

	-- A struct to deal with transferring skin weights from a one set of bones to another.
	struct s_AutoBiped_TransferSkinWeights (
		_nameMapping = undefined, 		-- A property to hold a s_NameMapping instance with the name mapping.
		srcBones = undefined, 			-- Holds the source bones objects
		destBones = undefined, 			-- Holds the destination bones objects
		srcBonesToKeep = undefined, 	-- Holds an array of source bones that don't have matching biped bones, so we can keep them in the skin modifier.
		
		newBonesIdsMapping = undefined, -- Holds an array for mapping the bone ids. This is needed due to a bug in max 2013RC4 (and perhaps later) 
										-- where the bone ids are changing based on order in which they appear in the bone list in the ui.
		
		
		
		fn removeTopModifiers meshObj =
		(
			local topModifiers = for m in meshObj.modifiers while not isKindOf m skin collect m
			for m in topModifiers do
				deleteModifier meshObj m
			
			return topModifiers
		),
		
		fn removeSkinModifier meshObj =
		(
			local skinModifiers = for m in meshObj.modifiers while isKindOf m skin collect m
			for m in skinModifiers do
				deleteModifier meshObj m				
		),
		
		fn restoreTopModifiers meshObj topModifiers =
		(
			-- Re-apply the top modifiers
			for i = topModifiers.count to 1 by -1 do
				addModifier meshObj topModifiers[i]
		),
		
		
		-- This function bakes the current mesh pose by making a snapshot of the 
		-- mesh, and placing it in a channel in a morpher modifier.
		fn bakeMesh meshObj =
		(
			-- If a mesh baker modifier already exists, then don't add another one.
			if not isProperty meshObj #AutoBiped_Mesh_Baker then (
				-- Create a snapshot of the mesh into a temp mesh
				local tempMesh = snapshot meshObj
				
				-- Add a morpher modifier before the skin modifier
				local morpherMod = morpher name:"AutoBiped Mesh Baker"
				-- Add the modifier
				addModifier meshObj morpherMod before:1
				
				-- Add the tempMesh to the first channel in the morpher.
				WM3_MC_BuildFromNode morpherMod 1 tempMesh
				
				-- Set the value of the first channel to 100%
				WM3_MC_SetValue morpherMod 1 100.
				
				-- Delete the temp mesh
				delete tempMesh
			)
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
			DialogMonitorOps.Enabled = true	--DialogMonitorOps.Enabled = false
			DialogMonitorOps.RegisterNotification (confirmLoadEnvelopes) id:#pressSkinOK
			skinOps.LoadEnvelope skinMod filename
			DialogMonitorOps.unRegisterNotification id:#pressSkinOK
			DialogMonitorOps.Enabled = false
			ok
		),
		
		
		
		-- Returns array of matching bones (mixamo bones with biped bones) and bones that have no matching bone.
		fn getBonesLists skinMod bipedObj reparseMapping:true =
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
				local destObj = _nameMapping.getBipedNode boneName bipedObj
				
				if isValidNode srcObj and isValidNode destObj then (
					append srcBones srcObj
					if isKindOf destObj Biped_Object and destObj == destObj.controller.rootNode then
						destObj = biped.getNode destObj #pelvis
					append destBones destObj
				) else (
					append srcBonesToKeep srcObj
					append srcBones srcObj
					append destBones srcObj
				)
			)
		),
		
		
		
		-- Reparent mixamo bones that didn't have a matching biped bone.
		fn reparentNonMatchingBones =
		(
			-- Loop throug the keep bones array
			for b in srcBonesToKeep do (
				-- Find the bone's parent index in the srcBones array
				local n = findItem srcBones b.parent
				
				-- If the bone's parent is in the srcBones array, that means the parent was swapped
				-- with a biped bone, so reparent the bone to the matching biped.
				if n > 0 then (
					b.parent = destBones[n]
				)
			)
		),
		
		
		
		
		-- Swaps the srcBones that are in the skin modifier with the destBones.
		-- srcBones and destBones are arrays of objects. They both must have the same number 
		-- of items, as the bones are replace by replacing the n'th source bone with the n'th dest bone.
		fn swapSkinBones meshObj bipedRootObj nameMapping: reparseMapping:true =
		(
			undo off 
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
				
				-- Make sure the skin modifier is selected in the modifier panel.
				max modify mode
				modPanel.setCurrentObject skinMod
				skinOps.closeWeightTool skinMod
				subobjectLevel = 0
				
				-- Get the srcBones and destBones arrays
				getBonesLists skinMod bipedRootObj reparseMapping:reparseMapping
				
				local oldJointNames = #()
				--instead of using ASCII and switching names in that file, we're going to save as binary and change the joint names!!!
				for i = 1 to srcBones.count do(
					
					append oldJointNames srcBones[i].name
					srcBones[i].name = destBones[i].name
				)		
				
				-- Since we don't have the rig in T-pose, we need to bake the mesh in it's current position
				-- so when we assign new bones to the skin modifier, the skinning won't break.
				-- We remove the top modifiers to solve an issue when sometimes when there are
				-- modifiers above the skin modifier, the scrip will crash when trying to add a 
				-- morpher modifier (needed for the baking) below the skin modifier.
				local topModifiers = removeTopModifiers meshObj
				bakeMesh meshObj
				restoreTopModifiers meshObj topModifiers
				
				-- First, save the current envelopes.
				local filename = sysInfo.tempDir + "~boneReplacer" + timeStamp() as string + ".env"
				skinOps.saveEnvelope skinMod filename
								
				bakeMesh meshObj
				skinModName = skinMod.name
				
				--restore names of joints for other meshes
				for i = 1 to srcBones.count do(
					srcBones[i].name = oldJointNames[i]
				)
				
				removeSkinModifier meshObj
				skinMod = Skin()
				addModifier meshObj skinMod
				skinMod.name = skinModName
				if srcBones.count > 0 do(
					-- ... and add the destination bones to the skin modifier.
					for b in destBones do(
						skinOps.addbone skinMod b 0
					)
				)
				
				-- Lastly, load the envelopes from the file saved erlier.
				loadEnvelopes skinMod filename
				
				-- Turn always deform in the skin modifier back on.
				skinMod.alwaysDeform = true
				
				-- Reparent non matching bones
				reparentNonMatchingBones()
				
			)
			-- Clean up
			gc light:true
		)
		
	) -- End of s_AutoBiped_TransferSkinWeights struct
	

)