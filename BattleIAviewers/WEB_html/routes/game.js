const WebSocket = require('ws');
var express = require('express');
var router = express.Router();

var map = null;


/* GET users listing. */
router.get('/', function(req, res, next) {
	let clients = [new WebSocket('ws://localhost:4626/display')];
	clients.map(client => {
	  client.on('message', msg=> manage_msg(msg,res));
	});
	// Wait for the client to connect using async/await
	new Promise(resolve => clients[0].once('open', resolve));

 });



function manage_msg(msg,res) {
  console.log(`========`);
  var msgByte = Buffer.from(msg)
  if (msg.charAt(0)=='M'){
  	console.log("[MAP]");
  	w = msgByte[1]+256*msgByte[2];
  	h = msgByte[3]+256*msgByte[4];
  	console.log(`Map Size ${w} x ${h}`);
  	
  	for (i=0;i<h;i++){
	  	l=""
	  	for (j=0;j<w;j++){
  			l=l+msgByte[5+i*w+j];
        map.add(l);
  		}
  		console.log(`${l}`);
  	}

  }
  if (msg.charAt(0)=='B'){
  	console.log("[BOT]");
  	nbBots= msgByte[1];
  	console.log(`${nbBots} listed : `);
  	for (i=0;i<nbBots;i++){
  		p=i*13
  		name=""
  		for (j=0;j<9;j++){
  			name=name+String.fromCharCode(msgByte[p+6+j]);
  		}
		console.log(`${i} NAME = ${name}`)
  		console.log(`${i} Energy = ${msgByte[p+2]}`)
  		console.log(`${i} X,Y = ${msgByte[p+3]},${msgByte[p+4]}`)
  		console.log(`${i} Score = ${msgByte[p+5]}`)
  		
  	}
  }
  res.render('index');

};

module.exports = router;
