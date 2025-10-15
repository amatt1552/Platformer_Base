Information to help understand the character animator.



Where is all the movement?
---------------------------------------------------------------------------------------------------
Idle, Idle rotation, walking, running, and sprinting are set in locomotion using a blend tree.
This improves the transitions to each movement state.


Where are the animations changed from?
---------------------------------------------------------------------------------------------------
Most animations are changed from the various states found in the character states folder.
Currently movement and rotation are in the character state machine but this might change.


DirX_OnStateChange
---------------------------------------------------------------------------------------------------
It allows the animator to know what direction the player was moving in before changing thier state.
This should only be updated once on state changed.
Currently used for jump and sliding animation