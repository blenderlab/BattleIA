#!/usr/bin/env python

# WS client example

import asyncio
import websockets

   
_map=[]
W=H=0


class bcolors:
    HEADER = '\033[95m'
    OKBLUE = '\033[94m'
    OKGREEN = '\033[92m'
    WARNING = '\033[93m'
    FAIL = '\033[91m'
    ENDC = '\033[0m'
    BOLD = '\033[1m'
    UNDERLINE = '\033[4m'

class _Map:
  def __init__(self):
    self.W=0
    self.H=0
    self.Map=[]

chars=[' ','O','#',"*","R","X","+"]

    
def display_map(m):
    c=0
    print ("Map = ", m.W, m.H)
    for i in range(0,m.H):
        print("")
        for j in range(0,m.W):
            if m.Map[c]==4:
                print(f"{bcolors.FAIL}{chars[m.Map[c]]}{bcolors.ENDC}", end='')
            if m.Map[c]==3:
                print(f"{bcolors.WARNING}{chars[m.Map[c]]}{bcolors.WARNING}", end='')
            if m.Map[c]==2:
                print(f"{bcolors.OKBLUE}{chars[m.Map[c]]}{bcolors.WARNING}", end='')
            if m.Map[c]==0:
                print(" ", end='')
            c=c+1

    print()


async def listen_for_message(websocket):
    async for message in websocket:
        m = _Map()
        count = 0
        if (message[0])=='M':
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
        if (message[0])=='P':
        	oldX=message[1]
        	oldY=message[2]
        	newX=message[3]
        	newY=message[4]
        	print (f"move from {oldX} {oldY} to {newX} {newY}")



async def my_app():
    wsurl = 'ws://localhost:4626/display'
    async with websockets.connect(wsurl) as websocket:
        await listen_for_message(websocket)

if __name__ == '__main__':
    loop = asyncio.get_event_loop()
    loop.run_until_complete(my_app())
    loop.run_forever()

