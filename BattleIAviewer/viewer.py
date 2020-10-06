#!/usr/bin/env python

# WS client example
import pygame
import sys
import traceback
import asyncio
import websockets
from pygame.locals import *
   
_map=[]
W=H=0


class _Map:
  def __init__(self):
    self.W=0
    self.H=0
    self.Map=[]
    self.nBots=0

class Bot:
    def __init__(self):
        self.posx=0
        self.posy=0
        self.name="----"
        self.energy=0
        self.score=0

chars=[' ','O','#',"*","R","X","+"]

    
def display_map(m):
    c=0
    WALL = (90, 20, 20)
    NRJ= (200,180,20)
    ROBOT=(0,0,200)
    GROUND = (0, 0, 0)
    for i in range(0,m.H):
        for j in range(0,m.W):
            rect = Rect(i*10, j*10, 10, 10)
            if m.Map[c]==8:
                color=ROBOT
                myfont = pygame.font.SysFont('system', 14)
                pygame.draw.rect(screen, color, rect)
                textsurface = myfont.render('R', False, (0, 0, 0))
                screen.blit(textsurface,(i*10+1,j*10+1))
            if m.Map[c]==3:
                color=NRJ
                pygame.draw.rect(screen, color, rect)

            if m.Map[c]==2:
                color=WALL
                pygame.draw.rect(screen, color, rect)
            if m.Map[c]==0:
                color=GROUND
                pygame.draw.rect(screen, color, rect)

            c=c+1
    pygame.display.flip()


def display_bot(bot):
    ROBOT=(0,0,200)
    rect = Rect(bot.posx*10, bot.posy*10, 10, 10)
    color=ROBOT
    myfont = pygame.font.SysFont('system', 14)
    pygame.draw.rect(screen, color, rect)
    textsurface = myfont.render('R', False, (0, 0, 0))
    screen.blit(textsurface,(bot.posx*10+1,bot.posy*10+1))
    pygame.display.flip()


async def listen_for_message(websocket):
    async for message in websocket:
        try:

            m = _Map()
            count = 0
            if (message[0])=='M':
                try:
                	print ("[DEBUG] MAP received")
                	for d in message:
                		if count < 0:
                			print(f"< {format(ord(d))}")
                		if count==1:
                			m.W=int(ord(d))
                		if count==3:
                			m.H=int(ord(d))
                		if count> 4 :
                			m.Map.append(ord(d))
                		count = count+ 1
                	display_map(m)
                except:
                    print("Error MAP")
                    var = traceback.format_exc()
                    print(var)
            
            if message[0]=='B':
                try:
                    print (f"[DEBUG] BOT !")
                    nBots=int(ord(message[1]))
                    print (f"[DEBUG] {nBots} BOT found")
                    
                    for nb in range(nBots):
                        name=""
                        nrgi=int(ord(message[2+nb*13]))
                        posx=int(ord(message[3+nb*13]))
                        posy=int(ord(message[4+nb*13]))
                        scor=int(ord(message[5+nb*13]))
                        for i in range(9):
                            name=name+format(message[6+nb*13+i])
                        bot = Bot()
                        bot.name=name
                        bot.nrgi=nrgi
                        bot.posx=posy
                        bot.posy=posx
                        print(f"[BOT] Name = {name} {posx} {posy}")
                        display_bot(bot)

                except:
                    print("Error BOT")
                    var = traceback.format_exc()
                    print(var)
        except:
            print("Error BOT")
            var = traceback.format_exc()
            print(var)

       


async def my_app():
    wsurl = 'ws://localhost:4626/display'
    async with websockets.connect(wsurl) as websocket:
        await listen_for_message(websocket)

if __name__ == '__main__':
    pygame.display.init()
    screen= pygame.display.set_mode(
            (600, 600), pygame.RESIZABLE
        )
    pygame.font.init() 
    loop = asyncio.get_event_loop()
    loop.run_until_complete(my_app())
    loop.run_forever()
    pygame.quit()
    quit()

