# gesture_detection_unity
An openCV based Unity game that is controlled via hand gesture detection from the player's live webcam feed.

## How to Play
To launch the game, you will need a machine running a Windows operating system and run the _Gesture Detection.exe_ file. 
Under _Single Player_ you can then choose the game that you want to play.

### Calibrate Program
When a game is selected, the game screen and two camera feed windows will appear. To calibrate the program, make sure to be in an environment with a non-dynamic background
and ideally static lighting. Hover your hand over the four green rectangles in the camera feed and make sure to cover them completely. Then, press SPACE to perform the skin
extraction based on your hand. You can adjust the _Binary Mask Threshold_ sliders in the game screen to change the sensitivity of the binary mask. If you are unhappy with
the skin extraction, you can repeat the process multiple times. 
When the binary mask shows a clear white hand contour without a lot of noise, press RETURN to launch the hand detection. You will see the results of the detection in the
feed, and the 3D hand model will follow your hand.

If you are in an environment with a noisy background, tick the background removal checkbox and try to adjust the sliders.

### Rock Paper Scissors
When the play button is pressed, the game counts down from three. It will compare your pose after the countdown has finished with the random pose performed by the AI.
Each round ends with either one of the players getting a point, or noone getting a point in case of a draw.

### Drawing Game
Your furthest fingertip is used to paint on a canvas. Use your open hand (paper gesture) to change to a different random color and a closed hand (rock pose) to erase
your drawings on the canvas.

## !!Experimental!! Multiplayer
There is another build of the game which includes a multiplayer version of Rock-Paper-Scissors. To play, one player should open the game clicking on _Multiplayer_
and then _Start Host_. Another player can then join by entering the correct IP address in the field below join session (port forwarding has to be enabled on the host)
and join the game. The game works in the same way as described above.
