--***************************************************************
--* AutoCAT_Manipulation.ms
--* This file is part of the Mixamo Auto-CAT script.
--* Copyright Mixamo www.mixamo.com 2011-2014
--* Concept by Mixamo, script created by Ofer Zelichover for Mixamo www.mixamo.com
--* Script updates by Dan Babcock
--*
--* This is a struct with functions to help set the transformations of CAT bones.
--* 
--* 
--***************************************************************

(

	-- This is a struct to help manipulate a CAT rig, since it seems CAT bones cannot
	-- be transformed easily by the normal means (like using the transform property).
	
	struct s_AutoCAT_Manipulation (
		
		-- Converts a direction (vector) to a matrix3 value
		fn getMatrixFromDir dir =
		(
			local upVector = [0,0,1]
			local x = normalize dir
			local z = normalize (cross x upVector)
			local y = normalize (cross x z)
			local tm = matrix3 x y z [0,0,0]
			
			return tm
		),
		
		
		-- Set the CAT bone's transform.
		-- If newTM is a matrix3 value then the bone is rotated and translated.
		-- If newTM is a point3 value then the bone is only translated.
		fn setCATBoneTM CATBoneObj newTM doPosition doRotation =
		(
			-- Since it seems you can't directly manipulate the CAT bone's transform matrix,
			-- We'll manipulate it using the rotate and move commands.
			
			-- If newTM is a matrix3 value, then the bone needs to be rotated.
			if isKindOf newTM matrix3 then (
				if doRotation then (
					-- Rotate the bone
					rotate CATBoneObj (CATBoneObj.transform.rotation - newTM.rotation)
				)
				
				-- Set newPos to the position part of newTM
				local newPos = newTM.pos
			) else (
				-- If newTM is not a matrix3 value (it's a point3 value), then assign it to newPos.
				local newPos = newTM
			)
			
			-- Now move the bone to the right position.
			if doPosition then (
				local posOffset = (newPos - CATBoneObj.transform.pos)
				move CATBoneObj posOffset
			)
			
			forceCompleteRedraw()
		),
		
		
		
		
		-- Match the IK and FK of the cat object
		-- This function presses the "Match IK and FK" button in the UI
		-- because I couldn't find any maxscript method to do this.
		fn MatchIkAndFk catObj =
		(
			-- Switch to the motion panel
			max motion mode
			-- Select the object
			select catObj
			forceCompleteRedraw()
			-- Get the HWND of the "Match IK and FK" button
			local hwnd = windows.getChildHWND #max "Match IK and FK"
			-- If the button was found, click it.
			if hwnd != undefined then (
				-- Press the button
				UIAccessor.SendMessage hwnd[1] 0x201 0 0 -- mouse down
				UIAccessor.SendMessage hwnd[1] 0x202 0 0 -- mouse up
				forceCompleteRedraw()
			)
		),
		
		
		
	--****** The next few functions deal specifically with CAT feet. ******--
		-- Returns an array of all the object's children (and children's children, etc.)
		fn getAllChildren obj =
		(
			local allChildren = #(obj)
			for c in obj.children do
				join allChildren (getAllChildren c)
			return allChildren
		),
		
		
		-- Change the foot's length. Compansate for toes movement by moving them back into place.
		fn changeFootLength footObj newLength =
		(
			-- Make sure we're in modify mode, as changing the length of the foot 
			-- in a different mode behaves differently!
			max modify mode
			-- Store the positions of the toes to move them later back into place.
			local allChildren = getAllChildren footObj
			local childPositions = for c in allChildren collect c.transform
			-- Store the current foot position, so it can be restored later.
			local oldPos = footObj.transform.pos
			-- Store the current end position of the foot, so it can be used later to calculate the rotation offset.
			
			-- Set the foot's length
			footObj.length = newLength
			-- Select the foot's IKTarget. This is a work around: it seems that CAT doesn't recalculate the foot 
			-- unless the IKTarget is selected.
			local footIKTarget = footObj.controller.Limb.IKTarget
			if isValidNode footIKTarget then
				select footIKTarget
			forceCompleteRedraw()
			
			-- Move the foot back to it's original position
			setCATBoneTM footObj oldPos true false
			
			-- Restore the positions of the toes.
			for i = 1 to allChildren.count do
				setCATBoneTM allChildren[i] childPositions[i] true true
			
		),
		
		
		-- Change the foot's length so that it reaches the toes.
		fn changeFootLengthByToes footObj =
		(
			-- Calculate the average toes position
			local p = 0 --[0,0,0]
			for c in footObj.children do 
				p += distance c footObj
			--now theres a valid case for no children on hands
			if p>0 then
				p /= footObj.children.count
			else
				p = (distance footObj.parent footObj)/3
			
			-- Calculate the length the foot needs to be so it reaches the toes' pivot.
			local l = p / footObj.CATUnits
			-- Change the foot's length
			changeFootLength footObj l
		),
		
		
		
		
		-- Prepare the CAT rig for manipulation.
		fn prepareCATForManipulation catParentObj =
		(
			-- As a workaround, make sure the bones are movable by script by moving the 
			-- pelvis and undoing the move.
			move catParentObj.rootHub.node [0,0,5]
			forceCompleteRedraw()
			move catParentObj.rootHub.node [0,0,-5]
			forceCompleteRedraw()
			
			-- Set the CAT rig to rig mode
			catParentObj.CATMode = 0
		),
		
		
		
		-- Prepare the CAT libms for manipulation by making them really long.
		fn prepareCATLimbsForManipulation catParentObj =
		(
			-- Make the legs very long.
			for o in catParentObj.CATRigNodes where try(matchPattern (o.controller.bone.name) pattern:"Calf" or matchPattern (o.controller.bone.name) pattern:"Leg")catch(false) do
				move o [0, (o.length * -1.5), 0]
			
			-- Make the arms very long.					
			for o in catParentObj.CATRigNodes where try(matchPattern (o.name) pattern:(rigName + "*_IKTarget"))catch(false) do
				in coordsys o move o [(o.length * -1.5), 0, 0]
			for o in catParentObj.CATRigNodes where try(matchPattern (o.controller.bone.name) pattern:"Forearm" and o.controller.SegID == 1)catch(false) do
				move o [0, (o.length * 10), 0]   
		),
		
		
		
		-- Finish the CAT rig manipulation
		fn finishCATManipulation catParentObj =
		(
			-- Add a new layer to hold the base pose
-- 			local newLayer = catParentObj.AppendLayer "base pose" #absolute
			
			-- Get the CAT out of set mode
-- 			catParentObj.CATMode = 1
		)
		
		
	) -- End of s_CATManipulation struct
)



