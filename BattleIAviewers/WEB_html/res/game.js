

var ctx = null;
var gameMap = [];
var tileW = 12, tileH = 12;
var mapW = 10, mapH = 10;
var currentSecond = 0, frameCount = 0, framesLastSecond = 0;
var server_ip = "localhost";
var server_port = "4626";
var server_state = "DOWN";

let socket = new WebSocket("ws://"+server_ip+":"+server_port+"/display");

var img_wall = new Image();
var img_bot = new Image();
var img_nrg = new Image();
var img_empty = new Image();
var img_respawn = new Image();
img_wall.src = 'res/tile_wall.png';
img_empty.src = 'res/tile_empty.png';
img_nrg.src = 'res/tile_power.png';
img_bot.src = 'res/tile_robot.png';
img_respawn.src = 'res/tile_respawn.png';
var p_wall;
var p_empty;
var p_nrg;
var p_bot;
var p_respawn;
	
window.onload = function()
{
	ctx = document.getElementById('game').getContext("2d");
	requestAnimationFrame(drawGame);
	ctx.font = "bold 10pt sans-serif";
	updateServerState();
	p_wall = ctx.createPattern(img_wall, 'repeat');
	p_empty = ctx.createPattern(img_empty, 'repeat');
	p_nrg = ctx.createPattern(img_nrg, 'repeat');
	p_bot = ctx.createPattern(img_bot, 'repeat');
	p_respawn = ctx.createPattern(img_respawn, 'repeat');
};
function ping() {
        socket.send('*');
        tm = setTimeout(function () {
    }, 5000);
}

function pong() {
    clearTimeout(tm);
}	

socket.onopen = function(e) {
	server_state="UP";
  	updateServerState();
  
};



socket.onmessage = function(event) {
 var  message = (event.data);
 console.log(message);
    if (message[0] == '*') {
        pong();
        return;
    }
	if (message[0]=="M"){
		console.log('[MSG] Map info ');
		updatemap(message)
	}
	if (message[0]=="P"){
		console.log('[MSG] Move player ');
		moveplayer(event.data.charCodeAt(1),event.data.charCodeAt(2),event.data.charCodeAt(3),event.data.charCodeAt(4));
	}
	if (message[0]=="R"){
		console.log('[MSG] remove player (dead!) ');
		removeplayer(event.data.charCodeAt(1),event.data.charCodeAt(2));
	}
	if (message[0]=="C"){
		console.log('[MSG] clear case ');
		removeplayer(event.data.charCodeAt(1),event.data.charCodeAt(2));
	}
	if (message[0]=="B"){
		console.log('[MSG] Bots info ');
		updateBots(message);

	}
};

socket.onclose = function(event) {
  if (event.wasClean) {
    alert(`[close] Connection closed cleanly, code=${event.code} reason=${event.reason}`);
  } else {
   socket = new WebSocket("ws://"+server_ip+":"+server_port+"/display");

  }
  server_state="DOWN";
  updateServerState();
};

socket.onerror = function(error) {
  server_state="DOWN";
  updateServerState();
};

function updateServerState(){
	document.getElementById( 'serverip' ).innerHTML=server_ip;
	document.getElementById( 'serverport' ).innerHTML=server_port;
	document.getElementById( 'serverstate' ).innerHTML=server_state;
}

function updatemap(message){
	nbBots=message.charCodeAt(1);
	mapW = message.charCodeAt(1)+256*message.charCodeAt(2);
	mapH = message.charCodeAt(3)+256*message.charCodeAt(4);
	console.log(`Map Size ${mapW} x ${mapH}`);
	for (i=0;i<mapH;i++){
		for (j=0;j<mapW;j++){
		gameMap.push(message.charCodeAt(5+i*mapW+j));
		}
	}
}

function moveplayer(x1,y1,x2,y2){
	gameMap[y1*mapW+x1]=0;
	gameMap[y2*mapW+x2]=4;
}

function removeplayer(x1,y1){
	gameMap[y1*mapW+x1]=0;
}


function clearcase(x1,y1){
	gameMap[y1*mapW+x1]=0;
}

function updateBots(message){
	nbBots=message.charCodeAt(1);
	console.log(nbBots+' bot(s) found: ');
	bot_container = document.getElementById( 'bots' );
	bot_container.innerHTML='';
	for (i=0;i<nbBots;i++){
		
		p=i*13
		name=""
		for (j=0;j<9;j++){
			name=name+String.fromCharCode(message.charCodeAt(p+6+j));
		}
	nrj =message.charCodeAt(p+2); 
	var bot_div ;
	var bot_container ;
	bot_div = document.createElement( 'li' );
	bot_div.innerHTML = "<div class='name'>"+name+"</div>"; 
	bot_div.innerHTML += "<div class='nrj'><span>Energy </span><progress class='progress is-primary' value='"+nrj+"' max='100'>Energy</progress></div>";
	bot_container.appendChild( bot_div ); 
	gameMap[message.charCodeAt(p+4)*mapW+message.charCodeAt(p+3)]=4;
	}
}

function drawGame()
{
	if(ctx==null) { return; }

	var sec = Math.floor(Date.now()/1000);
	if(sec!=currentSecond)
	{
		currentSecond = sec;
		framesLastSecond = frameCount;
		frameCount = 1;
	}
	else { frameCount++; }

	for(var y = 0; y < mapH; ++y)
	{
		for(var x = 0; x < mapW; ++x)
		{
			switch(gameMap[((y*mapW)+x)])
			{
				case 0: // Empty
					ctx.fillStyle = p_empty;
					break;
				case 1: // ???

					ctx.fillStyle = "#121211";
					break;
				case 2:
					// Wall 
					ctx.fillStyle = p_wall;
					break;
				case 3:
					// ?? 
					ctx.fillStyle = p_nrg;
					break;
				case 4:
					// Bot 
					ctx.fillStyle = p_bot;
					break;
				case 5:
					// Respawn 
					ctx.fillStyle = p_respawn;
					break;
				default:
					ctx.fillStyle = "#5aa457";
			}

			ctx.fillRect( x*tileW, y*tileH, tileW, tileH);
		}
	}

	ctx.fillStyle = "#CC0000";
	ctx.fillText("FPS: " + framesLastSecond, 0, 10);

	requestAnimationFrame(drawGame);
}
