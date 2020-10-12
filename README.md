# BattleIA

BattleIA is a project to teach & learn programming.

The Project is based on a game concept : an arena, with bots. 
Rules & objectives are to be defined & coded by the students.

The project is structured in standalone programs, comunicating through WebSockets.

  * BattleIAServer
  * SampleBot
  * BattleIAViewers
  
## BattleIAServer 

The game server. It manage the running of the game, to ensure the bots having the same rules, to give each bot its turn to play...

## SampleBot 

A Bot Template, with no intelligence programmed. The bot has to be coded to complete objectives of the game. The bot is totally blind. It don't see what's around him : walls, energy, ennemy... nor where it is... (sad!)

At his turn, the bot is asked for the distance around him to scan, and the server answer with a map of what's around.  

With those informations, it can compute a strategy to survive, and send back to the server the action he wants to perform (only 1 per turn: move, hide, shoot...)

## Viewers 

The game is made console-based. The viewers are also Websocket enabled. 
The server is able to send messages with the game information : 
  * Viewers : full game information (map + players position)
  * Cockpits : game information available for a bot (its surroundings) 
Some examples are available : 
  * 3D_Godot : a godot simple base
  * 2D_cli : a Python console viewer
  * 3D_Python : a pygame 3D version

## BattleIA Commons

This project is not a program, but stores every constants & message sizes to ensure a safe communication between programs.



