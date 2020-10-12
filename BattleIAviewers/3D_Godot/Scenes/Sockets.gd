extends Node

# The URL we will connect to.
export var websocket_url = "ws://localhost:4626/display"

# Our WebSocketClient instance.
var _client = WebSocketClient.new()

func _ready():
	# Connect base signals to get notified of connection open, close, and errors.
	_client.connect("connection_closed", self, "_closed")
	_client.connect("connection_error", self, "_closed")
	_client.connect("connection_established", self, "_connected")
	# This signal is emitted when not using the Multiplayer API every time
	# a full packet is received.
	# Alternatively, you could check get_peer(1).get_available_packets() in a loop.
	_client.connect("data_received", self, "_on_data")

	# Initiate connection to the given URL.
	var err = _client.connect_to_url(websocket_url)
	if err != OK:
		print("Unable to connect")
		set_process(false)


func _closed(was_clean = false):
	# was_clean will tell you if the disconnection was correctly notified
	# by the remote peer before closing the socket.
	print("Closed, clean: ", was_clean)
	set_process(false)


func _connected(proto = ""):
	# This is called on connection, "proto" will be the selected WebSocket
	# sub-protocol (which is optional)
	print("Connected with protocol: ", proto)
	# You MUST always use get_peer(1).put_packet to send data to server,
	# and not put_packet directly when not using the MultiplayerAPI.
	#_client.get_peer(1).put_packet("Test packet".to_utf8())


func _on_data():
	# Print the received packet, you MUST always use get_peer(1).get_packet
	# to receive data from server, and not get_packet directly when not
	# using the MultiplayerAPI.
	var data =  _client.get_peer(1).get_packet()
	print(char(data[0]))
	if char(data[0])=='M':
		var H = int() 
		var W =  int()

		H = data[1]
		W = data[3]
		print (data[1])
		print (data[3])
		var c= 5
		var d=0
		get_node('../terrain').set_terrain_height(H)
		get_node('../terrain').set_terrain_width(W)
		for i in range(W):
			for j in range(H):
				if data[c]==2:
					get_node('../terrain').addBlock(H-j,i,d)
					d+=1
				if data[c]==3:
					get_node('../terrain').addPower(H-j,i,d)
					d+=1
				if data[c]==4:
					get_node('../terrain').addRobot(H-j,i,d)
					d+=1
				c=c+1
			


func _process(_delta):
	# Call this in _process or _physics_process. Data transfer, and signals
	# emission will only happen when calling this function.
	_client.poll()


func _exit_tree():
	_client.disconnect_from_host()
