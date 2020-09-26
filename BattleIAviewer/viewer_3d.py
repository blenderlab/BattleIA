#!/usr/bin/env python

# WS client example

import websocket
from threading import Thread 

from ursina import *
from ursina.prefabs.first_person_controller import FirstPersonController

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

class Sky(Entity):

    def __init__(self, **kwargs):
        super().__init__(
            parent = render,
            name = 'sky',
            model = 'sky_dome',
            texture = 'sky_default',
            scale = 2900
            )

    def update(self):
        self.world_position = camera.world_position

class Voxel(Button):
    def __init__(self, position=(0,0,0)):
        super().__init__(
            parent = scene,
            position = position,
            model = 'cube',
            origin_y = .5,
            texture = 'white_cube',
            color = color.color(0, 0, random.uniform(.9, 1.0)),
        )

class Energy(Button):
    def __init__(self, position=(0,0,0)):
        super().__init__(
            parent = scene,
            position = position,
            model = 'sphere',
            origin_y = .5,
            texture = 'white_cube',
            color = color.rgb(200,120,20),
        )

class Robot(Button):
    def __init__(self, position=(0,0,0)):
        super().__init__(
            parent = scene,
            position = position,
            model = 'cube',
            origin_y = .5,
            texture = 'white_cube',
            color = color.rgb(20,120,200),
        )
    
def display_map(m):
    c=0
    #print ("Map = ", m.W, m.H)
    for i in range(0,m.H):
        for j in range(0,m.W):
            v = Voxel(position=(i,1,j))
            v.color = color.rgb(80,80,80)
            if m.Map[c]==4:
                #print(f"{bcolors.FAIL}{chars[m.Map[c]]}{bcolors.ENDC}", end='')
                e = Robot(position=(i,2,j))
            if m.Map[c]==3:
                #print(f"{bcolors.WARNING}{chars[m.Map[c]]}{bcolors.WARNING}", end='')
                e = Energy(position=(i,2,j))
            if m.Map[c]==2:
                #print(f"{bcolors.OKBLUE}{chars[m.Map[c]]}{bcolors.WARNING}", end='')
                v = Voxel(position=(i,2,j))
                v.color = color.rgb(100, 100, 100)   # Note I still can reference any individual object I want
            c=c+1


def on_message(ws, message):
    print(message)
    m = _Map()
    count = 0
    print("[Debug] message")
    if (message[0])=='M':
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

def on_error(ws, error):
    print(error)

def on_close(ws):
    print("### closed ###")

def on_open(ws):
    def run(*args):
        print("...")
    thread.start_new_thread(run, ())

def update():
    if t_socket.is_alive():
        t_socket.join()
    else : 
        t_socket.start()
    

def l_socket():
    wsurl = 'ws://localhost:4626/display'
    ws = websocket.WebSocketApp(wsurl,
                              on_message = on_message,
                              on_error = on_error,
                              on_close = on_close)
    ws.on_open = on_open
    ws.run_forever()

t_socket = Thread(target = l_socket)
    
if __name__ == '__main__':
 
    app = Ursina()
    scene.camera.orthographic = False
    scene.camera.position = (10, 15, -13)
    scene.camera.rotation = (25, 0, 0)
    scene.camera.fov = 70 
    window.color=color.black
    window.exit_button.visible = False
    window.fps_counter.enabled = False
    mouse.visible = False
    window.title = 'BattleIA Viewer'
    Sky()
    Light(type='ambient', color=(0.9,0.3,0.3,1))  # full spectrum
    window.borderless = False               # Show a border
    window.fullscreen = False               # Do not go Fullscreen
    window.exit_button.visible = False      # Do not show the in-game red X that loses the window
    window.fps_counter.enabled = True   
    app.run()
   
    

