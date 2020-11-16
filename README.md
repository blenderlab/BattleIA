![BAttleIA Logo](https://raw.githubusercontent.com/blenderlab/BattleIA/master/Design/logo_blue.png "BAttleIA")

# BattleIA : a Technologic School Project

BattleIA is a project to teach & learn programming, algorithms, and project management.
This is a skeletton project without written objectives. The target is to show to the stutdents  all faces on a project.

The main objective is to produce, alltogether, a complete solution :   
  * Application(s)
  * Logo & Visuals
  * Website 
  * Documentation
  * Sources
  
## Workshop Animation

The projects lacks lots of elements. So the instructor opened some Issues, and described them in categories (code, Gfx, documentation ...). It should be more issues than teams or students : each group/student have to find an issue for his level/interest. 

## How to start ...

The class is divided in teams of 2/3 people max. And each team choose an Issue, and start working on it.  
The exchanges, questions & remarks are stored on the project itself, so that there is no lack of information.  

One team has to endorse the role of Prpject Manager(PM), it's NOT the chief, but the main communication point between owner & the teams. It write down the teams names & issues chosen, the questions & so on. It can have a technical role in helping some people to find answers or solutions. 

### Runnings 

Each session starts by a  launching point : A quick presentation by each team of how they advanced, and the objectives of the session. The instructor has to check there is a real progress and ask for eventual caveats. The problems will be worked on during the session.  Once done, every one goes to work ! 

Instructor now has to go through teams to listen to there problems, and tries to direct them a bit of the team fails to find a way. The objectives can also be reviewed & rewritten if needed. It's easier to have more small objectives than only one big. 

During the session, the Instructor asks for written renders : each team has to give a quick status of the progress. (What is done, what is on purpose, what are the problems).
With those PQ (Progress Quote), inbstructor can evaluate each team, the real progress, and the involving of each student. 

The source code has to be forked by each student on its how account, to make all the updates needed, without blocking/breaking the main code. The Issues solved will be pushed to main project through a Pull Request. So that it will be possible for each project to cascade-it on its own fork. 



## Project Parts 


### Website

Will be hosted on [http://battleia.fr] 

### Logo & Visuals 

The Project is based on a game concept : an arena, with bots. 
Rules & objectives are to be defined & coded by the students.

The project is structured in standalone programs, comunicating through WebSockets.

  * BattleIAServer
  * SampleBot
  * BattleIAViewers
  
### BattleIAServer 

The game server. It manage the running of the game, to ensure the bots having the same rules, to give each bot its turn to play...

### SampleBot (robot)
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



